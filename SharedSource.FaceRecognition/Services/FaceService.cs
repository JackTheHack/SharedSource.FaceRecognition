using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ImageResizer;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using SharedSource.FaceRecognition.Models;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.Resources.Media;
using Sitecore.Search;
using Sitecore.SecurityModel;
using FaceMetadata = SharedSource.FaceRecognition.Models.FaceMetadata;
using Person = SharedSource.FaceRecognition.Models.Person;

namespace SharedSource.FaceRecognition.Services
{
    public class FaceService
    {
        private readonly Item _personsRoot;

        public FaceService()
        {
            var personsRootId = Settings.GetSetting("PersonsRoot");

            if (!string.IsNullOrEmpty(personsRootId))
            {
                _personsRoot = Sitecore.Data.Database.GetDatabase("web").GetItem(ID.Parse(personsRootId));
            }
        }

        public async void Train()
        {
            try
            {
                var personRoot = GetPersonGroup();

                using (
                    var faceServiceClient = new FaceServiceClient(
                        Settings.GetSetting("Cognitive.Key1"),
                        "https://eastus2.api.cognitive.microsoft.com/face/v1.0"))
                {
                    bool isTrained = false;

                    TrainingStatus trainingStatus = null;

                    try
                    {
                        trainingStatus =
                            await
                                faceServiceClient.GetPersonGroupTrainingStatusAsync(
                                    personRoot.PersonGroupId.ToLowerInvariant());

                        if (trainingStatus != null)
                        {
                            Log.Info(
                                $"Training status - {trainingStatus.Message} {trainingStatus.Status} {trainingStatus.LastActionDateTime} {trainingStatus.CreatedDateTime}",
                                this);
                        }

                    }
                    catch (FaceAPIException ex)
                    {
                        Log.Info("Person group was not trained at all..", this);
                        isTrained = false;
                    }

                    var persons = await faceServiceClient.ListPersonsAsync(personRoot.PersonGroupId.ToLowerInvariant());

                    foreach (var person in persons)
                    {
                        Log.Info($"Found person {person.PersonId} {person.Name}", this);
                    }

                        if (trainingStatus != null && trainingStatus.Status == Status.Succeeded)
                        {
                            isTrained = true;
                            Log.Info("Person group is successfully trained already.", this);
                        }


                    if(!isTrained)
                    {
                        await faceServiceClient.TrainPersonGroupAsync(personRoot.PersonGroupId.ToLowerInvariant());
                    }


                }
            }
            catch (FaceAPIException e)
            {
                Log.Info("FaceApi exception -"+ e.ErrorMessage, this);
            }
            catch (Exception e)
            {
                Log.Info("Exception during training - " + e.ToString(), this);
            }
        }

        public List<MediaItem> GetImages()
        {
            var root = Sitecore.Context.Database.GetItem(ID.Parse(Settings.GetSetting("ImagesRoot")));
            return root.Children.Select(i => new MediaItem(i)).ToList();
        }

        public List<Person> GetAllPersons()
        {
            return _personsRoot?.Children.Select(i => new Person(i)).ToList();
        }

        public PersonGroup GetPersonGroup()
        {
            return new PersonGroup()
            {
                Name = _personsRoot["Name"],
                PersonGroupId = _personsRoot["GroupId"]
            };
        }

        public List<Person> SearchPersons(string searchString)
        {
            var indexName = Sitecore.Context.Database.Name == "master" ? "sitecore_master_index" : "sitecore_web_index";

            var index = ContentSearchManager.GetIndex(indexName);

            using (var context = index.CreateSearchContext())
            {

                    var searchQuery = context.GetQueryable<PersonResultItem>()
                        .Where(x => x.Name.StartsWith(searchString) || x.Surname.StartsWith(searchString));

                    return searchQuery.ToList().Select(i => new Person(i)).ToList();
                }
            }

        public async void IdentifyTag(Guid tagId, Guid personId)
        {
            var indexName = Sitecore.Context.Database.Name == "master" ? "face_master_index" : "face_web_index";

            var index = ContentSearchManager.GetIndex(indexName);

            var personItem = GetPersonById(personId);

            if (personItem == null)
            {
                return;
            }

            Log.Info("Identify Tag - Person found " + personItem.Name, this);


            using (var context = index.CreateSearchContext())
            {
                var searchQuery = context.GetQueryable<FaceSearchResultItem>()
                    .Where(x => x.FaceTags.Contains(tagId));

                var resultItem = searchQuery.FirstOrDefault();

                if (resultItem != null)
                {
                    Log.Info("Identify Tag - Tag found - " + resultItem.ItemId + " " + resultItem.Faces, this);

                    var imageItem = Database.GetDatabase("master").GetItem(resultItem.ItemId);

                    if (imageItem == null)
                    {
                        return;
                    }

                    Log.Info("Identify Tag - Image item " + imageItem.Name, this);


                    var faces =
                        JsonConvert.DeserializeObject(resultItem.Faces, typeof (List<FaceMetadata>)) as
                            List<FaceMetadata>;
                    var face = faces?.FirstOrDefault(i => i.UniqueId == tagId);

                    await UpdateFaceDefinition(imageItem, face, personItem);

                    SetIdentifiedPersonInfo(tagId, imageItem, personItem);

                    PublishManager.PublishItem(imageItem, new[] { Database.GetDatabase("web"), }, new[] { imageItem.Language }, false, false);

                    index.Refresh(new SitecoreIndexableItem(imageItem), IndexingOptions.Default);
                }
                else
                {
                    Log.Info("Identify Tag - Tag doesnt exissts " + tagId, this);
                }
            }            
    }

        private void SetIdentifiedPersonInfo(Guid tagId, Item imageItem, Person personItem)
        {
            if (personItem == null || imageItem == null)
            {
                return;
            }

            var value = (!string.IsNullOrEmpty(imageItem["Description"])
                ? JsonConvert.DeserializeObject<IdentifiedPerson[]>(imageItem["Description"])
                : new IdentifiedPerson[0]).ToList();

            if (!value.Any(i => i.Id == tagId && i.Data != null && i.Data.ID == personItem.ID))
            {
                value.Add(new IdentifiedPerson
                {
                    Id = tagId,
                    Confidence = 1.0,
                    Data = personItem
                });
            }

            using (new SecurityDisabler())
            {
                imageItem.Editing.BeginEdit();
                imageItem.Fields["Description"].SetValue(JsonConvert.SerializeObject(value), true);
                imageItem.Editing.EndEdit(false, true);
            }            
        }

        private async Task UpdateFaceDefinition(Item imageItem, FaceMetadata face, Person personItem)
        {
            if (face != null)
            {            
                if (imageItem != null)
                {
                    var mediaItem = new MediaItem(imageItem);

                    using (var croppedImage = new MemoryStream())
                    {
                        try
                        {
                            CropImage(face.FacePosition, mediaItem.GetMediaStream(), croppedImage);

                            var faceServiceClient = new FaceServiceClient(Settings.GetSetting("Cognitive.Key1"),
                                "https://eastus2.api.cognitive.microsoft.com/face/v1.0");

                            croppedImage.Seek(0, SeekOrigin.Begin);

                            Log.Info(
                                $"Adding person face to person {personItem.AzurePersonGroupId} {personItem.AzureId}",
                                this);

                            await faceServiceClient.AddPersonFaceAsync(personItem.AzurePersonGroupId.ToLowerInvariant(),
                                Guid.Parse(personItem.AzureId), croppedImage);

                            Log.Info("Identify tag - Success", this);
                        }
                        catch (FaceAPIException faceEx)
                        {
                            Log.Info("FaceAPI exception + "+faceEx.ErrorMessage, this);
                        }
                        catch (Exception e)
                        {
                            Log.Info("Error during resize "+e.ToString(), this);
                        }
                    }
                }
            }
        }

        public void CropImage(Rectangle cp, Stream source, MemoryStream croppedImage)
        {
            var imageResizer = new ImageResizer.Configuration.Config();
            string cropFormat = $"?crop={cp.Left},{cp.Top},{cp.Right},{cp.Bottom}";
            var resizeJob = new ImageResizer.ImageJob(source, croppedImage,
                new Instructions(cropFormat));
            imageResizer.Build(resizeJob);
        }

        private Person GetPersonById(Guid personId)
        {
            return GetAllPersons().FirstOrDefault(i => i.ID == ID.Parse(personId));
        }

        public async Task<Guid> CreatePerson(string personGroupId, string personName)
        {
            try
            {
                var faceServiceClient = new FaceServiceClient(Settings.GetSetting("Cognitive.Key1"), "https://eastus2.api.cognitive.microsoft.com/face/v1.0");

                var result = await faceServiceClient.CreatePersonAsync(personGroupId.ToLowerInvariant(), personName);

                Log.Info($"Created person - {personGroupId} {personName} {result.PersonId}", this);


                return result.PersonId;
            }
            catch (FaceAPIException e)
            {
                Log.Info("CreatePerson failed - " + e.ToString(),this);
                throw;
            }
        }

        public async void CreatePersonGroupIfNotExists(string personGroupId, string personGroupName)
        {
            var faceServiceClient = new FaceServiceClient(Settings.GetSetting("Cognitive.Key1"), "https://eastus2.api.cognitive.microsoft.com/face/v1.0");
            PersonGroup personGroup = null;

            try
            {
                personGroup = await faceServiceClient.GetPersonGroupAsync(personGroupId.ToLowerInvariant());
            }
            catch (FaceAPIException e)
            {
            }

            try
            {
                if (personGroup == null)
                {                    
                    await faceServiceClient.CreatePersonGroupAsync(personGroupId.ToLowerInvariant(), personGroupName);

                    Log.Info($"Created person group " + personGroupId, this);
                }
                else
                {
                    Log.Info($"Person group already exists - " + personGroupId, this);
                }
            }
            catch (FaceAPIException e)
            {
                Log.Info("Exception during creating person group" + e.ToString(), this);
                throw;
            }

        }
    }
}
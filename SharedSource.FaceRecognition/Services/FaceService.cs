using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ImageResizer;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using SharedSource.FaceRecognition.Models;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Publishing;
using Sitecore.SecurityModel;
using FaceMetadata = SharedSource.FaceRecognition.Models.FaceMetadata;
using Person = SharedSource.FaceRecognition.Models.Person;

namespace SharedSource.FaceRecognition.Services
{
    public class FaceService : IFaceService
    {
        private StringBuilder StringLog { get; }

        private readonly Item _personsRoot;
        private readonly Item _mediaRoot;

        public FaceService()
        {
            StringLog = new StringBuilder();

            var settings = Sitecore.Context.Database.GetItem("{1B29F0D5-F935-4E0D-AC4A-07D6A3E0D4A1}");
            var personsRootId =  settings["PersonGroup"];
            var mediaRootId = settings["MediaFolder"];

            if (!string.IsNullOrEmpty(personsRootId) && !string.IsNullOrEmpty(mediaRootId))
            {
                _personsRoot = Database.GetDatabase("web").GetItem(ID.Parse(personsRootId));
                _mediaRoot = Database.GetDatabase("web").GetItem(ID.Parse(mediaRootId));
            }
        }

        public FaceService(StringBuilder log) : this()
        {
            StringLog = log;
        }

        public FaceServiceClient CreateAzureClient()
        {
            return new FaceServiceClient(
                Settings.GetSetting("Cognitive.Key1"),
                Settings.GetSetting("Cognitive.Url"));
        }

        public async Task<ExtendedTrainResult> GetTrainingStatusAsync()
        {
            using (var faceServiceClient = CreateAzureClient())
            {
                var personRoot = GetPersonGroup();                

                var trainingStatus =
                            await
                                faceServiceClient.GetPersonGroupTrainingStatusAsync(
                                    personRoot.PersonGroupId.ToLowerInvariant());

                if (trainingStatus != null)
                {
                    var result = new ExtendedTrainResult()
                    {
                        Status = trainingStatus.Status.ToString(),
                        Message = trainingStatus.Message,
                        LastAction = trainingStatus.LastActionDateTime,
                        IsTrained = true                        
                    };


                    try
                    {
                        var persons =
                            await faceServiceClient.ListPersonsAsync(personRoot.PersonGroupId.ToLowerInvariant());
                        result.Persons = persons?.Select(i => new PersonData() { FaceCount = i.PersistedFaceIds.Length, Name = i.Name, Id = i.PersonId }).ToList();
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e.ToString(), e, this);
                    }

                    try
                    {
                        var personGroup = await faceServiceClient.GetPersonGroupAsync(personRoot.PersonGroupId.ToLowerInvariant());

                        result.PersonGroup =
                                personGroup != null
                                    ? new PersonGroupData() {Id = personGroup.PersonGroupId, Name = personGroup.Name}
                                    : null;
                    }
                    catch (FaceAPIException e)
                    {
                        Log.Warn(e.ToString(), e, this);
                    }




                    Log.Info(
                        $"Training status - {trainingStatus.Message} {trainingStatus.Status} {trainingStatus.LastActionDateTime} {trainingStatus.CreatedDateTime}",
                        this);

                    return result;
                }
            }

            return null;
        }

        public async Task<TrainResult> TrainAsync(bool forceTraining = false)
        {
            try
            {
                var result = new TrainResult()
                {
                    Message = "No information"
                };

                var personRoot = GetPersonGroup();

                using (var faceServiceClient = CreateAzureClient())
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
                            result.Status = trainingStatus.Status.ToString();
                            result.Message = trainingStatus.Message;
                            result.LastAction = trainingStatus.LastActionDateTime;
                            result.IsTrained = true;

                            Log.Info(
                                $"Training status - {trainingStatus.Message} {trainingStatus.Status} {trainingStatus.LastActionDateTime} {trainingStatus.CreatedDateTime}",
                                this);
                        }

                    }
                    catch (FaceAPIException ex)
                    {
                        Log.Info("Person group was not trained at all..", this);
                        result.Message = ex.ErrorMessage;
                        result.IsTrained = false;
                    }

                    if (trainingStatus != null && trainingStatus.Status == Status.Succeeded)
                   {
                            isTrained = true;
                            result.Message = "Person group is successfully trained already";
                   }

                   if (!isTrained || forceTraining)
                   {
                        await faceServiceClient.TrainPersonGroupAsync(personRoot.PersonGroupId.ToLowerInvariant());
                        result.Message = "Started person group training";
                    }

                    return result;
                }
            }
            catch (FaceAPIException e)
            {
                Log.Info("FaceApi exception -"+ e.ErrorMessage, this);
                
            }
            catch (Exception e)
            {
                Log.Info("Exception during training - " + e, this);
            }   

            return null;
        }

        public List<MediaItem> GetImages()
        {
            return _mediaRoot != null ? _mediaRoot.Children.Select(i => new MediaItem(i)).ToList() : new List<MediaItem>();
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

        public async void IdentifyTagAsync(Guid tagId, Guid personId)
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

                    imageItem = Database.GetDatabase("master").GetItem(imageItem.ID);

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

            
            var value = (!string.IsNullOrEmpty(imageItem["IdentifiedPersons"])
                ? JsonConvert.DeserializeObject<IdentifiedPerson[]>(imageItem["IdentifiedPersons"])
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
                imageItem.Fields["IdentifiedPersons"].SetValue(JsonConvert.SerializeObject(value), true);
                imageItem.Editing.EndEdit(false);
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

                            var faceServiceClient = CreateAzureClient();

                            croppedImage.Seek(0, SeekOrigin.Begin);

                            Image img = Image.FromStream(croppedImage);
                            img.Save(HttpContext.Current.Server.MapPath("~/face.jpg"));

                            croppedImage.Seek(0, SeekOrigin.Begin);

                            Log.Info($"Adding person face to person {personItem.AzurePersonGroupId} {personItem.AzureId}", this);                            

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
                            Log.Info("Error during resize "+e, this);
                        }
                    }
                }
            }
        }

        private void CropImage(Rectangle cp, Stream source, MemoryStream croppedImage)
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

        private async Task<Guid> CreateOrUpdatePerson(string personGroupId, Guid personId, string personName)
        {
            try
            {
                var faceServiceClient = CreateAzureClient();

                var person = personId != Guid.Empty
                    ? await faceServiceClient.GetPersonAsync(personGroupId.ToLowerInvariant(), personId)
                    : null;                

                if (person == null)
                {
                    var result = await faceServiceClient.CreatePersonAsync(personGroupId.ToLowerInvariant(), personName);
                    Log.Info($"Created person - {personGroupId} {personName} {result.PersonId}", this);
                    return result.PersonId;
                }

                await
                    faceServiceClient.UpdatePersonAsync(personGroupId.ToLowerInvariant(), personId, personName,
                        person.UserData);

                return personId;
            }
            catch (FaceAPIException e)
            {
                Log.Info("CreatePerson failed - " + e.ToString(),this);
                throw;
            }
        }

        private async void CreatePersonGroupIfNotExists(string personGroupId, string personGroupName)
        {
            var faceServiceClient = CreateAzureClient();
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

        public async void CreatePersonByItemAsync(Item item)
        {
            try
            {
                //Person template
                if (item.TemplateID == ID.Parse("{1EB19995-B7D6-4AAC-ACF7-6411264DCE24}"))
                {
                    var parentGroup = item.Parent;

                    if (!string.IsNullOrEmpty(item["Title"]))
                    {
                        Guid personId;

                        if (!Guid.TryParse(item["Id"], out personId))
                        {
                            personId = Guid.Empty;
                        }

                        var person = await CreateOrUpdatePerson(parentGroup["GroupId"], personId, item["Title"]);

                        if(personId == Guid.Empty)
                        {
                            item.Editing.BeginEdit();
                            item.Fields["Id"].SetValue(person.ToString(), true);
                            item.Editing.EndEdit(true, false);
                        }
                    }
                }

                //Person group template
                if (item.TemplateID == ID.Parse("{157A7C04-DC65-4B1B-BD43-A4071C2B8604}"))
                {
                    if (!string.IsNullOrEmpty(item["GroupName"]) && !string.IsNullOrEmpty("GroupId"))
                    {
                        CreatePersonGroupIfNotExists(item["GroupId"], item["GroupName"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info("Error during creating entity in Azure - " + ex, this);
            }
        }

        public async Task<bool> RebuildPersonGroup()
        {
            try
            {

                if (_personsRoot != null && !string.IsNullOrEmpty(_personsRoot["GroupName"]))
                {
                    var faceServiceClient = CreateAzureClient();
                    await faceServiceClient.DeletePersonGroupAsync(_personsRoot["GroupId"]);

                    CreatePersonGroupIfNotExists(_personsRoot["GroupId"], _personsRoot["GroupName"]);

                    _personsRoot.Children.ForEach(CreatePersonByItemAsync);
                }

                return true;

            }
            catch (Exception ex)
            {
                Log.Info("Error during rebuilding Azure entities - " + ex, this);
                return false;
            }
        }
    }
}
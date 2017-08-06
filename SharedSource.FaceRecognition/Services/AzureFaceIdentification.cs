using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.ProjectOxford.Face;
using SharedSource.FaceRecognition.Models;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Services
{
    public class AzureFaceIdentification : IFaceIdentification
    {
        private readonly FaceService _faceService;

        public AzureFaceIdentification()
        {
            _faceService = new FaceService();
        }

        public List<IdentifiedPerson> Identify(List<FaceMetadata> faces, Stream stream)
        {
            return Task.Run(() => IdentifyAsync(faces, stream)).Result;
        }

        public async Task<List<IdentifiedPerson>> IdentifyAsync(List<FaceMetadata> faces, Stream stream)
        {
            var personRoot = _faceService.GetPersonGroup();

            var persons = _faceService.GetAllPersons();

                try
                {
                    using (
                        var faceServiceClient = new FaceServiceClient(
                            Settings.GetSetting("Cognitive.Key1"),
                            Settings.GetSetting("Cognitive.Url")))
                    {
                            var detectResult2 =
                                await
                                    faceServiceClient.IdentifyAsync(
                                        personRoot.PersonGroupId.ToLowerInvariant(),
                                        faces.Select(i => i.UniqueId).ToArray(), 0.5f, 3);

                        var result = new List<IdentifiedPerson>();
                        
                        foreach (var person in detectResult2)
                        {
                            result.AddRange(person.Candidates.Select(i => new IdentifiedPerson()
                            {
                                FaceId = person.FaceId,
                                Id = i.PersonId,
                                Confidence = i.Confidence,
                                Data =
                                    persons.FirstOrDefault(
                                        f => !string.IsNullOrEmpty(f.AzureId) && Guid.Parse(f.AzureId) == i.PersonId)
                            }).ToList());
                        }

                        return result;
                    }
                }
                catch (FaceAPIException faceEx)
                {
                    Log.Info("Face exception " + faceEx.ErrorMessage, this);
                }
                catch (Exception e)
                {
                    Log.Info("Exception during detection + " + e, this);
                }

            return new List<IdentifiedPerson>();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using SharedSource.FaceRecognition.Models;
using Sitecore.Data.Items;
using Person = SharedSource.FaceRecognition.Models.Person;

namespace SharedSource.FaceRecognition.Services
{
    public interface IFaceService
    {
        FaceServiceClient CreateAzureClient();
        Task<ExtendedTrainResult> GetTrainingStatusAsync();
        Task<TrainResult> TrainAsync(bool forceTraining = false);
        List<Person> GetAllPersons();
        PersonGroup GetPersonGroup();
        void IdentifyTagAsync(Guid tagId, Guid personId);
        void CreatePersonByItemAsync(Item item);
        Task<bool> RebuildPersonGroup();
        List<MediaItem> GetImages();
    }
}
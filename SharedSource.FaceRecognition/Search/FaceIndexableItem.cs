using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedSource.EngagementPlanViewer;
using SharedSource.FaceRecognition.Models;
using SharedSource.FaceRecognition.Services;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;

namespace SharedSource.FaceRecognition.Search
{
    public class FaceIndexableItem : SitecoreIndexableItem
    {
        public FaceIndexableItem(Item item) :  base(item)
        {
            var faceRecognitionLogic = new FaceRecognitionLogic();
            Task.Run(async () =>
            {
                Faces = await faceRecognitionLogic.DetectFaces(new MediaItem(item));
                IdentifiedPersons = !string.IsNullOrEmpty(item["Description"])
                    ? JsonConvert.DeserializeObject<IdentifiedPerson[]>(item["Description"])
                    : new IdentifiedPerson[0];
            }).Wait(TimeSpan.FromSeconds(3));
        }

        public IdentifiedPerson[] IdentifiedPersons { get; set; }

        public FaceMetadata[] Faces { get; set; }

    }
}
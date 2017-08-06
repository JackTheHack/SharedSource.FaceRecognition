using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedSource.FaceRecognition.Models;
using SharedSource.FaceRecognition.Services;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using static System.String;

namespace SharedSource.FaceRecognition.Search
{
    public class FaceIndexableItem : SitecoreIndexableItem
    {
        public FaceIndexableItem(Item item) :  base(item)
        {
            var faceIdentification = new AzureFaceIdentification();

                var mediaItem = new MediaItem(item);
            var mediaStream = mediaItem.GetMediaStream();

            if (mediaStream != null)
            {
                Faces =
                    !IsNullOrEmpty(item["FaceMetadata"])
                        ? JsonConvert.DeserializeObject<FaceMetadata[]>(item["FaceMetadata"])
                        : new FaceMetadata[0];
                IdentifiedPersons = !IsNullOrEmpty(item["IdentifiedPersons"])
                    ? JsonConvert.DeserializeObject<IdentifiedPerson[]>(item["IdentifiedPersons"])
                    : new IdentifiedPerson[0];

                Suggestions = faceIdentification.Identify(Faces.ToList(), mediaStream);
            }
        }

        public List<IdentifiedPerson> Suggestions { get; set; }

        public IdentifiedPerson[] IdentifiedPersons { get; set; }

        public FaceMetadata[] Faces { get; set; }

    }
}
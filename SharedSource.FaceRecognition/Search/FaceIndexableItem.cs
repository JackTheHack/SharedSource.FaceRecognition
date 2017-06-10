using SharedSource.EngagementPlanViewer;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;

namespace SharedSource.FaceRecognition.Search
{
    public class FaceIndexableItem : SitecoreIndexableItem
    {
        public FaceIndexableItem(Item item) :  base(item)
        {
            var faceRecognitionLogic = new FaceRecognitionLogic();
            Faces = faceRecognitionLogic.DetectFaces(new MediaItem(item));            
        }

        public FaceMetadata[] Faces { get; set; }

    }
}
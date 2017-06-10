using Newtonsoft.Json;
using Sitecore.ContentSearch.ComputedFields;

namespace SharedSource.FaceRecognition.Search
{
    public class FacePositionIndexableField : BaseFaceIndexableField, IComputedIndexField
    {
        protected override object GetFieldValue(FaceIndexableItem cognitiveIndexable)
        {
            return JsonConvert.SerializeObject(cognitiveIndexable.Faces);
        }        
    }
}
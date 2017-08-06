using Newtonsoft.Json;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Search
{
    public class FacePositionIndexableField : BaseFaceIndexableField, IComputedIndexField
    {
        protected override object GetFieldValue(FaceIndexableItem cognitiveIndexable)
        {
            var result = JsonConvert.SerializeObject(cognitiveIndexable.Faces);
            Log.Info("FacePositionIndexableField " + result, this);
            return result;
        }        
    }
}
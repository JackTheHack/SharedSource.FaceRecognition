using System.Linq;
using Newtonsoft.Json;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Search
{
    public class FaceTagIdIndexableField : BaseFaceIndexableField, IComputedIndexField
    {
        protected override object GetFieldValue(FaceIndexableItem cognitiveIndexable)
        {
            var result = cognitiveIndexable.Faces != null? cognitiveIndexable.Faces.Select(i => i.UniqueId).ToList() : null;
            Log.Info("FaceTagIdIndexableField " + result, this);
            return result;
        }        
    }
}
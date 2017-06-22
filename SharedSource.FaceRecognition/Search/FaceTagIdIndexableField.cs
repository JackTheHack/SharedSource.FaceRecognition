using System.Linq;
using Newtonsoft.Json;
using Sitecore.ContentSearch.ComputedFields;

namespace SharedSource.FaceRecognition.Search
{
    public class FaceTagIdIndexableField : BaseFaceIndexableField, IComputedIndexField
    {
        protected override object GetFieldValue(FaceIndexableItem cognitiveIndexable)
        {
            return cognitiveIndexable.Faces != null? cognitiveIndexable.Faces.Select(i => i.UniqueId).ToList() : null;
        }        
    }
}
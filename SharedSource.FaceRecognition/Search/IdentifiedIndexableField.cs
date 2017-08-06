using System.Linq;
using Newtonsoft.Json;
using SharedSource.FaceRecognition.Services;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Search
{
    public class IdentifiedIndexableField : BaseFaceIndexableField, IComputedIndexField
    {
        public IdentifiedIndexableField()
        {
        }

        protected override object GetFieldValue(FaceIndexableItem cognitiveIndexable)
        {
            var result = JsonConvert.SerializeObject(cognitiveIndexable.Suggestions);
            Log.Info("IndentifiedINdexableField " + result, this);
            return result;
        }        
    }
}
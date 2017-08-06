using System.Linq;
using Newtonsoft.Json;
using SharedSource.FaceRecognition.Models;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Search
{
    public class PersonsIndexableField : BaseFaceIndexableField, IComputedIndexField
    {
        protected override object GetFieldValue(FaceIndexableItem cognitiveIndexable)
        {
            var result = JsonConvert.SerializeObject(cognitiveIndexable.IdentifiedPersons);
            Log.Info("PersonsIndexableField "+result, this);
            return result;
        }
    }
}
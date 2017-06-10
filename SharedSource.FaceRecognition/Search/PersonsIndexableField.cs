using System.Linq;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Search
{
    public class PersonsIndexableField : BaseFaceIndexableField, IComputedIndexField
    {
        protected override object GetFieldValue(FaceIndexableItem cognitiveIndexable)
        {
            Log.Info("PersonsIndexableField", this);
            return cognitiveIndexable.Faces.Select(i => i.IdentifiedPerson).ToArray();
        }
    }
}
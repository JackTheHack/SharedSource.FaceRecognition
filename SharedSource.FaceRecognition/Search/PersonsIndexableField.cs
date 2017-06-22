﻿using System.Linq;
using Newtonsoft.Json;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Search
{
    public class PersonsIndexableField : BaseFaceIndexableField, IComputedIndexField
    {
        protected override object GetFieldValue(FaceIndexableItem cognitiveIndexable)
        {
            return JsonConvert.SerializeObject(cognitiveIndexable.IdentifiedPersons);
        }
    }
}
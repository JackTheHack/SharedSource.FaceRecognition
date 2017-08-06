using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Converters;
using Sitecore.ContentSearch.SearchTypes;

namespace SharedSource.FaceRecognition.Models
{
    public class FaceSearchResultItem : SearchResultItem
    {
        [IndexField("faces")]
        public string Faces { get; set; }
        [IndexField("persons")]
        public string Persons { get; set; }
        [IndexField("identified")]
        public string Identified { get; set; }
        [IndexField("facetagid")]
        public Guid[] FaceTags { get; set; }
        [IndexField("width")]
        public string Width { get; set; }
        [IndexField("height")]
        public string Height { get; set; }

    }
}
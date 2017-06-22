using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;

namespace SharedSource.FaceRecognition.Models
{
    public class PersonResultItem : SearchResultItem
    {
        [IndexField("title")]
        public string Name { get; set; }

        public string Surname { get; set; }
                
    }
}
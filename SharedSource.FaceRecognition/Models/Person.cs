using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace SharedSource.FaceRecognition.Models
{
    public class Person
    {
        public Person()
        {
            
        }

        public Person(PersonResultItem personResultItem)
        {
            Name = personResultItem.Name;
            Surname = personResultItem.Surname;
            ID = personResultItem.ItemId;
        }

        public Person(Item personResultItem)
        {
            Name = personResultItem["Title"];
            Surname = personResultItem["Surname"];
            AzureId = personResultItem["Id"];
            AzurePersonGroupId = personResultItem.Parent["GroupId"];
            AzurePersonGroupName = personResultItem.Parent["GroupName"];
            ID = personResultItem.ID;
        }

        public string AzurePersonGroupId { get; set; }

        public string AzurePersonGroupName { get; set; }

        public string AzureId { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public ID ID { get; set; }
    }
}
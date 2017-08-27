using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharedSource.FaceRecognition.Models
{
    public class TrainResult
    {
        public string Status { get; set; }
        public DateTime LastAction { get; set; }
        public string Message { get; set; }
        public bool IsTrained { get; set; }
    }

    public class ExtendedTrainResult : TrainResult
    {
        public List<PersonData> Persons { get; set; }
        public PersonGroupData PersonGroup { get; set; }
    }

    public class PersonData
    {
        public string Name { get; set; }
        public int FaceCount { get; set; }
        public Guid Id { get; set; }
    }

    public class PersonGroupData
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
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
}
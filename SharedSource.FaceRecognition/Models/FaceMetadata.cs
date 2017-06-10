using System.Drawing;

namespace SharedSource.FaceRecognition.Models
{
    public class FaceMetadata
    {
        public Rectangle FacePosition { get; set; }
        public string IdentifiedPerson { get; set; }
    }
}
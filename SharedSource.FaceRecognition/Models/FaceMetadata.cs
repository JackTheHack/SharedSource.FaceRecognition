using System;
using System.Drawing;

namespace SharedSource.FaceRecognition.Models
{
    public class FaceMetadata
    {
        public Guid UniqueId { get; set; }
        public Rectangle FacePosition { get; set; }
        public IdentifiedPerson[] Suggestions { get; set; }
        public IdentifiedPerson[] IdentifiedPersons { get; set; }
    }

    public class IdentifiedPerson
    {
        public Guid Id { get; set; }
        public double Confidence { get; set; }
        public Person Data { get; set; }
    }
}
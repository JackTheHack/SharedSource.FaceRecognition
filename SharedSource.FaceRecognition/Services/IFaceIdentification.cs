using System.Collections.Generic;
using System.IO;
using SharedSource.FaceRecognition.Models;

namespace SharedSource.FaceRecognition.Services
{
    public interface IFaceIdentification
    {
        List<IdentifiedPerson> Identify(List<FaceMetadata> faces, Stream stream);
    }
}
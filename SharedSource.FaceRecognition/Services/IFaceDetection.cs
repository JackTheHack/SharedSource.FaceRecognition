using System.Collections.Generic;
using System.IO;
using SharedSource.FaceRecognition.Models;

namespace SharedSource.FaceRecognition.Services
{
    public interface IFaceDetection
    {
        List<FaceMetadata> DetectFaces(Stream stream);
    }
}
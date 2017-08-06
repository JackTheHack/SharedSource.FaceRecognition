using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SharedSource.FaceRecognition.Models;

namespace SharedSource.FaceRecognition.Services
{
    public interface IFaceDetection
    {
        List<FaceMetadata> DetectFaces(Stream stream);
        Task<List<FaceMetadata>> DetectFacesAsync(Stream stream);
    }
}
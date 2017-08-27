using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.ProjectOxford.Face;
using SharedSource.FaceRecognition.Models;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Services
{
    public class AzureFaceDetection : IFaceDetection
    {
        private readonly IFaceService _faceService;

        public AzureFaceDetection()
        {
            _faceService = new FaceService();
        }

        public List<FaceMetadata> DetectFaces(Stream stream)
        {
            if (stream != null)
            {
                return Task.Run(() => DetectFacesAsync(stream)).Result;
            }
            Log.Info("Azure DetectFaces - stream is null", this);

            return null;
        }

        public async Task<List<FaceMetadata>> DetectFacesAsync(Stream stream)
        {
            var faceServiceClient = _faceService.CreateAzureClient();

            stream.Seek(0, SeekOrigin.Begin);

            var faces = await faceServiceClient.DetectAsync(stream, true, true);

            return faces.Select(i => new FaceMetadata()
                {
                    UniqueId = i.FaceId,
                    FacePosition =
                        new Rectangle(i.FaceRectangle.Left, i.FaceRectangle.Top, i.FaceRectangle.Width,
                            i.FaceRectangle.Height),
                    CreatedDate = DateTime.Now
            }).ToList();
        }
    }
}
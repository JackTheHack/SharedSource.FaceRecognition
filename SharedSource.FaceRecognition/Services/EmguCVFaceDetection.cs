using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Emgu.CV;
using Emgu.CV.Structure;
using SharedSource.FaceRecognition.Models;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Services
{
    public class EmguCvFaceDetection : IFaceDetection
    {
        public List<FaceMetadata> DetectFaces(Stream stream)
        {
            if (stream == null)
            {
                Log.Info("EmguCv DetectFaces - stream is null", this);
                return null;
            }

            using (var image = Image.FromStream(stream))
            using (var bitmap = new Bitmap(image))
            using (Image<Gray, Byte> imageData = new Image<Gray, Byte>(bitmap))
            {
                var stopwatch = Stopwatch.StartNew();

                var faces = new List<Rectangle>();

                var faceFileName =
                    HostingEnvironment.MapPath("~/App_Data/haarcascade_frontalface_default.xml");

                //Read the HaarCascade objects
                using (CascadeClassifier face = new CascadeClassifier(faceFileName))
                {
                   
                    //normalizes brightness and increases contrast of the image
                    CvInvoke.EqualizeHist(imageData, imageData);

                    //Detect the faces  from the gray scale image and store the locations as rectangle
                    //The first dimensional is the channel
                    //The second dimension is the index of the rectangle in the specific channel                     
                    Rectangle[] facesDetected = face.DetectMultiScale(
                        imageData,
                        1.1,
                        3,
                        new Size(0, 0));

                    faces.AddRange(facesDetected);
                    //}
                    stopwatch.Stop();
                }

                var result = faces.Select(i => new FaceMetadata
                {
                    UniqueId = Guid.NewGuid(),
                    FacePosition = i,
                    //Suggestions = null
                }).ToList();
                return result;
            }
        }

        public Task<List<FaceMetadata>> DetectFacesAsync(Stream stream)
        {
            return Task.Run(() => DetectFaces(stream));
        }
    }
}
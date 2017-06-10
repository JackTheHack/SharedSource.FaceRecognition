using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Web.Hosting;
using Emgu.CV;
using Emgu.CV.Structure;
using SharedSource.FaceRecognition.Models;
using Sitecore.Data.Items;

namespace SharedSource.FaceRecognition.Services
{
    public class FaceRecognitionLogic
    {
        public FaceMetadata[] DetectFaces(MediaItem item)
        {
            using (var imageStream = item.GetMediaStream())
            {

                var watch = Stopwatch.StartNew();

                var faces = new List<Rectangle>();

                using (var image = Image.FromStream(imageStream))

                using (var bitmap = new Bitmap(image))

                using (Image<Gray, Byte> imageData = new Image<Gray, Byte>(bitmap))
                {

                    var faceFileName =
                        HostingEnvironment.MapPath("~/App_Data/haarcascade_frontalface_default.xml");

                    //Read the HaarCascade objects
                    using (CascadeClassifier face = new CascadeClassifier(faceFileName))
                    {
                        //using (UMat ugray = new UMat())
                        //{
                            //CvInvoke.CvtColor(imageData, ugray, Emgu.CV.CvEnum.ColorConversion.Rgba2Gray);

                            //normalizes brightness and increases contrast of the image
                            CvInvoke.EqualizeHist(imageData, imageData);

                            //Detect the faces  from the gray scale image and store the locations as rectangle
                            //The first dimensional is the channel
                            //The second dimension is the index of the rectangle in the specific channel                     
                            Rectangle[] facesDetected = face.DetectMultiScale(
                                imageData,
                                1.1,
                                10,
                                new Size(20, 20));

                            faces.AddRange(facesDetected);
                        //}
                        watch.Stop();
                    }

                    return faces.Select(i => new FaceMetadata() {FacePosition = i, IdentifiedPerson = null}).ToArray();
                }
            }
        }
    }
}
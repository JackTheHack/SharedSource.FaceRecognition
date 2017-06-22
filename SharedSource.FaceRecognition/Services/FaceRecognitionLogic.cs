using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.ProjectOxford.Face;
using Newtonsoft.Json;
using SharedSource.FaceRecognition.Models;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Services
{
    public class FaceRecognitionLogic
    {
        public async Task<FaceMetadata[]> DetectFaces(MediaItem item)
        {

            var faceService = new FaceService();
            
            var result = !string.IsNullOrEmpty(item.InnerItem["Keywords"])
                        ? JsonConvert.DeserializeObject<FaceMetadata[]>(item.InnerItem["Keywords"])
                        : new FaceMetadata[0];

            await IdentifyFaces(result, faceService, item.GetMediaStream());

            return result.ToArray();
        }

       

        private async Task<bool> IdentifyFaces(FaceMetadata[] result, FaceService faceService, Stream imageStream)
        {
            var returnValue = true;

            var personRoot = faceService.GetPersonGroup();

            var persons = faceService.GetAllPersons();

            foreach(var face in result)
            {
                try
                {
                    using (
                        var faceServiceClient = new FaceServiceClient(
                            Settings.GetSetting("Cognitive.Key1"),
                            "https://eastus2.api.cognitive.microsoft.com/face/v1.0"))
                    {
                        using (var croppedImage = new MemoryStream())
                        {
                            faceService.CropImage(face.FacePosition, imageStream, croppedImage);

                            croppedImage.Seek(0, SeekOrigin.Begin);

                            var detectResult = await faceServiceClient.DetectAsync(croppedImage);

                            var faces = (detectResult.Select(i => i.FaceId));
                            
                            var detectResult2 =
                                await
                                    faceServiceClient.IdentifyAsync(personRoot.PersonGroupId.ToLowerInvariant(),
                                        faces.ToArray(), 0.8f, 3);

                            var identifiedPerson = detectResult2.FirstOrDefault();

                            if (identifiedPerson != null)
                            {
                                face.Suggestions = identifiedPerson.Candidates.Select(i => new IdentifiedPerson()
                                {
                                    Id = i.PersonId,
                                    Confidence = i.Confidence,
                                    Data =
                                        persons.FirstOrDefault(
                                            f => !string.IsNullOrEmpty(f.AzureId) && Guid.Parse(f.AzureId) == i.PersonId)
                                }).ToArray();
                            }
                        }
                    }
                }
                catch (FaceAPIException faceEx)
                {
                    Log.Info("Face exception " + faceEx.ErrorMessage, this);
                    returnValue = false;
                }
                catch (Exception e)
                {
                    Log.Info("Exception during detection + " + e.ToString(), this);
                    returnValue = false;
                }
            }

            return returnValue;

        }

        public List<FaceMetadata> DetectFaces(Image<Gray, byte> imageData)
        {
            var stopwatch = Stopwatch.StartNew();

            var faces = new List<Rectangle>();

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
                Suggestions = null
            }).ToList();
            return result;
        }
    }

    
}
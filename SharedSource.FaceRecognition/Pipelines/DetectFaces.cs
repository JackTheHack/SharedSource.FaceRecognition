using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using SharedSource.FaceRecognition.Models;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Pipelines
{
    public class DetectFaces
    {
        public void Process(DetectFacesArgs args)
        {
            if (args.MediaItem == null)
            {
                return;
            }

            try
            {

                Log.Info("Image recognition starting...", this);

                var faceRecognitionlogic = new FaceRecognition.Services.FaceRecognitionLogic();

                var mediaStream = args.MediaItem.GetMediaStream();

                if (mediaStream == null)
                {
                    Log.Info("Weird....  media item media stream is empty...", this);
                    return;
                }

                using (var image = Image.FromStream(mediaStream))
                using (var bitmap = new Bitmap(image))
                using (Image<Gray, Byte> imageData = new Image<Gray, Byte>(bitmap))
                {
                    var result = faceRecognitionlogic.DetectFaces(imageData);

                    args.MediaItem.BeginEdit();
                    args.MediaItem.InnerItem["Keywords"] = JsonConvert.SerializeObject(result);
                    args.MediaItem.EndEdit();
                }

                Log.Info("Image recognition completed...", this);
            }
            catch (Exception e)
            {
                Log.Info("Something went wrong..." + e.ToString(), this);
            }
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedSource.FaceRecognition.Models;
using SharedSource.FaceRecognition.Services;
using Sitecore.ContentSearch;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.SecurityModel;

namespace SharedSource.FaceRecognition.Pipelines
{
    public class DetectFaces
    {
        public void Process(DetectFacesArgs args)
        {
            Task.Run(() => RunProcess(args));
        }

        private async void RunProcess(DetectFacesArgs args)
        {
            if (args.MediaItem == null)
            {
                return;
            }

            try
            {
                
                    Log.Info("Image recognition starting...", this);

                    IFaceDetection faceDetection = new AzureFaceDetection();

                    var mediaStream = args.Stream ?? args.MediaItem.GetMediaStream();

                    if (mediaStream == null)
                    {
                        Log.Info("Weird....  media item media stream is empty...", this);
                        return;
                    }

                    var memoryStream = new MemoryStream();

                    await mediaStream.CopyToAsync(memoryStream);

                    var result = await faceDetection.DetectFacesAsync(memoryStream);

                using (new SecurityDisabler())
                {
                    args.MediaItem.InnerItem.Editing.BeginEdit();
                    args.MediaItem.InnerItem["FaceMetadata"] = JsonConvert.SerializeObject(result);
                    args.MediaItem.InnerItem.Editing.EndEdit(true, false);
                }

                PublishManager.PublishItem(args.MediaItem, new[] { Database.GetDatabase("web"), },
                                new[] { Language.Current }, false, false);

                var index = ContentSearchManager.GetIndex("face_master_index");

                index.Refresh(new SitecoreIndexableItem(args.MediaItem), IndexingOptions.Default);

                index = ContentSearchManager.GetIndex("face_web_index");

                index.Refresh(new SitecoreIndexableItem(args.MediaItem), IndexingOptions.Default);

                Log.Info("Image recognition completed...", this);
            }
            catch (Exception e)
            {
                Log.Info("Something went wrong..." + e, this);
            }
        }
    }
}
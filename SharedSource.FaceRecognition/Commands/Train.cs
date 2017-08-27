using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SharedSource.FaceRecognition.Models;
using SharedSource.FaceRecognition.Services;
using Sitecore;
using Sitecore.Shell.Framework.Commands;

namespace SharedSource.FaceRecognition.Commands
{
    public class Train : Command
    {
        public override void Execute(CommandContext context)
        {
            IFaceService faceService = new FaceService();
            Task.Run(() => faceService.TrainAsync(true));

            TrainResult result = null;

            var task = Task.Run(async() => result = await faceService.TrainAsync(true));

            task.Wait(2000);            

            if (result != null)
            {
                Context.ClientPage.ClientResponse.Alert(result.Status + " " + result.Message + " " + result.LastAction + (result.IsTrained ? " (Trained)" : " (Not Trained)"));
            }
            else
            {
                Context.ClientPage.ClientResponse.Alert("Timeout: Something bad happened during training.");
            }

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SharedSource.FaceRecognition.Services;
using Sitecore.Shell.Framework.Commands;

namespace SharedSource.FaceRecognition.Commands
{
    public class Train : Command
    {
        public override void Execute(CommandContext context)
        {
            var faceService = new FaceService();
            Task.Run(() => faceService.Train(true));
        }
    }
}
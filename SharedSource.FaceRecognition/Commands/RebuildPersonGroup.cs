using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SharedSource.FaceRecognition.Services;
using Sitecore.Shell.Framework.Commands;

namespace SharedSource.FaceRecognition.Commands
{
    public class RebuildPersonGroup : Command
    {
        public override void Execute(CommandContext context)
        {
            var faceService = new FaceService();
            faceService.RebuildPersonGroup();
        }
    }
}
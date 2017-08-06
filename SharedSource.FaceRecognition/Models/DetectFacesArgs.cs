using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace SharedSource.FaceRecognition.Models
{
    public class DetectFacesArgs : PipelineArgs
    {
        public DetectFacesArgs()
        {
            
        }

        public DetectFacesArgs(Item item)
        {
            MediaItem = new MediaItem(item);
        }

        public MediaItem MediaItem { get; set; }
        public Stream Stream { get; set; }
    }
}
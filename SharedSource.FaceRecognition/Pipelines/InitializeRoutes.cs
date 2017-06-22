using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Sitecore.Pipelines;

namespace SharedSource.FaceRecognition.Pipelines
{
    public class RegisterHttpRoutes
    {
        public void Process(PipelineArgs args)
        {
            GlobalConfiguration.Configure(Configure);
        }
        protected void Configure(HttpConfiguration configuration)
        {
            var routes = configuration.Routes;
            routes.MapHttpRoute("FaceApi", "sitecore/api/faceapi/identifytag/{tagId}/{personId}", new
            {
                controller = "FaceApi",
                action = "IdentifyTag"
            });
            routes.MapHttpRoute("Test", "sitecore/api/faceapi/test", new
            {
                controller = "FaceApi",
                action = "Test"
            });
        }
    }
}
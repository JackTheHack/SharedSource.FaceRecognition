using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SharedSource.FaceRecognition.Services;
using Sitecore.Mvc.Controllers;
using Sitecore.Services.Infrastructure.Web.Http;

namespace SharedSource.FaceRecognition.Controllers
{
    public class FaceApiController : ServicesApiController
    {
        [System.Web.Http.HttpGet]
        [System.Web.Mvc.AcceptVerbs("GET")]
        public string IdentifyTag(Guid tagId, Guid personId)
        {
            IFaceService faceService = new FaceService();
            faceService.IdentifyTagAsync(tagId, personId);

            return "OK";
        }

        [System.Web.Http.HttpGet]
        [System.Web.Mvc.AcceptVerbs("GET")]
        public string Test()
        {
            return "Test";
        }
    }
}
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using SharedSource.FaceRecognition.Models;
using SharedSource.FaceRecognition.Services;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.Publishing;
using Sitecore.SecurityModel;

namespace SharedSource.FaceRecognition.Controllers
{
    public class FaceController : Controller
    {
        public ActionResult IdentifyTag()
        {
            IFaceService service = new FaceService();
            return View("~/Views/Shared/IdentifyTag.cshtml", service.GetAllPersons());
        }
        

        public ActionResult UploadImage()
        {
            bool isSavedSuccessfully = true;
            string fName = "";
            try
            {
                using (new SecurityDisabler())
                {
                    foreach (string fileName in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[fileName];
                        //Save file content goes here
                        if (file != null && file.ContentLength > 0)
                        {

                            var tempStream = new MemoryStream();

                            file.InputStream.CopyTo(tempStream);

                            tempStream.Seek(0, SeekOrigin.Begin);

                            fName = file.FileName;

                            var newFileName =
                                Guid.NewGuid()
                                    .ToString()
                                    .Replace("-", string.Empty)
                                    .Replace("{", string.Empty)
                                    .Replace("}", string.Empty);

                            var imagesRootId = ID.Parse(Settings.GetSetting("ImagesRoot"));
                            var imagesRoot = Sitecore.Context.Database.GetItem(imagesRootId);

                            // Create the options
                            Sitecore.Resources.Media.MediaCreatorOptions options =
                                new Sitecore.Resources.Media.MediaCreatorOptions();
                            // Store the file in the database, not as a file
                            options.FileBased = false;
                            // Remove file extension from item name
                            options.IncludeExtensionInItemName = false;
                            // Overwrite any existing file with the same name
                            options.KeepExisting = false;
                            // Do not make a versioned template
                            options.Versioned = false;
                            options.IncludeExtensionInItemName = false;
                            // set the path
                            options.Destination = imagesRoot.Paths.FullPath + "/" + newFileName;
                            // Set the database
                            options.Database = Sitecore.Configuration.Factory.GetDatabase("master");                            

                                // Now create the file
                                Sitecore.Resources.Media.MediaCreator creator =
                                    new Sitecore.Resources.Media.MediaCreator();

                                MediaItem mediaItem = creator.CreateFromStream(tempStream,
                                    newFileName + Path.GetExtension(file.FileName), options);

                            PublishManager.PublishItem(mediaItem, new[] { Database.GetDatabase("web"), },
                                new[] { Language.Current }, false, false);
            

                            CorePipeline.Run("detectFaces", new DetectFacesArgs(mediaItem.InnerItem));


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isSavedSuccessfully = false;
            }

            if (isSavedSuccessfully)
            {
                return Json(new { Message = fName });
            }

            return Json(new { Message = "Error in saving file" });
        }
    }
}
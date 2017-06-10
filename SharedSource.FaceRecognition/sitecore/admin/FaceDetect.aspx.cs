using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using SharedSource.FaceRecognition;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;

namespace SharedSource.EngagementPlanViewer.sitecore.admin
{
    public partial class FaceDetect : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnDetectFaces_OnClick(object sender, EventArgs e)
        {
            var folder =
                Database.GetDatabase("master").GetItem(Sitecore.Data.ID.Parse("{61BBE5BB-9AE9-426F-A717-1B4ADF1C5798}"));

            var faceRecognition = new FaceRecognitionLogic();

            foreach (Item img in folder.Children)
            {
                var mediaItem = new MediaItem(img);
                var faces = faceRecognition.DetectFaces(mediaItem);
                img.Editing.BeginEdit();
                using (new EditContext(mediaItem, SecurityCheck.Disable))
                {
                    var faceMetadata = img.Fields["FaceMetadata"];
                    if (faceMetadata != null)
                        faceMetadata.Value = JsonConvert.SerializeObject(faces);
                    img.Editing.EndEdit(true);
                }
            }
        }
    }
}
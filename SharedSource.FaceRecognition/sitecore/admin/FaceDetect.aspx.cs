using System;
using System.Threading.Tasks;
using SharedSource.FaceRecognition.Services;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.sitecore.admin
{
    public partial class FaceDetect : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnDetectFaces_OnClick(object sender, EventArgs e)
        {
            var faceService = new FaceService();
            Task.Run(() =>
            {
                Log.Info("Training face recognition before indexing...", this);

                var result = faceService.Train(chkForce.Checked).Result;

                if (result != null)
                {
                    lblResult.Text = result.Status + " "+ result.Message + " " + result.LastAction + " " + result.IsTrained;
                    txtLog.Text = faceService.StringLog.ToString();
                }
                else
                {
                    lblResult.Text = "Something happened...";
                }

            }).Wait(TimeSpan.FromSeconds(5));
        }


        protected void btnTrainingStatus_OnClick(object sender, EventArgs e)
        {
            var faceService = new FaceService();
            Task.Run(() =>
            {
                Log.Info("Training face recognition before indexing...", this);

                var result = faceService.GetTrainingStatus().Result;

                if (result != null)
                {
                    lblResult.Text = result.Status + " " + result.Message + " " + result.LastAction + " " + result.IsTrained;
                    txtLog.Text = faceService.StringLog.ToString();
                }
                else
                {
                    lblResult.Text = "Something happened...";
                }

            }).Wait(TimeSpan.FromSeconds(5));
        }
    }
}
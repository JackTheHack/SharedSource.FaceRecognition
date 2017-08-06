using System.Threading.Tasks;
using SharedSource.FaceRecognition.Services;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace SharedSource.FaceRecognition.Commands
{
    public class GetTrainStatus : Command
    {
        public override void Execute(CommandContext context)
        {
            var faceService = new FaceService();
            Task.Run(async () =>
            {
                var result = await faceService.GetTrainingStatus();
                SheerResponse.Alert(
                    result.Status + " " + result.Message + " " + result.LastAction + " " + result.IsTrained, false);
            });
        }
    }
}
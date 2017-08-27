using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using SharedSource.FaceRecognition.Models;
using SharedSource.FaceRecognition.Services;
using Sitecore;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace SharedSource.FaceRecognition.Commands
{
    public class GetTrainStatus : Command
    {
        public override void Execute(CommandContext context)
        {
            IFaceService faceService = new FaceService();

            ExtendedTrainResult result = null;

            var task = Task.Run(async () => { result = await faceService.GetTrainingStatusAsync(); });

            task.Wait(2000);

            if (result != null)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("# Training Info #\n\n");
                sb.AppendFormat("Is Trained: {0}\n", result.IsTrained);
                sb.AppendFormat("Training Status: {0} \n", result.Status);
                sb.AppendFormat("Last Training: {0}\n\n\n", result.LastAction);

                if (result.IsTrained && result.PersonGroup != null && result.Persons != null)
                {
                    sb.AppendFormat("# Persongroup info #\n");
                    sb.AppendFormat("{0} {1}\n\n", result.PersonGroup.Name, result.PersonGroup.Id);

                    foreach (var person in result.Persons)
                    {
                        sb.AppendFormat("{0} - {1} faces\n", person.Name, person.FaceCount);
                    }
                }

                Context.ClientPage.ClientResponse.Alert(sb.ToString());
            } else {
                Context.ClientPage.ClientResponse.Alert("Timeout: Cannot get the training status from Azure.");
            }
        }
    }
}
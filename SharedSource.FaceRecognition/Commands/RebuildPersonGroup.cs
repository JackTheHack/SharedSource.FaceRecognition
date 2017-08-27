using System.Threading.Tasks;
using SharedSource.FaceRecognition.Services;
using Sitecore;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace SharedSource.FaceRecognition.Commands
{
    public class RebuildPersonGroup : Command
    {
        public override void Execute(CommandContext context)
        {
            Sitecore.Context.ClientPage.Start(this, "Confirm", context.Parameters);
        }

        protected void Confirm(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (args.Result == "yes")
                {
                    RunCommand();
                }
            }
            else
            {
                SheerResponse.Confirm("!!!Warning!!!\nAll your face data will be lost! Are you sure? ");
                args.WaitForPostBack();
            }
        }

        private void RunCommand()
        {
            IFaceService faceService = new FaceService();
            bool result = false;

            var task = Task.Run(async () => result = await faceService.RebuildPersonGroup());

            task.Wait(2000);

            if (result)
            {
                Context.ClientPage.ClientResponse.Alert(result ? "Successfully rebuilt the definitions" : "Sorry, something bad happened.");
            }
        }


        
    }
}
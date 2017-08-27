using System;
using System.Threading.Tasks;
using SharedSource.FaceRecognition.Services;
using Sitecore.Data.Items;
using Sitecore.Events;

namespace SharedSource.FaceRecognition.Pipelines
{
    public class ItemPersonSaved
    {
        public void ItemSaved(object sender, EventArgs e)
        {
            var item = Event.ExtractParameter(e, 0) as Item;

            if (item == null)
            {
                return;
            }

            Task.Run( () => CreatePersonDefinition(item));
        }

        private void CreatePersonDefinition(Item item)
        {
            IFaceService faceService = new FaceService();
            faceService.CreatePersonByItemAsync(item);
        }
    }
}
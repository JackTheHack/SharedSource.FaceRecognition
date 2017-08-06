using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SharedSource.FaceRecognition.Constants;
using SharedSource.FaceRecognition.Models;
using SharedSource.FaceRecognition.Services;
using Sitecore.ContentSearch;
using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Pipelines;

namespace SharedSource.FaceRecognition.Pipelines
{
    public class ItemPersonSaved
    {
        public void Process(object sender, EventArgs e)
        {
            var item = Event.ExtractParameter(e, 0) as Item;

            if (item == null)
            {
                return;
            }

            var faceService = new FaceService();

            Task.Run(async () =>
            {
                try
                {
                    //Person template
                    if (item.TemplateID == ID.Parse("{1EB19995-B7D6-4AAC-ACF7-6411264DCE24}"))
                    {
                        var parentGroup = item.Parent;

                        if (string.IsNullOrEmpty(item["Id"]) && !string.IsNullOrEmpty(item["Title"]))
                        {
                            var person = await faceService.CreatePerson(parentGroup["GroupId"], item["Title"]);

                            item.Editing.BeginEdit();
                            item["Id"] = person.ToString();
                            item.Editing.EndEdit(true, true);
                        }
                    }

                    //Person group template
                    if (item.TemplateID == ID.Parse("{157A7C04-DC65-4B1B-BD43-A4071C2B8604}"))
                    {
                        if (!string.IsNullOrEmpty(item["GroupName"]) && !string.IsNullOrEmpty("GroupId"))
                        {
                            faceService.CreatePersonGroupIfNotExists(item["GroupId"], item["GroupName"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Info("Error during creating entity in Azure - " + ex.ToString(), this);
                }
            });
            
        }
    }
}
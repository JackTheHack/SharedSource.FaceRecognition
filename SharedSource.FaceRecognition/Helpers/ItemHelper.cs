using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SharedSource.FaceRecognition.Models;
using Sitecore.ContentSearch;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace SharedSource.FaceRecognition.Helpers
{
    public static class ItemHelper
    {
        public static string GetImageFieldChromeData(Item item, string fieldName)
        {
                var imageField = (ImageField) item.Fields[fieldName];

                if (imageField != null && imageField.MediaItem != null)
                {
                    return GetImageChromeData(imageField.MediaItem);
                }

            return string.Empty;
        }

        public static string GetImageChromeData(MediaItem item)
        {
            var indexName = Sitecore.Context.Database.Name == "master" ? "face_master_index" : "face_web_index";

            var index = ContentSearchManager.GetIndex(indexName);

            using (var context = index.CreateSearchContext())
            {
                if (item != null)
                {

                    var searchQuery = context.GetQueryable<FaceSearchResultItem>()
                        .Where(x => x.ItemId == item.ID)
                        .ToList()
                        .Where(x => x.Faces != null);

                    var resultItem = searchQuery.FirstOrDefault();

                    if (resultItem != null)
                        return HttpUtility.UrlEncode(resultItem.Faces);
                }
            }

            return string.Empty;
        }
    }
}
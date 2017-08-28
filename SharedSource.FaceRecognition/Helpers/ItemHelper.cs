using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SharedSource.FaceRecognition.Models;
using Sitecore.ContentSearch;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;

namespace SharedSource.FaceRecognition.Helpers
{
    public static class ItemHelper
    {
        public static string GetImageFieldSizeChromeData(Item item, string fieldName)
        {
            var imageField = (ImageField)item.Fields[fieldName];

            if (imageField != null && imageField.MediaItem != null)
            {
                return $"{imageField.Width},{imageField.Height}";
            }

            return string.Empty;
        }

        public static string GetImageFieldChromeData(Item item, string fieldName)
        {
                var imageField = (ImageField) item.Fields[fieldName];

                if (imageField != null && imageField.MediaItem != null)
                {
                    return GetImageChromeData(imageField.MediaItem);
                }

            return string.Empty;
        }

        public static HtmlString RenderFaceImage(this HtmlHelper helper, MediaItem image)
        {
            if (image == null)
            {
                return MvcHtmlString.Empty;
            }

            var imageUrl = MediaManager.GetMediaUrl(image);

            var face = FindFace(image);

            if (face == null)
            {
                return new HtmlString($"<img src='{imageUrl}'/>");
            }

            var faceData = HttpUtility.UrlEncode(face.Faces);

            var identifiedPersons = HttpUtility.UrlEncode(image.InnerItem["IdentifiedPersons"]);

            var suggestions = HttpUtility.UrlEncode(face.Identified);

            var imgSize = $"{image.InnerItem["Width"]},{image.InnerItem["Height"]}";

            return new HtmlString($"<img src='{imageUrl}' data-suggestions='{suggestions}'" +
                                  $" data-identified='{identifiedPersons}' data-facetags='{faceData}' " +
                                  $"data-imgsize='{imgSize}' showfacetags='True'/>");
        }

        public static FaceSearchResultItem FindFace(MediaItem item)
        {
            try
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

                        return resultItem;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetImageChromeData(MediaItem item)
        {
            var face = FindFace(item);
                        if (face != null)
                            return HttpUtility.UrlEncode(face.Faces);

                return string.Empty;
        }
    }
}
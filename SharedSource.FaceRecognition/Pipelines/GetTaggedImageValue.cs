using System.Linq;
using System.Web;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Fields;
using Sitecore.Pipelines.RenderField;
using Sitecore.Xml.Xsl;

namespace SharedSource.FaceRecognition.Pipelines
{
    public class GetTaggedImageFieldValue : GetImageFieldValue
    {
        protected override ImageRenderer CreateRenderer()
        {
            return new TaggedImageRenderer();
        }
    }

    public class TaggedImageRenderer : ImageRenderer
    {
        public override RenderFieldResult Render()
        {
            if (Parameters.ContainsKey("showfacetags") && Parameters["showfacetags"] == "True")
            {
                var indexName = Sitecore.Context.Database.Name == "master" ? "face_master_index" : "face_web_index";

                var index = ContentSearchManager.GetIndex(indexName);

                using (var context = index.CreateSearchContext())
                {
                    var imageField = (ImageField) Item.Fields[FieldName];

                    if (imageField != null && imageField.MediaItem != null)
                    {

                        var searchQuery = context.GetQueryable<FaceSearchResultItem>()
                            .Where(x => x.ItemId == imageField.MediaID);

                        var resultItem = searchQuery.FirstOrDefault();

                        if (!string.IsNullOrEmpty(resultItem?.Faces))
                        {
                            Parameters.Add("data-facetags", HttpUtility.HtmlEncode(resultItem.Faces));
                        }
                    }
                }
            }

            return base.Render();
        }
    }

    public class FaceSearchResultItem : SearchResultItem
    {
        [IndexField("faces")]
        public string Faces { get; set; }
        [IndexField("persons")]
        public string[] Persons { get; set; }

    }
}
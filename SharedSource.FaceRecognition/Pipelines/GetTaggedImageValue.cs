using SharedSource.FaceRecognition.Helpers;
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
                string chromeData = ItemHelper.GetImageFieldChromeData(Item, FieldName);

                if (Parameters.ContainsKey("data-facetags"))
                {
                    Parameters.Remove("data-facetags");
                }

                if (!string.IsNullOrEmpty(chromeData))
                {
                    Parameters.Add("data-facetags", chromeData);
                }
            }

            return base.Render();
        }
    }

    
}
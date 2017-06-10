using System;
using Sitecore;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace SharedSource.FaceRecognition.Search
{
    public class BaseFaceIndexableField : AbstractComputedIndexField
    {
        public override object ComputeFieldValue(IIndexable indexable)
        {
            Log.Info("Running computed index", this);
            Assert.ArgumentNotNull(indexable, "indexable");

            Item indexItem = indexable as SitecoreIndexableItem;
            if (indexItem == null)
                return null;

            if (indexItem?.Database?.Name == "core")
                return null;

            if (indexItem.ID == ItemIDs.MediaLibraryRoot)
                return null;

            try
            {
                FaceIndexableItem cognitiveIndexable;
                try
                {
                    cognitiveIndexable = (FaceIndexableItem)indexable;
                }
                catch (Exception ex)
                {
                    CrawlingLog.Log.Warn("Unable to convert indexable to CognitiveIndexableItem. Id: " + indexable.UniqueId + ", Message: " + ex.Message);
                    return null;
                }
                return GetFieldValue(cognitiveIndexable);
            }
            catch (Exception e)
            {
                Log.Error($"Error indexing field: {FieldName}, Item Path: {indexItem.Paths.FullPath}", e, GetType());
            }

            return null;
        }

        protected virtual object GetFieldValue(FaceIndexableItem cognitiveIndexable)
        {
            return null;
        }

    }
}
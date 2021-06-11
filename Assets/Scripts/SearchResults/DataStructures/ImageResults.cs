using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.SearchResults.DataStructures
{
    [Serializable]
    public class ImageAnswer
    {
        // Other potentially useful attributes of note below - 
        // { pivotSuggestions, queryExpansions, relatedSearches, similarTerms}
        public int nextOffset;
        public List<Image> value;
        public long totalEstimatedMatches;
        public Dictionary<string, string> relevantHeaders = new Dictionary<string, string>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Image img in value)
            {
                sb.AppendLine(img.ToString());
            }
            return sb.ToString();
        }
    }

    [Serializable]
    public class Image
    {
        // Other potentially useful attributes of note below - 
        // { accentColor, creativeCommons, encodingFormat }
        public string contentUrl;
        public ushort height;
        public ushort width;
        public string hostPageDisplayUrl;
        public string hostPageUrl;
        public string id;
        public string name;
        public MediaSize thumbnail;
        public string thumbnailUrl;


        public override string ToString()
        {
            return $"( \n\tid : {id} ,\n\tname : {name},\n\tpageUrl: {hostPageDisplayUrl},\n\tcontentUrl: {contentUrl} ,\n\tthumbUrl: {thumbnailUrl})";
        }
    }


    [Serializable]
    public class MediaSize
    {
        public int height;
        public int width;
    }

}
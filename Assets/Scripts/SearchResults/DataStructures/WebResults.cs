using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.SearchResults.DataStructures
{
    [Serializable]
    public class SearchResponse
    {
        public WebSearchResults webPages;
        public RankingResponse rankingResponse;
        public Dictionary<string, string> relevantHeaders = new Dictionary<string, string>();
        // Other potentially useful attributes of note below - 
        // Places (for map results), Computation, Translations, TimeZone

        public override string ToString()
        {
            return webPages.ToString();
        }
    }

    [Serializable]
    public class WebSearchResults
    {
        public List<WebSearchResult> value;
        public long totalEstimatedMatches;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (WebSearchResult result in value)
            {
                sb.AppendLine(result.ToString());
            }
            return sb.ToString();
        }
    }

    [Serializable]
    public class WebSearchResult
    {
        public string id;
        public string name;
        public string displayUrl;
        public string url;
        public string snippet;

        public override string ToString()
        {
            return $"( \n\tid : {id} ,\n\tname : {name},\n\turl: {url},\n\tsnippet : {snippet})";
        }
    }
}
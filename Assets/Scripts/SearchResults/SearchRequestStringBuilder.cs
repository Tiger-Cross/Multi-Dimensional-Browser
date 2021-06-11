using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.SearchResults
{
    // TODO: make an actual builder pattern
    public static class SearchRequestStringBuilder 
    {
        // TODO: refactor
        // Use this for initialization
        private static readonly string webUrlBase = "https://api.bing.microsoft.com/v7.0/search";
        private static readonly string imgUrlBase = "https://api.bing.microsoft.com/v7.0/images/search";

        // Search query is just the string being searched for. 
        // count is number of results per page
        // offset indicates which page to get results from. (multiple of count)
        public static string BuildWebQueryString(string searchQuery, int count, int offset)
        {
            return $"{webUrlBase}?q={Uri.EscapeDataString(searchQuery)}&responseFilter=webPages&count={count}&offset={offset}&mkt=en-gb";
        }

        public static string BuildImageQueryString(string searchQuery, int count, int offset)
        {
            return $"{imgUrlBase}?q={Uri.EscapeDataString(searchQuery)}&count={count}&offset={offset}&mkt=en-gb";
        }

        // TODO: Create "with" methods
        // NB: Use count and offset to set max number of returned results and paginate them
        // NB: Make calls to separate API endpoints for other answer types, e.g. images
    }
}
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Assets.Scripts.Input;
using Assets.Scripts.SearchResults.DataStructures;
using Assets.Scripts.Utilities;

namespace Assets.Scripts.SearchResults
{

    public class SearchController : MonoBehaviour
    {

        #region Properties

        // TODO: make this accessible to both this class and speech / keyboard input classes.
        private static readonly List<string> nonSearchStrings = new List<string> { "Press the speak button and say what you're looking for. \nThen press the search button!", "Waiting for mic permission", "startRecoButton property is null! Assign a UI Button to it.", "No result types set. Please open the preferences menu and set at least one type of result.", "Listening...", "Loading...", "No valid query was successfully transcribed. Press the speak button to try again.", "We couldn't detect what you were trying to say. Please ensure there is no background noise.", "We're sorry. Something went wrong.", string.Empty };

        private SpeechRecognizer speechRecognizer;

        public PressableButton searchButton;

        private string searchQuery;
        public TextMeshPro searchText;

        private int numCols;
        private static readonly int numRows = 3;
        private int resultsPerPage;
        private int assetsPerPage;
        private long totalEstimatedWebMatches;
        private long totalEstimatedImgMatches;
        private int currWebPage;
        private int currImgPage;
        private int currImgOffset;
        private int nextImgOffset;

        private GameObject webObjectsParent;
        private GameObject imgObjectsParent;
        private Camera cameraRef;

        #endregion

        #region MonoBehaviour methods

        private void Awake()
        {
            cameraRef = Camera.main;
        }

        void Start() 
        {
            searchButton.ButtonPressed.AddListener(ExecuteWebSearch);
            speechRecognizer = GameObject.Find("Input Manager").GetComponentInChildren<SpeechRecognizer>();
            SetNumresults();
        }

        void Update()
        {
            if (webObjectsParent != null)
            {
                Transform parentTf = webObjectsParent.transform;
                float parentHeight = parentTf.position.y;

                if (parentHeight < 0
                    && -parentHeight % 1.2f > 0f
                    && -parentHeight % 1.2f < 0.05f
                    && -parentHeight / 1.2f > currWebPage
                    && parentTf.childCount <= (currWebPage + 1) * assetsPerPage)
                {
                    LoadNextPageWebResults();
                }
            }
            if (imgObjectsParent != null)
            {
                Transform imgParentTf = imgObjectsParent.transform;
                float imgParentHeight = imgParentTf.position.y;

                if (imgParentHeight < 0
                    && -imgParentHeight % 1.2f > 0f
                    && -imgParentHeight % 1.2f < 0.05f
                    && -imgParentHeight / 1.2f > currImgPage
                    && imgParentTf.childCount <= (currImgPage + 1) * assetsPerPage)
                {
                    LoadNextPageImageResults();
                }
            }
        }

        #endregion

        #region Public Methods

        public void ExecuteWebSearch(string searchQuery)
        {
            speechRecognizer.SetMessage(searchQuery);
            searchText.text = searchQuery;
            ExecuteWebSearch();
        }

        public void ExecuteWebSearch()
        {
            if (searchText != null && speechRecognizer != null)
            {
                if (!nonSearchStrings.Contains(searchText.text))
                {
                    SetNumresults();
                    currWebPage = 0;
                    currImgOffset = 0;
                    currImgPage = 0;
                    searchQuery = searchText.text;
                    GetComponent<HistoryManager>().AddToHistory(searchQuery);
                    int webResultsEnabled = PlayerPrefs.GetInt("WebResultsEnabled");
                    int imageResultsEnabled = PlayerPrefs.GetInt("ImageResultsEnabled");
                    if (webResultsEnabled == 0 && imageResultsEnabled == 0)
                    {
                        speechRecognizer.SetMessage("No result types set. Please open the preferences menu and set at least one type of result.");
                    }
                    else
                    {
                        speechRecognizer.SetMessage("Loading...");
                        if (webResultsEnabled == 1)
                        {
                            Debug.Log("Executing web search");
                            StartCoroutine(MakeWebSearchRequest(currWebPage, CreateWebAssetsFromResults));
                        }
                        if (imageResultsEnabled == 1)
                        {
                            Debug.Log("Executing img search");
                            StartCoroutine(MakeImageSearchRequest(currImgOffset, CreateImageAssetsFromResults));
                        }
                    }
                }
                else
                {
                    // TODO: set input buttons enabled here.
                    speechRecognizer.SetMessage("No valid query was successfully transcribed. Press the speak button to try again.");
                }
            }
            else
            {
                Debug.LogError("SearchText and/or speech recognizer not assigned. Please assign gameobjects in Unity Editor.");
            }
        }

        #endregion

        #region Private Methods

        private void SetNumresults()
        {
            numCols = PlayerPrefs.GetInt("NumResultColumns");
            resultsPerPage = numCols * numRows;
            // 3 results is edge case since we don't create scrollbars for middle result.
            assetsPerPage = numCols == 3 ? (resultsPerPage + numRows * 2) : (resultsPerPage * 2);
        }

        private void CreateWebAssetsFromResults(SearchResponse results)
        {
            if (webObjectsParent == null)
            {
                webObjectsParent = CreateResultsParent("Web Objects Parent");
            }
            ResultsGenerator generator = GetComponent<ResultsGenerator>();
            generator.GenerateWebResults(results, webObjectsParent, currWebPage);
        }

        private IEnumerator MakeWebSearchRequest(int resultOffset, Action<SearchResponse> onSuccess)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(SearchRequestStringBuilder.BuildWebQueryString(searchQuery, resultsPerPage, resultOffset)))
            {
                if (string.IsNullOrEmpty(AppSecretKeys.SearchSubKey))
                {
                    Debug.LogError("Unable to get search subscription key from resource object. Ensure the envrionment variable is set and the object is saved.");
                }
                req.SetRequestHeader("Ocp-Apim-Subscription-Key", AppSecretKeys.SearchSubKey);
                // NB: Unity sets the user-agent header by default.
                // NB: May want to incorporate more info such as location and client ID though the headers:
                // X-MSEdge-ClientID, X-MSEdge-ClientIP, X-Search-Location

                yield return req.SendWebRequest();
                while (!req.isDone)
                    yield return null;

                byte[] result = req.downloadHandler.data;
                string resultJSON = System.Text.Encoding.Default.GetString(result);

                // Create a result object.
                SearchResponse webSearchResult = JsonUtility.FromJson<SearchResponse>(resultJSON);
                totalEstimatedWebMatches = webSearchResult.webPages.totalEstimatedMatches;

                Debug.Log(webSearchResult.rankingResponse.ToString());

                // Extract Bing HTTP headers.
                foreach (String header in req.GetResponseHeaders().Keys)
                {
                    if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                        webSearchResult.relevantHeaders[header] = req.GetResponseHeader(header);
                }
                
                if (speechRecognizer != null)
                {
                    speechRecognizer.SetMessage(string.Empty);
                }
                onSuccess(webSearchResult);
            }
        }

        private void LoadNextPageWebResults()
        {
            if (searchQuery != null)
            {
                // Increment page and make request for next page.
                currWebPage++;
                int offset = currWebPage * resultsPerPage;
                if (offset < totalEstimatedWebMatches - resultsPerPage)
                {
                    StartCoroutine(MakeWebSearchRequest(offset, CreateWebAssetsFromResults));
                }
            }
        }

        private void CreateImageAssetsFromResults(ImageAnswer results)
        {
            if (imgObjectsParent == null)
            {
                imgObjectsParent = CreateResultsParent("Image Objects Parent");
            }
            ResultsGenerator generator = GetComponent<ResultsGenerator>();
            generator.GenerateImageResults(results, imgObjectsParent, currImgPage);
        }

        // Image-related search function:
        private IEnumerator MakeImageSearchRequest(int resultOffset, Action<ImageAnswer> onSuccess)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(SearchRequestStringBuilder.BuildImageQueryString(searchQuery, resultsPerPage, resultOffset)))
            {
                if (string.IsNullOrEmpty(AppSecretKeys.SearchSubKey))
                {
                    Debug.LogError("Unable to get search subscription key from resource object. Ensure the envrionment variable is set and the object is saved.");
                }
                req.SetRequestHeader("Ocp-Apim-Subscription-Key", AppSecretKeys.SearchSubKey);
                // NB: Unity sets the user-agent header by default.
                // NB: May want to incorporate more info such as location and client ID though the headers:
                // X-MSEdge-ClientID, X-MSEdge-ClientIP, X-Search-Location

                yield return req.SendWebRequest();
                while (!req.isDone)
                    yield return null;

                byte[] result = req.downloadHandler.data;
                string resultJSON = System.Text.Encoding.Default.GetString(result);

                // Create a result object.
                ImageAnswer imageSearchResult = JsonUtility.FromJson<ImageAnswer>(resultJSON);
                nextImgOffset = imageSearchResult.nextOffset;
                totalEstimatedImgMatches = imageSearchResult.totalEstimatedMatches;

                // Extract Bing HTTP headers.
                foreach (String header in req.GetResponseHeaders().Keys)
                {
                    if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                        imageSearchResult.relevantHeaders[header] = req.GetResponseHeader(header);
                }

                if (speechRecognizer != null)
                {
                    speechRecognizer.SetMessage(string.Empty);
                }

                onSuccess(imageSearchResult);
            }
        }

        private void LoadNextPageImageResults()
        {
            if (searchQuery != null)
            {
                // Increment page and make request for next page.
                currImgOffset = nextImgOffset;
                currImgPage++;
                if (currImgOffset < totalEstimatedImgMatches - resultsPerPage)
                {
                    StartCoroutine(MakeImageSearchRequest(currImgOffset, CreateImageAssetsFromResults));
                }
            }
        }

        private GameObject CreateResultsParent(string parentName)
        {
            Transform cameraTf = cameraRef.transform;
            GameObject resultsParent = new GameObject(parentName);
            resultsParent.tag = "WebResult";
            resultsParent.transform.position = new Vector3(cameraTf.position.x, 0, cameraTf.position.z);
            Quaternion cameraRot = Quaternion.Euler(0, 0, 0);
            resultsParent.transform.rotation = cameraRot;
            return resultsParent;
        }

        #endregion
    }
}
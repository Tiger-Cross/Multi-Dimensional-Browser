using Assets.Scripts.SearchResults.DataStructures;
using Assets.Scripts.Utilities;
using Microsoft.MixedReality.Toolkit.Experimental.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.SearchResults
{
    public class ResultsGenerator : MonoBehaviour
    {

        // TODO: Refactor into WebResultsGenerator andf ImageResultsGenerator

        #region Properties

        // The prefabs will be added in the UnityEditor
        public GameObject webResultPrefab;
        public GameObject imageResultPrefab;
        public GameObject scrollPanePrefab;
        public GameObject directionalIndicatorPrefab;

        private Camera mainCam;
        private static readonly int numRows = 3;
        private float radius = 0.7f;
        private float maxWebResultSize;
        private static readonly string faviconUrlBase = "https://api.faviconkit.com/";

        #endregion properties

        private void Awake()
        {
            mainCam = Camera.main;
        }

        #region Public methods

        public void GenerateWebResults(SearchResponse results, GameObject webObjsParent, int currPage)
        {
            List<WebSearchResult> webResultsData = results.webPages.value;
            Transform refTf = webObjsParent.transform;
            int numCols = PlayerPrefs.GetInt("NumResultColumns");
            maxWebResultSize = PlayerPrefs.GetFloat("MaxWebResultSize");
            if (currPage == 0)
            {
                // Generate directional indicator
                Vector3 indicatorPos = refTf.TransformPoint(0, -0.1f, radius);
                GameObject indicatorTarget = new GameObject("WebIndicatortTarget");
                indicatorTarget.transform.position = indicatorPos;
                indicatorTarget.tag = "WebResult";
                GenerateDirectionalIndicator(indicatorTarget.transform, "web");
                // Generate scroll pane
                Vector3 scrollPanePos = refTf.TransformPoint(0, 1.2f, 0.4f);
                Quaternion scrollPaneRot = Quaternion.Euler(36.5650512f, refTf.localEulerAngles.y, 0);
                GameObject scrollPane = Instantiate(scrollPanePrefab, scrollPanePos, scrollPaneRot);
                scrollPane.GetComponentInChildren<TranslateObjectOnPan>().Target = refTf;
            }
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    // Allows for a semi-circle-like positioning
                    float angle = -(Mathf.PI / 8) * (numCols - 1) + (Mathf.PI / 4) * j;
                    float x = Mathf.Sin(angle) * radius;
                    float z = Mathf.Cos(angle) * radius;

                    // Set offset values
                    float xOffset = Mathf.Sign(x) * 0.05f * Convert.ToInt32(numCols == 4 && (j == 0 || j == numCols - 1));
                    float heightOffset = ((currPage * numRows) + i) * 0.4f + 1.4f;
                    float rowZOffset = 0.2f * ((currPage * numRows) + i);
                    float edgeZOffset = 0.075f * Convert.ToInt32((j == 0 || j == numCols - 1)) * (numCols - 2);

                    // Set position relative to user
                    Vector3 posTransform = new Vector3(x + xOffset, heightOffset, z + rowZOffset + edgeZOffset);
                    Vector3 pos = refTf.TransformPoint(posTransform);

                    float angleDivisor = numCols == 4 ? 1.75f : 2;
                    // calculate rotation (also relative to user)
                    float angleDegrees = (angle / angleDivisor) * Mathf.Rad2Deg;
                    float yRot = refTf.rotation.eulerAngles.y + angleDegrees;
                    Quaternion rot = Quaternion.Euler(0, yRot, 0);

                    // Generate result prefab in scene
                    GameObject webResult = GenerateWebResult(webResultsData[numCols * i + j], pos, rot);
                    webResult.transform.parent = refTf;
                }
            }
        }

        public GameObject GenerateWebResult(WebSearchResult webResultData, Vector3 pos, Quaternion rot)
        {
            GameObject webResult = Instantiate(webResultPrefab, pos, rot);
            // Set correct url for WebView
            webResult.GetComponent<CreateWebViewOnMaxScale>().WebViewUrl = webResultData.url;
            // Set text fields
            TextMeshPro[] textFields = webResult.GetComponentsInChildren<TextMeshPro>();
            textFields[0].text = webResultData.displayUrl;
            textFields[1].text = webResultData.name;
            textFields[2].text = webResultData.snippet;
            // Set favicon.
            Uri resultUri = new Uri(webResultData.url);
            SpriteRenderer sr = webResult.GetComponentInChildren<SpriteRenderer>();
            string faviconUrl = $"{faviconUrlBase}{resultUri.Host}/144";
            StartCoroutine(DownloadImageToSpriteRenderer(faviconUrl, sr, 0.1f, 0.15f));
            BookmarksManager bmManager = GetComponent<BookmarksManager>();
            // Assume buttons are returned in order: {bookmark, follow, destroy}
            PressableButton[] buttons = webResult.GetComponentsInChildren<PressableButton>();
            buttons[0].ButtonPressed.AddListener(() => bmManager.ToggleBookmark(webResultData));
            Interactable bookmarkToggle = buttons[0].GetComponent<Interactable>();
            bookmarkToggle.IsToggled = bmManager.IsInBookmarks(webResultData.name);
            buttons[2].ButtonPressed.AddListener(() => Destroy(webResult));
            // Set maximum size to transform into web view;
            webResult.GetComponent<CreateWebViewOnMaxScale>().MaxScale = maxWebResultSize;
            return webResult;
        }

        public void GenerateImageResults(ImageAnswer results, GameObject imgObjsParent, int currPage)
        {
            List<Image> imageResultsData = results.value;
            Transform refTf = imgObjsParent.transform;
            int numCols = PlayerPrefs.GetInt("NumResultColumns");
            int webResultsEnabled = PlayerPrefs.GetInt("WebResultsEnabled");
            int imagesInfront = webResultsEnabled == 1 ? -1 : 1;
            if (currPage == 0)
            {
                // Generate directional indicator
                Vector3 indicatorPos = refTf.TransformPoint(0, -0.1f, radius * imagesInfront);
                GameObject indicatorTarget = new GameObject("ImagesIndicatorTarget");
                indicatorTarget.tag = "WebResult";
                indicatorTarget.transform.position = indicatorPos;
                GenerateDirectionalIndicator(indicatorTarget.transform, "Images");
                // Generate scroll pane
                Vector3 scrollPanePos = refTf.TransformPoint(0, 1.2f, 0.4f * imagesInfront);
                Quaternion scrollPaneRot = Quaternion.Euler(26.5650512f, refTf.localEulerAngles.y + 180 * webResultsEnabled, 0);
                GameObject scrollPane = Instantiate(scrollPanePrefab, scrollPanePos, scrollPaneRot);
                scrollPane.GetComponentInChildren<TranslateObjectOnPan>().Target = refTf;
            }
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    // Allows for a semi-circle-like positioning
                    float angle = -(Mathf.PI / 8) * (numCols - 1) + (Mathf.PI / 4) * j;
                    float x = Mathf.Sin(angle) * radius * imagesInfront;
                    float z = Mathf.Cos(angle) * (radius + 0.2f) * imagesInfront;

                    // Set offset values
                    float xOffset = Mathf.Sign(x) * 0.05f * Convert.ToInt32(numCols == 4 && (j == 0 || j == numCols - 1));
                    float heightOffset = ((currPage * numRows) + i) * 0.4f + 1.4f;
                    float rowZOffset = 0.2f * ((currPage * numRows) + i) * -imagesInfront;
                    float edgeZOffset = 0.075f * Convert.ToInt32((j == 0 || j == numCols - 1)) * (numCols - 2) * -imagesInfront;

                    // Set position relative to user
                    Vector3 posTransform = new Vector3(x + xOffset, heightOffset, z - rowZOffset - edgeZOffset);
                    Vector3 pos = refTf.TransformPoint(posTransform);

                    float angleOffset = Mathf.PI * webResultsEnabled;
                    float angleDivisor = numCols == 4 ? 1.75f : 2;

                    // calculate rotation (also relative to user)
                    float angleDegrees = (angle / angleDivisor + angleOffset) * Mathf.Rad2Deg;
                    float yRot = refTf.rotation.eulerAngles.y + angleDegrees;
                    Quaternion rot = Quaternion.Euler(0, yRot, 0);

                    // Generate result prefab in scene
                    GameObject imageResult = GenerateImageResult(imageResultsData[numCols * i + j], pos, rot);
                    imageResult.transform.parent = refTf;
                }
            }
        }

        #endregion

        #region Private Methods

        private void GenerateDirectionalIndicator(Transform target, string text)
        {
            GameObject directionalIndicator = Instantiate(directionalIndicatorPrefab, Vector3.zero, Quaternion.identity);
            DirectionalIndicator solver = directionalIndicator.GetComponentInChildren<DirectionalIndicator>();
            solver.DirectionalTarget = target.transform;
            TextMeshPro indicatorText = directionalIndicator.GetComponentInChildren<TextMeshPro>();
            indicatorText.text = text;
            Billboard billboard = indicatorText.GetComponent<Billboard>();
            billboard.TargetTransform = mainCam.transform;
        }

        private GameObject GenerateImageResult(Image imageData, Vector3 pos, Quaternion rot)
        {
            GameObject imageResult = Instantiate(imageResultPrefab, pos, rot);
            SpriteRenderer imageSr = imageResult.GetComponentInChildren<SpriteRenderer>();

            // Download thumbnail to sprite.
            StartCoroutine(DownloadImageToSpriteRenderer(imageData.thumbnailUrl, imageSr, 0.8f, 0.6f, true));
            // Set text fields with image data
            TextMeshPro[] textFields = imageResult.GetComponentsInChildren<TextMeshPro>();
            textFields[0].text = imageData.name;
            textFields[1].text = $"Go to: {imageData.hostPageDisplayUrl}";
            // TODO: create button that allows switching between thumnail image and contentURL image.
            // Assume buttons are returned in order: {text, follow, destroy}
            PressableButton[] buttons = imageResult.GetComponentsInChildren<PressableButton>();
            WebViewCreator webviewCreator = imageResult.GetComponent<WebViewCreator>();
            buttons[0].ButtonPressed.AddListener(() => webviewCreator.CreateWebView(imageData.hostPageUrl));
            buttons[2].ButtonPressed.AddListener(() => Destroy(imageResult));
            return imageResult;
        }

        private IEnumerator DownloadImageToSpriteRenderer(string imageUrl, SpriteRenderer renderer, float targetX, float targetY, bool preserveRatio = false)
        {
            using (UnityWebRequest textureReq = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                //Send Request and wait
                yield return textureReq.SendWebRequest();

                DownloadHandler handle = textureReq.downloadHandler;
                if (textureReq.isHttpError || textureReq.isNetworkError)
                {
                    Debug.LogError("Error while Receiving: " + textureReq.error);
                }
                else
                {
                    //Load Image
                    Texture2D texture2d = DownloadHandlerTexture.GetContent(textureReq);
                    int imgWidth = texture2d.width;
                    int imgHeight = texture2d.height;

                    Sprite sprite = null;
                    sprite = Sprite.Create(texture2d, new Rect(0, 0, imgWidth, imgHeight), Vector2.zero);
                    
                    if (sprite != null)
                    {
                        Vector3 spriteSize = sprite.bounds.size;
                        renderer.sprite = sprite;
                        float xScale = targetX / spriteSize.x;
                        float yScale = targetY / spriteSize.y; 
                        if (preserveRatio)
                        {
                            if (targetX / targetY > spriteSize.x / spriteSize.y)
                            {
                                xScale *= spriteSize.x * yScale;
                            }
                            // TODO: How to preserve ratio for long images in x?
                            //else
                            //{
                            //    yScale *= spriteSize.y / xScale;
                            //}
                        }
                        Transform rendererTf = renderer.transform;
                        rendererTf.localScale = new Vector3(xScale, yScale, rendererTf.localScale.z);
                        rendererTf.position = rendererTf.TransformPoint(-spriteSize.x / 2, -spriteSize.y / 2, 0);
                    }
                }
            }

            #endregion
        }
    }
}
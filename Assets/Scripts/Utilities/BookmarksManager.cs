using Assets.Scripts.SearchResults;
using Assets.Scripts.SearchResults.DataStructures;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    // TODO: reduce duplication between this and history manager.
    // TODO: extend this to allow bookmarks for images etc.
    public class BookmarksManager : MonoBehaviour
    {
        // Assigned in unity editor
        public GameObject bookmarkObj;
        public GameObject buttonPrefab;

        private Dictionary<string, WebSearchResult> bookmarks = new Dictionary<string, WebSearchResult>();
        private List<GameObject> bookmarksButtons = new List<GameObject>();
        private static int numButtons = 4;
        private int currPage = 0;

        private Camera mainCam;

        public void ToggleBookmark(WebSearchResult webResult)
        {
            string pageName = webResult.name;
            if (bookmarks.ContainsKey(pageName))    
            {
                RemoveFromBookmarks(pageName);
            }
            else
            {
                AddToBookmarks(webResult);
            }
        }

        private void AddToBookmarks(WebSearchResult webResult)
        {
            string pageName = webResult.name;
            if (!bookmarks.ContainsKey(pageName))
            {
                Transform bookmarkContentTf = bookmarkObj.transform.Find("BookmarkContent");
                Vector3 bookmarkBtnPos = bookmarkContentTf.TransformPoint(0, -0.05f * (bookmarksButtons.Count % numButtons), 0);
                GameObject bookmarkButton = Instantiate(buttonPrefab, bookmarkBtnPos, bookmarkContentTf.rotation);
                bookmarkButton.transform.parent = bookmarkContentTf;
                PressableButton[] buttons = bookmarkButton.GetComponentsInChildren<PressableButton>();
                buttons[0].GetComponentInChildren<TextMeshPro>().text = pageName;
                ResultsGenerator webGenerator = GetComponent<ResultsGenerator>();
                Vector3 pos = mainCam.transform.TransformPoint(0, -0.2f, 0.4f);
                Quaternion rot = Quaternion.Euler(0, mainCam.transform.localEulerAngles.y, 0);
                buttons[0].ButtonPressed.AddListener(() => webGenerator.GenerateWebResult(webResult, pos, rot));
                buttons[1].ButtonPressed.AddListener(() => RemoveFromBookmarks(bookmarkButton));
                bookmarksButtons.Add(bookmarkButton);
                bookmarks.Add(pageName, webResult);
                bookmarkButton.SetActive(false);
            }
        }

        private void RemoveFromBookmarks(string pageName)
        {
            bookmarks.Remove(pageName);
            GameObject buttonToDelete;
            PressableButton[] buttons;
            for (int i = 0; i < bookmarksButtons.Count; i++)
            {
                buttons = bookmarksButtons[i].GetComponentsInChildren<PressableButton>();
                string currPageName = buttons[0].GetComponentInChildren<TextMeshPro>().text;
                if (pageName.Equals(currPageName))
                {
                    buttonToDelete = bookmarksButtons[i];
                    bookmarksButtons.Remove(buttonToDelete);
                    Destroy(buttonToDelete);
                    break;
                }
            }
        }

        private void RemoveFromBookmarks(GameObject bookmarkitem)
        {
            PressableButton[] buttons = bookmarkitem.GetComponentsInChildren<PressableButton>();
            string pageName = buttons[0].GetComponentInChildren<TextMeshPro>().text;
            bookmarks.Remove(pageName);
            bookmarksButtons.Remove(bookmarkitem);
            Destroy(bookmarkitem);
        }

        internal bool IsInBookmarks(string pageName)
        {
            return bookmarks.ContainsKey(pageName);
        }

        // TODO: the rest is common functionality with the history class.

        private void Start()
        {
            mainCam = Camera.main;
            // Add page button listeners
            Transform pageContentTf = bookmarkObj.transform.Find("PageContent");
            GameObject pageContent = pageContentTf.gameObject;
            // Buttons list is ordered as follows {prevPageButton, NextPageButton}
            PressableButton[] buttons = pageContent.GetComponentsInChildren<PressableButton>();
            buttons[0].ButtonPressed.AddListener(() => { currPage--; });
            buttons[1].ButtonPressed.AddListener(() => { currPage++; });
        }

        private void Update()
        {
            if (bookmarkObj != null)
            {
                if (bookmarkObj.activeSelf)
                {
                    if (bookmarksButtons.Count == 0)
                    {
                        DisplayMessageOnEmptyList();
                    }
                    else
                    {
                        DisplayBookmarksPage();
                    }
                }
            }
            else
            {
                Debug.Log("Bookmarks game object is null. Please assign in Unity Editor.");
            }
        }

        private void DisplayMessageOnEmptyList()
        {
            SetNamedObjectActive("EmptyMessage", true);
            SetBookmarkButtonsActive(false);
            SetNamedObjectActive("PageContent", false);
        }

        private void DisplayBookmarksPage()
        {
            SetNamedObjectActive("EmptyMessage", false);
            SetBookmarkButtonsActive(true);
            Transform pageContentTf = bookmarkObj.transform.Find("PageContent");
            GameObject pageContent = pageContentTf.gameObject;
            pageContent.SetActive(true);
            pageContent.GetComponentInChildren<TextMeshPro>().text = $"Page {currPage + 1}";
            // Buttons list is ordered as follows {prevPageButton, NextPageButton}
            PressableButton[] buttons = pageContent.GetComponentsInChildren<PressableButton>();
            buttons[0].enabled = currPage > 0;
            buttons[1].enabled = bookmarksButtons.Count > (currPage + 1) * numButtons;
        }

        private void SetBookmarkButtonsActive(bool active)
        {
            if (bookmarkObj != null && bookmarkObj.activeSelf)
            {
                int startIdx = currPage * numButtons;
                Transform histContentTf = bookmarkObj.transform.Find("BookmarkContent");
                for (int i = 0; i < bookmarksButtons.Count; i++)
                {

                    if (i >= startIdx && i < startIdx + numButtons)
                    {
                        bookmarksButtons[i].SetActive(active);
                        bookmarksButtons[i].transform.position = histContentTf.TransformPoint(0, -0.05f * (i % numButtons), 0);
                    }
                    else
                    {
                        bookmarksButtons[i].SetActive(false);
                    }
                }
            }
        }

        private void SetNamedObjectActive(string name, bool active)
        {
            Transform namedObjTf = bookmarkObj.transform.Find(name);
            GameObject namedObj = namedObjTf.gameObject;
            namedObj.SetActive(active);
        }
    }
}
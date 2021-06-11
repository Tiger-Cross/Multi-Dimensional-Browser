using Assets.Scripts.SearchResults;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class HistoryManager : MonoBehaviour
    {
        // Assigned in unity editor
        public GameObject searchHistoryObj;
        public GameObject historyButtonPrefab;

        private HashSet<string> historyStrs = new HashSet<string>();
        private List<GameObject> historyButtons = new List<GameObject>();
        private static int numHistoryButtons = 4;
        private int currPage = 0;

        public void AddToHistory(string searchQuery)
        {
            if (!historyStrs.Contains(searchQuery))
            {
                Transform histContentTf = searchHistoryObj.transform.Find("HistoryContent");
                Vector3 histBtnPos = histContentTf.TransformPoint(0, -0.05f * (historyButtons.Count % numHistoryButtons), 0);
                GameObject historyButton = Instantiate(historyButtonPrefab, histBtnPos, histContentTf.rotation);
                historyButton.transform.parent = histContentTf;
                PressableButton[] buttons = historyButton.GetComponentsInChildren<PressableButton>();
                buttons[0].GetComponentInChildren<TextMeshPro>().text = searchQuery;
                SearchController webController = GetComponent<SearchController>();
                buttons[0].ButtonPressed.AddListener(() => webController.ExecuteWebSearch(searchQuery));
                buttons[1].ButtonPressed.AddListener(() => RemoveFromHistory(historyButton));
                historyButtons.Add(historyButton);
                historyStrs.Add(searchQuery);
                historyButton.SetActive(false);
            }
        }

        public void RemoveFromHistory(GameObject historyItem)
        {
            PressableButton[] buttons = historyItem.GetComponentsInChildren<PressableButton>();
            string searchQuery = buttons[0].GetComponentInChildren<TextMeshPro>().text;
            historyStrs.Remove(searchQuery);
            historyButtons.Remove(historyItem);
            Destroy(historyItem);
        }

        private void Start()
        {
            // Add page button listeners
            Transform pageContentTf = searchHistoryObj.transform.Find("PageContent");
            GameObject pageContent = pageContentTf.gameObject;
            // Buttons list is ordered as follows {prevPageButton, NextPageButton}
            PressableButton[] buttons = pageContent.GetComponentsInChildren<PressableButton>();
            buttons[0].ButtonPressed.AddListener(() => { currPage--; });
            buttons[1].ButtonPressed.AddListener(() => { currPage++; });
        }

        private void Update()
        {
            if (searchHistoryObj != null)
            {
                if (searchHistoryObj.activeSelf)
                {
                    if (historyButtons.Count == 0)
                    {
                        DisplayMessageOnEmptyList();
                    }
                    else
                    {
                        DisplayHistoryPage();
                    }
                }
            } else
            {
                Debug.Log("Search History game object is null. Please assign in Unity Editor.");
            }
        }

        private void DisplayMessageOnEmptyList()
        {
            SetNamedObjectActive("EmptyMessage", true);
            SetHistoryButtonsActive(false);
            SetNamedObjectActive("PageContent", false);
        }

        private void DisplayHistoryPage()
        {
            SetNamedObjectActive("EmptyMessage", false);
            SetHistoryButtonsActive(true);
            Transform pageContentTf = searchHistoryObj.transform.Find("PageContent");
            GameObject pageContent = pageContentTf.gameObject;
            pageContent.SetActive(true);
            pageContent.GetComponentInChildren<TextMeshPro>().text = $"Page {currPage + 1}";
            // Buttons list is ordered as follows {prevPageButton, NextPageButton}
            PressableButton[] buttons = pageContent.GetComponentsInChildren<PressableButton>();
            buttons[0].enabled = currPage > 0;
            buttons[1].enabled = historyButtons.Count > (currPage + 1) * numHistoryButtons;
        }

        private void SetHistoryButtonsActive(bool active)
        {
            if (searchHistoryObj != null && searchHistoryObj.activeSelf)
            {
                int startIdx = currPage * numHistoryButtons;
                Transform histContentTf = searchHistoryObj.transform.Find("HistoryContent");
                for (int i = 0; i < historyButtons.Count; i++)
                {

                    if (i >= startIdx && i < startIdx + numHistoryButtons)
                    {
                        historyButtons[i].SetActive(active);
                        historyButtons[i].transform.position = histContentTf.TransformPoint(0, -0.05f * (i % numHistoryButtons), 0);
                    }
                    else
                    {
                        historyButtons[i].SetActive(false);
                    }
                }
            }
        }

        private void SetNamedObjectActive(string name, bool active)
        {
            Transform namedObjTf = searchHistoryObj.transform.Find(name);
            GameObject namedObj = namedObjTf.gameObject;
            namedObj.SetActive(active);
        }
    }
}
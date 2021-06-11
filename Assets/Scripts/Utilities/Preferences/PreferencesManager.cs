using Microsoft.MixedReality.Toolkit.UI;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Utilities.Preferences
{
    public class PreferencesManager : MonoBehaviour
    {
        // Assigned in unity editor
        public GameObject prefsObj;

        // Assume buttons on prefsObj are returned in order: {follow, close, toggleweb, toggleimages, save}
        private PressableButton[] prefButtons;
        // Assume sliders on prefsObj are returned in order: {numCols, maxWebResultSize}
        private PinchSlider[] prefSliders;

        void Start()
        {
            // Set default player preferences
            PlayerPrefs.SetInt("WebResultsEnabled", 1);
            PlayerPrefs.SetInt("ImageResultsEnabled", 1);
            PlayerPrefs.SetInt("NumResultColumns", 3);
            PlayerPrefs.SetFloat("MaxWebResultSize", 1f);
            PlayerPrefs.Save();
            // Get buttons and sliders from preferences UI
            prefButtons = prefsObj.GetComponentsInChildren<PressableButton>();
            prefSliders = prefsObj.GetComponentsInChildren<PinchSlider>();
            // Add button listeners for result type toggles
            prefButtons[2].ButtonPressed.AddListener(() => ToggleResultTypeEnabled("Web"));
            prefButtons[3].ButtonPressed.AddListener(() => ToggleResultTypeEnabled("Image"));
            // Add button listener for save preferences button
            prefButtons[4].ButtonPressed.AddListener(SavePlayerPreferences);
        }

        void SavePlayerPreferences()
        {
            PlayerPrefs.SetInt("NumResultColumns", GetNumCols());
            PlayerPrefs.SetFloat("MaxWebResultSize", GetMaxWebResultSize());
            PlayerPrefs.Save();
        }

        private void ToggleResultTypeEnabled(string resultType)
        {
            string accessStr = $"{resultType}ResultsEnabled";
            int newValue = PlayerPrefs.GetInt(accessStr) == 1 ? 0 : 1;
            PlayerPrefs.SetInt(accessStr, newValue);
        }

        private int GetNumCols()
        {
            TextMeshPro numColsValue = prefSliders[0].GetComponentInChildren<TextMeshPro>();
            return Mathf.RoundToInt(Convert.ToSingle(numColsValue.text));
        }

        private float GetMaxWebResultSize()
        {
            TextMeshPro maxWebResultSize = prefSliders[1].GetComponentInChildren<TextMeshPro>();
            return Convert.ToSingle(maxWebResultSize.text);
        }
    }
}
using Assets.Scripts.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class ButtonBehaviour: MonoBehaviour
    {
        // To be added / configured in the unity editor
        public GameObject searchHistory;
        public GridObjectCollection buttonCollection;

        private PressableButton[] buttons;
        private SpeechRecognizer speechRecognizer;
        private static string INITIAL_MESSAGE = "Press the speak button and say what you're looking for. \nThen press the search button!";

        void Start()
        {
            // Assume following button ordering:
            // {speak, keyboard, search, clear, history, bookmarks, tutorial, preferences}
            buttons = buttonCollection.GetComponentsInChildren<PressableButton>();
            GameObject inputManager = GameObject.Find("Input Manager");
            speechRecognizer = inputManager.GetComponentInChildren<SpeechRecognizer>();
            KeyboardManager keyboardManager = inputManager.GetComponentInChildren<KeyboardManager>();
            
            buttons[2].ButtonPressed.AddListener(() => SetInputsEnabled(false));
            if (speechRecognizer != null)
            {
                // TODO: should hide this keyboard in ExecuteSearchFunction
                buttons[2].ButtonPressed.AddListener(keyboardManager.HideKeyboard);
                buttons[3].ButtonPressed.AddListener(ResetInitialMessage);
            } 
            else
            {
                Debug.LogError("Speech recognizer was null. Ensure the game object exists and has the correct script.");
            }
            buttons[3].ButtonPressed.AddListener(DestroyExistingWebResults);
            buttons[3].ButtonPressed.AddListener(() => SetInputsEnabled(true));
            buttons[4].ButtonPressed.AddListener(() => ToggleNamedObjectActive("SearchHistory"));
            buttons[4].ButtonPressed.AddListener(() => HideNamedObject("Bookmarks"));
            buttons[4].ButtonPressed.AddListener(() => HideNamedObject("Preferences"));
            buttons[5].ButtonPressed.AddListener(() => ToggleNamedObjectActive("Bookmarks"));
            buttons[5].ButtonPressed.AddListener(() => HideNamedObject("SearchHistory"));
            buttons[5].ButtonPressed.AddListener(() => HideNamedObject("Preferences"));
            buttons[6].ButtonPressed.AddListener(() => StartCoroutine(TransitionToScene())); 
            buttons[7].ButtonPressed.AddListener(() => ToggleNamedObjectActive("Preferences"));
            buttons[7].ButtonPressed.AddListener(() => HideNamedObject("SearchHistory"));
            buttons[7].ButtonPressed.AddListener(() => HideNamedObject("Bookmarks"));
        }

        private void ResetInitialMessage()
        {
            speechRecognizer.SetMessage(INITIAL_MESSAGE);
        }

        private void SetInputsEnabled(bool enabled)
        {
            buttons[0].enabled = enabled;
            buttons[1].enabled = enabled;
        }

        private IEnumerator TransitionToScene()
        {
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene("Tutorial");
        }

        private void ToggleNamedObjectActive(string objectName)
        {
            Transform obj = transform.Find(objectName);
            if (obj != null)
            {
                obj.gameObject.SetActive(!obj.gameObject.activeSelf);
            }
        }

        private void HideNamedObject(string objectName)
        {
            Transform obj = transform.Find(objectName);
            obj.gameObject.SetActive(false);
        }

        private void DestroyExistingWebResults()
        {
            GameObject[] webResults = GameObject.FindGameObjectsWithTag("WebResult");
            for (var i = 0; i < webResults.Length; i++)
            {
                Destroy(webResults[i]);
            }
        }
    }
}
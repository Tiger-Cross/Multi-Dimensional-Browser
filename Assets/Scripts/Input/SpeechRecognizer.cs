
// This script is adapted from Microsoft's cognitive services Speech-to-text FromMicrophone sample
//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using Assets.Scripts.SearchResults;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech.Audio;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

namespace Assets.Scripts.Input
{
    // TODO: potentially adapt below code to use coroutines
    [RequireComponent(typeof(KeyboardManager))]
    public class SpeechRecognizer : MonoBehaviour
    {

        #region Properties 

        // TODO: possibly have more than one instruction text item.
        // Hook up the properties below in the Unity Editor.
        public TextMeshPro outputText;
        public PressableButton speakButton;
        public PressableButton keyboardButton;

        private static string INITIAL_MESSAGE = "Press the speak button and say what you're looking for. \nThen press the search button!";
        private readonly List<string> nonSearchStrings = new List<string> { INITIAL_MESSAGE, "Waiting for mic permission", "startRecoButton property is null! Assign a UI Button to it.", "No result types set. Please open the preferences menu and set at least one type of result.", "Listening...", "Loading...", "No valid query was successfully transcribed. Press the speak button to try again.", "We couldn't detect what you were trying to say. Please ensure there is no background noise.", "We're sorry. Something went wrong.", string.Empty };

        private object threadLocker = new object();
        private Microsoft.CognitiveServices.Speech.SpeechRecognizer recognizer;
        private bool stopRecognition;

        // TODO: decouple this string from each button (and web controller)
        // TODO: have a search message manager component
        // TODO: can we get rid of the threadlocker and make this get / settable?
        private string message;
        private bool micPermissionGranted = false;

#if PLATFORM_ANDROID 
        // Required to manifest microphone permission, cf.
        // https://docs.unity3d.com/Manual/android-manifest.html
        private Microphone mic;
#endif

        #endregion

        #region Speech Methods
        public void RecognizeContinuous()
        {
            SetMessage("Listening...");
            recognizer.StartContinuousRecognitionAsync();
        }

        public async void RecognizeOnce()
        {
            if (recognizer != null)
            {
                SetMessage("Listening...");

                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks result.
                string newMessage = string.Empty;
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    newMessage = result.Text;
                    Debug.Log($"Successfully transcribed query: {result.Text}");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    newMessage = "We couldn't detect what you were trying to say. Please ensure there is no background noise.";
                    Debug.Log("No match for result. Could be due to background noise or microphone not picking up utterance.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    newMessage = "We're sorry. Something went wrong.";
                    Debug.Log($"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}");
                }

                SetMessage(newMessage);
            }
            else
            {
                SetMessage("Speechrecognizer was null. Is the API key set correctly?");
            }
        }
        #endregion

        #region Keyboard Methods

        private void HandleKeyboardInput(string key)
        {
            // If we try to type with pre-defined message, set message to string.empty first
            if (nonSearchStrings.Contains(message))
            {
                SetMessage(string.Empty);
            }
            if (key.Length == 1)
            {
                SetMessage(message + key);
            }
            else if (key == "Del")
            {
                SetMessage(message.Remove(message.Length - 1, 1));
            }
            else if (key == "Space")
            {
                SetMessage(message + " ");
            }
            else if (key == "Enter")
            {
                GetComponent<KeyboardManager>().HideKeyboard();
                GameObject.Find("Web Controller").GetComponent<SearchController>().ExecuteWebSearch();
            }
        }

        #endregion

        #region Monobehaviour Methods

        void Start()
        {
            if (outputText == null)
            {
                UnityEngine.Debug.LogError("outputText property is null! Assign a UI Text element to it.");
            }
            else if (speakButton == null)
            {
                message = "startRecoButton property is null! Assign a UI Button to it.";
                UnityEngine.Debug.LogError(message);
            }
            else
            {
                // Continue with normal initialization, Text and Button objects are present.
#if PLATFORM_ANDROID
                // Request to use the microphone, cf.
                // https://docs.unity3d.com/Manual/android-RequestingPermissions.html
                message = "Waiting for mic permission";
                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    Permission.RequestUserPermission(Permission.Microphone);
                }
#else
                micPermissionGranted = true;
                message = INITIAL_MESSAGE;
#endif

                if (string.IsNullOrEmpty(AppSecretKeys.SpeechSubKey))
                {
                    Debug.LogError("Unable to get speech subscription key from resource object. Ensure the envrionment variable is set and the object is saved.");
                }
                var config = SpeechConfig.FromSubscription(AppSecretKeys.SpeechSubKey, "uksouth");
                config.SpeechRecognitionLanguage = "en-GB";
                recognizer = new Microsoft.CognitiveServices.Speech.SpeechRecognizer(config);

                // Subscribes to events.
                recognizer.Recognizing += (s, e) =>
                {
                    SetMessage(e.Result.Text);
                    Debug.Log($"RECOGNIZING: Text={e.Result.Text}");
                };

                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        SetMessage(e.Result.Text);
                        Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        SetMessage("We couldn't detect what you were trying to say. Please ensure there is no background noise.");
                        Debug.Log($"NOMATCH: Speech could not be recognized.");
                    }
                    stopRecognition = true;
                };

                recognizer.Canceled += (s, e) =>
                {
                    Debug.Log($"CANCELED: Reason={e.Reason}");
                    SetMessage("We're sorry. Something went wrong.");
                    if (e.Reason == CancellationReason.Error)
                    {
                        Debug.Log($"CANCELED: ErrorCode={e.ErrorCode}");
                        Debug.Log($"CANCELED: ErrorDetails={e.ErrorDetails}");
                    }
                };

                recognizer.SessionStarted += (s, e) =>
                {
                    Debug.Log("\n    Session started event.");
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    Debug.Log("\n    Session stopped event.");
                };

#if UNITY_EDITOR
                speakButton.ButtonPressed.AddListener(RecognizeOnce);
#else
                speakButton.ButtonPressed.AddListener(RecognizeContinuous);
#endif
                KeyboardManager keyboardManager = GetComponent<KeyboardManager>();
                if (keyboardManager != null)
                {
                    keyboardButton.ButtonPressed.AddListener(() => keyboardManager.OnMainKeyboardButtonClick(HandleKeyboardInput));
                }
                else
                {
                    Debug.Log("Keyboard manager script not found. Please attach the component to the current gameObject.");
                }
            }
        }

        void Update()
        {
#if PLATFORM_ANDROID
            if (!micPermissionGranted && Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                micPermissionGranted = true;
                message = INITIAL_MESSAGE;
            }
#endif

            if (stopRecognition)
            {
                recognizer.StopContinuousRecognitionAsync();
                stopRecognition = false;
            }

            lock (threadLocker)
            {
                if (outputText != null)
                {
                    outputText.text = message;
                }
            }
        }

#endregion

#region Misc. Methods

        public void SetMessage(string value)
        {
            lock (threadLocker)
            {
                message = value;
            }
        }

#endregion
    }
}
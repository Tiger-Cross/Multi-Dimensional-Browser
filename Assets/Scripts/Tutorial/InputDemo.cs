using Assets.Scripts.Input;
using Microsoft.CognitiveServices.Speech;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

// TODO: potentially adapt below code to use coroutines
// TODO: hugely duplicated between tutorial and main scene. refactor and re-use correctly.
[RequireComponent(typeof(KeyboardManager))]
public class InputDemo : MonoBehaviour
{

    #region Properties 

    // TODO: possibly have more than one instruction text item.
    // Hook up the properties below in the Unity Editor.
    public TextMeshPro outputText;
    public PressableButton speakButton;
    public PressableButton keyboardButton;

    private static string INITIAL_MESSAGE = "Try entering text here.";
    private readonly List<string> nonSearchStrings = new List<string> { INITIAL_MESSAGE, "Waiting for mic permission", "startRecoButton property is null! Assign a UI Button to it.", "Listening...", "Loading...", "No valid query was successfully transcribed. Press the speak button to try again.", "We couldn't detect what you were trying to say. Please ensure there is no background noise.", "We're sorry. Something went wrong.", string.Empty };

    private object threadLocker = new object();
    private Camera mainCam;

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
    public async void OnSpeakButtonClick()
    {
        // TODO: check if this is causing the issue in the build
        lock (threadLocker)
        {
            message = "Listening...";
        }

        // Creates an instance of a speech config with specified subscription key and service region.
        // Replace with your own subscription key and service region (e.g., "westus").
        if (string.IsNullOrEmpty(AppSecretKeys.SpeechSubKey))
        {
            Debug.LogError("Unable to get speech subscription key from resource object. Ensure the envrionment variable is set and the object is saved.");
        }
        var config = SpeechConfig.FromSubscription(AppSecretKeys.SpeechSubKey, "uksouth");
        config.SpeechRecognitionLanguage = "en-GB";

        // Make sure to dispose the recognizer after use!
        using (var recognizer = new Microsoft.CognitiveServices.Speech.SpeechRecognizer(config))
        {

            // Starts speech recognition, and returns after a single utterance is recognized. The end of a
            // single utterance is determined by listening for silence at the end or until a maximum of 15
            // seconds of audio is processed.  The task returns the recognition text as result.
            // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
            // shot recognition like command or query.
            // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
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

            lock (threadLocker)
            {
                message = newMessage;
            }
        }
    }
    #endregion

    #region Keyboard Methods

    private void HandleKeyboardInput(string key)
    {
        // If we try to type with pre-defined message, set message to string.empty first
        if (nonSearchStrings.Contains(message))
        {
            SetMesssage(string.Empty);
        }
        if (key.Length == 1)
        {
            SetMesssage(message + key);
        }
        else if (key == "Del")
        {
            SetMesssage(message.Remove(message.Length - 1, 1));
        }
        else if (key == "Space")
        {
            SetMesssage(message + " ");
        }
    }

    #endregion

    #region Monobehaviour Methods

    private void Awake()
    {
        mainCam = Camera.main;
    }

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
            speakButton.ButtonPressed.AddListener(OnSpeakButtonClick);

            KeyboardManager keyboardManager = GetComponent<KeyboardManager>();
            Transform cameraTf = mainCam.transform;
            // Set keyboard pos and rot
            Vector3 pos = cameraTf.TransformPoint(0, 1.5f, 0.5f);
            Quaternion rot = Quaternion.Euler(45, cameraTf.localEulerAngles.y, 0);
            if (keyboardManager != null)
            {
                keyboardButton.ButtonPressed.AddListener(() => keyboardManager.OnKeyboardButtonClick(HandleKeyboardInput, pos, rot));
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

    public void SetMesssage(string value)
    {
        lock (threadLocker)
        {
            message = value;
        }
    }

    #endregion
}

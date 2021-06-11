using Assets.Scripts.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using Vuplex.WebView;

namespace Assets.Scripts.SearchResults
{
    [RequireComponent(typeof(KeyboardManager))]
    public class WebViewCreator : MonoBehaviour
    {
        #region Properties
        // Initialise in Unity Editor
        public GameObject webViewControlsPrefab;

        WebViewPrefab webViewPrefab;
        GameObject webViewControls;
        KeyboardManager keyboardManager;

        private static Vector3 originalScale = new Vector3(0.45f, 0.29f, 0.011f);

        #endregion

        private void Start()
        {
            keyboardManager = GetComponent<KeyboardManager>();
        }

        public void CreateWebView(string url)
        {
            Transform tf = gameObject.transform;
            // Hide prefab and instantiate webview prefab in it's place
            gameObject.SetActive(false);
            float yScale = tf.localScale.y;
            if (webViewPrefab == null)
            {
                webViewPrefab = WebViewPrefab.Instantiate(tf.localScale.x, yScale);
                //// Make easier to detect poke interactions
                webViewPrefab.DragThreshold = 100;
                //// Load a URL once the prefab finishes initializing
                webViewPrefab.Initialized += (initializedSender, initializedEventArgs) =>
                {
                    webViewPrefab.WebView.LoadUrl(url);
                };
                SetWebViewSortingLayers();
            } else
            {
                webViewPrefab.gameObject.SetActive(true);
                webViewPrefab.Resize(tf.localScale.x, yScale);
            }
            //// Position the webview where the web result prefab was
            webViewPrefab.transform.localPosition = tf.TransformPoint(Vector3.up * yScale / 2);
            Vector3 webViewRot = new Vector3(0, tf.localEulerAngles.y + 180, 0);
            webViewPrefab.transform.localEulerAngles = webViewRot;

            // Create webview control panel
            Vector3 controlsPos = tf.TransformPoint(Vector3.up * (yScale / 1.75f + 0.1f));
            Quaternion controlsRot = webViewPrefab.transform.localRotation * Quaternion.Euler(180, 0, 0);
            if (webViewControls != null)
            {
                webViewControls.SetActive(true);
                webViewControls.transform.position = controlsPos;
                webViewControls.transform.rotation = controlsRot;
            }
            else
            {
                webViewControls = Instantiate(webViewControlsPrefab, controlsPos, controlsRot);
            }

            // Set keyboard pos and rot
            Transform webViewTf = webViewPrefab.transform;
            Vector3 pos = webViewTf.TransformPoint(0, -transform.localScale.y - 0.1f, 0.1f);
            Quaternion rot = Quaternion.Euler(45, tf.localEulerAngles.y, 0);
            AddButtonListeners(pos, rot);
        }

        private void SetWebViewSortingLayers()
        {
            MeshRenderer[] mrs = webViewPrefab.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < mrs.Length; i++)
            {
                mrs[i].sortingLayerName = "WebView";
            }
        }

        #region Control Button Methods

        private void AddButtonListeners(Vector3 keyboardPos, Quaternion keyboardRot)
        {
            if (webViewControls != null)
            {
                // Assume buttons come in order {Destroy, Reduce, Keyboard}
                PressableButton[] webViewControlButtons = webViewControls.GetComponentsInChildren<PressableButton>();
                webViewControlButtons[0].ButtonPressed.AddListener(DestroyWebObjects);
                webViewControlButtons[1].ButtonPressed.AddListener(ReduceWebView);
                webViewControlButtons[2].ButtonPressed.AddListener(() => keyboardManager.OnKeyboardButtonClick(HandleKeyboardInput, keyboardPos, keyboardRot));
            }
        }

        private void RemoveButtonListeners()
        {
            if (webViewControls != null)
            {
                PressableButton[] webViewControlButtons = webViewControls.GetComponentsInChildren<PressableButton>();
                for (int i = 0; i < webViewControlButtons.Length; i++)
                {
                    webViewControlButtons[i].ButtonPressed.RemoveAllListeners();
                }
            }
        }

        private void DestroyWebObjects()
        {
            webViewPrefab.gameObject.SetActive(false);
            keyboardManager.DestroyKeyboard();
            Destroy(webViewControls);
            Destroy(gameObject);
        }

        private void ReduceWebView()
        {
            webViewPrefab.gameObject.SetActive(false);
            keyboardManager.HideKeyboard();
            RemoveButtonListeners();
            webViewControls.SetActive(false);
            gameObject.transform.localScale = originalScale;
            gameObject.SetActive(true);
        }

        private void HandleKeyboardInput(string key)
        {
            if (key.Length == 1)
            {
                webViewPrefab.WebView.HandleKeyboardInput(key);
            }
            else if (key == "Del")
            {
                webViewPrefab.WebView.HandleKeyboardInput("BackSpace");
            }
            else if (key == "Space")
            {
                webViewPrefab.WebView.HandleKeyboardInput(" ");
            }
            else if (key == "Enter")
            {
                webViewPrefab.WebView.HandleKeyboardInput("Enter");
            }
        }

        #endregion
    }
}
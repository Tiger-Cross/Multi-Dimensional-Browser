using UnityEngine;

namespace Assets.Scripts.SearchResults
{
    [RequireComponent(typeof(WebViewCreator))]
    public class CreateWebViewOnMaxScale : MonoBehaviour
    {
        #region Properties
        private string webViewUrl;

        public string WebViewUrl
        {
            get => webViewUrl;
            set => webViewUrl = value;
        }

        private Vector3 maxScaleVect;
        private float maxScale = 1;

        public float MaxScale
        {
            get => maxScale;
            set
            {
                maxScale = value;
                maxScaleVect = new Vector3(maxScale, maxScale, maxScale);
            }
        }

        #endregion

        #region Monobehaviour Methods

        // Update is called once per frame
        void Update()
        {
            Transform tf = gameObject.transform;
            if (Vector3.Min(maxScaleVect, tf.localScale) != tf.localScale)
            {
                WebViewCreator webViewCreator = GetComponent<WebViewCreator>();
                webViewCreator.CreateWebView(webViewUrl);
            }
        }

        #endregion
    }
}

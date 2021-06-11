using System.Collections;
using UnityEngine;

namespace Assets.Scripts.SearchResults
{
    public class ParentRemover : MonoBehaviour
    {

        private Transform parentTf;
        private Vector3 originalPos;
        private Vector3 originalAngles;
        private Vector3 originalScale;

        private void Start()
        {
            parentTf = transform.parent;
            originalPos = transform.localPosition;
            originalAngles = transform.localEulerAngles;
            originalScale = transform.localScale;
        }

        public void RemoveParent()
        {
            if (transform.parent != null)
            {
                transform.parent = null;
                // un-tag the object so it's not destroyed when pressing clear.
                tag = "Untagged";
            }
        }

        public void AddBackParentAndReset()
        {
            if (transform.parent == null)
            {
                transform.SetParent(parentTf);
                transform.localPosition = originalPos;
                transform.localEulerAngles = originalAngles;
                transform.localScale = originalScale;
            }
        }

    }
}
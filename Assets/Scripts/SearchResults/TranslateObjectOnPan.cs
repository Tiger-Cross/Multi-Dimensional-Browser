using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

namespace Assets.Scripts.SearchResults
{
    public class TranslateObjectOnPan : MonoBehaviour
    {
        [SerializeField]
        private Transform target;
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        // TODO: potentially make this adjustable in preferences.
        private static readonly float panScaleFactor = 1;

        public void OnHandPan(HandPanEventData eventData)
        {
            if (target != null)
            {
                float ParentXRot = transform.parent.rotation.x;
                float magnitude = eventData.PanDelta.y;
                float yDelta = -magnitude * Mathf.Cos(ParentXRot) * panScaleFactor;
                float zDelta = magnitude * Mathf.Sin(ParentXRot) * panScaleFactor;
                target.position += transform.TransformDirection(0, yDelta, zDelta);
            }
        }
    }
}

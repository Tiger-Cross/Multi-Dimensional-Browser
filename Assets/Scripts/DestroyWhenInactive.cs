using Microsoft.MixedReality.Toolkit.Experimental.Utilities;
using UnityEngine;

namespace Assets.Scripts
{
    public class DestroyWhenInactive : MonoBehaviour
    {
        private DirectionalIndicator indicator;

        // Start is called before the first frame update
        void Start()
        {
            indicator = GetComponentInChildren<DirectionalIndicator>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!indicator.enabled)
            {
                Destroy(gameObject);
            }
        }
    }
}

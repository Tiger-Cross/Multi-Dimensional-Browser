using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.SearchResults
{
    public class FadeOutWebObject : MonoBehaviour
    {
        private float fadeOutUb = 2.5f;
        private float maxHeight = 2;
        private float minHeight = 1;
        private float fadeOutLb = 0.5f;
        private float maxAlpha = 0.5f;
        private int colourID;
        private int faceColourID;

        // Use this for initialization
        void Start()
        {
            colourID = Shader.PropertyToID("_Color");
            faceColourID = Shader.PropertyToID("_FaceColor");
            MeshRenderer mr = GetComponent<MeshRenderer>();
            FadeOutObject(colourID, maxAlpha, mr.material);
        }

        // Update is called once per frame
        void Update()
        {
            if (transform.parent != null)
            {
                float currHeight = transform.position.y;
                if (currHeight < minHeight)
                {
                    // May not generalise correctly. DO y = mx + c if changing values.
                    float newAlpha = (1  / fadeOutLb) * currHeight - minHeight;
                    FadeOutObjects(newAlpha);
                }
                if (currHeight > maxHeight)
                {
                    // May not generalise correctly. DO y = mx + c if changing values.
                    float newAlpha = (-1 / (fadeOutUb - maxHeight)) * currHeight + fadeOutUb / (fadeOutUb - maxHeight);
                    FadeOutObjects(newAlpha);
                }
            }
        }

        private void FadeOutObjects(float newAlpha)
        {
            // Fade out meshes
            MeshRenderer mr;
            MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < mrs.Length; i++)
            {
                FadeOutObject(colourID, newAlpha, mrs[i].material);
            }
            // Fade out text
            TextMeshPro[] textFields = GetComponentsInChildren<TextMeshPro>();
            for (int i = 0; i < textFields.Length; i++)
            {
                mr = textFields[i].GetComponent<MeshRenderer>();
                FadeOutObject(faceColourID, newAlpha, mr.material);
            }
            // Fade out image
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                FadeOutObject(colourID, newAlpha, sr.material);
            }
        }

        private void FadeOutObject(int colourID, float newAlpha, Material mat)
        {
            if (mat.HasProperty(colourID))
            {
                Color currColour = mat.GetColor(colourID);
                currColour.a = newAlpha;
                mat.SetColor(colourID, currColour);
            }
        }
    }
}
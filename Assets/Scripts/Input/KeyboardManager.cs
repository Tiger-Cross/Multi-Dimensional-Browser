using Microsoft.MixedReality.Toolkit.UI;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Input
{
    public class KeyboardManager : MonoBehaviour
    {

        #region Properties
        // Assigned in Unity Editor.
        public GameObject keyboardPrefab;

        private GameObject keyboard;
        private Camera cameraRef;

        #endregion

        private void Awake()
        {
            cameraRef = Camera.main;
        }


        #region public methods

        public void OnMainKeyboardButtonClick(Action<string> handleKeyboardInput)
        {
            Transform cameraTf = cameraRef.transform;
            // Set keyboard pos and rot
            Vector3 pos = cameraTf.TransformPoint(0, -0.2f, 0.5f);
            Quaternion rot = Quaternion.Euler(45, cameraTf.localEulerAngles.y, 0);
            OnKeyboardButtonClick(handleKeyboardInput, pos, rot);
        }

        public void OnKeyboardButtonClick(Action<string> handleKeyboardInput, Vector3 pos, Quaternion rot)
        {
            if (keyboard != null)
            {
                ToggleKeyboardActive();
                keyboard.transform.position = pos;
                keyboard.transform.rotation = rot;
            }
            else
            {
                CreateKeyboard(handleKeyboardInput, pos, rot);
            }
        }

        public void HideKeyboard()
        {
            if (keyboard != null)
            {
                keyboard.gameObject.SetActive(false);
            }
        }

        public void DestroyKeyboard()
        {
            Destroy(keyboard);
        }

        #endregion

        #region Private methods

        private void ToggleKeyboardActive()
        {
            if (keyboard != null)
            {
                GameObject keyboardObj = keyboard.gameObject;
                if (keyboardObj.activeSelf)
                {
                    keyboardObj.SetActive(false);
                }
                else
                {
                    keyboardObj.SetActive(true);
                }
            }
        }

        private void CreateKeyboard(Action<string> handleKeyboardInput, Vector3 pos, Quaternion rot)
        {
            // Create keyboard
            keyboard = Instantiate(keyboardPrefab, pos, rot);
            // Hook up the keyboard so that characters are routed to the input handler.
            RegisterKeyboardButtons(handleKeyboardInput);
        }

        private void RegisterKeyboardButtons(Action<string> handleKeyboardInput)
        {
            PressableButton[] keyboardButtons = keyboard.GetComponentsInChildren<PressableButton>();
            for (int i = 0; i < keyboardButtons.Length; i++)
            {
                string currBtnTxt = keyboardButtons[i].GetComponentInChildren<TextMeshPro>().text;
                keyboardButtons[i].ButtonPressed.AddListener(() => handleKeyboardInput(currBtnTxt));
            }
        }

        #endregion
    }
}
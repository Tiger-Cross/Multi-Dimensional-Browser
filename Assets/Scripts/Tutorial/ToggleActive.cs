using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    // Assigned in unity editor
    public PressableButton toggleButton;
    public GameObject obj;

    void Start()
    {
        toggleButton.ButtonPressed.AddListener(ToggleObjectActive);
    }

    void ToggleObjectActive()
    {
        obj.SetActive(!obj.activeSelf);
    }
}

using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitions : MonoBehaviour
{
    // Assigned in Unity editor.
    public PressableButton skipTutButton;
    public PressableButton finishTutButton;

    private void Start()
    {
        skipTutButton.ButtonPressed.AddListener(() => StartCoroutine(TransitionToScene()));
        finishTutButton.ButtonPressed.AddListener(() => StartCoroutine(TransitionToScene()));
    }

    private IEnumerator TransitionToScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Main");
    }
}

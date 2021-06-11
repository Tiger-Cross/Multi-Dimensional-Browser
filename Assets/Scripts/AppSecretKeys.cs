using System;
using UnityEngine;

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
public sealed class AppSecretKeys : ScriptableObject
{

    [SerializeField]
    private string speechSubKey = "";

    [SerializeField]
    private string searchSubKey = "";

    public static string SpeechSubKey
    {
        get { return Instance.speechSubKey; }
        set { Instance.speechSubKey = value; }
    }
    public static string SearchSubKey
    {
        get { return Instance.searchSubKey; }
        set { Instance.searchSubKey = value; }
    }

    private static AppSecretKeys instance;
    public static AppSecretKeys Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<AppSecretKeys>("AppSecretKeys");

                // This can happen if the developer never input their secret keys into the Unity Editor
                // Use a dummy object with defaults for the getters so we don't have a null pointer exception
                if (instance == null)
                {
                    instance = ScriptableObject.CreateInstance<AppSecretKeys>();

#if UNITY_EDITOR
                    // Only in the editor should we save it to disk
                    string properPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Resources");
                    if (!System.IO.Directory.Exists(properPath))
                    {
                        UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    string fullPath = System.IO.Path.Combine(
                        System.IO.Path.Combine("Assets", "Resources"),
                        "AppSecretKeys.asset"
                    );
                    UnityEditor.AssetDatabase.CreateAsset(instance, fullPath);
                    UnityEditor.AssetDatabase.SaveAssets();
#endif
                }
            }
            return instance;
        }

        set
        {
            instance = value;
        }
    }
}

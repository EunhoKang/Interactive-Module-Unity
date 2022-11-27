using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.IO;
using System;

using NuitrackSDKEditor.ErrorSolver;
using NuitrackSDKEditor.Documentation;
using NuitrackSDKEditor.Activation;


namespace NuitrackSDKEditor
{
    [InitializeOnLoad]
    public static class NuitrackMenu
    {
        static readonly string nuitrackScriptsPath = "Assets/NuitrackSDK/Nuitrack/Prefabs/NuitrackScripts.prefab";

        [MenuItem("Nuitrack/Prepare The Scene")]
        public static void AddNuitrackToScene()
        {
            UnityEngine.Object nuitrackScriptsPrefab = AssetDatabase.LoadAssetAtPath(nuitrackScriptsPath, typeof(GameObject));

            if (nuitrackScriptsPrefab == null)
                Debug.LogAssertion(string.Format("Prefab NuitrackScripts was not found at {0}", nuitrackScriptsPath));
            else
            {
                NuitrackManager nuitrackManager = UnityEngine.Object.FindObjectOfType<NuitrackManager>();

                if (nuitrackManager != null)
                {
                    EditorGUIUtility.PingObject(nuitrackManager);
                    Debug.LogWarning("NuitrackManager already exists on the scene.");
                }
                else
                {
                    UnityEngine.Object nuitrackScripts = PrefabUtility.InstantiatePrefab(nuitrackScriptsPrefab);
                    Undo.RegisterCreatedObjectUndo(nuitrackScripts, string.Format("Create object {0}", nuitrackScripts.name));
                    Selection.activeObject = nuitrackScripts;
                }
            }
        }

        [MenuItem("Nuitrack/Help/Open Github Page", priority = 20)]
        public static void GoToGithubPage()
        {
            Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/");
        }

        [MenuItem("Nuitrack/Help/Open tutorials list", priority = 21)]
        public static void OpenTutoralList()
        {
            NuitrackTutorialsEditorWindow.Open();
        }

        [MenuItem("Nuitrack/Help/Open Troubleshooting Page", priority = 22)]
        public static void GoToTroubleshootingPage()
        {
            Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Troubleshooting.md#troubleshooting");
        }

        [MenuItem("Nuitrack/Activate Nuitrack", priority = 0)]
        public static void ActivateNuitrackWizard()
        {
            NuitrackActivationWizard.Open();
        }

        [MenuItem("Nuitrack/Play Test Unity Scene", priority = 1)]
        public static void OpenNuitrackTestScene()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            else
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(Path.Combine("Assets", "NuitrackSDK", "NuitrackDemos", "All Modules", "AllModulesScene.unity"));
                    EditorApplication.EnterPlaymode();
                }
            }
        }

        [MenuItem("Nuitrack/Open Nuitrack Test Sample", priority = 2)]
        public static void OpenNuitrackTestSample()
        {
#if !NUITRACK_PORTABLE
            string nuitrackHomePath = Environment.GetEnvironmentVariable("NUITRACK_HOME");
            string workingDir = Path.Combine(nuitrackHomePath, "bin");
            if (nuitrackHomePath == null)
                return;
#else
            string workingDir = Path.Combine(Application.dataPath, "NuitrackSDK", "Plugins", "x86_64");
#endif
            string path = Path.Combine(workingDir, "nuitrack_sample.exe");

            ProgramStarter.Run(path, workingDir, true);
        }

        [MenuItem("Nuitrack/Open Nuitrack Activation Tool", priority = 3)]
        public static void OpenNuitrackApp()
        {
#if !NUITRACK_PORTABLE
            string nuitrackHomePath = Environment.GetEnvironmentVariable("NUITRACK_HOME");
            string workingDir = Path.Combine(nuitrackHomePath, "activation_tool");
            if (nuitrackHomePath == null)
                return;
#else
            string workingDir = Path.Combine(Application.dataPath, "NuitrackSDK", "Plugins", "x86_64");
#endif
            string path = Path.Combine(workingDir, "Nuitrack.exe");
            ProgramStarter.Run(path, workingDir, true);
        }

        [MenuItem("Nuitrack/Manage Nuitrack License", priority = 4)]
        public static void GoToLicensePage()
        {
            Application.OpenURL("https://cognitive.3divi.com");
        }
    }
}
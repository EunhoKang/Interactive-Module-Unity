using UnityEngine;
using UnityEditor;
using System.IO;
using System;


namespace NuitrackSDKEditor.ErrorSolver
{
    public class ProgramStarter
    {
        public static void Run(string appPath, string workingDirectory, bool blockEditor = false)
        {
#if UNITY_EDITOR_WIN
            try
            {
                if (File.Exists(appPath))
                {
                    System.Diagnostics.Process app = new System.Diagnostics.Process();
                    app.StartInfo.FileName = appPath;
                    app.StartInfo.WorkingDirectory = workingDirectory;
                    app.Start();
                    if (blockEditor)
                        app.WaitForExit();
                }
                else
                {
                    EditorUtility.DisplayDialog("Program not found", appPath + " not found!", "");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to launch app: " + e.Message);
            }
#endif
        }
    }
}
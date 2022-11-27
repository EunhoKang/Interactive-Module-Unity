using UnityEngine;
#if UNITY_EDITOR_WIN
using UnityEditor;
#endif
using System;
using System.IO;

namespace NuitrackSDK.ErrorSolver
{
    public class NuitrackErrorSolver
    {
        static string noSensorMessage = "<color=red><b>" + "Can't create DepthSensor module. Sensor connected? Is the connection stable? Are the wires okay?" + "</b></color>";

        public static string NuitrackHomePath
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    #if !NUITRACK_PORTABLE
                return Environment.GetEnvironmentVariable("NUITRACK_HOME");
    #else
                return Path.Combine(Application.dataPath, "NuitrackSDK", "plugins", "x86_64");
    #endif
#else
                return null;
#endif
            }
        }

        public static string CheckError(System.Exception ex, bool showInLog = true, bool showTroubleshooting = true)
        {
            return CheckError(ex.ToString(), showInLog, showTroubleshooting);
        }

        public static string CheckError(string error, bool showInLog = true, bool showTroubleshooting = true)
        {
            string errorMessage = error;
            string troubleshootingPageMessage = "<color=red><b>Also look Nuitrack Troubleshooting page:</b></color>github.com/3DiVi/nuitrack-sdk/blob/master/doc/Troubleshooting.md" +
                "\nIf all else fails and you decide to contact our technical support, do not forget to attach the Unity Log File (https://docs.unity3d.com/ScriptReference/Debug.Log.html or ADB) and specify the Nuitrack version";
            string incorrectInstallingMessage =
                "1.Is Nuitrack installed at all? (github.com/3DiVi/nuitrack-sdk/tree/master/Platforms) \n" +
                "2.Try restart PC \n" +
                "3.Check your Environment Variables in Windows settings. " +
                "There should be two variables \"NUITRACK_HOME\" with a path to \"Nuitrack\\nuitrack\\nutrack\" and a \"Path\" with a path to %NUITRACK_HOME%\\bin " +
                "Maybe the installation probably did not complete correctly, in this case, look Troubleshooting Page.";
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
    #if !NUITRACK_PORTABLE
            string nuitrackHomePath = System.Environment.GetEnvironmentVariable("NUITRACK_HOME");
            string nuitrackTbbPath = Path.Combine(nuitrackHomePath, "bin", "tbb.dll");
    #else
            string nuitrackHomePath = Path.Combine(Application.dataPath, "NuitrackSDK", "plugins", "x86_64");
            string nuitrackTbbPath = Path.Combine(nuitrackHomePath, "tbb.dll");
    #endif
#endif
            if (error.Contains("TBB"))
            {
#if UNITY_STANDALONE_WIN
    #if UNITY_EDITOR_WIN
                string unityTbbPath = EditorApplication.applicationPath.Replace("Unity.exe", "") + "tbb.dll";
                errorMessage = "<color=red><b>You need to replace the file " + unityTbbPath + " with Nuitrack compatible file " + nuitrackTbbPath + " (Don't forget to close the editor first)</b></color>";
    #else
                errorMessage = "<color=red><b>Problem with the file tbb.dll in the Nuitrack folder " + nuitrackTbbPath + ". Reinstall Nuitrack</b></color>";
    #endif
#endif
                return errorMessage;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (error.Contains("Can't create DepthSensor"))
                errorMessage = noSensorMessage;
            else if (error.Contains("INIT_NUITRACK_MANAGER_NOT_INSTALLED"))
            {
                errorMessage = "<color=red><b>" + "Install VicoVR App from Google Play or Nuitrack App. https://github.com/3DiVi/nuitrack-sdk/tree/master/Platforms" + "</b></color>";
            }
            else if (error.Contains("INIT_NUITRACK_RESOURCES_NOT_INSTALLED"))
            {
                errorMessage = "<color=red><b>" + "Launch Nuitrack application to install additional resources" + "</b></color>";
            }
            else if (error.Contains("LicenseNotAcquiredException"))
            {
                if (NuitrackManager.Instance.LicenseInfo.Trial)
                    errorMessage = "<color=red><b>" + "Nuitrack Trial time is over. Restart app. For unlimited time of use, you can switch to another license https://nuitrack.com/#pricing" + "</b></color>";
                else
                    errorMessage = "<color=red><b>" + "Activate Nuitrack license. Open Nuitrack App" + "</b></color>";
            }
            else if (error.Contains("nuitrack.ModuleNotInitializedException"))
            {
               errorMessage = "Your application and Nuitrack application may have incompatible architectures. " +
                    "Check that the correct architecture is set in the player settings (only one) and the correct version of the Nuitrack App is downloaded https://github.com/3DiVi/nuitrack-sdk/tree/master/Platforms";
            }
#endif
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
    #if NUITRACK_PORTABLE
            errorMessage = GetStandaloneErrorMessage(error, nuitrackHomePath);
    #else
            if (nuitrackHomePath == null)
                errorMessage = "<color=red><b>" + "Environment Variable [NUITRACK_HOME] not found" + "</b></color>" +
                    "\n" + incorrectInstallingMessage;
            else
            {
                string nuitrackModulePath = nuitrackHomePath + "\\middleware\\NuitrackModule.dll";
                if (!(File.Exists(nuitrackModulePath)))
                {
                    errorMessage = "<color=red><b>" + "File: " + nuitrackModulePath + " not exists or Unity doesn't have enough rights to access it." + "</b></color>" + " Nuitrack path is really: " +
                        nuitrackHomePath + " ?\n" + incorrectInstallingMessage + "\n" +
                        "4." + GetAccessDeniedMessage(nuitrackHomePath + "\\middleware");
                }
                else
                {
                    using (FileStream test = new FileStream(nuitrackModulePath, FileMode.Open, FileAccess.Read, FileShare.Read)) { }

                    errorMessage = GetStandaloneErrorMessage(error, nuitrackHomePath);
                }
            }
    #endif
#endif
            if (showInLog) Debug.LogError(errorMessage);
            if (showInLog && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "AllModulesScene") Debug.LogError("<color=red><b>It is recommended to test on AllModulesScene</b></color>");
            if (showInLog && showTroubleshooting) Debug.LogError(troubleshootingPageMessage);
            if (showInLog) Debug.LogError(error);

            if (showTroubleshooting)
                errorMessage += "\n" + troubleshootingPageMessage;

            return (errorMessage);
        }

        static string GetStandaloneErrorMessage(string error, string nuitrackHomePath)
        {
            string errorMessage = "";
            string nuitrack_sample_test = "Does this example work? " + nuitrackHomePath + "\\bin\\nuitrack_sample.exe.";

            try
            {
                if (error.Contains("Can't create DepthSensor"))
                    errorMessage = noSensorMessage + " \n" + nuitrack_sample_test;
                else if (error.Contains("nuitrack_SetParam"))
                    errorMessage = "<color=red><b>" + "Update Nuitrack Runtime https://github.com/3DiVi/nuitrack-sdk/tree/master/Platforms" + "</b></color>";
                else if (error.Contains("System.DllNotFoundException: libnuitrack"))
                    errorMessage = "<color=red><b>" + "Perhaps installed Nuitrack Runtime version for x86 (nuitrack-windows-x86.exe), in this case, install x64 version (github.com/3DiVi/nuitrack-sdk/blob/master/Platforms/nuitrack-windows-x64.exe)" + "</b></color>";
                else if (error.Contains("LicenseNotAcquiredException"))
                {
                    if (NuitrackManager.Instance.LicenseInfo.Trial)
                        errorMessage = "<color=red><b>" + "Nuitrack Trial time is over. Restart app. For unlimited time of use, you can switch to another license https://nuitrack.com/#pricing" + "</b></color>";
                    else
                        errorMessage = "<color=red><b>" + "Activate Nuitrack license. Unity Main menu: Nuitrack/Activate Nuitrack" + "</b></color>";
                }
                else
                    errorMessage = "<color=red><b>" + "Perhaps the sensor is already being used in other program. " + "</b></color>" + " Or some unexpected error." + "\n" + nuitrack_sample_test;
            }
            catch (System.Exception)
            {
                if (error.Contains("Cannot load library module"))
                    errorMessage = "<color=red><b>" + GetAccessDeniedMessage(nuitrackHomePath + "\\middleware") + " </b></color>" +
                        "Path: " + nuitrackHomePath +
                        "\nIf that doesn't work, check to see if you have used any other skeleton tracking software. If so, try uninstalling it and rebooting.";
            }

            return errorMessage;
        }

        static string GetAccessDeniedMessage(string path)
        {
            string accessDeniedMessage = "Check the read\\write permissions for the folder where Nuitrack Runtime is installed, as well as for all subfolders and files. " +
                            "Can you create text-file in " + path + " folder?" + " Try allow Full controll permissions for Users. " +
                            "(More details: winaero.com/how-to-take-ownership-and-get-full-access-to-files-and-folders-in-windows-10/)";

            return accessDeniedMessage;
        }

#if UNITY_EDITOR_WIN
        static public bool CheckCudnn()
        {
            string fileName = "cudnn64_7.dll";
            string editorCudnnPath = EditorApplication.applicationPath.Replace("Unity.exe", "");

            if (Environment.GetEnvironmentVariable("CUDA_PATH") == null)
            {
                string message = "CUDA not found" +
                                "\nCheck Nuitrack Object Detection requirements";

                if(EditorUtility.DisplayDialog("CUDA not found", message, "Open the documentation page", "Cancel"))
                {
                    Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Nuitrack_AI.md#nuitrack-ai-object-detection");
                }

                return false;
            }

            string cudaCudnnPath = Path.Combine(Environment.GetEnvironmentVariable("CUDA_PATH"), "bin");

            if (!FileCompare(Path.Combine(cudaCudnnPath, fileName), Path.Combine(editorCudnnPath, fileName)))
            {
                string message = "1. Close the Unity editor" +
                                "\n2. Copy the " + fileName + " library from " + cudaCudnnPath + " to your Unity editor folder " + editorCudnnPath +
                                "\n3. Run the Unity editor again";

                EditorUtility.DisplayDialog(fileName, message, "OK");
                return false;
            }

            return true;
        }
#endif

        static bool TryGetFileLength(FileInfo file, out long fileLength)
        {
            fileLength = 0;
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    fileLength = file.Length;
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        static bool FileCompare(string file1, string file2)
        {
            if (TryGetFileLength(new FileInfo(file1), out long file1Size) || TryGetFileLength(new FileInfo(file2), out long file2Size))
                return false;

            return file1Size == file2Size;
        }
    }
}
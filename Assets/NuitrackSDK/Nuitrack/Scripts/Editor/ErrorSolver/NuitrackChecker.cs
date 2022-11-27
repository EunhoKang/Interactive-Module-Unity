using UnityEngine;
using UnityEditor;
using System.IO;

using System.Collections.Generic;


namespace NuitrackSDKEditor.ErrorSolver
{
    [InitializeOnLoad]
    public class NuitrackChecker
    {
        static BuildTargetGroup buildTargetGroup;
        static string backendMessage;

        static readonly string filename = "nuitrack.lock";

        static NuitrackChecker()
        {
            PingNuitrack();
        }

        public static void PingNuitrack()
        {
            buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            backendMessage = "Current Scripting Backend " + PlayerSettings.GetScriptingBackend(buildTargetGroup) + "  Target:" + buildTargetGroup;

            bool isPortable = Directory.Exists(Application.dataPath + "/NuitrackSDK/Plugins/x86_64");

            string scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
#if NUITRACK_PORTABLE
            if (!isPortable)
            {
                scriptingDefineSymbols = scriptingDefineSymbols.Replace("NUITRACK_PORTABLE", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, scriptingDefineSymbols);
            }
#endif
#if !NUITRACK_PORTABLE
            if (isPortable)
            {
                scriptingDefineSymbols += ";NUITRACK_PORTABLE";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, scriptingDefineSymbols);
                Debug.Log("Switched to nuitrack_portable");
            }
#endif

            try
            {
                nuitrack.Nuitrack.Init();

                string nuitrackType = "Runtime";
                if (isPortable)
                    nuitrackType = "Portable";

                string initSuccessMessage = "<color=green><b>Test Nuitrack (ver." + nuitrack.Nuitrack.GetVersion() + ") init was successful! (type: " + nuitrackType + ")</b></color>\n" + backendMessage;

                //bool haveActiveLicense = false;
                //bool deviceConnect = false;

                if((int)nuitrack.Nuitrack.GetVersion() < 3513)
                {
                    nuitrack.device.NuitrackDevice[] devices = nuitrack.Nuitrack.GetDeviceList().ToArray();

                    foreach (nuitrack.device.NuitrackDevice device in devices)
                    {
                        string sensorName = device.GetInfo(nuitrack.device.DeviceInfoType.DEVICE_NAME);
                        nuitrack.device.ActivationStatus activationStatus = device.GetActivationStatus();

                        initSuccessMessage += "\nDevice " + " [Sensor Name: " + sensorName + ", License: " + activationStatus + "]";

                        //if (activationStatus != nuitrack.device.ActivationStatus.NONE)
                        //    haveActiveLicense = true;

                        //deviceConnect = true;
                    }

                    //if (deviceConnect && !haveActiveLicense)
                    //    Activation.NuitrackActivationWizard.Open(false);

                    if (devices.Length == 0)
                        initSuccessMessage += "\nSensor not connected";
                }

                nuitrack.Nuitrack.Release();
                Debug.Log(initSuccessMessage);
            }
            catch (System.Exception ex)
            {
                Debug.Log("<color=red><b>Test Nuitrack init failed!</b></color>\n" +
                    "<color=red><b>It is recommended to test on AllModulesScene. (Start the scene and follow the on-screen instructions)</b></color>\n" + backendMessage);

                Debug.Log(ex.ToString());
            }

            if (!File.Exists(filename))
            {
                FileInfo fi = new FileInfo(filename);
                fi.Create();
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            }
        }

        public static bool HaveConnectDevices(out List<string> sensorsNames, out List<nuitrack.device.ActivationStatus> licensesTypes)
        {
            sensorsNames = new List<string>();
            licensesTypes = new List<nuitrack.device.ActivationStatus>();

            try
            {
                nuitrack.Nuitrack.Init();
                bool haveDevices = nuitrack.Nuitrack.GetDeviceList().Count > 0;


                if (haveDevices)
                {
                    foreach (nuitrack.device.NuitrackDevice device in nuitrack.Nuitrack.GetDeviceList())
                    {
                        string sensorName = device.GetInfo(nuitrack.device.DeviceInfoType.DEVICE_NAME);
                        nuitrack.device.ActivationStatus activationStatus = device.GetActivationStatus();

                        sensorsNames.Add(sensorName);
                        licensesTypes.Add(activationStatus);
                    }
                }

                nuitrack.Nuitrack.Release();

                return haveDevices;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.ToString());
                return false;
            }
        }

        public static bool HaveConnectDevices()
        {
            try
            {
                nuitrack.Nuitrack.Init();
                bool haveDevices = nuitrack.Nuitrack.GetDeviceList().Count > 0;
                nuitrack.Nuitrack.Release();

                return haveDevices;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.ToString());
                return false;
            }
        }

        public static Dictionary<string, nuitrack.device.ActivationStatus> GetLicensTypes()
        {
            try
            {
                nuitrack.Nuitrack.Init();

                Dictionary<string, nuitrack.device.ActivationStatus> sensorActivate = null;

                if (nuitrack.Nuitrack.GetDeviceList().Count > 0)
                {
                    sensorActivate = new Dictionary<string, nuitrack.device.ActivationStatus>();

                    foreach (nuitrack.device.NuitrackDevice device in nuitrack.Nuitrack.GetDeviceList())
                    {
                        string sensorName = device.GetInfo(nuitrack.device.DeviceInfoType.DEVICE_NAME);
                        nuitrack.device.ActivationStatus activationStatus = device.GetActivationStatus();

                        sensorActivate.Add(sensorName, activationStatus);
                    }
                }

                nuitrack.Nuitrack.Release();

                return sensorActivate;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.ToString());
                return null;
            }
        }
    }
}
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

using System;
using System.Linq;
using System.IO;

using NuitrackSDKEditor.ErrorSolver;
using System.Collections.Generic;


namespace NuitrackSDKEditor.Activation
{
    public class NuitrackActivationWizard : EditorWindow
    {
        int step = 0;

        Color mainColor = Color.green;

        static Vector2 windowSize = new Vector2(500, 300);

        bool haveDevices = false;
        List<string> devicesNames = null;
        List<nuitrack.device.ActivationStatus> deviceActivations = null;

        List<UnityAction> drawMenus = null;

        public static void Open()
        {
            NuitrackActivationWizard window = GetWindow<NuitrackActivationWizard>();
            window.titleContent = new GUIContent("Nuitrack Activation Wizard");
            window.minSize = windowSize;
            window.maxSize = windowSize;

            window.Show();
        }

        void UpdateSensorState()
        {
            if (!EditorApplication.isPlaying)
                haveDevices = NuitrackChecker.HaveConnectDevices(out devicesNames, out deviceActivations);
        }

        void OnEnable()
        {
            step = 0;

            UpdateSensorState();

            bool haveActivated = deviceActivations != null && deviceActivations.Count(k => k != nuitrack.device.ActivationStatus.NONE) > 0;
            bool withDeviceInfo = !EditorApplication.isPlaying && (!haveDevices || haveActivated);

            drawMenus = new List<UnityAction>()
            {
                DrawStartMenu,
                DrawOpenActivationTool,
                DrawComplate
            };

            if (withDeviceInfo)
                drawMenus.Insert(0, DrawAwakeMenu);
        }

        void NextMenu()
        {
            if (step < drawMenus.Count)
                step++;
        }

        void PreviewMenu()
        {
            if (step > 0)
                step--;
        }

        void OnGUI()
        {
            drawMenus[step]();
        }

        void DrawHeader(string textHeader)
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                padding = new RectOffset(30, 20, 20, 20),
                fontStyle = FontStyle.Bold,
                fontSize = 20
            };

            string hedearText = string.Format("[{0}/{1}] {2}", step + 1, drawMenus.Count, textHeader);

            GUIContent gUIContent = new GUIContent(hedearText);

            Rect rect = GUILayoutUtility.GetRect(gUIContent, titleStyle);

            using (new GUIColor(mainColor))
                GUI.Box(rect, "");

            GUI.Label(rect, gUIContent, titleStyle);
        }

        void DrawMessage(string message, string imageName = null, bool withPanel = false)
        {
            if (imageName != null)
            {
                Texture2D background = Resources.Load(imageName) as Texture2D;

                Rect rect = GUILayoutUtility.GetLastRect();

                Rect backgroundRect = new Rect(0, rect.yMax, background.width, background.height);

                GUI.DrawTexture(backgroundRect, background);
            }

            GUIStyle textStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                padding = new RectOffset(30, 30, 30, 30),
                richText = true,
                wordWrap = true,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };

            GUIContent gUIContent = new GUIContent(message);

            GUILayout.Label(gUIContent, textStyle);
        }

        void DrawButtons(string text, Color color, UnityAction action, bool showBackButton = false, UnityAction advAction = null, string advButtonText = "Skip", Color advColor = default)
        {
            GUIContent buttonContent = new GUIContent(text);
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.GetStyle("Button"))
            {
                padding = new RectOffset(10, 10, 10, 10),
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };

            Vector2 buttonSize = buttonStyle.CalcSize(buttonContent);
            Vector2 buttonPosition = new Vector2(position.width - buttonSize.x - 10, position.height - buttonSize.y - 10);

            Rect buttonRect = new Rect(buttonPosition, buttonSize);

            color = Color.Lerp(color, GUI.color, 0.5f);

            using (new GUIColor(color, true))
                if (GUI.Button(buttonRect, buttonContent, buttonStyle))
                    action.Invoke();

            if(advAction != null)
            {
                GUIContent advButtonContent = new GUIContent(advButtonText);
                Vector2 advButtonSize = buttonStyle.CalcSize(advButtonContent);
                Vector2 advButtonPosition = new Vector2(buttonRect.xMin - advButtonSize.x - 10, buttonRect.y);

                Rect advButtonRect = new Rect(advButtonPosition, advButtonSize);

                advColor = advColor.Equals(default) ? Color.gray : advColor;

                using (new GUIColor(advColor))
                    if (GUI.Button(advButtonRect, advButtonContent, buttonStyle))
                        advAction.Invoke();
            }

            if (showBackButton)
            {
                GUIContent backButtonContent = new GUIContent("Back");
                Vector2 backButtonSize = buttonStyle.CalcSize(backButtonContent);
                Vector2 backButtonPosition = new Vector2(10, buttonRect.y);

                Rect backButtonRect = new Rect(backButtonPosition, backButtonSize);

                if (GUI.Button(backButtonRect, backButtonContent, buttonStyle))
                    PreviewMenu();
            }
        }

        void DrawAwakeMenu()
        {
            if (!haveDevices)
            {
                DrawHeader("No connected sensors found!");
                DrawMessage("Check whether the sensor is connected and whether this model is included in the list of supported models.");

                UnityAction updateAction = delegate
                {
                    UpdateSensorState();

                    bool haveActivated = deviceActivations != null && deviceActivations.Count(k => k != nuitrack.device.ActivationStatus.NONE) > 0;
                    bool withDeviceInfo = !EditorApplication.isPlaying && (!haveDevices || haveActivated);

                    if (!withDeviceInfo)
                        NextMenu();
                };

                Color updateColor = Color.Lerp(mainColor, GUI.color, 0.5f);
                DrawButtons("Skip", Color.gray, NextMenu, false, updateAction, "Update", updateColor);
            }
            else
            {
                DrawHeader("You have activated licenses!");

                string message = "";
                int index = 1;

                for(int i = 0; i < devicesNames.Count; i++)
                    message += string.Format("{0}. {1}: license type - {2}\n", index++, devicesNames[i], deviceActivations[i]);

                DrawMessage(message);

                DrawButtons("Continue anyway", mainColor, NextMenu, false);
            }
        }

        void DrawStartMenu()
        {
            DrawHeader("Let's get a license!");
            DrawMessage("Click \"Get a license\", fill out a simple form, and the key will be in your mail!", "StartBackground");

            UnityAction openWebSite = delegate
            {
                Application.OpenURL("https://nuitrack.com/#pricing");
                NextMenu();
            };

            DrawButtons("Get a license!", mainColor, openWebSite, false, NextMenu);
        }

        void OpenActivationtool()
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

        void DrawOpenActivationTool()
        {
            DrawHeader("Let's activate Nuitrack!");
            DrawMessage("After the key is in your mail, open the activation-tool, select the sensor, enter and activate the key.", "ActivationBackground");

            UnityAction openActivationTool = delegate
            {
                OpenActivationtool();
                NextMenu();
            };

            DrawButtons("I have the key. Open the Activation-tool!", mainColor, openActivationTool, true, NextMenu);
        }

        void DrawComplate()
        {
            DrawHeader("The fun continues!");
            DrawMessage("After activating the license, you can continue!", "ComplateBackground");

            UnityAction openActivationTool = delegate
            {
                if (EditorApplication.isPlaying)
                {
                    NuitrackManager.Instance.StopNuitrack();
                    NuitrackManager.Instance.StartNuitrack();
                }
                
                Close();
            };

            DrawButtons("I have activated the license, continue!", mainColor, openActivationTool, true);
        }
    }
}
using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections.Generic;

using NuitrackSDK;
using NuitrackSDK.Frame;


namespace NuitrackSDKEditor
{
    [CustomEditor(typeof(NuitrackManager), true)]
    public class NuitrackManagerEditor : NuitrackSDKEditor
    {
        bool openModules = false;

        TextureCache rgbCache = null;
        TextureCache depthCache = null;

        RenderTexture rgbTexture = null;
        RenderTexture depthTexture = null;

        string ClassParamName(string paramName)
        {
            return string.Format("{0}_{1}", GetType().Name, paramName);
        }

        bool ShowPreview
        {
            get
            {
                return EditorPrefs.GetBool(ClassParamName("showPreview"), false);
            }
            set
            {
                EditorPrefs.SetBool(ClassParamName("showPreview"), value);
            }
        }

        void OnDisable()
        {
            if (rgbCache != null)
                rgbCache.Dispose();

            if (depthCache != null)
                depthCache.Dispose();
        }

        public override void OnInspectorGUI()
        {
            DrawModules();

            DrawDefaultInspector();

            serializedObject.DrawPropertyField("runInBackground");

            DrawSensorOptions();
            DrawAdvanced();

            DrawInitEvetn();

            DrawFramePreview();
        }

        void DrawModules()
        {
            openModules = EditorGUILayout.BeginFoldoutHeaderGroup(openModules, "Modules");

            if (openModules)
            {
                using (new VerticalGroup(EditorStyles.helpBox))
                {
                    serializedObject.DrawPropertyField("depthModuleOn");
                    serializedObject.DrawPropertyField("colorModuleOn");
                    serializedObject.DrawPropertyField("userTrackerModuleOn");
                    serializedObject.DrawPropertyField("skeletonTrackerModuleOn");
                    serializedObject.DrawPropertyField("gesturesRecognizerModuleOn");
                    serializedObject.DrawPropertyField("handsTrackerModuleOn");

                    NuitrackSDKGUI.PropertyWithHelpButton(
                        serializedObject,
                        "useFaceTracking",
                        "https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Unity_Face_Tracking.md",
                        "Track and get information about faces with Nuitrack (position, angle of rotation, box, emotions, age, gender)");
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void DrawSensorOptions()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Sensor options", EditorStyles.boldLabel);

            serializedObject.DrawPropertyField("depth2ColorRegistration");

            SerializedProperty mirrorProp = serializedObject.DrawPropertyField("mirror");
            SerializedProperty sensorRotation = serializedObject.FindProperty("sensorRotation");

            if (mirrorProp.boolValue)
            {
                sensorRotation.enumValueIndex = 0;
                serializedObject.ApplyModifiedProperties();
            }

            using (new EditorGUI.DisabledGroupScope(mirrorProp.boolValue))
                serializedObject.DrawPropertyField("sensorRotation");
        }

        void DrawAdvanced()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);

            NuitrackSDKGUI.PropertyWithHelpButton(
                serializedObject,
                "wifiConnect",
                "https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/TVico_User_Guide.md#wireless-case",
                "Only skeleton. PC, Unity Editor, MacOS and IOS");


            NuitrackSDKGUI.PropertyWithHelpButton(
                serializedObject,
                "useNuitrackAi",
                "https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Nuitrack_AI.md",
                "ONLY PC! Nuitrack AI is the new version of Nuitrack skeleton tracking middleware");

            SerializedProperty useFileRecordProp = serializedObject.DrawPropertyField("useFileRecord", "Use record file");

            if (useFileRecordProp.boolValue)
            {
                SerializedProperty pathProperty = serializedObject.FindProperty("pathToFileRecord");

                pathProperty.stringValue = NuitrackSDKGUI.OpenFileField(pathProperty.stringValue, "Bag or oni file", "bag", "oni");

                serializedObject.ApplyModifiedProperties();
            }

            #region Object detector

            UnityEngine.Events.UnityAction helpClick = delegate
            {
                Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Nuitrack_AI.md#nuitrack-ai-object-detection");
            };

            Rect propertyRect = NuitrackSDKGUI.WithRightButton(helpClick, "_Help", "Track and get information about objects with Nuitrack");

            string nuitrackHome = NuitrackSDK.ErrorSolver.NuitrackErrorSolver.NuitrackHomePath;
            if (nuitrackHome != null)
            {
                string configPath = Path.Combine(nuitrackHome, "data", "nuitrack.config");

                UnityEngine.Events.UnityAction openFolderClick = delegate { EditorUtility.RevealInFinder(configPath); };

                using (new EditorGUI.DisabledGroupScope(!File.Exists(configPath)))
                    propertyRect = NuitrackSDKGUI.WithRightButton(propertyRect, openFolderClick, "Project", "Open config file folder");
            }

            using (new EditorGUI.DisabledGroupScope(true))
            {
                GUIContent objDetectionGUIContent = new GUIContent("Use Object Detection",
                    "Track and get information about objects with Nuitrack (set in nuitrack.config and restart scene)");

                bool objDetectionActive = nuitrack.Nuitrack.GetConfigValue("CnnDetectionModule.ToUse") == "true";

                EditorGUI.Toggle(propertyRect, objDetectionGUIContent, objDetectionActive);
            }

            #endregion
        }

        void DrawInitEvetn()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Init", EditorStyles.boldLabel);

            serializedObject.DrawPropertyField("asyncInit");
            serializedObject.DrawPropertyField("initEvent");
        }

        void DrawFramePreview()
        {
            float pointScale = 0.025f;
            float lineScale = 0.01f;

            ShowPreview = EditorGUILayout.BeginFoldoutHeaderGroup(ShowPreview, "Frame viewer");

            if (ShowPreview)
            {
                if (!EditorApplication.isPlaying)
                    NuitrackSDKGUI.DrawMessage("RGB and depth frames will be displayed run time.", LogType.Log);
                else
                {
                    if (rgbCache == null)
                        rgbCache = new TextureCache();

                    if (depthCache == null)
                        depthCache = new TextureCache();

                    List<Vector2> pointCoord = new List<Vector2>();

                    rgbTexture = NuitrackManager.ColorFrame.ToRenderTexture(rgbCache);
                    depthTexture = NuitrackManager.DepthFrame.ToRenderTexture(textureCache: depthCache);

                    Rect rgbRect = NuitrackSDKGUI.DrawFrame(rgbTexture, "RGB frame");

                    Texture pointTexture = EditorGUIUtility.IconContent("sv_icon_dot0_pix16_gizmo").image;

                    float lineSize = rgbRect.size.magnitude * lineScale;

                    foreach (UserData user in NuitrackManager.Users)
                        if (user.Skeleton != null)
                        {
                            Color userColor = FrameUtils.SegmentToTexture.GetColorByID(user.ID);

                            foreach (nuitrack.JointType jointType in System.Enum.GetValues(typeof(nuitrack.JointType)))
                            {
                                nuitrack.JointType parentJointType = jointType.GetParent();

                                UserData.SkeletonData.Joint joint = user.Skeleton.GetJoint(jointType);

                                if (joint.Confidence > 0.1f)
                                {
                                    Vector2 startPoint = new Vector2(rgbRect.x + rgbRect.width * joint.Proj.x, rgbRect.y + rgbRect.height * (1 - joint.Proj.y));

                                    pointCoord.Add(startPoint);

                                    if (jointType.GetParent() != nuitrack.JointType.None)
                                    {
                                        UserData.SkeletonData.Joint parentJoint = user.Skeleton.GetJoint(parentJointType);

                                        if (parentJoint.Confidence > 0.1f)
                                        {
                                            Vector2 endPoint = new Vector2(rgbRect.x + rgbRect.width * parentJoint.Proj.x, rgbRect.y + rgbRect.height * (1 - parentJoint.Proj.y));
                                            Handles.DrawBezier(startPoint, endPoint, startPoint, endPoint, userColor, null, lineSize);
                                        }
                                    }
                                }
                            }

                            float pointSize = rgbRect.size.magnitude * pointScale;

                            foreach (Vector3 point in pointCoord)
                            {
                                Rect rect = new Rect(point.x - pointSize / 2, point.y - pointSize / 2, pointSize, pointSize);
                                GUI.DrawTexture(rect, pointTexture, ScaleMode.ScaleToFit);
                            }
                        }

                    NuitrackSDKGUI.DrawFrame(depthTexture, "Depth frame");

                    Repaint();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
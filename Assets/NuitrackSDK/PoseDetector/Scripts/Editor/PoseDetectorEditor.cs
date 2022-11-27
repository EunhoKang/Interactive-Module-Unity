using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

using System.IO;
using System.Collections.Generic;

using NuitrackSDK;
using NuitrackSDK.Poses;
using UnityEditorInternal;

using NuitrackSDK.Frame;


namespace NuitrackSDKEditor.Poses
{
    [CustomEditor(typeof(PoseDetector), true)]
    public class PoseDetectorEditor : NuitrackSDKEditor
    {
        ReorderableList listView = null;

        TextureCache rgbCache = null;
        RenderTexture rgbTexture = null;

        PoseDetector PoseDetector
        {
            get
            {
                return target as PoseDetector;
            }
        }

        SerializedProperty PosesProperty
        {
            get
            {
                return serializedObject.FindProperty("posesCollection");
            }
        }

        SerializedProperty GetPose(int index)
        {
            return PosesProperty.GetArrayElementAtIndex(index);
        }

        NuitrackManager NuitrackManager
        {
            get
            {
                return FindObjectOfType<NuitrackManager>();
            }
        }

        void OnEnable()
        {
            listView = new ReorderableList(serializedObject, PosesProperty, true, true, true, true)
            {
                drawHeaderCallback = DrawHeaderCallback,
                elementHeightCallback = ElemetnHeigthCallback,
                onAddDropdownCallback = AddDropdownCallback,
                drawElementCallback = DrawElementCallback
            };
        }

        void OnDisable()
        {
            if (rgbCache != null)
                rgbCache.Dispose();
        }

        void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Poses collection");
        }

        float ElemetnHeigthCallback(int index)
        {
            SerializedProperty poseProperty = GetPose(index);
            return EditorGUI.GetPropertyHeight(poseProperty) + 2f;
        }

        void AddDropdownCallback(Rect buttonRect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create new pose"), false, AddNewPose);
            menu.AddItem(new GUIContent("Add empty item"), false, AddEmptyPose);
            menu.ShowAsContext();
        }

        void AddEmptyPose()
        {
            PosesProperty.arraySize += 1;

            SerializedProperty poseProperty = GetPose(PosesProperty.arraySize - 1);

            poseProperty.FindPropertyRelative("pose").objectReferenceValue = null;
            poseProperty.FindPropertyRelative("poseProcessEvent").FindPropertyRelative("m_PersistentCalls.m_Calls").ClearArray();

            serializedObject.ApplyModifiedProperties();
            listView.index = PosesProperty.arraySize - 1;
        }

        void AddNewPose()
        {
            string newPoseName = string.Format("Pose {0}", PosesProperty.arraySize + 1);
            NuitrackPose newPose = NuitrackEditorHelper.CreateAsset<NuitrackPose>(newPoseName, NuitrackPoseEditor.SavePosePath);

            PosesProperty.arraySize += 1;
            SerializedProperty poseProperty = GetPose(PosesProperty.arraySize - 1);

            poseProperty.FindPropertyRelative("pose").objectReferenceValue = newPose;
            poseProperty.FindPropertyRelative("poseProcessEvent").FindPropertyRelative("m_PersistentCalls.m_Calls").ClearArray();

            serializedObject.ApplyModifiedProperties();

            listView.index = PosesProperty.arraySize - 1;
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty property = GetPose(index);

            rect.xMin += 10;
            rect.yMin += 2f;

            Rect foldoutHeaderRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutHeaderRect, property.isExpanded, string.Empty);

            SerializedProperty poseProperty = property.FindPropertyRelative("pose");

            Rect poseRect = new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(poseProperty));

            string lableText = poseProperty.objectReferenceValue != null ? poseProperty.objectReferenceValue.name : "(none)";
            GUIContent lableGuiContent = new GUIContent(lableText);
            EditorGUI.PropertyField(poseRect, poseProperty, lableGuiContent);

            if (property.isExpanded)
            {
                SerializedProperty poseEventProperty = property.FindPropertyRelative("poseProcessEvent");

                Rect eventRect = new Rect(rect.x, poseRect.yMax + EditorGUIUtility.singleLineHeight / 2, rect.width, EditorGUI.GetPropertyHeight(poseEventProperty));
                EditorGUI.PropertyField(eventRect, poseEventProperty, new GUIContent("Pose event (Pose, UserID, Match)"));
            }
        }

        public override void OnInspectorGUI()
        {
            NuitrackSDKGUI.MessageIfNuitrackNotExist();

            DrawDefaultInspector();
            EditorGUILayout.Space();

            listView.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            DrawRuntimePosesMatching();
            EditorGUILayout.Space();

            DrawRuntimeWizard();
        }

        string GetClassParamName(string paramName)
        {
            return string.Format("{0}_{1}", nameof(PoseDetectorEditor), paramName);
        }

        #region Runtime pose wizard

        bool ShowRuntimePoseWizard
        {
            get
            {
                return EditorPrefs.GetBool(GetClassParamName("showRuntimePoseWizard"), false);
            }
            set
            {
                EditorPrefs.SetBool(GetClassParamName("showRuntimePoseWizard"), value);
            }
        }

        string DefaultPath
        {
            get
            {
                return Path.Combine(Application.dataPath, Path.Combine(NuitrackPoseEditor.SavePosePath));
            }
        }

        string SaveFolder
        {
            get
            {
                return EditorPrefs.GetString(GetClassParamName("saveRuntimePosePath"), DefaultPath);
            }
            set
            {
                EditorPrefs.SetString(GetClassParamName("saveRuntimePosePath"), value);
            }
        }

        void DrawRuntimeWizard()
        {
            GUIContent wizardLabel = new GUIContent("Save runtime pose wizard", EditorGUIUtility.IconContent("AvatarSelector").image);

            ShowRuntimePoseWizard = EditorGUILayout.BeginFoldoutHeaderGroup(ShowRuntimePoseWizard, wizardLabel);

            if (ShowRuntimePoseWizard)
            {
                if (NuitrackManager == null)
                {
                    UnityAction fixNuitrack = delegate { NuitrackMenu.AddNuitrackToScene(); };
                    NuitrackSDKGUI.DrawMessage("The wizard requires NuitrackScripts on the scene.", LogType.Error, fixNuitrack, "Fix");

                    Selection.activeObject = target;
                }
                else
                {
                    if (!EditorApplication.isPlaying)
                    {
                        using (new VerticalGroup(EditorStyles.helpBox))
                            EditorGUILayout.LabelField("1) Play \n2) Stand in a pose \n3) Save pose", EditorStyles.wordWrappedLabel);

                        EditorGUILayout.Space();
                        SaveFolder = NuitrackSDKGUI.OpenFolderField(SaveFolder, "Save pose folder", true, DefaultPath);

                        GUIContent playScene = new GUIContent("Play", EditorGUIUtility.IconContent("PlayButton").image);

                        EditorGUILayout.Space();
                        if (GUILayout.Button(playScene))
                            EditorApplication.isPlaying = true;
                    }
                    else
                    {
                        UserData user = NuitrackManager.Users.Current;

                        bool disable = user == null || user.Skeleton == null;

                        Texture icon = disable ? NuitrackSDKGUI.GetMessageIcon(LogType.Warning) : null;
                        Color backgroundColor = disable ? Color.yellow : Color.green;

                        string message = "After saving a pose, playback will stop and the saved pose will open";

                        if (disable)
                            message = string.Format("{0}\n{1}", "The user was not found. Stand in front of the sensor to save a pose.", message);

                        GUIContent messageGUI = new GUIContent(message, icon);
                        UnityAction savePose = delegate { SaveRuntimePose(user.Skeleton); };

                        GUIContent iconAvatar = new GUIContent("Save runtime pose", EditorGUIUtility.IconContent("SaveAs").image);

                        EditorGUI.BeginDisabledGroup(disable);
                        NuitrackSDKGUI.DrawMessage(messageGUI, backgroundColor, savePose, iconAvatar);
                        EditorGUI.EndDisabledGroup();

                        Repaint();

                        if (rgbCache == null)
                            rgbCache = new TextureCache();

                        rgbTexture = NuitrackManager.ColorFrame.ToRenderTexture(rgbCache);

                        NuitrackSDKGUI.DrawFrame(rgbTexture, "RGB Preview");
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void SaveRuntimePose(UserData.SkeletonData skeleton)
        {
            string name = "Runtime pose";
            NuitrackPose newPose = new NuitrackPose(name, skeleton);

            string saveFolder = SaveFolder.Replace(Application.dataPath, "");
            string[] separatePath = saveFolder.Split(new char[] { '\\', '/' }, System.StringSplitOptions.RemoveEmptyEntries);

            NuitrackPose poseAsset = NuitrackEditorHelper.CreateAsset<NuitrackPose>(name, separatePath);
            NuitrackPoseWrapper nuitrackPoseWrapper = new NuitrackPoseWrapper(new SerializedObject(poseAsset));

            nuitrackPoseWrapper.CopyFrom(newPose);

            Destroy(newPose);

            EditorApplication.isPlaying = false;
            Selection.activeObject = poseAsset;
        }

        #endregion

        #region Pose matching

        bool ShowRuntimePoseMatching
        {
            get
            {
                return EditorPrefs.GetBool(GetClassParamName("showRuntimePoseMatching"), false);
            }
            set
            {
                EditorPrefs.SetBool(GetClassParamName("showRuntimePoseMatching"), value);
            }
        }

        void DrawUserPoseMatch(string userName, float matchValue)
        {
            using (new GUIColor(Mathf.Approximately(matchValue, 1) ? Color.green : GUI.color))
            {
                Rect progressRect = EditorGUILayout.GetControlRect();
                EditorGUI.ProgressBar(progressRect, matchValue, userName);
            }
        }

        void DrawRuntimePosesMatching()
        {
            ShowRuntimePoseMatching = EditorGUILayout.BeginFoldoutHeaderGroup(ShowRuntimePoseMatching, "Runtime matches of poses");

            if (ShowRuntimePoseMatching)
            {
                if (EditorApplication.isPlaying)
                {
                    if (PoseDetector.CountPoses > 0)
                    {
                        using (new VerticalGroup(EditorStyles.helpBox))
                        {
                            foreach (NuitrackPose pose in PoseDetector)
                                using (new VerticalGroup())
                                {
                                    EditorGUILayout.LabelField(pose.name, EditorStyles.boldLabel);

                                    using (new VerticalGroup(EditorStyles.helpBox))
                                    {
                                        if (PoseDetector.Matches[pose].Count > 0)
                                            foreach (KeyValuePair<int, float> poseMatch in PoseDetector.Matches[pose])
                                                DrawUserPoseMatch(string.Format("User id: {0}", poseMatch.Key), poseMatch.Value);
                                        else
                                            DrawUserPoseMatch("User not found", 0);
                                    }
                                }
                        }
                    }
                    else
                        NuitrackSDKGUI.DrawMessage("Not a single pose is set", LogType.Log);

                    Repaint();
                }
                else
                    NuitrackSDKGUI.DrawMessage("Play the scene to see the matching poses for the users", LogType.Log);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion
    }
}
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using System.Linq;
using System.Collections.Generic;

using NuitrackSDK;
using NuitrackSDK.Poses;
using NuitrackSDKEditor.Avatar;


namespace NuitrackSDKEditor.Poses
{
    [CustomEditor(typeof(NuitrackPose), true)]
    public class NuitrackPoseEditor : NuitrackSDKEditor
    {
        static string[] savePosePath = new string[] { "NuitrackSDK", "PoseDetector", "PosesCollection", "Custom" };

        public static string[] SavePosePath
        {
            get
            {
                return savePosePath;
            }
        }

        NuitrackPoseWrapper poseWrapper = null;

        readonly ColorTheme colorTheme = new ColorTheme()
        {
            mainColor = new Color(0.65f, 0.85f, 0.45f),
            disableColor = new Color(0.4f, 0.4f, 0.4f)
        };

        SkeletonMapperGUI<Transform> skeletonMapper = null;
        SkeletonPoseView skeletonPoseView = null;

        bool openAdvanced = false;
        nuitrack.JointType selectedJoint = nuitrack.JointType.None;

        public nuitrack.JointType SelectedJoint
        {
            get
            {
                return selectedJoint;
            }
            set
            {
                selectedJoint = value;

                if (skeletonMapper != null)
                    skeletonMapper.SelectedJoint = value;

                if (skeletonPoseView != null)
                    skeletonPoseView.SelectedJoint = value;

                SceneView.RepaintAll();
                Repaint();
            }
        }

        #region CallBack events

        void OnBoneActivated(nuitrack.JointType jointType)
        {
            if (!poseWrapper[jointType].IsActive)
                poseWrapper[jointType].IsActive = true;

            SelectedJoint = jointType;
        }

        void OnBoneDeactivated(nuitrack.JointType jointType)
        {
            if (poseWrapper[jointType].IsActive)
            {
                poseWrapper[jointType].IsActive = false;

                if (!poseWrapper[jointType].IsActive && jointType == SelectedJoint)
                    SelectedJoint = nuitrack.JointType.None;
            }
        }

        void SkeletonPoseView_OnBoneToleranceChanged(nuitrack.JointType jointType, float tolerance)
        {
            poseWrapper[jointType].Tolerance = tolerance;
        }

        void SkeletonPoseView_OnBoneRotate(nuitrack.JointType jointType, Quaternion rotation)
        {
            Quaternion deltaRotation = rotation * Quaternion.Inverse(poseWrapper[jointType].Orientation);

            poseWrapper[jointType].Orientation = rotation;

            if (!Mathf.Approximately(Quaternion.Dot(deltaRotation, Quaternion.identity), 1))
            {
                List<nuitrack.JointType> childs = jointType.GetChilds(true);
                List<nuitrack.JointType> jointsMask = NuitrackPoseWrapper.JointsMask;

                foreach (nuitrack.JointType childJoint in childs)
                    if (jointsMask.Contains(childJoint))
                        poseWrapper[childJoint].Orientation = deltaRotation * poseWrapper[childJoint].Orientation;
            }
        }

        void SkeletonMapper_OnSelected(nuitrack.JointType jointType)
        {
            if (jointType == nuitrack.JointType.None)
                SelectedJoint = nuitrack.JointType.None;
            else
            {
                if (poseWrapper[jointType].IsActive && SelectedJoint != jointType)
                    SelectedJoint = jointType;
                else
                {
                    if (poseWrapper[jointType].IsActive)
                        OnBoneDeactivated(jointType);
                    else
                        OnBoneActivated(jointType);
                }
            }
        }

        #endregion

        private void OnEnable()
        {
            NuitrackPreviewStage stage = CreateInstance<NuitrackPreviewStage>();
            StageUtility.GoToStage(stage, true);
            stage.SceneSetup(target);

            Selection.activeObject = target;

            poseWrapper = new NuitrackPoseWrapper(serializedObject);

            skeletonMapper = new SkeletonMapperGUI<Transform>(NuitrackPoseWrapper.JointsMask, null, colorTheme);
            skeletonMapper.OnSelected += SkeletonMapper_OnSelected;

            skeletonPoseView = new SkeletonPoseView(null, NuitrackPoseWrapper.JointsMask, colorTheme);
            skeletonPoseView.OnBoneSetActive += OnBoneActivated;
            skeletonPoseView.OnBoneDelete += OnBoneDeactivated;
            skeletonPoseView.OnBoneRotate += SkeletonPoseView_OnBoneRotate;
            skeletonPoseView.OnBoneToleranceChanged += SkeletonPoseView_OnBoneToleranceChanged;

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            skeletonMapper.OnSelected -= SkeletonMapper_OnSelected;
            skeletonMapper = null;

            SelectedJoint = nuitrack.JointType.None;

            skeletonPoseView.OnBoneSetActive -= OnBoneActivated;
            skeletonPoseView.OnBoneDelete -= OnBoneDeactivated;

            skeletonPoseView.OnBoneRotate -= SkeletonPoseView_OnBoneRotate;
            skeletonPoseView.OnBoneToleranceChanged -= SkeletonPoseView_OnBoneToleranceChanged;

            skeletonPoseView.Dispose();
            skeletonPoseView = null;

            StageUtility.GoToMainStage();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DrawPoseGUI();

            EditorGUILayout.Space();

            using (new HorizontalGroup())
            {
                GUIContent createNewGUIContent = new GUIContent("Create new pose", EditorGUIUtility.IconContent("Toolbar Plus").image);

                if (GUILayout.Button(createNewGUIContent))
                {
                    NuitrackPose newPose = NuitrackEditorHelper.CreateAsset<NuitrackPose>("Pose", SavePosePath);
                    Selection.activeObject = newPose;
                }

                GUIContent copyNewGUIContent = new GUIContent("Duplicate pose", EditorGUIUtility.IconContent("TreeEditor.Duplicate").image);

                if (GUILayout.Button(copyNewGUIContent))
                {
                    string newName = string.Format("{0} (clone)", target.name);
                    NuitrackPose copyPose = NuitrackEditorHelper.CreateAsset<NuitrackPose>(newName, SavePosePath);

                    NuitrackPoseWrapper copyPoseWrapper = new NuitrackPoseWrapper(new SerializedObject(copyPose));
                    copyPoseWrapper.CopyFrom(poseWrapper);

                    Selection.activeObject = copyPose;
                }
            }
        }

        void DrawPoseGUI()
        {
            GUIStyle contentStyle = new GUIStyle(GUIStyle.none) { margin = new RectOffset(20, 0, 2, 2) };
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };

            List<nuitrack.JointType> jointsMask = NuitrackPoseWrapper.JointsMask;

            using (new VerticalGroup(contentStyle))
            {
                GUIContent relativeButtonContent = new GUIContent("Relative mode", "In relative mode, the rotation of the trunk will not affect the correspondence of the pose");
                poseWrapper.RelativeMode = EditorGUILayout.Toggle(relativeButtonContent, poseWrapper.RelativeMode);

                List<nuitrack.JointType> activeJoints = jointsMask.Where(k => poseWrapper[k].IsActive).ToList();
                skeletonMapper.Draw(activeJoints);

                if (skeletonPoseView != null)
                    skeletonPoseView.DrawGUIInspector();

                if (SelectedJoint != nuitrack.JointType.None)
                    NuitrackSDKGUI.DrawMessage("You can disable the selected joint by clicking on it.", LogType.Log);

                EditorGUILayout.Space();
                openAdvanced = GUILayout.Toggle(openAdvanced, "Advanced settings", foldoutStyle);

                if (openAdvanced)
                    using (new VerticalGroup(contentStyle))
                        DrawPoseAdvanced();

                EditorGUILayout.Space();
            }
        }

        void DrawPoseAdvanced()
        {
            List<nuitrack.JointType> jointsMask = NuitrackPoseWrapper.JointsMask;

            foreach (KeyValuePair<AvatarMaskBodyPart, SkeletonStyles.GUIBodyPart> bodyPartItem in SkeletonStyles.BodyParts)
            {
                AvatarMaskBodyPart bodyPart = bodyPartItem.Key;
                SkeletonStyles.GUIBodyPart guiBodyPart = bodyPartItem.Value;

                bool drawBodyPart = guiBodyPart.guiJoint.Any(x => jointsMask.Contains(x.JointType));

                if (drawBodyPart)
                {
                    EditorGUILayout.LabelField(guiBodyPart.Lable, EditorStyles.boldLabel);

                    foreach (SkeletonStyles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                    {
                        nuitrack.JointType jointType = guiJoint.JointType;

                        if (jointsMask.Contains(jointType))
                        {
                            NuitrackPoseWrapper.JointWrapper jointWrapper = poseWrapper[jointType];

                            using (new GUIColor(jointWrapper.IsActive ? GUI.color : Color.gray))
                            {
                                GUIStyle contentStyle = new GUIStyle(EditorStyles.helpBox) { margin = new RectOffset(20, 0, 2, 2) };
                                Color selectColor = SelectedJoint == jointType ? colorTheme.mainColor : GUI.color;

                                using (new VerticalGroup(contentStyle, null, selectColor))
                                {
                                    string displayName = NuitrackSDKGUI.GetUnityDisplayBoneName(jointType.ToUnityBones(), bodyPart);

                                    if (!jointWrapper.IsActive)
                                        displayName += " (disabled)";

                                    bool isActive = EditorGUILayout.ToggleLeft(displayName, jointWrapper.IsActive, EditorStyles.boldLabel);

                                    if (isActive != jointWrapper.IsActive)
                                    {
                                        if (jointWrapper.IsActive && jointType == SelectedJoint)
                                            SelectedJoint = nuitrack.JointType.None;

                                        jointWrapper.IsActive = isActive;
                                    }

                                    if (isActive)
                                    {
                                        //jointWrapper.Orientation = Quaternion.Euler(EditorGUILayout.Vector3Field("", jointWrapper.Orientation.eulerAngles));
                                        jointWrapper.Tolerance = EditorGUILayout.Slider("Tolerance", jointWrapper.Tolerance, 0.1f, 0.98f);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            List<nuitrack.JointType> jointsMask = NuitrackPoseWrapper.JointsMask;

            List<nuitrack.JointType> activeJoints = jointsMask.Where(k => poseWrapper[k].IsActive).ToList();
            Dictionary<nuitrack.JointType, Quaternion> jointsRotations = jointsMask.ToDictionary(k => k, v => poseWrapper[v].Orientation);
            Dictionary<nuitrack.JointType, float> jointsTolerance = jointsMask.ToDictionary(k => k, v => poseWrapper[v].Tolerance);

            skeletonPoseView.DrawScenePose(activeJoints, jointsRotations, jointsTolerance);
        }
    }
}
using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using JointType = nuitrack.JointType;

using NuitrackSDK;
using NuitrackSDK.Avatar;
using UnityEditor.SceneManagement;

namespace NuitrackSDKEditor.Avatar
{
    [CustomEditor(typeof(NuitrackAvatar), true)]
    public class NuitrackAvatarEditor : BaseAvatarEditor
    {
        SkeletonMapperGUI<Transform> skeletonMapper = null;
        SkeletonMapperListGUI<Transform> skeletonJointListUI = null;
        SkeletonBonesView skeletonBonesView = null;

        readonly GUIContent autoMapping = EditorGUIUtility.TrTextContent("Automap");
        readonly GUIContent enforceTPose = EditorGUIUtility.TrTextContent("Enforce T-Pose");
        readonly GUIContent clearMapping = EditorGUIUtility.TrTextContent("Clear");

        readonly Dictionary<JointType, string> jointFieldMap = new Dictionary<JointType, string>()
        {
            { JointType.Waist, "waist" },
            { JointType.Torso, "torso" },
            { JointType.LeftCollar, "collar" },
            { JointType.RightCollar, "collar" },
            {  JointType.Neck, "neck" },
            { JointType.Head , "head" },

            { JointType.LeftShoulder, "leftShoulder" },
            { JointType.LeftElbow, "leftElbow" },
            { JointType.LeftWrist, "leftWrist" },

            { JointType.RightShoulder, "rightShoulder" },
            { JointType.RightElbow, "rightElbow" },
            { JointType.RightWrist, "rightWrist" },

            { JointType.LeftHip, "leftHip" },
            { JointType.LeftKnee, "leftKnee" },
            { JointType.LeftAnkle, "leftAnkle" },

            { JointType.RightHip, "rightHip" },
            { JointType.RightKnee, "rightKnee" },
            { JointType.RightAnkle, "rightAnkle" }
        };

        readonly List<JointType> optionalJoints = new List<JointType>()
        {
            JointType.Head,
            JointType.Neck
        };

        readonly Color firstAutomapColor = new Color(0.5f, 1f, 0.5f, 1);

        Transform GetTransformFromField(JointType jointType)
        {
            return GetJointProperty(jointType).objectReferenceValue as Transform;
        }

        SerializedProperty GetJointProperty(JointType jointType)
        {
            return serializedObject.FindProperty(jointFieldMap[jointType]);
        }

        protected override JointType SelectJoint 
        {
            get 
            {
                return base.SelectJoint;
            }
            set
            {
                base.SelectJoint = value;

                if(skeletonMapper != null)
                    skeletonMapper.SelectedJoint = value;

                if (skeletonJointListUI != null)
                    skeletonJointListUI.SelectedJoint = value;

                if (skeletonBonesView != null)
                    skeletonBonesView.SelectedJoint = value;

                Repaint();
            }
        }

        SkeletonBonesView.ViewMode ViewMode
        {
            get
            {
                return (SkeletonBonesView.ViewMode)EditorPrefs.GetInt(target.GetType().FullName + "SkeletonViewMode", 1);
            }
            set
            {
                EditorPrefs.SetInt(target.GetType().FullName + "SkeletonViewMode", (int)value);
            }
        }

        protected virtual void OnEnable()
        {
            NuitrackAvatar avatar = target as NuitrackAvatar;

            List<JointType> jointMask = jointFieldMap.Keys.ToList();

            skeletonMapper = new SkeletonMapperGUI<Transform>(jointMask, optionalJoints);
            skeletonMapper.OnDrop += SkeletonMapper_onDrop;
            skeletonMapper.OnSelected += SkeletonMapper_onSelected;

            skeletonJointListUI = new SkeletonMapperListGUI<Transform>(jointMask, optionalJoints);
            skeletonJointListUI.OnDrop += SkeletonMapper_onDrop;
            skeletonJointListUI.OnSelected += SkeletonMapper_onSelected;

            skeletonBonesView = new SkeletonBonesView(avatar.transform, ViewMode);
            skeletonBonesView.OnBoneSelected += SkeletonBonesView_OnBoneSelected;
            skeletonBonesView.OnRemoveBone += SkeletonBonesView_OnRemoveBone;
        }

        protected virtual void OnDisable()
        {
            skeletonMapper.OnDrop -= SkeletonMapper_onDrop;
            skeletonMapper.OnSelected -= SkeletonMapper_onSelected;
            skeletonMapper = null;

            skeletonJointListUI.OnDrop -= SkeletonMapper_onDrop;
            skeletonJointListUI.OnSelected -= SkeletonMapper_onSelected;
            skeletonJointListUI = null;

            ViewMode = skeletonBonesView.CurrentViewMode;

            skeletonBonesView.OnBoneSelected -= SkeletonBonesView_OnBoneSelected;
            skeletonBonesView.OnRemoveBone -= SkeletonBonesView_OnRemoveBone;
            skeletonBonesView = null;
        }

        #region Skeleton Mapper events

        void SkeletonMapper_onDrop(Transform newJoint, JointType jointType)
        {
            if (!jointFieldMap.ContainsKey(jointType))
                return;

            EditJoint(jointType, newJoint);

            if (newJoint != null)
                EditorGUIUtility.PingObject(newJoint);
        }

        void SkeletonMapper_onSelected(JointType jointType)
        {
            SelectJoint = jointType;

            if (!jointFieldMap.ContainsKey(jointType))
                return;

            Transform selectTransform = GetTransformFromField(jointType);

            if(selectTransform != null)
                EditorGUIUtility.PingObject(selectTransform);
        }

        #endregion

        #region Skeleton Viewer events

        void SkeletonBonesView_OnBoneSelected(SkeletonBonesView.ViewMode viewMode, JointType jointType, Transform boneTransform)
        {
            switch (viewMode)
            {
                case SkeletonBonesView.ViewMode.ModelBones:
                    if (SelectJoint != JointType.None)
                    {
                        SkeletonMapper_onDrop(boneTransform, SelectJoint);
                        SkeletonMapper_onSelected(JointType.None);
                    }

                    if(boneTransform != null)
                        EditorGUIUtility.PingObject(boneTransform);
                    break;

                case SkeletonBonesView.ViewMode.AssignedBones:
                    SkeletonMapper_onSelected(jointType);
                    break;
            }
        }

        void SkeletonBonesView_OnRemoveBone(SkeletonBonesView.ViewMode viewMode, JointType jointType, Transform boneTransform)
        {
            switch (viewMode)
            {
                case SkeletonBonesView.ViewMode.AssignedBones:
                    SkeletonMapper_onDrop(null, jointType);
                    break;
            }
        }

        #endregion

        void EditJoint(JointType jointType, Transform objectTransform)
        {
            SerializedProperty jointProperty = GetJointProperty(jointType);

            if (jointProperty != null)
            {
                jointProperty.objectReferenceValue = objectTransform;
                serializedObject.ApplyModifiedProperties();
            }
        }

        void SetSensorSpace(SerializedProperty sensorSpaceProp, NuitrackSDK.SensorEnvironment.SensorSpace sensorSpace)
        {
            sensorSpaceProp.objectReferenceValue = sensorSpace.transform;
            serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawAvatarGUI()
        {
            EditorGUILayout.Space();

            SerializedProperty vrModeProperty = serializedObject.DrawPropertyField("vrMode", "VR mode");

            if (vrModeProperty.boolValue)
            {
                serializedObject.DrawPropertyField("vrHead", "VR head");
                serializedObject.DrawPropertyField("headTransform", "Head transform");
                EditorGUILayout.Space();
            }

            SerializedProperty needBorderGrid = serializedObject.DrawPropertyField("needBorderGrid", "Need border grid");

            if (needBorderGrid.boolValue)
            {
                serializedObject.DrawPropertyField("borderGrid", "Border grid");
                EditorGUILayout.Space();
            }

            SerializedProperty sensorSpaceProp = serializedObject.DrawPropertyField("sensorSpace", "Sensor space");

            if (StageUtility.GetCurrentStage() == StageUtility.GetMainStage())
            {
                NuitrackSDK.SensorEnvironment.SensorSpace sensorSpace = FindObjectOfType<NuitrackSDK.SensorEnvironment.SensorSpace>();

                if (sensorSpaceProp.objectReferenceValue == null && sensorSpace != null)
                {
                    UnityEngine.Events.UnityAction fixAction = delegate { SetSensorSpace(sensorSpaceProp, sensorSpace); };
                    NuitrackSDKGUI.DrawMessage(string.Format("Hint. Assign transform \"{0}\" as SensorSpace?", sensorSpace.name), LogType.Log, fixAction, "Apply");
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Calibration", EditorStyles.boldLabel);
            serializedObject.DrawPropertyField("recenterOnSuccess");

            DrawSkeletonMap();
        }

        void OnSceneGUI()
        {
            if (skeletonBonesView.CurrentViewMode != SkeletonBonesView.ViewMode.None)
            {
                Dictionary<JointType, Transform> includeJoints = jointFieldMap.Keys.
                    Where(k => GetTransformFromField(k) != null).
                    ToDictionary(k => k, v => GetTransformFromField(v));

                skeletonBonesView.DrawSceneGUI(includeJoints);
            }
        }

        void DrawSkeletonMap()
        {
            IEnumerable<JointType> activeJoints = jointFieldMap.Keys.Where(k => GetTransformFromField(k) != null);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Avatar map", EditorStyles.boldLabel);

            if (skeletonMapper != null)
                skeletonMapper.Draw(activeJoints.ToList());

            GUIContent toObjectGUIContent = new GUIContent("Centered in view", EditorGUIUtility.IconContent("SceneViewCamera").image);
            if (GUILayout.Button(toObjectGUIContent))
            {
                NuitrackAvatar avatar = target as NuitrackAvatar;
                SkeletonUtils.CenteredInView(avatar.transform);
            }

            if (skeletonBonesView != null)
                skeletonBonesView.DrawInspectorGUI();

            EditorGUILayout.Space();

            if (skeletonJointListUI != null)
            { 
                Dictionary<JointType, Transform> jointDict = activeJoints.ToDictionary(k => k, v => GetTransformFromField(v));
                skeletonJointListUI.Draw(jointDict);  
            }

            EditorGUILayout.Space();
            DrawAutomapTools(activeJoints); 
        }

        void DrawAutomapTools(IEnumerable<JointType> activeJoints)
        {
            bool disableClearbutton = activeJoints.Count() == 0;

            using (new VerticalGroup(EditorStyles.helpBox))
            {
                string text = disableClearbutton ?
                    "Click \"Automap\" to quickly configure the avatar" :
                    "To make the avatar exactly repeat your movements, put the avatar in the T-pose";

                GUIContent gUIContent = new GUIContent(text, NuitrackSDKGUI.GetMessageIcon(LogType.Log));
                EditorGUILayout.LabelField(gUIContent, EditorStyles.wordWrappedLabel);

                using (new HorizontalGroup())
                {
                    using (new GUIColor(disableClearbutton ? firstAutomapColor : GUI.color))
                        if (GUILayout.Button(autoMapping))
                            AutoMapping();

                    EditorGUI.BeginDisabledGroup(disableClearbutton);

                    if (GUILayout.Button(enforceTPose))
                        SetToTPose();

                    if (GUILayout.Button(clearMapping))
                    {
                        if (EditorUtility.DisplayDialog("Skeleton map", "Do you really want to clear the skeleton map?", "Yes", "No"))
                            Clear();
                    }
                    EditorGUI.EndDisabledGroup();

                }
            }
        }

        #region AutoMapping

        readonly List<JointType> excludeAutoFillJoints = new List<JointType>()
        {
            JointType.LeftCollar,
            JointType.RightCollar
        };

        void AutoMapping()
        {
            NuitrackAvatar avatar = target as NuitrackAvatar;

            Dictionary<HumanBodyBones, Transform> skeletonBonesMap = SkeletonUtils.GetBonesMap(avatar.transform);

            if (skeletonBonesMap == null || skeletonBonesMap.Count == 0)
            {
                Debug.LogError("It is not possible to automatically fill in the skeleton map. Check the correctness of your model.");
                return;
            }

            List<HumanBodyBones> failFoundBones = new List<HumanBodyBones>();

            foreach (JointType jointType in jointFieldMap.Keys)
            {
                HumanBodyBones humanBodyBones = jointType.ToUnityBones();

                if (GetTransformFromField(jointType) == null)
                {
                    if (excludeAutoFillJoints.Contains(jointType) || !skeletonBonesMap.ContainsKey(humanBodyBones))
                        failFoundBones.Add(humanBodyBones);
                    else
                        EditJoint(jointType, skeletonBonesMap[humanBodyBones]);
                }
            }

            if (failFoundBones.Count > 0)
                Debug.Log(string.Format("For bones: <color=orange><b>{0}</b></color>, could not be found object Transforms", string.Join(", ", failFoundBones)));
        }

        void Clear()
        {
            foreach (JointType jointType in jointFieldMap.Keys)
                EditJoint(jointType, null);
        }

        #endregion

        void SetToTPose()
        {
            Dictionary<HumanBodyBones, Transform> includeJoints = jointFieldMap.Keys.
                  Where(k => GetTransformFromField(k) != null).
                  ToDictionary(k => k.ToUnityBones(), v => GetTransformFromField(v));

            Object[] bones = includeJoints.Values.ToArray();
            Undo.RegisterCompleteObjectUndo(bones, "Set T-Pose");

            NuitrackAvatar avatar = target as NuitrackAvatar;

            SkeletonUtils.SetToTPose(avatar.transform, includeJoints);
        }
    }
}

using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using NuitrackSDK;


namespace NuitrackSDKEditor.Avatar
{
    public class SkeletonBonesView
    {
        public enum ViewMode
        {
            ModelBones = 0,
            AssignedBones = 1,
            None
        }

        public delegate void BoneHandler(ViewMode viewMode, nuitrack.JointType jointType, Transform boneTransform);

        public event BoneHandler OnRemoveBone;
        public event BoneHandler OnBoneSelected;

        readonly Color select = Color.white;
        readonly Color hoverColor = Color.black;

        readonly Color mainColor = new Color(0.1f, 0.5f, 0.9f, 1f);
        readonly Color unusedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        readonly Color waitModelBoneSelect = Color.yellow;

        const float jointSphereMult = 0.3f;
        const float minSizeAssignBoneMark = 0.1f;
        const float minSizeModelBoneMark = 0.05f;

        readonly Dictionary<nuitrack.JointType, List<nuitrack.JointType>> childsList = null;

        readonly Transform root = null;
        readonly Dictionary<Transform, bool> validBones = null;

        readonly GUIContent[] skeletonModeGuiContent = null;

        ViewMode currentViewMode = ViewMode.ModelBones;

        public virtual nuitrack.JointType SelectedJoint { get; set; } = nuitrack.JointType.None;

        public ViewMode CurrentViewMode
        {
            get
            {
                return currentViewMode;
            }
            set
            {
                currentViewMode = value;
                SceneView.RepaintAll();
            }
        }

        bool ModelBonesSelectionMode
        {
            get
            {
                return CurrentViewMode == ViewMode.ModelBones && SelectedJoint != nuitrack.JointType.None;
            }
        }

        public SkeletonBonesView(Transform root, ViewMode viewMode = ViewMode.AssignedBones)
        {
            CurrentViewMode = viewMode;
            this.root = root;

            validBones = SkeletonUtils.GetValidBones(root);

            childsList = new Dictionary<nuitrack.JointType, List<nuitrack.JointType>>();

            foreach (nuitrack.JointType jointType in Enum.GetValues(typeof(nuitrack.JointType)))
            {
                nuitrack.JointType parent = jointType.GetParent();

                if (parent != nuitrack.JointType.None)
                {
                    if (!childsList.ContainsKey(parent))
                        childsList.Add(parent, new List<nuitrack.JointType>());

                    childsList[parent].Add(jointType);
                }
            }

            // UI toolbar elements

            GUIContent modelBonesContent = new GUIContent("Model bones", EditorGUIUtility.IconContent("scenepicking_pickable-mixed_hover").image);
            GUIContent assignBonesContent = new GUIContent("Assigned bones", EditorGUIUtility.IconContent("AvatarSelector").image);
            GUIContent noneContent = new GUIContent(EditorGUIUtility.IconContent("animationvisibilitytoggleoff").image);

            skeletonModeGuiContent = new GUIContent[] { modelBonesContent, assignBonesContent, noneContent };
        }

        /// <summary>
        /// Draw the skeleton of the avatar in the Scene View.
        /// Use this in method OnSceneGUI of your custom editors.
        /// </summary>
        /// <param name="root">Root transform of the skeleton object</param>
        /// <param name="includeBones">List of bones to hide</param>
        public void DrawSceneGUI(Dictionary<nuitrack.JointType, Transform> includeBones)
        {
            switch (CurrentViewMode)
            {
                case ViewMode.ModelBones:
                    DrawModelBones(includeBones);
                    break;

                case ViewMode.AssignedBones:
                    DrawAssignedBones(includeBones);
                    break;
            }
        }

        /// <summary>
        /// Draw a GUI in the inspector.
        /// Use this in method OnInspectorGUI of your custom editors.
        /// </summary>
        public void DrawInspectorGUI()
        {
            EditorGUILayout.LabelField("Skeleton display mode", EditorStyles.boldLabel);

            switch (CurrentViewMode)
            {
                case ViewMode.ModelBones:
                    string modelBonesMessage = SelectedJoint == nuitrack.JointType.None ?
                        "Select the joint on the avatar, and then specify the joint on the model in order to set the match." :
                        "Specify the joint on the model in order to set the match. \nClick \"Deselect\" for cancels selection.";

                    NuitrackSDKGUI.DrawMessage(modelBonesMessage, LogType.Log);

                    break;

                case ViewMode.AssignedBones:
                    string assignedBonesMessage = "The mode displays the specified joints of the skeleton. You can blow out the joints on the model.";
                    NuitrackSDKGUI.DrawMessage(assignedBonesMessage, LogType.Log);
                    break;
            }

            CurrentViewMode = (ViewMode)GUILayout.Toolbar((int)CurrentViewMode, skeletonModeGuiContent);
        }

        void DrawModelBones(Dictionary<nuitrack.JointType, Transform> includeBones)
        {
            foreach (KeyValuePair<Transform, bool> validBone in validBones)
                if (validBone.Value)
                {
                    Transform transform = validBone.Key;

                    bool hasParent = transform.parent != null && validBones.ContainsKey(transform.parent) && validBones[transform.parent];

                    float dist = hasParent ? Vector3.Distance(transform.position, transform.parent.position) : 0;
                    int countJoints = hasParent ? transform.childCount + 2 : transform.childCount + 1;

                    List<Transform> childs = new List<Transform>();

                    foreach (Transform child in transform)
                        if (validBones.ContainsKey(child) && validBones[child])
                        {
                            dist += Vector3.Distance(transform.position, child.position);
                            childs.Add(child);
                        }

                    dist = Math.Max(minSizeModelBoneMark, dist / countJoints);

                    int controlID = GUIUtility.GetControlID(root.name.GetHashCode(), FocusType.Passive);

                    using (new HandlesColor(includeBones.ContainsValue(transform) ? Color.green : unusedColor))
                        DrawBoneController(controlID, transform, childs, nuitrack.JointType.None, dist * jointSphereMult);
                }
        }

        void DrawAssignedBones(Dictionary<nuitrack.JointType, Transform> includeBones)
        {
            foreach (KeyValuePair<nuitrack.JointType, Transform> jointTransform in includeBones)
            {
                nuitrack.JointType joint = jointTransform.Key;
                nuitrack.JointType parent = joint.GetParent();
                Transform transform = jointTransform.Value;

                float dist = includeBones.ContainsKey(parent) ? Vector3.Distance(transform.position, includeBones[parent].position) : 0;
                List<Transform> childs = new List<Transform>();

                Handles.color = SelectedJoint == joint ? Color.Lerp(mainColor, select, 0.5f) : mainColor;

                if (childsList.ContainsKey(joint))
                {
                    foreach (nuitrack.JointType childJoint in childsList[joint])
                        if (includeBones.ContainsKey(childJoint))
                        {
                            Transform childTransform = includeBones[childJoint];
                            dist += Vector3.Distance(transform.position, childTransform.position);

                            childs.Add(childTransform);
                        }
                }

                int countJoints = childs.Count + (includeBones.ContainsKey(parent) ? 2 : 1);
                dist = Math.Max(minSizeAssignBoneMark, dist / countJoints);

                int controlID = GUIUtility.GetControlID(root.name.GetHashCode(), FocusType.Passive);

                DrawBoneController(controlID, transform, childs, joint, dist * jointSphereMult);
            }
        }

        void DrawBoneController(int controllerID, Transform boneTransform, List<Transform> childs, nuitrack.JointType jointType, float size)
        {
            childs ??= new List<Transform>();

            Event e = Event.current;

            //We divide the size by 2, since strange behavior is detected when an element falls into the selection.
            //The size of the visual element is set by the diameter, and the selection area by the radius.
            Handles.SphereHandleCap(controllerID, boneTransform.position, boneTransform.rotation, size / 2, EventType.Layout);

            switch (e.GetTypeForControl(controllerID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controllerID && e.button == 0)
                    {
                        // Respond to a press on this handle. Drag starts automatically.
                        GUIUtility.hotControl = controllerID;
                        GUIUtility.keyboardControl = controllerID;

                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controllerID && e.button == 0)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();

                        OnBoneSelected?.Invoke(CurrentViewMode, jointType, boneTransform);
                    }
                    break;

                case EventType.Repaint:
                    Color withHoverColor = Handles.color;

                    if (GUIUtility.hotControl == 0 && HandleUtility.nearestControl == controllerID)
                        withHoverColor = Color.Lerp(Handles.color, hoverColor, 0.5f);

                    using (new HandlesColor(withHoverColor))
                    {
                        Handles.SphereHandleCap(controllerID, boneTransform.position, boneTransform.rotation, size, EventType.Repaint);

                        foreach (Transform child in childs)
                            SkeletonUtils.DrawBone(boneTransform.position, child.position);
                    }
                    break;

                case EventType.KeyDown:
                    if ((e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete) && GUIUtility.keyboardControl == controllerID)
                    {
                        GUIUtility.keyboardControl = 0;
                        e.Use();

                        OnRemoveBone?.Invoke(CurrentViewMode, jointType, boneTransform);
                    }
                    break;
            }
        }
    }
}

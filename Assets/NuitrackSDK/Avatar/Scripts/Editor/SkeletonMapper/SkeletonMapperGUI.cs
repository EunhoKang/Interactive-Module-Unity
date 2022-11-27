using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using nuitrack;


namespace NuitrackSDKEditor.Avatar
{
    public class ColorTheme
    {
        public Color mainColor;
        public Color disableColor;
    }

    /// <summary>
    /// Skeleton bones mapper, similar to the map in Avatar Configuration
    /// </summary>
    /// <typeparam name="T">Type of bone object (usually used by Transform)</typeparam>
    public class SkeletonMapperGUI<T> : SkeletonMapper<T> where T : Object
    {
        readonly List<JointType> jointMask = null;
        readonly List<JointType> optionalJoints = null;

        readonly ColorTheme colorTheme = new ColorTheme()
        {
            mainColor = new Color(0.2f, 0.6f, 1f, 1f), // Color.blue;
            disableColor = new Color(0.5f, 0.5f, 0.6f, 1f)
        };

        /// <summary>
        /// View of a avatar with a map of joints.
        /// </summary>
        /// <param name="jointMask">The mask of the displayed joints. If null, all available joints will be drawn.</param>
        /// <param name="colorTheme">Color theme. If null is set, the default theme will be used.</param>
        public SkeletonMapperGUI(List<JointType> jointMask, List<JointType> optionalJoints = null, ColorTheme colorTheme = null)
        {
            this.jointMask = jointMask ?? new List<JointType>();
            this.optionalJoints = optionalJoints ?? new List<JointType>();
            this.colorTheme = colorTheme ?? this.colorTheme;
        }

        List<AvatarMaskBodyPart> GetActiveBodyParts(List<JointType> jointsList)
        {
            List<AvatarMaskBodyPart> bodyParts = new List<AvatarMaskBodyPart>(SkeletonStyles.BodyParts.Keys);

            foreach (KeyValuePair<AvatarMaskBodyPart, SkeletonStyles.GUIBodyPart> bodyPart in SkeletonStyles.BodyParts)
                foreach (SkeletonStyles.GUIJoint guiJoint in bodyPart.Value.guiJoint)
                    if (!optionalJoints.Contains(guiJoint.JointType) && !jointsList.Contains(guiJoint.JointType) && jointMask.Contains(guiJoint.JointType))
                    {
                        bodyParts.Remove(bodyPart.Key);
                        break;
                    }

            return bodyParts;
        }

        Rect DrawAvatarJointIcon(Rect mainRect, SkeletonStyles.GUIJoint guiJoint, bool filled, bool selected)
        {
            Vector2 pos = guiJoint.MapPosition;
            pos.Scale(new Vector2(mainRect.width * 0.5f, -mainRect.height * 0.5f));
            pos += mainRect.center;

            Rect jointRect = SkeletonStyles.Dot.DrawСentered(pos, optionalJoints.Contains(guiJoint.JointType), filled, selected);
            return jointRect;
        }

        /// <summary>
        /// Draw a map of joints
        /// </summary>
        /// <param name="activeJoints">Active joints (will be displayed as filled dots)</param>
        public void Draw(List<JointType> activeJoints)
        {
            Rect rect = GUILayoutUtility.GetRect(SkeletonStyles.UnityDude, GUIStyle.none, GUILayout.MaxWidth(SkeletonStyles.UnityDude.image.width));
            rect.x += (EditorGUIUtility.currentViewWidth - rect.width) / 2;

            Color grayColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

            using (new GUIColor(grayColor))
                GUI.DrawTexture(rect, SkeletonStyles.UnityDude.image);

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold
            };

            foreach (KeyValuePair<GUIContent, Vector2> labelData in SkeletonStyles.Labels)
            {
                Vector2 position = labelData.Value;
                position.Scale(rect.size);
                position.x -= EditorGUIUtility.labelWidth * 0.5f;

                Rect labelRect = new Rect(rect.center + position, new Vector2(EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight));

                GUI.Label(labelRect, labelData.Key, centeredStyle);
            }

            List<AvatarMaskBodyPart> filled = GetActiveBodyParts(activeJoints);

            foreach (KeyValuePair<AvatarMaskBodyPart, SkeletonStyles.GUIBodyPart> bodyPart in SkeletonStyles.BodyParts)
            {
                Color partColor = filled.Contains(bodyPart.Key) ? colorTheme.mainColor : colorTheme.disableColor;

                using (new GUIColor(partColor))
                {
                    foreach (GUIContent guiContent in bodyPart.Value.guiContents)
                        GUI.DrawTexture(rect, guiContent.image);
                }
            }

            foreach (KeyValuePair<AvatarMaskBodyPart, SkeletonStyles.GUIBodyPart> bodyPartItem in SkeletonStyles.BodyParts)
            {
                AvatarMaskBodyPart bodyPart = bodyPartItem.Key;
                SkeletonStyles.GUIBodyPart guiBodyPart = bodyPartItem.Value;

                foreach (SkeletonStyles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                {
                    JointType jointType = guiJoint.JointType;

                    if (jointMask.Contains(jointType))
                    {
                        Rect jointPointRect = DrawAvatarJointIcon(rect, guiJoint, activeJoints.Contains(jointType), jointType == SelectedJoint);

                        int keyboardID = GUIUtility.GetControlID(FocusType.Keyboard, jointPointRect);

                        T newJoint = HandleDragDrop(keyboardID, jointPointRect);

                        if (newJoint != null)
                            OnDropAction(newJoint, jointType);

                        if (HandleClick(keyboardID, jointPointRect))
                            OnSelectedAction(jointType);

                        if (HandleDelete(keyboardID))
                            OnDropAction(default, jointType);
                    }
                }
            }

            GUIContent gUIContent = new GUIContent("Deselect", EditorGUIUtility.IconContent("AvatarInspector/DotSelection").image);

            EditorGUI.BeginDisabledGroup(SelectedJoint == JointType.None);

            if (GUILayout.Button(gUIContent))
                OnSelectedAction(JointType.None);

            EditorGUI.EndDisabledGroup();
        }

        T HandleDragDrop(int controlID, Rect dropRect)
        {
            EventType eventType = Event.current.type;

            T dropObject = default;

            if ((eventType == EventType.DragPerform || eventType == EventType.DragUpdated) &&
                dropRect.Contains(Event.current.mousePosition) && GUI.enabled)
            {
                Object[] references = DragAndDrop.objectReferences;
                Object validatedObject = references.Length == 1 ? references[0] : null;

                T validObj = GetValidType(validatedObject);

                if (validObj != null && !validObj.Equals(default(T)))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

                    if (eventType == EventType.DragPerform)
                    {
                        dropObject = validObj;
                        GUIUtility.keyboardControl = controlID;
                    }
                    GUI.changed = true;
                    DragAndDrop.AcceptDrag();
                    DragAndDrop.activeControlID = 0;
                }
            }

            return dropObject;
        }

        T GetValidType(Object validatedObject)
        {
            if (EditorUtility.IsPersistent(validatedObject))
                return default;

            if (validatedObject is T t)
                return t;
            else if (validatedObject is GameObject go)
                return go.GetComponent<T>();

            return default;
        }
    }
}

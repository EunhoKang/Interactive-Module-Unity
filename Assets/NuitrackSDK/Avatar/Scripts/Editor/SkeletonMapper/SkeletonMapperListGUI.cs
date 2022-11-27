using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using NuitrackSDK;
using nuitrack;


namespace NuitrackSDKEditor.Avatar
{
    /// <summary>
    /// Skeleton mapper bones list, similar to the map in Avatar Configuration
    /// </summary>
    /// <typeparam name="T">Type of bone object (usually used by Transform)</typeparam>
    public class SkeletonMapperListGUI<T> : SkeletonMapper<T> where T : Object
    {
        bool foldOpenned = false;
        readonly Dictionary<JointType, int> controlsID = new Dictionary<JointType, int>();

        public override JointType SelectedJoint
        {
            get => base.SelectedJoint;
            set
            {
                base.SelectedJoint = value;

                if (controlsID.ContainsKey(value))
                    GUIUtility.keyboardControl = controlsID[value];
            }
        }

        readonly List<JointType> jointMask = null;
        readonly List<JointType> optionalJoints = null;

        /// <summary>
        /// View of the list of joints.
        /// </summary>
        /// <param name="jointMask">The mask of the displayed joints. If null, all available joints will be drawn.</param>
        public SkeletonMapperListGUI(List<JointType> jointMask, List<JointType> optionalJoints = null)
        {
            this.jointMask = jointMask ?? new List<JointType>();
            this.optionalJoints = optionalJoints ?? new List<JointType>();
        }

        /// <summary>
        /// Draw a list of joints
        /// </summary>
        /// <param name="jointsDict">Dictionary of joints and joint targets</param>
        public void Draw(Dictionary<JointType, T> jointsDict)
        {
            controlsID.Clear();

            foldOpenned = EditorGUILayout.BeginFoldoutHeaderGroup(foldOpenned, "Avatar bones list");

            if (foldOpenned)
            {
                foreach (KeyValuePair<AvatarMaskBodyPart, SkeletonStyles.GUIBodyPart> bodyPartItem in SkeletonStyles.BodyParts)
                {
                    AvatarMaskBodyPart bodyPart = bodyPartItem.Key;
                    SkeletonStyles.GUIBodyPart guiBodyPart = bodyPartItem.Value;

                    EditorGUILayout.LabelField(guiBodyPart.Lable, EditorStyles.boldLabel);

                    using (new VerticalGroup(EditorStyles.helpBox))
                    {
                        foreach (SkeletonStyles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                        {
                            JointType jointType = guiJoint.JointType;

                            if (jointMask.Contains(jointType))
                            {
                                T jointItem = jointsDict.ContainsKey(jointType) ? jointsDict[jointType] : null;

                                Rect controlRect = EditorGUILayout.GetControlRect();
                                Vector2 position = new Vector2(controlRect.x, controlRect.y);

                                Rect jointRect = SkeletonStyles.Dot.Draw(position, optionalJoints.Contains(guiJoint.JointType), jointItem != null, jointType == SelectedJoint);
                                controlRect.xMin += jointRect.width;

                                string displayName = NuitrackSDKGUI.GetUnityDisplayBoneName(jointType.ToUnityBones(), bodyPart);

                                T newJointObject = EditorGUI.ObjectField(controlRect, displayName, jointItem, typeof(T), true) as T;

                                int keyboardID = GUIUtility.GetControlID(FocusType.Keyboard, controlRect);
                                controlsID.Add(jointType, keyboardID);

                                if (newJointObject != jointItem)
                                    OnDropAction(newJointObject, jointType);

                                if (HandleClick(keyboardID, controlRect))
                                    OnSelectedAction(jointType);

                                if (HandleDelete(keyboardID))
                                    OnDropAction(default, jointType);
                            }
                        }
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
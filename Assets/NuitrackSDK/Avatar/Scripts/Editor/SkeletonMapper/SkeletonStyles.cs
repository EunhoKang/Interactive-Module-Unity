using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using nuitrack;


namespace NuitrackSDKEditor.Avatar
{
    public static class SkeletonStyles
    {
        public class GUIBodyPart
        {
            public string labelKey;

            public List<GUIJoint> guiJoint;
            public List<GUIContent> guiContents;

            public string Lable
            {
                get
                {
                    return EditorGUIUtility.TrTextContent(labelKey).text;
                }
            }
        }

        public class GUIJoint
        {
            public JointType JointType { get; private set; }

            public Vector2 MapPosition { get; private set; }

            public GUIJoint(JointType jointType, Vector2 mapPosition)
            {
                JointType = jointType;
                MapPosition = mapPosition;
            }
        }

        public static class Dot
        {
            static Color color = new Color(0.7f, 0.7f, 0.7f, 1);

            static readonly GUIContent fill = EditorGUIUtility.IconContent("AvatarInspector/DotFill");
            static readonly GUIContent frame = EditorGUIUtility.IconContent("AvatarInspector/DotFrame");
            static readonly GUIContent frameDotted = EditorGUIUtility.IconContent("AvatarInspector/DotFrameDotted");
            static readonly GUIContent selection = EditorGUIUtility.IconContent("AvatarInspector/DotSelection");

            /// <summary>
            /// Draw a centered dot representing the GUI element for the joint
            /// </summary>
            /// <param name="position">Centered position</param>
            /// <param name="optional">If optional, the point will be drawn as a dotted frame, otherwise it will be solid</param>
            /// <param name="filled">If filled in, a placeholder will be added inside the point</param>
            /// <param name="selected">If selected, a contour will be added around the dot</param>
            /// <returns>Rectangle describing a dot</returns>
            public static Rect DrawСentered(Vector2 position, bool optional, bool filled = false, bool selected = false)
            {
                Texture dotTexture = (optional ? frameDotted : frame).image;
                Vector2 newPosition = new Vector2(position.x - dotTexture.width * 0.5f, position.y - dotTexture.height * 0.5f);

                return Draw(newPosition, optional, filled, selected);
            }

            /// <summary>
            /// Draw a dot representing the GUI element for the joint
            /// </summary>
            /// <param name="position">Centered position</param>
            /// <param name="optional">If optional, the point will be drawn as a dotted frame, otherwise it will be solid</param>
            /// <param name="filled">If filled in, a placeholder will be added inside the point</param>
            /// <param name="selected">If selected, a contour will be added around the dot</param>
            /// <returns>Rectangle describing a dot</returns>
            public static Rect Draw(Vector2 position, bool optional, bool filled = false, bool selected = false)
            {
                Texture dotTexture = (optional ? frameDotted : frame).image;

                Rect rect = new Rect(position.x, position.y, dotTexture.width, dotTexture.height);

                using (new GUIColor(color))
                    GUI.DrawTexture(rect, dotTexture);

                if (filled)
                    GUI.DrawTexture(rect, fill.image);

                if (selected)
                    GUI.DrawTexture(rect, selection.image);

                return rect;
            }
        }

        public static readonly GUIContent UnityDude = EditorGUIUtility.IconContent("AvatarInspector/BodySIlhouette");

        public static Dictionary<GUIContent, Vector2> Labels = new Dictionary<GUIContent, Vector2>()
        {
            { new GUIContent("Left side"), new Vector2(0.6f, 0.0f) },
            { new GUIContent("Right side"), new Vector2(-0.6f, 0.0f) }
        };

        public static readonly Dictionary<AvatarMaskBodyPart, GUIBodyPart> BodyParts = new Dictionary<AvatarMaskBodyPart, GUIBodyPart>()
            {
                {
                    AvatarMaskBodyPart.Body, new GUIBodyPart()
                    {
                        labelKey = "Body",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(JointType.Waist, new Vector2(0.00f, 0.08f)),
                            new GUIJoint(JointType.Torso, new Vector2(0.00f, 0.35f)),

                            new GUIJoint(JointType.LeftCollar, new Vector2(0.12f, 0.60f)),
                            new GUIJoint(JointType.RightCollar, new Vector2(-0.12f, 0.60f)),
                        },

                        guiContents = new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/Torso") }
                    }
                    
                    //{ HumanBodyBones.Spine, new Vector2(0.00f, 0.20f) },
                    //{ HumanBodyBones.UpperChest, new Vector2(0.00f, 0.50f) },
                },

                {
                    AvatarMaskBodyPart.Head, new GUIBodyPart()
                    {
                        labelKey = "Head",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(JointType.Neck, new Vector2(0.00f, 0.7f)),
                            new GUIJoint(JointType.Head, new Vector2(0.00f, 0.80f))
                        },

                        guiContents = new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/Head") }
                    }
                },

                {
                    AvatarMaskBodyPart.LeftLeg, new GUIBodyPart()
                    {
                        labelKey = "Left Leg",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(JointType.LeftHip, new Vector2(0.16f, 0.01f)),
                            new GUIJoint(JointType.LeftKnee, new Vector2(0.21f, -0.40f)),
                            new GUIJoint(JointType.LeftAnkle, new Vector2(0.23f, -0.80f)),
                            //new GUIJoint(JointType.LeftFoot, new Vector2(0.25f, -0.89f)),
                        },

                        guiContents = new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/LeftLeg") }
                    }

                },

                {
                    AvatarMaskBodyPart.RightLeg, new GUIBodyPart()
                    {
                        labelKey = "Right Leg",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(JointType.RightHip, new Vector2(-0.16f, 0.01f)),
                            new GUIJoint(JointType.RightKnee,  new Vector2(-0.21f, -0.40f)),
                            new GUIJoint(JointType.RightAnkle, new Vector2(-0.23f, -0.80f)),
                            //new GUIJoint(JointType.RightFoot, new Vector2(-0.25f, -0.89f))
                        },

                        guiContents = new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/RightLeg") }
                    }
                },

                {
                    AvatarMaskBodyPart.LeftArm, new GUIBodyPart()
                    {
                        labelKey = "Left Arm",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(JointType.LeftShoulder, new Vector2(0.30f, 0.57f)),
                            new GUIJoint(JointType.LeftElbow, new Vector2(0.48f, 0.30f)),
                            new GUIJoint(JointType.LeftWrist, new Vector2(0.66f, 0.03f))
                        },

                        guiContents = new List<GUIContent>()
                        {
                            EditorGUIUtility.IconContent("AvatarInspector/LeftArm"),
                            EditorGUIUtility.IconContent("AvatarInspector/LeftFingers")
                        }
                    }
                },

                {
                    AvatarMaskBodyPart.RightArm, new GUIBodyPart()
                    {
                        labelKey = "Right Arm",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(JointType.RightShoulder, new Vector2(-0.30f, 0.57f)),
                            new GUIJoint(JointType.RightElbow, new Vector2(-0.48f, 0.30f)),
                            new GUIJoint(JointType.RightWrist, new Vector2(-0.66f, 0.03f))
                        },

                        guiContents = new List<GUIContent>()
                        {
                            EditorGUIUtility.IconContent("AvatarInspector/RightArm"),
                            EditorGUIUtility.IconContent("AvatarInspector/RightFingers")
                        }
                    }
                }
            };
    }
}
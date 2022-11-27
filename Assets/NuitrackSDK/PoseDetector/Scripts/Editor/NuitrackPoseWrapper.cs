using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using NuitrackSDK.Poses;


namespace NuitrackSDKEditor.Poses
{
    public class NuitrackPoseWrapper : IEnumerable
    {
        readonly SerializedObject serializedObject;

        readonly static Dictionary<nuitrack.JointType, string> jointMapFields = new Dictionary<nuitrack.JointType, string>()
        {
            { nuitrack.JointType.Waist, "waist" },

            { nuitrack.JointType.LeftShoulder, "leftShoulder" },
            { nuitrack.JointType.LeftElbow, "leftElbow" },

            { nuitrack.JointType.RightShoulder, "rightShoulder" },
            { nuitrack.JointType.RightElbow, "rightElbow" },

            { nuitrack.JointType.LeftHip, "leftHip" },
            { nuitrack.JointType.LeftKnee, "leftKnee" },

            { nuitrack.JointType.RightHip, "rightHip" },
            { nuitrack.JointType.RightKnee, "rightKnee" }
        };

        public static List<nuitrack.JointType> JointsMask
        {
            get
            {
                return new List<nuitrack.JointType>(jointMapFields.Keys);
            }
        }

        public class JointWrapper
        {
            readonly SerializedProperty jointProperty;
            readonly SerializedObject serializedObject;

            public SerializedProperty IsActiveProperty
            {
                get
                {
                    return jointProperty.FindPropertyRelative("isActive");
                }
            }

            public bool IsActive
            {
                get
                {
                    return IsActiveProperty.boolValue;
                }
                set
                {
                    IsActiveProperty.boolValue = value;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            public SerializedProperty OrientationProperty
            {
                get
                {
                    return jointProperty.FindPropertyRelative("orientation");
                }
            }

            public Quaternion Orientation
            {
                get
                {
                    return OrientationProperty.quaternionValue;
                }
                set
                {
                    if (!Mathf.Approximately(Quaternion.Dot(OrientationProperty.quaternionValue, value), 1))
                    {
                        OrientationProperty.quaternionValue = value;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            public SerializedProperty ToleranceProperty
            {
                get
                {
                    return jointProperty.FindPropertyRelative("tolerance");
                }
            }

            public float Tolerance
            {
                get
                {
                    return ToleranceProperty.floatValue;
                }
                set
                {
                    value = Mathf.Clamp(value, 0.1f, 0.99f);

                    if (!Mathf.Approximately(ToleranceProperty.floatValue, value))
                    {
                        ToleranceProperty.floatValue = value;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            public JointWrapper(SerializedProperty jointProperty, SerializedObject serializedObject)
            {
                this.jointProperty = jointProperty;
                this.serializedObject = serializedObject;
            }

            public void CopyFrom(JointWrapper sourceJointWrapper)
            {
                IsActive = sourceJointWrapper.IsActive;
                Orientation = sourceJointWrapper.Orientation;
                Tolerance = sourceJointWrapper.Tolerance;
            }

            public void CopyFrom(NuitrackPose.JointReference sourceJointRef)
            {
                IsActive = sourceJointRef.IsActive;
                Orientation = sourceJointRef.Orientation;
                Tolerance = sourceJointRef.Tolerance;
            }
        }

        public SerializedProperty RelativeModeProperty
        {
            get
            {
                return serializedObject.FindProperty("relativeMode");
            }
        }

        public bool RelativeMode
        {
            get
            {
                return RelativeModeProperty.boolValue;
            }
            set
            {
                RelativeModeProperty.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public JointWrapper this[nuitrack.JointType jointType]
        {
            get
            {
                return new JointWrapper(serializedObject.FindProperty(jointMapFields[jointType]), serializedObject);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return jointMapFields.Keys.GetEnumerator();
        }

        public NuitrackPoseWrapper(SerializedObject serializedObject)
        {
            this.serializedObject = serializedObject;
        }

        public void CopyFrom(NuitrackPoseWrapper sourcePose)
        {
            RelativeMode = sourcePose.RelativeMode;

            foreach (nuitrack.JointType jointType in this)
                this[jointType].CopyFrom(sourcePose[jointType]);

            serializedObject.ApplyModifiedProperties();
        }

        public void CopyFrom(NuitrackPose sourcePose)
        {
            RelativeMode = sourcePose.RelativeMode;

            foreach (nuitrack.JointType jointType in this)
                this[jointType].CopyFrom(sourcePose[jointType]);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
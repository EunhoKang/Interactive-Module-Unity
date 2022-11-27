using UnityEngine;
using UnityEditor;

using NuitrackSDK.Poses;


namespace NuitrackSDKEditor.Poses
{
    [CustomPropertyDrawer(typeof(NuitrackPose))]
    public class NuitrackPosePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
                position = NuitrackSDKGUI.WithRightButton(position, delegate { CreateNewPose(property); }, "Toolbar Plus", "Create a new pose and add it to the list");
            else
                position = NuitrackSDKGUI.WithRightButton(position, delegate { ViewPose(property); }, "animationvisibilitytoggleon", "View pose");

            EditorGUI.PropertyField(position, property, label);
        }

        void ViewPose(SerializedProperty poseProperty)
        {
            if (poseProperty.objectReferenceValue != null)
                Selection.activeObject = poseProperty.objectReferenceValue;
        }

        void CreateNewPose(SerializedProperty poseProperty)
        {
            poseProperty.objectReferenceValue = NuitrackEditorHelper.CreateAsset<NuitrackPose>("Pose ", NuitrackPoseEditor.SavePosePath);
            poseProperty.serializedObject.ApplyModifiedProperties();

            GUIUtility.ExitGUI();
        }
    }
}
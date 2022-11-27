using UnityEditor;

using NuitrackSDK.Frame;
using NuitrackSDK;
using UnityEngine;


namespace NuitrackSDKEditor.Frame
{
    [CustomEditor(typeof(FrameProvider), true)]
    public class FrameProviderEditor : NuitrackSDKEditor
    {
        public override void OnInspectorGUI()
        {
            NuitrackSDKGUI.MessageIfNuitrackNotExist();

            SerializedProperty frameTypeProp = serializedObject.DrawPropertyField("frameType", "Frame type");

            switch (frameTypeProp.enumValueIndex)
            {
                case 1:

                    EditorGUILayout.Space();
                    DrawDepthOptions();
                    EditorGUILayout.Space();
                    break;

                case 2:

                    EditorGUILayout.Space();
                    DrawSegmentOptions();
                    EditorGUILayout.Space();
                    break;
            }

            serializedObject.DrawPropertyField(
                "textureMode", "Texture mode",
                "Recommended mode: Texture (as fast as possible, but for manual processing on the CPU, depending on the platform, you may need to transfer from the GPU)");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            serializedObject.DrawPropertyField("onFrameUpdate", "On frame update event");

            DrawDefaultInspector();
        }

        void DrawSegmentOptions()
        {
            using (new VerticalGroup(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Segment options", EditorStyles.boldLabel);

                SerializedProperty segmentModeProp = serializedObject.DrawPropertyField(
                    "segmentMode", "Segment mode",
                    "Display segments of all users or only single(current user or specified by ID)");

                EditorGUILayout.Space();

                if (segmentModeProp.enumValueIndex == (int)FrameProvider.SegmentMode.All)
                {
                    SerializedProperty useCustomUsersColorsProp = serializedObject.DrawPropertyField(
                        "useCustomUsersColors",
                        "Use custom users colors",
                        "If not specified, the default user colors from SegmentToTexture is used");

                    if (useCustomUsersColorsProp.boolValue)
                    {
                        GUIStyle contentStyle = new GUIStyle(GUIStyle.none)
                        {
                            padding = new RectOffset(15, 5, 0, 0)
                        };

                        using (new VerticalGroup(contentStyle))
                            serializedObject.DrawPropertyField("customUsersColors", "Custom users colors");
                    }
                }
                else
                {
                    serializedObject.DrawPropertyField("userColor", "User color");

                    SerializedProperty useCurrentUserTrackerProp = serializedObject.DrawPropertyField("useCurrentUserTracker", "Use current user tracker");

                    if (!useCurrentUserTrackerProp.boolValue)
                    {
                        SerializedProperty userID = serializedObject.FindProperty("userID");
                        userID.intValue = EditorGUILayout.IntSlider("User ID", userID.intValue, Users.MinID, Users.MaxID);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        void DrawDepthOptions()
        {
            using (new VerticalGroup(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Depth options", EditorStyles.boldLabel);

                SerializedProperty useCustomDepthGradientProp = serializedObject.DrawPropertyField(
                    "useCustomDepthGradient",
                    "Use custom depth gradient",
                    "If not specified, the default gradient from DepthToTexture is used");

                if (useCustomDepthGradientProp.boolValue)
                    serializedObject.DrawPropertyField("customDepthGradient", "Custom depth gradient");
            }
        }
    }
}
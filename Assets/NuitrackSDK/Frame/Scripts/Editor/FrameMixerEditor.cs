using UnityEditor;

using NuitrackSDK.Frame;


namespace NuitrackSDKEditor.Frame
{
    [CustomEditor(typeof(FrameMixer), true)]
    public class FrameMixerEditor : NuitrackSDKEditor
    {
        public override void OnInspectorGUI()
        {
            SerializedProperty useStaticMainTextureProp = serializedObject.DrawPropertyField(
                "useStaticMainTexture",
                "Use static main texture",
                "Specify a static texture to perform the transformation on it. Otherwise, use MainTexture");

            if (useStaticMainTextureProp.boolValue)
            {
                serializedObject.DrawPropertyField("staticMainTexture", "Main texture");
                EditorGUILayout.Space();
            }

            SerializedProperty useStaticMaskTextureProp = serializedObject.DrawPropertyField(
                "useStaticMaskTexture",
                "Use static mask texture",
                "Specify the static texture of the mask to perform the conversion with it. Otherwise, use MaskTexture");

            if (useStaticMaskTextureProp.boolValue)
            {
                serializedObject.DrawPropertyField("staticMaskTexture", "Mask texture");
                EditorGUILayout.Space();
            }

            DrawDefaultInspector();
        }
    }
}
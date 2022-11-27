using UnityEngine;
using UnityEditor;

using NuitrackSDK.Calibration;


namespace NuitrackSDKEditor.Calibration
{
    [CustomEditor(typeof(CalibrationHandler), true)]
    public class CalibrationHandlerEditor : NuitrackSDKEditor
    {
        bool Open
        {
            get
            {
                return EditorPrefs.GetBool(string.Format("{0}_OpenEvents", nameof(CalibrationHandlerEditor)), false);
            }
            set
            {
                EditorPrefs.SetBool(string.Format("{0}_OpenEvents", nameof(CalibrationHandlerEditor)), value);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            Open = EditorGUILayout.BeginFoldoutHeaderGroup(Open, "Events");

            if (Open)
            {
                serializedObject.DrawPropertyField("onStartEvent");
                serializedObject.DrawPropertyField("onProgressEvent");
                serializedObject.DrawPropertyField("onFailEvent");
                serializedObject.DrawPropertyField("onSuccessEvent");
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
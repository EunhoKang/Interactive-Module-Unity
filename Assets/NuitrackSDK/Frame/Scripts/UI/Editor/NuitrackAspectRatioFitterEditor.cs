using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using UnityEditor;
using UnityEditor.UI;

using NuitrackSDK.Frame;


namespace NuitrackSDKEditor.Frame
{
    [CustomEditor(typeof(NuitrackAspectRatioFitter), true)]
    public class NuitrackAspectRatioFitterEditor : AspectRatioFitterEditor
    {
        AspectRatioFitter.AspectMode AspectMode
        {
            get
            {
                return (AspectRatioFitter.AspectMode)serializedObject.FindProperty("m_AspectMode").enumValueIndex;
            }
            set
            {
                serializedObject.FindProperty("m_AspectMode").enumValueIndex = (int)value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            NuitrackSDKGUI.MessageIfNuitrackNotExist();

            serializedObject.DrawPropertyField("frameMode", "Frame mode");

            EditorGUILayout.Space();

            base.OnInspectorGUI();

            if (AspectMode != AspectRatioFitter.AspectMode.FitInParent)
            {
                UnityAction fixAspectMode = delegate { AspectMode = AspectRatioFitter.AspectMode.FitInParent; };

                string message = string.Format("Aspect Mode is set to {0}." +
                    "The frame from the sensor may not be displayed correctly." +
                    "\nRecommended: Fit In Parent.",
                    AspectMode);

                NuitrackSDKGUI.DrawMessage(message, LogType.Warning, fixAspectMode, "Fix");
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

using NuitrackSDK.SensorEnvironment;


namespace NuitrackSDKEditor.SensorEnvironment
{
    [CustomEditor(typeof(SensorSpace), true)]
    public class SensorSpace_Editor : NuitrackSDKEditor
    {
        Transform transform
        {
            get

            {
                return (serializedObject.targetObject as SensorSpace).transform;
            }
        }

        public override void OnInspectorGUI()
        {
            NuitrackSDKGUI.MessageIfNuitrackNotExist();

            SerializedProperty cameraFovAlign = serializedObject.DrawPropertyField("cameraFovAlign", "Camera fov align");

            if (cameraFovAlign.boolValue)
            {
                Canvas canvas = serializedObject.FindProperty("viewCanvas").objectReferenceValue as Canvas;

                serializedObject.DrawPropertyField("viewCanvas", "View canvas");

                if (canvas != null)
                {
                    if (canvas.renderMode == RenderMode.WorldSpace || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        string message = string.Format("The canvas rendering mode is specified: {0}.\nIt is recommended to use: {1}",
                           canvas.renderMode, RenderMode.ScreenSpaceCamera);

                        string buttonLabel = string.Format("Switch to {0}", RenderMode.ScreenSpaceCamera);

                        UnityAction fixAction = delegate { FixCanvasRenderMode(); };

                        NuitrackSDKGUI.DrawMessage(message, LogType.Warning, fixAction, buttonLabel);
                    }
                }
                else
                {
                    string message = "View Canvas is not set. The screen size will be used to align the camera's fov.";
                    NuitrackSDKGUI.DrawMessage(message, LogType.Log);
                }
            }

            SerializedProperty floorTracking = serializedObject.DrawPropertyField("floorTracking", "Floor tracking");

            if (floorTracking.boolValue)
            {
                SerializedProperty sensorSpaceProp = serializedObject.DrawPropertyField(
                    "sensorSpace", "Sensor space transform",
                    "Used as a reference point of the space for positioning the sensor");

                if (sensorSpaceProp.objectReferenceValue == null)
                {
                    UnityAction fixAction = null;
                    string fixMessage = null;

                    if (transform.parent == null)
                    {
                        fixAction = delegate { FixCreateSensorSpaceTransform(); };
                        fixMessage = "Create and set Sensor space transform";
                    }
                    else
                    {
                        fixAction = delegate { FixSensorSpaceTransform(); };
                        fixMessage = string.Format("Set \"{0}\" as Sensor space transform", transform.parent.name);
                    }

                    NuitrackSDKGUI.DrawMessage(
                        "It is necessary to set Sensor space transform. It is most convenient to use the parent Transform.",
                        LogType.Error, fixAction, fixMessage);
                }

                serializedObject.DrawPropertyField("deltaHeight", "Delta height");
                serializedObject.DrawPropertyField("deltaAngle", "Delta angle");
                serializedObject.DrawPropertyField("floorCorrectionSpeed", "Floor correction speed");
            }
        }

        void FixSensorSpaceTransform()
        {
            serializedObject.FindProperty("sensorSpace").objectReferenceValue = transform.parent;
            serializedObject.ApplyModifiedProperties();
        }

        void FixCreateSensorSpaceTransform()
        {
            GameObject sensorSpace = new GameObject("SensorSpace");

            sensorSpace.transform.position = transform.position;
            sensorSpace.transform.rotation = transform.rotation;

            transform.SetParent(sensorSpace.transform);

            serializedObject.FindProperty("sensorSpace").objectReferenceValue = sensorSpace.transform;
            serializedObject.ApplyModifiedProperties();
        }

        void FixCanvasRenderMode()
        {
            Canvas canvas = serializedObject.FindProperty("viewCanvas").objectReferenceValue as Canvas;

            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;

                canvas.worldCamera = transform.GetComponent<Camera>();
                canvas.planeDistance = 15;
            }
        }
    }
}

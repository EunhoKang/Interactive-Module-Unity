using UnityEngine;

using NuitrackSDK.Frame;


namespace NuitrackSDK.SensorEnvironment
{
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu("NuitrackSDK/SensorEnvironment/Sensor Space")]
    public class SensorSpace : MonoBehaviour
    {
        new Camera camera;

        [Header("Camera")]
        [SerializeField] bool cameraFovAlign = true;

        [Tooltip ("(optional) If not specified, the screen size is used.")]

        [SerializeField] Canvas viewCanvas;
        RectTransform canvasRect;

        ulong lastTimeStamp = 0;

        [Header("Floor")]
        [SerializeField] bool floorTracking = false;

        [SerializeField] Transform sensorSpace;

        [SerializeField, Range(0.001f, 1f)] float deltaHeight = 0.1f;
        [SerializeField, Range(0.1f, 90f)] float deltaAngle = 3f;
        [SerializeField, Range(0.1f, 32f)] float floorCorrectionSpeed = 8f;

        Plane floorPlane;

        Vector3 localCameraPosition;

        public Camera Camera
        {
            get
            {
                if (camera == null)
                    camera = GetComponent<Camera>();

                return camera;
            }
        }

        public float HeightToFloor
        {
            get
            {
                if (NuitrackManager.Floor == null)
                    return 0;

                Plane floorPlane = (Plane)NuitrackManager.Floor;

                return floorPlane.GetDistanceToPoint(Vector3.zero);
            }
    }

        RectTransform CanvasRect
        {
            get
            {
                if (viewCanvas != null && canvasRect == null)
                    canvasRect = viewCanvas.GetComponent<RectTransform>();
                
                return canvasRect;
            }
        }

        void Update()
        {
            if (cameraFovAlign)
            {
                if (NuitrackManager.DepthFrame == null || NuitrackManager.DepthFrame.Timestamp == lastTimeStamp)
                    return;

                lastTimeStamp = NuitrackManager.DepthFrame.Timestamp;

                NuitrackManager_onColorUpdate();
            }

            if (floorTracking)
                UpdateFloor();
        }

        float ViewWidth
        {
            get
            {
                if (CanvasRect != null)
                    return CanvasRect.rect.width;
                else
                    return Screen.width;
            }
        }

        float ViewHeight
        {
            get
            {
                if (CanvasRect != null)
                    return CanvasRect.rect.height;
                else
                    return Screen.height;
            }
        }

        void NuitrackManager_onColorUpdate()
        {
            if (ViewWidth / ViewHeight > FrameUtils.DepthToTexture.AspectRatio)
                Camera.fieldOfView = FrameUtils.DepthToTexture.VFOV;
            else
            {
                nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();

                float targetAspectRatio = ViewHeight / ViewWidth;
                float vFOV = 2 * Mathf.Atan(Mathf.Tan(mode.HFOV * 0.5f) * targetAspectRatio) * Mathf.Rad2Deg;

                Camera.fieldOfView = vFOV;
            }
        }

        void UpdateFloor()
        {
            if (NuitrackManager.Floor == null)
                return;

            Plane newFloor = (Plane)NuitrackManager.Floor;

            if (Mathf.Approximately(newFloor.normal.sqrMagnitude, 0))
                return;

            if (floorPlane.Equals(default(Plane)))
                floorPlane = newFloor;

            float newFloorHeight = newFloor.GetDistanceToPoint(Vector3.zero);
            float floorHeight = floorPlane.GetDistanceToPoint(Vector3.zero);

            if (Vector3.Angle(newFloor.normal, floorPlane.normal) >= deltaAngle || Mathf.Abs(newFloorHeight - floorHeight) >= deltaHeight)
                floorPlane = newFloor;

            Vector3 reflectNormal = sensorSpace.TransformDirection(Vector3.Reflect(-floorPlane.normal, Vector3.up));
            Vector3 forward = sensorSpace.forward;
            Vector3.OrthoNormalize(ref reflectNormal, ref forward);

            Quaternion targetRotation = Quaternion.LookRotation(forward, reflectNormal);
            camera.transform.rotation = Quaternion.Lerp(camera.transform.rotation, targetRotation, Time.deltaTime * floorCorrectionSpeed);

            if (localCameraPosition.Equals(default))
            {
                localCameraPosition = Vector3.up * floorHeight;
                camera.transform.position = sensorSpace.TransformPoint(localCameraPosition);
            }
            else
            {
                localCameraPosition = Vector3.up * floorHeight;
                Vector3 worldCameraPosition = sensorSpace.TransformPoint(localCameraPosition);
                camera.transform.position = Vector3.Lerp(camera.transform.position, worldCameraPosition, Time.deltaTime * floorCorrectionSpeed);
            }
        }
    }
}
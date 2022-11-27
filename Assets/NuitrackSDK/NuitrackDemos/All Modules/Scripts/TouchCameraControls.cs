using UnityEngine;

namespace NuitrackSDK.NuitrackDemos
{
    public class TouchCameraControls : MonoBehaviour
    {
        [SerializeField] Transform targetCamera;
        [SerializeField] float minCameraDistance = 1f;
        [SerializeField] float maxCameraDistance = 10f;

        [SerializeField] Renderer pivotRenderer;
        [SerializeField] bool showPivot = true;

        [SerializeField] float speed = 0.05f;

        Vector3 minPivotPos = new Vector3(-2.5f, -2.5f, 0f);
        Vector3 maxPivotPos = new Vector3(2.5f, 2.5f, 5f);

        float xAngle = 0f, yAngle = 0f;

        float cameraDistance;
        bool had2touches = false;

        Vector2 mid2touches = Vector2.zero;

        //initial position camera info:

        Vector3 pivotStartPos;
        Quaternion pivotStartRot;
        float cameraStartDistance;

        Vector3 lastMousePositon;

        void Start()
        {
            pivotRenderer.enabled = false;
            cameraDistance = targetCamera.localPosition.z;

            pivotStartPos = transform.position;
            pivotStartRot = transform.rotation;
            cameraStartDistance = targetCamera.localPosition.z;
        }

        float prev2touchesDistance;

        [SerializeField] float doubleTapInterval = 0.1f;
        int taps = 0;
        float tapTimer = 0f;

        void UpdateTapCounter()
        {
            tapTimer += Time.deltaTime;
            if (tapTimer > doubleTapInterval)
            {
                taps = 0;
                tapTimer = 0f;
            }

            if (Input.touchCount == 1)
            {
                if (Input.touches[0].phase == TouchPhase.Began || Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Debug.Log("Touch began, taps: " + taps.ToString());
                    taps++;
                    tapTimer = 0f;
                }
            }
        }

        void ResetCamera()
        {
            xAngle = 0f;
            yAngle = 0f;
            cameraDistance = cameraStartDistance;
            transform.position = pivotStartPos;
            transform.rotation = pivotStartRot;
            targetCamera.localPosition = new Vector3(targetCamera.localPosition.x, targetCamera.localPosition.y, cameraStartDistance);
        }

        void Update()
        {
            UpdateTapCounter();
            bool doubleTapped = (taps == 2);

            if (doubleTapped) ResetCamera();

            if (showPivot) pivotRenderer.enabled = (Input.touchCount > 0) && (Input.touchCount < 3);

            if (Input.GetKey(KeyCode.W))
                targetCamera.localPosition += new Vector3(0, speed, 0);

            if (Input.GetKey(KeyCode.S))
                targetCamera.localPosition += new Vector3(0, -speed, 0);

            if (Input.GetKey(KeyCode.A))
                targetCamera.localPosition += new Vector3(-speed, 0, 0);

            if (Input.GetKey(KeyCode.D))
                targetCamera.localPosition += new Vector3(speed, 0, 0);

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                transform.rotation = Quaternion.identity;
                targetCamera.localPosition = new Vector3(0, 0, cameraStartDistance);
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                lastMousePositon = Input.mousePosition;
            }

            if (Input.touchCount == 1 || Input.GetKey(KeyCode.Mouse0)) // rotations
            {
                had2touches = false;
                if (Application.platform == RuntimePlatform.Android)
                {
                    xAngle -= Input.touches[0].deltaPosition.y;
                    yAngle += Input.touches[0].deltaPosition.x;
                }
                else
                {
                    Vector2 deltaPositon = Input.mousePosition - lastMousePositon;

                    lastMousePositon = Input.mousePosition;
                    xAngle -= deltaPositon.y;
                    yAngle += deltaPositon.x;
                }

                xAngle = Mathf.Clamp(xAngle, -90f, 90f);
                while (yAngle > 360f)
                {
                    yAngle -= 360f;
                }
                while (yAngle < -360f)
                {
                    yAngle += 360f;
                }

                transform.rotation = Quaternion.Euler(xAngle, yAngle, 0f);
            }
            else if (Input.touchCount == 2 || Input.mouseScrollDelta.y != 0) //scale + translation of pivot
            {
                if (!had2touches && Input.mouseScrollDelta.y == 0)
                {
                    prev2touchesDistance = (Input.touches[0].position - Input.touches[1].position).magnitude;
                    had2touches = true;

                    //translation of pivot point part:
                    mid2touches = 0.5f * (Input.touches[0].position + Input.touches[1].position);
                }
                else
                {
                    if (Input.mouseScrollDelta.y != 0)
                    {
                        targetCamera.localPosition += new Vector3(0, 0, Input.mouseScrollDelta.y);
                    }
                    else
                    {
                        float current2touchesDistance = (Input.touches[0].position - Input.touches[1].position).magnitude;
                        cameraDistance *= prev2touchesDistance / current2touchesDistance;
                        cameraDistance = Mathf.Clamp(cameraDistance, -maxCameraDistance, -minCameraDistance);
                        targetCamera.localPosition = new Vector3(0f, 0f, cameraDistance);
                        prev2touchesDistance = current2touchesDistance;

                        //translation of pivot point part:
                        Vector2 newMid2touches = 0.5f * (Input.touches[0].position + Input.touches[1].position);
                        Vector2 midDiff = newMid2touches - mid2touches;
                        mid2touches = newMid2touches;

                        transform.position = transform.position + (-0.005f * midDiff.x) * targetCamera.right + (-0.005f * midDiff.y) * targetCamera.up;
                        transform.position = new Vector3(
                          Mathf.Clamp(transform.position.x, minPivotPos.x, maxPivotPos.x),
                          Mathf.Clamp(transform.position.y, minPivotPos.y, maxPivotPos.y),
                          Mathf.Clamp(transform.position.z, minPivotPos.z, maxPivotPos.z));
                    }
                }
            }
            else
            {
                had2touches = false;
            }
        }
    }
}
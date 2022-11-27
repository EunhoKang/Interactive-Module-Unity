using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;

using System.Collections.Generic;


namespace NuitrackSDK.Tutorials.HandTracker
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Hand Tracker/Image Item")]
    public class ImageItem : Button, IDragHandler
    {
        List<PointerEventData> touches = new List<PointerEventData>();

        Vector3 deltaRectPosition;

        [SerializeField]
        [Range(0.1f, 10)]
        float minScale = 0.5f;

        [SerializeField]
        [Range(0.1f, 10)]
        float maxScale = 5;

        bool viewMode = false;

        public void EnterViewMode()
        {
            if (!viewMode)
            {
                viewMode = true;
                InstantClearState();
            }
        }

        public void ExitViewMode()
        {
            viewMode = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!touches.Contains(eventData))
            {
                touches.Add(eventData);
                UpdateInitialState();
            }

            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            touches.Remove(eventData);
            base.OnPointerUp(eventData);
            UpdateInitialState();
            InstantClearState();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!viewMode)
                return;

            if (OneTouch)
            {
                Vector3 firstPoint = GetWorldPointPosition(touches[0]);
                Vector3 localPointPosition = Rect.InverseTransformPoint(firstPoint);
                Rect.position = Rect.TransformPoint(localPointPosition - deltaRectPosition);
            }
            else if (MultiTouch)
            {
                Vector3 firstPosition = GetWorldPointPosition(touches[0]);
                Vector3 secondPosition = GetWorldPointPosition(touches[1]);

                Vector3 lastFirstPosition = GetWorldPointLastPosition(touches[0]);
                Vector3 lastSecondPosition = GetWorldPointLastPosition(touches[1]);

                float deltaFP = (firstPosition - lastFirstPosition).magnitude;
                float deltaSP = (secondPosition - lastSecondPosition).magnitude;
                float deltaSumm = deltaFP + deltaSP;

                if (!Mathf.Approximately(deltaSumm, 0))
                {
                    // Change rotation

                    Vector3 rotaionCenter = Vector3.Lerp(firstPosition, secondPosition, deltaFP / deltaSumm);

                    float newAngle = Angle(firstPosition, secondPosition);
                    float lastAngle = Angle(lastFirstPosition, lastSecondPosition);

                    Rect.RotateAround(rotaionCenter, Vector3.forward, newAngle - lastAngle);

                    // Change position

                    Vector3 localPointPosition = Rect.InverseTransformPoint((firstPosition + secondPosition) / 2);
                    Vector3 newPosition = Rect.TransformPoint(localPointPosition - deltaRectPosition);
                    newPosition.z = Rect.position.z;
                    Rect.position = newPosition;

                    // Change scale

                    float addScale = (firstPosition - secondPosition).magnitude / (lastFirstPosition - lastSecondPosition).magnitude;

                    bool validateScale = true;
                    for (int i = 0; i < 3 && validateScale; i++)
                        validateScale = validateScale && Rect.localScale[i] * addScale > minScale && Rect.localScale[i] * addScale < maxScale;

                    if (validateScale)
                        Rect.localScale *= addScale;
                }
            }
        }

        void UpdateInitialState()
        {
            if (OneTouch)
            {
                Vector3 firstPosition = GetWorldPointPosition(touches[0]);
                deltaRectPosition = Rect.InverseTransformPoint(firstPosition);
            }
            else if (MultiTouch)
            {
                Vector3 firstPosition = GetWorldPointPosition(touches[0]);
                Vector3 secondPosition = GetWorldPointPosition(touches[1]);

                deltaRectPosition = Rect.InverseTransformPoint((firstPosition + secondPosition) / 2);
            }
        }

        Vector3 GetWorldPointPosition(PointerEventData pointerEventData)
        {
            return Camera.main.ScreenToWorldPoint(pointerEventData.position);
        }

        Vector3 GetWorldPointLastPosition(PointerEventData pointerEventData)
        {
            return Camera.main.ScreenToWorldPoint(pointerEventData.position - pointerEventData.delta);
        }

        float Angle(Vector3 fP, Vector3 sP)
        {
            Vector3 pointRelativeToZero = fP - sP;
            return Mathf.Atan2(pointRelativeToZero.y, pointRelativeToZero.x) * Mathf.Rad2Deg;
        }

        bool MultiTouch
        {
            get
            {
                return touches.Count > 1;
            }
        }

        bool OneTouch
        {
            get
            {
                return touches.Count == 1;
            }
        }

        public RectTransform Rect
        {
            get
            {
                return image.rectTransform;
            }
        }
    }
}
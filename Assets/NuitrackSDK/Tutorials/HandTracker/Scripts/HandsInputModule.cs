/*
 * This module extends the capabilities of StandaloneInputModule 
 * by adding control of your custom controllers on a par with 
 * classic cursor or touch control.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;


namespace NuitrackSDK.Tutorials.HandTracker
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Hand Tracker/Hands Input Module")]
    public class HandsInputModule : StandaloneInputModule
    {
        [SerializeField] List<Pointer> pointers;

        Dictionary<Pointer, MouseButtonEventData> pointerEvents = new Dictionary<Pointer, MouseButtonEventData>();
        Dictionary<Pointer, bool> lastPressState = new Dictionary<Pointer, bool>();

        List<RaycastResult> raycastResults = new List<RaycastResult>();

        protected override void Awake()
        {
            base.Awake();

            int pointerId = 0;
            foreach (Pointer p in pointers)
            {
                MouseButtonEventData pointerData = new MouseButtonEventData();
                pointerData.buttonData = new PointerEventData(eventSystem);
                // Set Touch id for when simulating touches on a non touch device.
                pointerData.buttonData.pointerId = kFakeTouchesId;

                m_PointerData.Add(pointerId++, pointerData.buttonData);
                pointerEvents.Add(p, pointerData);

                lastPressState.Add(p, false);
            }
        }

        public override void Process()
        {
            foreach (KeyValuePair<Pointer, MouseButtonEventData> pe in pointerEvents)
            {
                Pointer pointer = pe.Key;
                MouseButtonEventData buttonEventData = pe.Value;
                PointerEventData pointerEventData = buttonEventData.buttonData;

                // Update position pointer

                Vector2 pointOnScreenPosition = Camera.main.WorldToScreenPoint(pointer.Position);
                pointerEventData.delta = pointOnScreenPosition - pointerEventData.position;
                pointerEventData.position = pointOnScreenPosition;

                // Update UI Raycast data

                raycastResults.Clear();
                eventSystem.RaycastAll(pointerEventData, raycastResults);
                pointerEventData.pointerCurrentRaycast = FindFirstRaycast(raycastResults);

                // Update press state

                PointerEventData.FramePressState framePressState = PointerEventData.FramePressState.NotChanged;

                if (pointer.Press && !lastPressState[pointer])
                    framePressState = PointerEventData.FramePressState.Pressed;
                else if (!pointer.Press && lastPressState[pointer])
                    framePressState = PointerEventData.FramePressState.Released;

                lastPressState[pointer] = pointer.Press;
                buttonEventData.buttonState = framePressState;

                // Call processes of parent class

                ProcessMove(pointerEventData);
                ProcessDrag(pointerEventData);
                ProcessMousePress(buttonEventData);

                // Zero the Delta after use, for correct Drag, because the number 
                // of UI event calls is equal to the number of controllers Pointer.
                pointerEventData.delta = Vector2.zero;
            }
            base.Process();
        }
    }
}
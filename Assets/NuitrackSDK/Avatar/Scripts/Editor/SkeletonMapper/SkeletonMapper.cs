using UnityEngine;

using nuitrack;


namespace NuitrackSDKEditor.Avatar
{
    public abstract class SkeletonMapper<T> where T : Object
    {
        public delegate void DropHandler(T dropObject, JointType jointType);
        public delegate void SelectHandler(JointType jointType);

        public event DropHandler OnDrop;
        public event SelectHandler OnSelected;

        public virtual JointType SelectedJoint { get; set; } = JointType.None;

        /// <summary>
        /// Trigger the drag-and-drop end event
        /// </summary>
        /// <param name="dropObject">The object being dragged</param>
        /// <param name="jointType">Joint type</param>
        protected void OnDropAction(T dropObject, JointType jointType)
        {
            OnDrop?.Invoke(dropObject, jointType);
        }

        /// <summary>
        /// Trigger a joint select event
        /// </summary>
        /// <param name="jointType">Joint type</param>
        protected void OnSelectedAction(JointType jointType)
        {
            OnSelected?.Invoke(jointType);
        }

        /// <summary>
        /// Check whether the click was in the specified <see cref="Rect"/>
        /// </summary>
        /// <param name="controllerID">GUI controller ID</param>
        /// <param name="clickRect">Target rect</param>
        /// <returns>True if there was a click, otherwise false</returns>
        protected bool HandleClick(int controllerID, Rect clickRect)
        {
            Event evt = Event.current;

            if (evt.type == EventType.MouseDown && clickRect.Contains(evt.mousePosition))
            {
                evt.Use();
                GUIUtility.keyboardControl = controllerID;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if there was a deletion event for a given GUI controller
        /// </summary>
        /// <param name="controllerID">GUI controller ID</param>
        /// <returns>True if there was a deletion event, otherwise false</returns>
        public bool HandleDelete(int controllerID)
        {
            Event evt = Event.current;
            if (evt.type == EventType.KeyDown)
            {
                if (GUIUtility.keyboardControl == controllerID)
                {
                    if ((evt.keyCode == KeyCode.Backspace) || (evt.keyCode == KeyCode.Delete))
                    {
                        GUI.changed = true;
                        evt.Use();

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
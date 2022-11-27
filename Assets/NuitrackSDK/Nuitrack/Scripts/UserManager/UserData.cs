using UnityEngine;
using System;

using NuitrackSDK.Frame;


namespace NuitrackSDK
{
    public class UserData : IDisposable
    {
        /// <summary>
        /// Wrapper for Nuitrack Skeleton <see cref="nuitrack.Skeleton"/>
        /// </summary>
        public class SkeletonData
        {
            /// <summary>
            /// Wrapper for Nuitrack Joint <see cref="nuitrack.Joint"/>
            /// </summary>
            public class Joint
            {
                /// <summary>
                /// Raw nuitrack.Joint data
                /// </summary>
                public nuitrack.Joint RawJoint
                {
                    get; private set;
                }

                public Joint(nuitrack.Joint joint)
                {
                    RawJoint = joint;
                }

                /// <summary>
                /// Confidence for this joint
                /// </summary>
                public float Confidence
                {
                    get
                    {
                        return RawJoint.Confidence;
                    }
                }

                /// <summary>
                /// Joint type from nuitrack
                /// </summary>
                public nuitrack.JointType NuitrackType
                {
                    get
                    {
                        return RawJoint.Type;
                    }
                }

                /// <summary>
                /// The corresponding type of Unity bone
                /// </summary>
                public HumanBodyBones HumanBodyBoneType
                {
                    get
                    {
                        return RawJoint.Type.ToUnityBones();
                    }
                }

                /// <summary>
                /// Joint position in meters
                /// </summary>
                public Vector3 Position
                {
                    get
                    {
                        return RawJoint.ToVector3() * 0.001f;
                    }
                }

                /// <summary>
                /// Joint orientation
                /// </summary>
                public Quaternion Rotation
                {
                    get
                    {
                        return RawJoint.ToQuaternion(); //Quaternion.Inverse(CalibrationInfo.SensorOrientation)
                    }
                }

                /// <summary>
                /// Mirrored orientation of the joint
                /// </summary>
                public Quaternion RotationMirrored
                {
                    get
                    {
                        return RawJoint.ToQuaternionMirrored();
                    }
                }

                /// <summary>
                /// Projection and normalized joint coordinates
                /// </summary>
                public Vector2 Proj
                {
                    get
                    {
                        return new Vector2(Mathf.Clamp01(RawJoint.Proj.X), Mathf.Clamp01(1 - RawJoint.Proj.Y));
                    }
                }

                /// <summary>
                /// Get joint relative position on the rect
                /// </summary>
                /// <param name="width">Rect width</param>
                /// <param name="height">Rect height</param>
                /// <returns>Relative position on the rect</returns>
                public Vector2 RelativePosition(float width, float height)
                {
                    Vector2 projPos = Proj;

                    return new Vector2(projPos.x * width, projPos.y * height);
                }

                /// <summary>
                /// Get the Point of the joint relative to the parent Rect
                /// for the corresponding RectTransform taking into account the anchor
                /// </summary>
                /// <param name="rectTransform">Parent Rect</param>
                /// <param name="parentRect">RectTransform reference for current Joint</param>
                /// <returns>Vector2 of the joint relative to the parent Rect (anchoredPosition)</returns>
                public Vector2 AnchoredPosition(Rect parentRect, RectTransform rectTransform)
                {
                    return Vector2.Scale(Proj - rectTransform.anchorMin, parentRect.size);
                }
            }

            /// <summary>
            /// Raw nuitrack.Skeleton data
            /// </summary>
            public nuitrack.Skeleton RawSkeleton
            {
                get; private set;
            }

            public SkeletonData(nuitrack.Skeleton skeleton)
            {
                RawSkeleton = skeleton;
            }

            /// <summary>
            /// Get a wrapper object for the specified joint. Maybe null.
            /// </summary>
            /// <param name="jointType">Joint type <see cref="nuitrack.JointType"/></param>
            /// <returns>Shell object <see cref="Joint"/></returns>
            public Joint GetJoint(nuitrack.JointType jointType)
            {
                nuitrack.Joint joint = RawSkeleton.GetJoint(jointType);
                return new Joint(joint);
            }

            /// <summary>
            /// Get a wrapper object for the specified joint. Maybe null.
            /// </summary>
            /// <param name="humanBodyBone">Bone type <see cref="HumanBodyBones"/></param>
            /// <returns>Shell object <see cref="Joint"/></returns>
            public Joint GetJoint(HumanBodyBones humanBodyBone)
            {
                return GetJoint(humanBodyBone.ToNuitrackJoint());
            }
        }

        /// <summary>
        /// Wrapper for Nuitrack HandContent <see cref=nuitrack.HandContent"/>
        /// </summary>
        public class Hand
        {
            /// <summary>
            /// Raw nuitrack.HandContent data
            /// </summary>
            public nuitrack.HandContent RawHandContent
            {
                get; private set;
            }

            /// <summary>
            /// Projection coordinates of the hand with the starting point in the upper left corner.
            /// </summary>
            public Vector2 Proj
            {
                get
                {
                    return new Vector2(Mathf.Clamp01(RawHandContent.X), Mathf.Clamp01(1 - RawHandContent.Y));
                }
            }

            /// <summary>
            /// Real 3D hand position with depth
            /// </summary>
            public Vector3 Position
            {
                get
                {
                    return new Vector3(RawHandContent.XReal, RawHandContent.YReal, RawHandContent.ZReal) * 0.001f;
                }
            }

            /// <summary>
            /// Is the click currently being executed
            /// </summary>
            public bool Click
            {
                get
                {
                    return RawHandContent.Click;
                }
            }

            /// <summary>
            /// Projection and normalized joint coordinates
            /// Compression of the hand (in percent), where 0 - corresponds to an open palm, 1- a clenched fist
            /// </summary>
            public int Pressure
            {
                get
                {
                    return RawHandContent.Pressure;
                }
            }

            /// <summary>
            ///  Get hand relative position on the rect
            /// </summary>
            /// <param name="width">Rect width</param>
            /// <param name="height">Rect height</param>
            /// <returns>Rect point</returns>
            public Vector2 RelativePosition(float width, float height)
            {
                Vector2 projPos = Proj;

                return new Vector2(projPos.x * width, projPos.y * height);
            }

            /// <summary>
            /// Get the Point of the hand relative to the parent Rect
            /// for the corresponding RectTransform taking into account the anchor
            /// </summary>
            /// <param name="rectTransform">Parent Rect</param>
            /// <param name="parentRect">RectTransform reference for current Hand</param>
            /// <returns>Vector2 of the hand relative to the parent Rect (anchoredPosition)</returns>
            public Vector2 AnchoredPosition(Rect parentRect, RectTransform rectTransform)
            {
                return Vector2.Scale(Proj - rectTransform.anchorMin, parentRect.size);
            }

            public Hand(nuitrack.HandContent handContent)
            {
                RawHandContent = handContent;
            }
        }

        public nuitrack.UserHands RawUserHands
        {
            get; private set;
        }

        public nuitrack.Gesture? RawGesture
        {
            get; private set;
        }

        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// User skeleton. Maybe null.
        /// </summary>
        public SkeletonData Skeleton
        {
            get; private set;
        }

        /// <summary>
        /// User left hand. Maybe null.
        /// </summary>
        public Hand LeftHand
        {
            get; private set;
        }

        /// <summary>
        /// User right hand. Maybe null.
        /// </summary>
        public Hand RightHand
        {
            get; private set;
        }

        /// <summary>
        /// The user's last gesture. The data is up-to-date for one Update frame after the event occurs.
        /// </summary>
        public nuitrack.GestureType? GestureType
        {
            get
            {
                return RawGesture?.Type;
            }
        }

        nuitrack.Face face = null;

        /// <summary>
        /// User face. Maybe null.
        /// </summary>
        public nuitrack.Face Face
        {
            get
            {
                if (!NuitrackManager.Instance.UseFaceTracking)
                    Debug.LogWarning("Face tracking disabled! Enable it on the Nuitrack Manager component");

                return face;
            }
        }

        public UserData(int id)
        {
            ID = id;
        }

        /// <summary>
        /// Get a BoundBox rectangle from a rect with the specified width and height
        /// </summary>
        /// <param name="width">Width of the rect</param>
        /// <param name="height">Height of the rect</param>
        /// <returns>BoundBox Rect</returns>
        public Rect BoundingBox(float width, float height)
        {
            nuitrack.User user = NuitrackManager.UserFrame.GetUserByID(ID);

            if (user.ID == 0)
                return default;

            nuitrack.BoundingBox boundingBox = user.Box;

            Rect userRect = new Rect(boundingBox.Left, boundingBox.Top, boundingBox.Right - boundingBox.Left, boundingBox.Bottom - boundingBox.Top);
            Vector2 rectSize = new Vector2(width, height);

            userRect.position = Vector2.Scale(userRect.position, rectSize);
            userRect.size = Vector2.Scale(userRect.size, rectSize);

            return userRect;
        }

        /// <summary>
        /// Get the Rect of the user relative to the parent Rect
        /// for the corresponding RectTransform taking into account the anchor
        /// </summary>
        /// <param name="rectTransform">Parent Rect</param>
        /// <param name="parentRect">RectTransform reference for current User</param>
        /// <returns>Rect of the user relative to the parent Rect (anchoredPosition)</returns>
        public Rect AnchoredRect(Rect parentRect, RectTransform rectTransform)
        {
            nuitrack.User user = NuitrackManager.UserFrame.GetUserByID(ID);

            if (user.ID == 0)
                return default;

            nuitrack.BoundingBox boundingBox = user.Box;

            Rect projRect = new Rect(boundingBox.Left, boundingBox.Top, boundingBox.Right - boundingBox.Left, boundingBox.Bottom - boundingBox.Top);

            Vector2 pivot = Vector2.Scale(projRect.size, rectTransform.pivot);

            Vector2 rectPosition = Vector2.Scale(projRect.position - rectTransform.anchorMin + pivot, parentRect.size);
            Vector2 rectSize = Vector2.Scale(projRect.size, parentRect.size);

            return new Rect(rectPosition, rectSize);
        }

        TextureCache textureCache = null;

        public void Dispose()
        {
            if (textureCache != null)
            {
                textureCache.Dispose();
                textureCache = null;
            }
        }

        Color[] GetUserColors(Color userColor)
        {
            Color[] userColors = new Color[Users.MaxID + 1];

            for (int id = 0; id < userColors.Length; id++)
                userColors[id] = id == ID ? userColor : Color.clear;

            return userColors;
        }

        /// <summary>
        /// Get the Texture2D segment of this user
        /// </summary>
        /// <param name="userColor">The color in which the user segment will be colored.</param>
        /// <returns>Texture <see cref="SegmentToTexture.GetTexture2D(nuitrack.UserFrame, Color[], TextureCache)"/></returns>
        public Texture2D SegmentTexture2D(Color userColor)
        {
            if (textureCache == null)
                textureCache = new TextureCache();

            return NuitrackManager.UserFrame.ToTexture2D(GetUserColors(userColor), textureCache);
        }

        /// <summary>
        /// Get the RenderTexture segment of this user
        /// </summary>
        /// <param name="userColor">The color in which the user segment will be colored.</param>
        /// <returns>Texture <see cref="SegmentToTexture.GetRenderTexture(nuitrack.UserFrame, Color[], TextureCache)"/></returns>
        public RenderTexture SegmentRenderTexture(Color userColor)
        {
            if (textureCache == null)
                textureCache = new TextureCache();

            return NuitrackManager.UserFrame.ToRenderTexture(GetUserColors(userColor), textureCache);
        }

        /// <summary>
        /// Get the Texture (Texture2D or RenderTexture - the fastest way for your platform will be selected) segment of this user
        /// </summary>
        /// <param name="userColor">The color in which the user segment will be colored.</param>
        /// <returns>Texture <see cref="SegmentToTexture.GetTexture(nuitrack.UserFrame, Color[], TextureCache)"/></returns>
        public Texture SegmentTexture(Color userColor)
        {
            if (textureCache == null)
                textureCache = new TextureCache();

            return NuitrackManager.UserFrame.ToTexture(GetUserColors(userColor), textureCache);
        }

        internal void AddData(nuitrack.Skeleton skeleton)
        {
            Skeleton = new SkeletonData(skeleton);
        }

        internal void AddData(nuitrack.UserHands userHands)
        {
            RawUserHands = userHands;

            if (userHands.LeftHand != null)
                LeftHand = new Hand(userHands.LeftHand.Value);

            if (userHands.RightHand != null)
                RightHand = new Hand(userHands.RightHand.Value);
        }

        internal void AddData(nuitrack.Face face)
        {
            this.face = (face != null && face.IsEmpty) ? null : face;
        }

        internal void AddData(nuitrack.Gesture? gesture)
        {
            RawGesture = gesture;
        }

        internal void Reset()
        {
            Skeleton = null;
            RawUserHands = null;
            RawGesture = null;
            face = null;

            RightHand = null;
            LeftHand = null;
        }
    }
}
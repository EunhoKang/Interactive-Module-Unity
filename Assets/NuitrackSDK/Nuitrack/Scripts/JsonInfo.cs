using UnityEngine;


namespace nuitrack
{
    [System.Serializable]
    public class JsonInfo
    {
        public ulong Timestamp;
        public Instances[] Instances;
    }

    [System.Serializable]
    public class Instances
    {
        public int id;
        public string @class;
        public Face face;
        public Rectangle bbox;
    }

    [System.Serializable]
    public class Face
    {
        public enum GenderType
        {
            any,
            male,
            female
        }

        public Rectangle rectangle;
        public Vector2[] landmark;
        public Vector2 left_eye;
        public Vector2 right_eye;
        public Angles angles;
        public Emotions emotions;
        public Age age;
        public string gender;

        public bool IsEmpty
        {
            get
            {
                return rectangle == null && landmark == null && angles == null && emotions == null && age == null;
            }
        }

        public GenderType Gender
        {
            get
            {
                if (System.Enum.TryParse(gender, out GenderType type))
                    return type;
                else
                    return GenderType.any;
            }
        }

        public Rect Rect
        {
            get
            {
                return rectangle;
            }
        }


        public Age.Type AgeType
        {
            get
            {
                return age;
            }
        }

        public float GetEmotionValue(Emotions.Type type)
        {
            return emotions[type];
        }

        public Emotions.Type PrevailingEmotion
        {
            get
            {
                return emotions.Prevailing;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return angles;
            }
        }

        /// <summary>
        /// Convert Rect to virtual screen Rect
        /// </summary>
        /// <param name="width">Width of the virtual screen</param>
        /// <param name="height">Width of the virtual screen</param>
        /// <returns>Virtual screen Rect</returns>
        public Rect ScreenRect(float width, float height)
        {
            Rect faceRect = rectangle;
            Vector2 screenSize = new Vector2(width, height);

            faceRect.position = Vector2.Scale(faceRect.position, screenSize);
            faceRect.size = Vector2.Scale(faceRect.size, screenSize);

            return faceRect;
        }

        /// <summary>
        /// Get the Rect of the face relative to the parent Rect
        /// for the corresponding RectTransform taking into account the anchor
        /// </summary>
        /// <param name="rectTransform">Parent Rect</param>
        /// <param name="parentRect">RectTransform reference for current Face</param>
        /// <returns>Rect of the face relative to the parent Rect (anchoredPosition)</returns>
        public Rect AnchoredRect(Rect parentRect, RectTransform rectTransform)
        {
            Rect projRect = rectangle;

            Vector2 pivot = Vector2.Scale(projRect.size, rectTransform.pivot);

            Vector2 rectPosition = Vector2.Scale(projRect.position - rectTransform.anchorMin + pivot, parentRect.size);
            Vector2 rectSize = Vector2.Scale(projRect.size, parentRect.size);

            return new Rect(rectPosition, rectSize);
        }
    }

    [System.Serializable]
    public class Rectangle
    {
        public float left;
        public float top;
        public float width;
        public float height;

        public static implicit operator Rect(Rectangle rectangle)
        {
            return new Rect(
                Mathf.Clamp01(rectangle.left),
                Mathf.Clamp01(1 - rectangle.top - rectangle.height),
                Mathf.Clamp01(rectangle.width),
                Mathf.Clamp01(rectangle.height));
        }
    }

    [System.Serializable]
    public class Angles
    {
        public float yaw;
        public float pitch;
        public float roll;

        public static implicit operator Quaternion(Angles ang)
        {
            return Quaternion.Euler(-ang.pitch, -ang.yaw, ang.roll);
        }
    }

    [System.Serializable]
    public class Emotions
    {
        public enum Type
        {
            any,
            happy,
            surprise,
            neutral,
            angry
        }

        public float neutral;
        public float angry;
        public float surprise;
        public float happy;

        public float this[Type type]
        {
            get
            {
                return type switch
                {
                    Type.neutral => neutral,
                    Type.angry => angry,
                    Type.surprise => surprise,
                    Type.happy => happy,
                    _ => 0,
                };
            }
        }

        public Type Prevailing
        {
            get
            {
                float maxConfidence = 0;
                Type prevailingEmotion = Type.any;

                foreach (Type emotion in System.Enum.GetValues(typeof(Type)))
                {
                    float emotionVal = this[emotion];

                    if (emotionVal > maxConfidence)
                    {
                        prevailingEmotion = emotion;
                        maxConfidence = emotionVal;
                    }
                }

                return prevailingEmotion;
            }
        }
    }

    [System.Serializable]
    public class Age
    {
        public enum Type
        {
            any,
            kid,
            young,
            adult,
            senior,
            none
        }

        public string type;
        public float years;

        public static implicit operator Type(Age age)
        {
            if (System.Enum.TryParse(age.type, out Type type))
                return type;
            else
                return Type.none;
        }
    }
}
using UnityEngine;
using UnityEngine.Events;


namespace NuitrackSDK.Frame
{
    public abstract class Cropper : TrackedUser
    {
        [Header("Visualisation")]
        [SerializeField, NuitrackSDKInspector, Range(0, 1)]
        float margin = 0.1f;

        [SerializeField, NuitrackSDKInspector]
        Texture2D noUserImage;

        [SerializeField, NuitrackSDKInspector]
        bool smoothMove = true;

        [SerializeField, NuitrackSDKInspector, Range(0.1f, 24f)]
        float smoothSpeed = 4f;

        Rect currentRect = default;
        Rect targerRect;

        [Header("Output")]
        [SerializeField, NuitrackSDKInspector]
        UnityEvent<Texture> onFrameUpdate;

        [SerializeField, NuitrackSDKInspector]
        UnityEvent<float> aspectRatioUpdate;

        TextureCache textureCache;

        /// <summary>
        /// Cropped texture (may be null)
        /// </summary>
        public RenderTexture CroppedTexture
        {
            get; private set;
        }

        void ResetFrame()
        {
            onFrameUpdate.Invoke(noUserImage);

            if (noUserImage != null)
            {
                float aspectRatio = (float)noUserImage.width / noUserImage.height;
                aspectRatioUpdate.Invoke(aspectRatio);
            }

            currentRect = default;
        }

        void OnDestroy()
        {
            if (textureCache != null)
            {
                textureCache.Dispose();
                textureCache = null;
            }

            if (CroppedTexture != null)
                Destroy(CroppedTexture);
        }

        protected abstract bool IsUserLost(UserData userData);

        protected abstract Rect GetFrameRect(UserData userData, float width, float height);

        public void CropFrame(Texture frame)
        {
            if (frame == null || IsUserLost(ControllerUser))
            {
                ResetFrame();
                return;
            }

            targerRect = GetFrameRect(ControllerUser, frame.width, frame.height);

            if (currentRect.Equals(default))
                currentRect = targerRect;

            Vector2 deltaSize = targerRect.size * margin;

            targerRect.position -= deltaSize * 0.5f;
            targerRect.size += deltaSize;

            currentRect.xMin = Mathf.Clamp(currentRect.xMin, 0, frame.width);
            currentRect.xMax = Mathf.Clamp(currentRect.xMax, 0, frame.width);

            currentRect.yMin = Mathf.Clamp(currentRect.yMin, 0, frame.height);
            currentRect.yMax = Mathf.Clamp(currentRect.yMax, 0, frame.height);

            if (CroppedTexture != null)
                Destroy(CroppedTexture);

            CroppedTexture = new RenderTexture((int)currentRect.width, (int)currentRect.height, 0, RenderTextureFormat.ARGB32);
            
            Graphics.CopyTexture(frame, 0, 0, (int)currentRect.x, (int)currentRect.y, (int)currentRect.width, (int)currentRect.height, CroppedTexture, 0, 0, 0, 0);

            onFrameUpdate.Invoke(CroppedTexture);

            float aspectRatio = (float)CroppedTexture.width / CroppedTexture.height;
            aspectRatioUpdate.Invoke(aspectRatio);
        }

        void Update()
        {
            if (currentRect.Equals(default))
                return;

            if (smoothMove)
            {
                currentRect.position = Vector2.Lerp(currentRect.position, targerRect.position, Time.deltaTime * smoothSpeed);
                currentRect.size = Vector2.Lerp(currentRect.size, targerRect.size, Time.deltaTime * smoothSpeed);
            }
            else
                currentRect = targerRect;
        }
    }
}
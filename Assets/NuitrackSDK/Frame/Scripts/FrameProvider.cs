using UnityEngine;

using UnityEngine.Events;


namespace NuitrackSDK.Frame
{
    [AddComponentMenu("NuitrackSDK/Frame/Frame Provider")]
    public class FrameProvider : MonoBehaviour
    {
        public enum FrameType
        {
            Color = 0,
            Depth = 1,
            Segment = 2
        }

        public enum TextureMode
        {
            RenderTexture = 0,
            Texture2D = 1,
            Texture = 2
        }

        public enum SegmentMode
        {
            All = 0,
            Single = 1
        }

        [SerializeField, NuitrackSDKInspector] FrameType frameType;

        // Depth options
        [SerializeField, NuitrackSDKInspector] bool useCustomDepthGradient = false;
        [SerializeField, NuitrackSDKInspector] Gradient customDepthGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[5] {
                new GradientColorKey(new Color(1, 0, 0), 0.1f),
                new GradientColorKey(new Color(1, 1, 0), 0.3f),
                new GradientColorKey(new Color(0, 1, 0), 0.5f),
                new GradientColorKey(new Color(0, 1, 1), 0.65f),
                new GradientColorKey(new Color(0, 0, 1), 1.0f),
            },
            alphaKeys = new GradientAlphaKey[2] {
                new GradientAlphaKey(1, 0),
                new GradientAlphaKey(1, 1)
            }
        };

        // Segment options
        [SerializeField, NuitrackSDKInspector] SegmentMode segmentMode = SegmentMode.All;

        [SerializeField, NuitrackSDKInspector] bool useCustomUsersColors = false;
        [SerializeField, NuitrackSDKInspector] Color[] customUsersColors = new Color[]
        {
            Color.clear,
            Color.red,
            Color.green,
            Color.blue,
            Color.magenta,
            Color.yellow,
            Color.cyan,
            Color.grey
        };

        [SerializeField, NuitrackSDKInspector] bool useCurrentUserTracker = true;
        [SerializeField, Range(1, 6), NuitrackSDKInspector] int userID = 1;
        [SerializeField, NuitrackSDKInspector] Color userColor = Color.red;

        [SerializeField, NuitrackSDKInspector] TextureMode textureMode;

        [SerializeField, NuitrackSDKInspector] UnityEvent<Texture> onFrameUpdate;

        TextureCache textureCache;

        Gradient DepthGradient
        {
            get
            {
                return useCustomDepthGradient ? customDepthGradient : null;
            }
        }

        Color[] UsersColors
        {
            get
            {
                return useCustomUsersColors ? customUsersColors : null;
            }
        }

        void Awake()
        {
            textureCache = new TextureCache();
        }

        void OnDestroy()
        {
            if (textureCache != null)
                textureCache.Dispose();
        }

        void Update()
        {
            Texture texture = GetTexture();
            onFrameUpdate.Invoke(texture);
        }

        Texture GetTexture()
        {
            return frameType switch
            {
                FrameType.Color => GetColorTexture(),
                FrameType.Depth => GetDepthTexture(),
                FrameType.Segment => GetSegmentTexture(),
                _ => null,
            };
        }

        Texture GetColorTexture()
        {
            if (!NuitrackManager.Instance.UseColorModule)
                return null;

            return textureMode switch
            {
                TextureMode.RenderTexture => NuitrackManager.ColorFrame.ToRenderTexture(),
                TextureMode.Texture2D => NuitrackManager.ColorFrame.ToTexture2D(),
                TextureMode.Texture => NuitrackManager.ColorFrame.ToTexture(),
                _ => null,
            };
        }

        Texture GetDepthTexture()
        {
            if (!NuitrackManager.Instance.UseDepthModule)
                return null;

            return textureMode switch
            {
                TextureMode.RenderTexture => NuitrackManager.DepthFrame.ToRenderTexture(DepthGradient, DepthGradient != null ? textureCache : null),
                TextureMode.Texture2D => NuitrackManager.DepthFrame.ToTexture2D(DepthGradient, DepthGradient != null ? textureCache : null),
                TextureMode.Texture => NuitrackManager.DepthFrame.ToTexture(DepthGradient, DepthGradient != null ? textureCache : null),
                _ => null,
            };
        }

        Texture GetSegmentTexture()
        {
            if (!NuitrackManager.Instance.UseUserTrackerModule)
                return null;

            if (segmentMode == SegmentMode.Single)
            {
                UserData userData = useCurrentUserTracker ? NuitrackManager.Users.Current : NuitrackManager.Users.GetUser(userID);

                if (userData == null)
                    return null;

                return textureMode switch
                {
                    TextureMode.RenderTexture => userData.SegmentRenderTexture(userColor),
                    TextureMode.Texture2D => userData.SegmentTexture2D(userColor),
                    TextureMode.Texture => userData.SegmentTexture(userColor),
                    _ => null,
                };
            }
            else
            {
                return textureMode switch
                {
                    TextureMode.RenderTexture => NuitrackManager.UserFrame.ToRenderTexture(UsersColors, UsersColors != null ? textureCache : null),
                    TextureMode.Texture2D => NuitrackManager.UserFrame.ToTexture2D(UsersColors, UsersColors != null ? textureCache : null),
                    TextureMode.Texture => NuitrackManager.UserFrame.ToTexture(UsersColors, UsersColors != null ? textureCache : null),
                    _ => null,
                };
            }
        }
    }
}
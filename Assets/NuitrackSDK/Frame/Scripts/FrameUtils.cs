using UnityEngine;

using nuitrack;

namespace NuitrackSDK.Frame
{
    [RequireComponent(typeof(RGBToTexture))]
    [RequireComponent(typeof(DepthToTexture))]
    [RequireComponent(typeof(SegmentToTexture))]
    [RequireComponent(typeof(TextureUtils))]
    public class FrameUtils : MonoBehaviour
    {
        static FrameUtils instance;

        RGBToTexture rgbToTexture;
        DepthToTexture depthToTexture;
        SegmentToTexture segmentToTexture;

        TextureUtils textureUtils;

        public static FrameUtils Instance
        {
            get
            {
                if (instance == null)
                    Debug.LogError("FrameUtils not found. Add a prefab FrameUtils to the scene.");

                return instance;
            }
        }

        public static RGBToTexture RGBToTexture
        {
            get
            {
                return Instance.rgbToTexture;
            }
        }

        public static DepthToTexture DepthToTexture
        {
            get
            {
                return Instance.depthToTexture;
            }
        }

        public static SegmentToTexture SegmentToTexture
        {
            get
            {
                return Instance.segmentToTexture;
            }
        }

        public static TextureUtils TextureUtils
        {
            get
            {
                return instance.textureUtils;
            }
        }

        void Awake()
        {
            instance = this;

            rgbToTexture = GetComponent<RGBToTexture>();
            depthToTexture = GetComponent<DepthToTexture>();
            segmentToTexture = GetComponent<SegmentToTexture>();
            textureUtils = GetComponent<TextureUtils>();
        }
    }

    public static class FrameOverloadUtils
    {
        #region ColorFrame

        /// <summary>
        /// Get the ColorFrame as a RenderTexture. 
        /// Recommended method for platforms with ComputeShader support.
        /// </summary>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>ColorFrame converted to RenderTexture</returns>
        public static RenderTexture ToRenderTexture(this ColorFrame frame, TextureCache textureCache = null)
        {
            return FrameUtils.RGBToTexture.GetRenderTexture(frame, textureCache);
        }

        /// <summary>
        /// Get a ColorFrame in the form of Texture2D. 
        /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
        /// If possible, use GetRenderTexture.
        /// </summary>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>ColorFrame converted to RenderTexture</returns>
        public static Texture2D ToTexture2D(this ColorFrame frame, TextureCache textureCache = null)
        {
            return FrameUtils.RGBToTexture.GetTexture2D(frame, textureCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture(T, TextureCache)"/> 
        /// </summary>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>Texture = (RenderTexture or Texture2D)</returns>
        public static Texture ToTexture(this ColorFrame frame, TextureCache textureCache = null)
        {
            return FrameUtils.RGBToTexture.GetTexture(frame, textureCache);
        }

        #endregion

        #region DepthFrame

        /// <summary>
        /// Get the DepthFrame as a RenderTexture. 
        /// Recommended method for platforms with ComputeShader support.
        /// </summary>
        /// <param name="gradient"> Gradient for depth coloring, where the light at 0 corresponds to the nearest
        /// point to the sensor, at 1 further point from the sensor.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>DepthFrame converted to RenderTexture</returns>
        public static RenderTexture ToRenderTexture(this DepthFrame frame, Gradient gradient = null, TextureCache textureCache = null)
        {
            return FrameUtils.DepthToTexture.GetRenderTexture(frame, gradient, textureCache);
        }

        /// <summary>
        /// Get a DepthFrame in the form of Texture2D. 
        /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
        /// If possible, use GetRenderTexture.
        /// </summary>
        /// <param name="gradient"> Gradient for depth coloring, where the light at 0 corresponds to the nearest
        /// point to the sensor, at 1 further point from the sensor.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>DepthFrame converted to Texture2D</returns>
        public static Texture2D ToTexture2D(this DepthFrame frame, Gradient gradient = null, TextureCache textureCache = null)
        {
            return FrameUtils.DepthToTexture.GetTexture2D(frame, gradient, textureCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture(T, TextureCache)"/> 
        /// </summary>
        /// <param name="gradient"> Gradient for depth coloring, where the light at 0 corresponds to the nearest
        /// point to the sensor, at 1 further point from the sensor.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>Texture = (RenderTexture or Texture2D)</returns>
        public static Texture ToTexture(this DepthFrame frame, Gradient gradient = null, TextureCache textureCache = null)
        {
            return FrameUtils.DepthToTexture.GetTexture(frame, gradient, textureCache);
        }

        #endregion

        #region UserFrame

        /// <summary>
        /// Get the UserFrame as a RenderTexture. 
        /// Recommended method for platforms with ComputeShader support.
        /// </summary>
        /// <param name="userColors">Colors for user segments (optional).</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>UserFrame converted to RenderTexture</returns>
        public static RenderTexture ToRenderTexture(this UserFrame frame, Color[] userColors = null, TextureCache textureCache = null)
        {
            return FrameUtils.SegmentToTexture.GetRenderTexture(frame, userColors, textureCache);
        }

        /// <summary>
        /// Get a UserFrame in the form of Texture2D. 
        /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
        /// If possible, use GetRenderTexture.
        /// </summary>
        /// <param name="userColors">Colors for user segments (optional).</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>UserFrame converted to Texture2D</returns>
        public static Texture2D ToTexture2D(this UserFrame frame, Color[] userColors = null, TextureCache textureCache = null)
        {
            return FrameUtils.SegmentToTexture.GetTexture2D(frame, userColors, textureCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture(T, TextureCache)"/> 
        /// </summary>
        /// <param name="userColors">Colors for user segments (optional)</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>Texture = (RenderTexture or Texture2D)</returns>
        public static Texture ToTexture(this UserFrame frame, Color[] userColors = null, TextureCache textureCache = null)
        {
            return FrameUtils.SegmentToTexture.GetTexture(frame, userColors, textureCache);
        }

        #endregion
    }
}
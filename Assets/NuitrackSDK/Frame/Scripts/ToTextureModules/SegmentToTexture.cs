/*
 * This script converts the source data of nuitrack.UserFrame to textures (RenderTexture / Texture2D / Texture) 
 * using the fastest available method for this platform. 
 * If the platform supports ComputeShader, the conversion is performed using the GPU, which is several times faster than the CPU conversion.
 * 
 * Learn more about supported platforms and the graphics API: https://docs.unity3d.com/Manual/class-ComputeShader.html
*/


using UnityEngine;
using System.Runtime.InteropServices;

using nuitrack;

namespace NuitrackSDK.Frame
{
    public class SegmentToTexture : FrameToTexture<UserFrame, ushort>
    {
        [SerializeField]
        Color[] defaultColors = new Color[]
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

        ComputeBuffer userColorsBuffer;
        ComputeBuffer sourceDataBuffer;

        byte[] segmentDataArray = null;
        byte[] segmentArray = null;
        byte[] mirrorArray = null;

        public Color GetColorByID(int id)
        {
            if (id < defaultColors.Length)
                return defaultColors[id];
            else
                return Color.white;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (userColorsBuffer != null)
            {
                userColorsBuffer.Release();
                userColorsBuffer = null;
            }

            if (sourceDataBuffer != null)
            {
                sourceDataBuffer.Release();
                userColorsBuffer = null;
            }

            segmentDataArray = null;
            segmentArray = null;
            mirrorArray = null;
        }

        Texture2D GetCPUTexture(UserFrame frame, TextureCache textureCache, Color[] userColors = null)
        {
            ref Texture2D destTexture = ref textureCache.texture2D;

            if (frame.Timestamp == textureCache.timeStamp && textureCache.texture2D != null)
                return textureCache.texture2D;
            else
            {
                if (userColors == null)
                    userColors = defaultColors;

                int datasize = frame.DataSize;

                if (segmentArray == null)
                    segmentArray = new byte[datasize];

                if (mirrorArray == null)
                    mirrorArray = new byte[frame.Cols * frame.Rows * 4];

                Marshal.Copy(frame.Data, segmentArray, 0, frame.DataSize);

                //The transformation is performed with the image reflected vertically.
                for (int i = 0, pxl = 0; i < datasize; i += 2, pxl++)
                {
                    int userIndex = segmentArray[i + 1] << 8 | segmentArray[i];
                    Color currentColor = userColors[userIndex];

                    int ptr = (frame.Cols * (frame.Rows - (pxl / frame.Cols) - 1) + pxl % frame.Cols) * 4;

                    mirrorArray[ptr] = (byte)(255f * currentColor.a);
                    mirrorArray[ptr + 1] = (byte)(255f * currentColor.r);
                    mirrorArray[ptr + 2] = (byte)(255f * currentColor.g);
                    mirrorArray[ptr + 3] = (byte)(255f * currentColor.b);
                }

                if (destTexture == null)
                    destTexture = new Texture2D(frame.Cols, frame.Rows, TextureFormat.ARGB32, false);

                destTexture.LoadRawTextureData(mirrorArray);
                destTexture.Apply();


                textureCache.timeStamp = frame.Timestamp;

                return destTexture;
            }
        }

        RenderTexture GetGPUTexture(UserFrame frame, TextureCache textureCache, Color[] userColors = null)
        {
            ref RenderTexture destTexture = ref textureCache.renderTexture;

            if (frame.Timestamp == textureCache.timeStamp && textureCache.renderTexture != null)
                return textureCache.renderTexture;
            else
            {
                textureCache.timeStamp = frame.Timestamp;

                if (instanceShader == null)
                {
                    InitShader("Segment2Texture");
                    instanceShader.SetInt("textureWidth", frame.Cols);
                    instanceShader.SetInt("textureHeight", frame.Rows);

                    /*
                       We put the source data in the buffer, but the buffer does not support types 
                       that take up less than 4 bytes(instead of ushot(Int16), we specify uint(Int32)).

                       For optimization, we specify a length half the original length,
                       since the data is correctly projected into memory
                       (sizeof(ushot) * sourceDataBuffer / 2 == sizeof(uint) * sourceDataBuffer / 2)
                    */

                    sourceDataBuffer = new ComputeBuffer(frame.DataSize / 2, sizeof(uint));
                    instanceShader.SetBuffer(kernelIndex, "UserIndexes", sourceDataBuffer);
                }

                if (userColors == null)
                    userColors = defaultColors;

                if (userColorsBuffer == null || userColorsBuffer.count != userColors.Length)
                {
                    if (userColorsBuffer != null)
                        userColorsBuffer.Release();

                    userColorsBuffer = new ComputeBuffer(userColors.Length, sizeof(float) * 4);
                    instanceShader.SetBuffer(kernelIndex, "UserColors", userColorsBuffer);
                }

                userColorsBuffer.SetData(userColors);

                if (destTexture == null)
                    destTexture = InitRenderTexture(frame.Cols, frame.Rows);

                instanceShader.SetTexture(kernelIndex, "Result", destTexture);

                if (segmentDataArray == null)
                    segmentDataArray = new byte[frame.DataSize];

                Marshal.Copy(frame.Data, segmentDataArray, 0, frame.DataSize);
                sourceDataBuffer.SetData(segmentDataArray);

                instanceShader.Dispatch(kernelIndex, destTexture.width / (int)x, destTexture.height / (int)y, (int)z);

                return destTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetRenderTexture(T, TextureCache)"/> 
        /// </summary>
        /// <returns>UserFrame converted to RenderTexture</returns>
        public override RenderTexture GetRenderTexture(UserFrame frame, TextureCache textureCache = null)
        {
            return GetRenderTexture(frame, defaultColors, textureCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetRenderTexture(T, TextureCache)"/> 
        /// </summary>
        /// <param name="userColors">Colors for user segments.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>UserFrame converted to RenderTexture</returns>
        public RenderTexture GetRenderTexture(UserFrame frame, Color[] userColors, TextureCache textureCache = null)
        {
            if (frame == null)
            {
                CheckModuleDisableIssue(NuitrackManager.Instance.UseUserTrackerModule, "User tracker");
                return null;
            }

            if (GPUSupported)
                return GetGPUTexture(frame, textureCache ?? localCache, userColors);
            else
            {
                TextureCache cache = textureCache ?? localCache;

                cache.texture2D = GetCPUTexture(frame, cache, userColors);
                FrameUtils.TextureUtils.Copy(cache.texture2D, ref cache.renderTexture);

                return cache.renderTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T, TextureCache)"/> 
        /// </summary>
        /// <returns>UserFrame converted to Texture2D</returns>
        public override Texture2D GetTexture2D(UserFrame frame, TextureCache textureCache = null)
        {
            return GetTexture2D(frame, defaultColors, textureCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T, TextureCache)"/> 
        /// </summary>
        /// <param name="userColors">Colors for user segments.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>UserFrame converted to Texture2D</returns>
        public Texture2D GetTexture2D(UserFrame frame, Color[] userColors, TextureCache textureCache = null)
        {
            if (frame == null)
            {
                CheckModuleDisableIssue(NuitrackManager.Instance.UseUserTrackerModule, "User tracker");
                return null;
            }

            if (GPUSupported)
            {
                TextureCache cache = textureCache ?? localCache;

                cache.renderTexture = GetGPUTexture(frame, cache, userColors);
                FrameUtils.TextureUtils.Copy(cache.renderTexture, ref cache.texture2D);
                return cache.texture2D;
            }
            else
                return GetCPUTexture(frame, textureCache ?? localCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T, TextureCache)"/> 
        /// </summary>
        /// <param name="userColors">Colors for user segments.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>Texture = (RenderTexture or Texture2D)</returns>
        public Texture GetTexture(UserFrame frame, Color[] userColors, TextureCache textureCache = null)
        {
            if (GPUSupported)
                return GetRenderTexture(frame, userColors, textureCache);
            else
                return GetTexture2D(frame, userColors, textureCache);
        }
    }
}
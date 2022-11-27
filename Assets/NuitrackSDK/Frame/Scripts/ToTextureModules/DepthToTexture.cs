/*
 * This script converts the source data of nuitrack.DepthFrame to textures (RenderTexture / Texture2D / Texture) 
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
    public class DepthToTexture : FrameToTexture<DepthFrame, ushort>
    {
        class GradientBuffer : System.IDisposable
        {
            const int gradientColorSize = sizeof(float) * 4 + sizeof(float);
            const int gradientAlphaSize = sizeof(float) + sizeof(float);
            Gradient gradientSource;

            public ComputeBuffer ColorBuffer
            {
                get;
                private set;
            }

            public ComputeBuffer AlphaBuffer
            {
                get;
                private set;
            }

            public bool Equals(Gradient gradient)
            {
                return gradientSource.Equals(gradient);
            }

            public GradientBuffer(Gradient gradient)
            {
                gradientSource = new Gradient();
                gradientSource.SetKeys(gradient.colorKeys, gradient.alphaKeys);
                gradientSource.mode = gradient.mode;

                ColorBuffer = new ComputeBuffer(gradient.colorKeys.Length, gradientColorSize);
                ColorBuffer.SetData(gradient.colorKeys);

                AlphaBuffer = new ComputeBuffer(gradient.alphaKeys.Length, gradientAlphaSize);
                AlphaBuffer.SetData(gradient.alphaKeys);
            }

            public void Dispose()
            {
                if (ColorBuffer != null)
                    ColorBuffer.Release();

                if (AlphaBuffer != null)
                    AlphaBuffer.Release();
            }
        }

        class GradientCache
        {
            Color[] colors;
            Gradient gradientSource;

            public bool Equals(Gradient gradient)
            {
                return gradientSource.Equals(gradient);
            }

            public GradientCache(Gradient gradient)
            {
                gradientSource = new Gradient();
                gradientSource.SetKeys(gradient.colorKeys, gradient.alphaKeys);
                gradientSource.mode = gradient.mode;

                colors = new Color[256];

                for (int i = 0; i < 256; i ++)
                    colors[i] = gradient.Evaluate(i / 255f);
            }

            public Color Evaluate(float time)
            {
                int index = (int)Mathf.Clamp(time * 255f, 0f, 255f);
                return colors[index];
            }
        }

        [SerializeField] Gradient defaultGradient = new Gradient()
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

        [Range(0f, 32.0f)]
        [SerializeField] float maxDepthSensor = 10f;

        ComputeBuffer sourceDataBuffer;

        GradientBuffer gradientsBuffer;
        GradientCache gradientCache;

        byte[] depthDataArray = null;
        byte[] depthArray = null;
        byte[] mirrorArray = null;

        public float MaxSensorDepth
        {
            get
            {
                return maxDepthSensor;
            }
        }

        /// <summary>
        /// Get the hFOV of the DepthFrame in degrees
        /// </summary>
        public float HFOV
        {
            get
            {
                OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();
                return mode.HFOV * Mathf.Rad2Deg;
            }
        }

        /// <summary>
        /// Get the vFOV of the DepthFrame in degrees
        /// </summary>
        public float VFOV
        {
            get
            {
                OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();

                float aspectRatio = (float)mode.YRes / mode.XRes;
                float vFOV = 2 * Mathf.Atan(Mathf.Tan(mode.HFOV * 0.5f) * aspectRatio);

                return vFOV * Mathf.Rad2Deg;

            }
        }

        public int Width
        {
            get
            {
                return NuitrackManager.DepthSensor.GetOutputMode().XRes;
            }
        }

        public int Height
        {
            get
            {
                return NuitrackManager.DepthSensor.GetOutputMode().YRes;
            }
        }

        public float AspectRatio
        {
            get
            {
                OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();

                return (float)mode.XRes / mode.YRes;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (sourceDataBuffer != null)
            {
                sourceDataBuffer.Release();
                sourceDataBuffer = null;
            }

            if (gradientsBuffer != null)
            {
                gradientsBuffer.Dispose();
                gradientsBuffer = null;
            }

            depthDataArray = null;
            depthArray = null;
            mirrorArray = null;
        }

        Texture2D GetCPUTexture(DepthFrame frame, TextureCache textureCache, Gradient gradient)
        {
            ref Texture2D destTexture = ref textureCache.texture2D;

            if (frame.Timestamp == textureCache.timeStamp && textureCache.texture2D != null)
                return textureCache.texture2D;
            else
            {
                int datasize = frame.DataSize;

                if (depthArray == null)
                    depthArray = new byte[datasize];

                if(mirrorArray == null)
                    mirrorArray = new byte[frame.Cols * frame.Rows * 4];

                if(gradient == null)
                    gradient = defaultGradient;

                if (gradientCache == null || !gradientCache.Equals(gradient))
                    gradientCache = new GradientCache(gradient);

                Marshal.Copy(frame.Data, depthArray, 0, frame.DataSize);

                float depthDivisor = 1f / (1000.0f * maxDepthSensor);

                //The transformation is performed with the image reflected vertically.
                for (int i = 0, pxl = 0; i < datasize; i += 2, pxl++)
                {
                    float normalDepth = (depthArray[i + 1] << 8 | depthArray[i]) * depthDivisor;

                    if (normalDepth == 0)
                        normalDepth = 1;

                    Color depthColor = gradientCache.Evaluate(normalDepth);

                    int ptr = (frame.Cols * (frame.Rows - (pxl / frame.Cols) - 1) + pxl % frame.Cols) * 4;

                    mirrorArray[ptr] = (byte)(255f * depthColor.a);         // A
                    mirrorArray[ptr + 1] = (byte)(255f * depthColor.r);     // R
                    mirrorArray[ptr + 2] = (byte)(255f * depthColor.g);     // G
                    mirrorArray[ptr + 3] = (byte)(255f * depthColor.b);     // B
                }

                if (destTexture == null)
                    destTexture = new Texture2D(frame.Cols, frame.Rows, TextureFormat.ARGB32, false);

                destTexture.LoadRawTextureData(mirrorArray);
                destTexture.Apply();

                textureCache.timeStamp = frame.Timestamp;

                return destTexture;
            }
        }

        RenderTexture GetGPUTexture(DepthFrame frame, TextureCache textureCache, Gradient gradient)
        {
            ref RenderTexture destTexture = ref textureCache.renderTexture;

            if (frame.Timestamp == textureCache.timeStamp && textureCache.renderTexture != null)
                return textureCache.renderTexture;
            else
            {
                textureCache.timeStamp = frame.Timestamp;

                if (instanceShader == null)
                {
                    InitShader("Depth2Texture");
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
                    instanceShader.SetBuffer(kernelIndex, "DepthFrame", sourceDataBuffer);
                }

                if (gradient == null)
                    gradient = defaultGradient;

                if (gradientsBuffer == null)
                    gradientsBuffer = new GradientBuffer(gradient);
                else if (!gradientsBuffer.Equals(gradient))
                {
                    gradientsBuffer.Dispose();
                    gradientsBuffer = new GradientBuffer(gradient);
                }

                instanceShader.SetBuffer(kernelIndex, "gradientColors", gradientsBuffer.ColorBuffer);
                instanceShader.SetBuffer(kernelIndex, "gradientAlpha", gradientsBuffer.AlphaBuffer);
                instanceShader.SetBool("fixedMode", gradient.mode == GradientMode.Fixed);

                if (destTexture == null)
                    destTexture = InitRenderTexture(frame.Cols, frame.Rows);

                if(depthDataArray == null)
                    depthDataArray = new byte[frame.DataSize];

                instanceShader.SetTexture(kernelIndex, "Result", destTexture);

                Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
                sourceDataBuffer.SetData(depthDataArray);

                instanceShader.SetFloat("maxDepthSensor", maxDepthSensor);
                instanceShader.Dispatch(kernelIndex, destTexture.width / (int)x, destTexture.height / (int)y, (int)z);

                return destTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetRenderTexture(T, TextureCache)"/> 
        /// </summary>
        /// <returns>DepthFrame converted to RenderTexture</returns>
        public override RenderTexture GetRenderTexture(DepthFrame frame, TextureCache textureCache = null)
        {
            return GetRenderTexture(frame, defaultGradient, textureCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetRenderTexture(T, TextureCache)"/> 
        /// </summary>
        /// <param name="frame">Source Depth Frame</param>
        /// <param name="gradient"> Gradient for depth coloring, where the light at 0 corresponds to the nearest 
        /// point to the sensor, at 1 further point from the sensor.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>DepthFrame converted to RenderTexture</returns>
        public RenderTexture GetRenderTexture(DepthFrame frame, Gradient gradient, TextureCache textureCache = null)
        {
            if (frame == null)
            {
                CheckModuleDisableIssue(NuitrackManager.Instance.UseDepthModule, "Depth");
                return null;
            }

            if (GPUSupported)
                return GetGPUTexture(frame, textureCache ?? localCache, gradient);
            else
            {
                TextureCache cache = textureCache ?? localCache;

                cache.texture2D = GetCPUTexture(frame, cache, gradient);
                FrameUtils.TextureUtils.Copy(cache.texture2D, ref cache.renderTexture);

                return cache.renderTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T, TextureCache)"/> 
        /// </summary>
        /// <returns>DepthFrame converted to Texture2D</returns>
        public override Texture2D GetTexture2D(DepthFrame frame, TextureCache textureCache = null)
        {
            return GetTexture2D(frame, defaultGradient, textureCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T, TextureCache)"/> 
        /// </summary>
        /// <param name="frame">Source Depth Frame</param>
        /// <param name="gradient"> Gradient for depth coloring, where the light at 0 corresponds to the nearest 
        /// point to the sensor, at 1 further point from the sensor.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>DepthFrame converted to Texture2D</returns>
        public Texture2D GetTexture2D(DepthFrame frame, Gradient gradient, TextureCache textureCache = null)
        {
            if (frame == null)
            {
                CheckModuleDisableIssue(NuitrackManager.Instance.UseDepthModule, "Depth");
                return null;
            }

            if (GPUSupported)
            {
                TextureCache cache = textureCache ?? localCache;

                cache.renderTexture = GetGPUTexture(frame, cache, gradient);
                FrameUtils.TextureUtils.Copy(cache.renderTexture, ref cache.texture2D);
                return cache.texture2D;
            }
            else
                return GetCPUTexture(frame, textureCache ?? localCache, gradient);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture(T, TextureCache)"/>
        /// </summary>
        /// <param name="frame">Source Depth Frame</param>
        /// <param name="gradient"> Gradient for depth coloring, where the light at 0 corresponds to the nearest 
        /// point to the sensor, at 1 further point from the sensor.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>Texture = (RenderTexture or Texture2D)</returns>
        public Texture GetTexture(DepthFrame frame, Gradient gradient, TextureCache textureCache = null)
        {
            if (GPUSupported)
                return GetRenderTexture(frame, gradient, textureCache);
            else
                return GetTexture2D(frame, gradient, textureCache);
        }
    }
}
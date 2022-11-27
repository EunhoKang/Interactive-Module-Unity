using UnityEngine;
using System;

using nuitrack;

namespace NuitrackSDK.Frame
{
    [System.Serializable]
    public class TextureCache : IDisposable
    {
        public RenderTexture renderTexture;
        public Texture2D texture2D;

        public ulong timeStamp;

        public void Dispose()
        {
            if (renderTexture != null)
                UnityEngine.Object.Destroy(renderTexture);

            if(texture2D != null)
                UnityEngine.Object.Destroy(texture2D);
        }
    }

    public abstract class FrameToTexture<T, U> : MonoBehaviour
        where T : Frame<U>
        where U : struct
    {
#if UNITY_EDITOR
        bool DEBUG_USE_CPU = false;
#endif

        [SerializeField] ComputeShader computeShader;
        protected ComputeShader instanceShader;

        protected TextureCache localCache = new TextureCache();

        protected Rect rect;

        protected uint x, y, z;
        protected int kernelIndex;

        protected bool GPUSupported
        {
            get
            {
#if UNITY_EDITOR
                return SystemInfo.supportsComputeShaders && !DEBUG_USE_CPU;
#else
                return SystemInfo.supportsComputeShaders;
#endif
            }
        }

        protected void CheckModuleDisableIssue(bool useModule, string moduleName)
        {
            if (!useModule)
                Debug.LogError(string.Format("{0} module is disabled! Enable it on the Nuitrack Manager component", moduleName));
        }

        protected void InitShader(string kernelName)
        {
            if (!GPUSupported)
            {
#if UNITY_EDITOR && !UNITY_STANDALONE
                Debug.LogWarning("Compute shaders are not supported for the Android platform in the editor.\n" +
                    "A software conversion will be used (may cause performance issues)\n" +
                    "Switch the platform to Standalone (this is not relevant for the assembled project).");
#else
                Debug.LogWarning("Compute shaders are not supported. A software conversion will be used (may cause performance issues).");
#endif
            }

            instanceShader = Instantiate(computeShader);
            kernelIndex = instanceShader.FindKernel(kernelName);
            instanceShader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        }

        protected RenderTexture InitRenderTexture(int width, int height)
        {
            RenderTexture renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            return renderTexture;
        }

        /// <summary>
        /// Get the frame as a RenderTexture. 
        /// Recommended method for platforms with ComputeShader support.
        /// </summary>
        /// <param name="SourceFrame">Source frame of nuitrack.Frame</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>Frame converted to RenderTexture</returns>
        public abstract RenderTexture GetRenderTexture(T SourceFrame, TextureCache textureCache = null);

        /// <summary>
        /// Get a frame in the form of Texture2D. 
        /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
        /// If possible, use GetRenderTexture.
        /// </summary>
        /// <param name="SourceFrame">Source frame of nuitrack.Frame</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>Frame converted to Texture2D</returns>
        public abstract Texture2D GetTexture2D(T SourceFrame, TextureCache textureCache = null);

        /// <summary>
        /// Convert Frame to Texture. 
        /// The method will select the most productive way to get the texture. 
        /// This can be either RenderTexture or Texture2D. 
        /// Use this method if you don't care about the texture type.
        /// </summary>
        /// <param name="SourceFrame">Source frame of nuitrack.Frame</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>Texture = (RenderTexture or Texture2D)</returns>
        public virtual Texture GetTexture(T SourceFrame, TextureCache textureCache = null)
        {
            if (GPUSupported)
                return GetRenderTexture(SourceFrame, textureCache);
            else
                return GetTexture2D(SourceFrame, textureCache);
        }

        protected virtual void OnDestroy()
        {
            if (instanceShader != null)
                Destroy(instanceShader);

            if (localCache != null)
            {
                localCache.Dispose();
                localCache = null;
            }
        }
    }
}
using UnityEngine;
using System.Runtime.InteropServices;

using NuitrackSDK.Frame;


namespace NuitrackSDK.Tutorials.ARNuitrack
{
    [AddComponentMenu("NuitrackSDK/Tutorials/AR Nuitrack/AR Nuitrack")]
    public class ARNuitrack : MonoBehaviour
    {
        ulong frameTimestamp;

        [Header("RGB shader")]
        Texture2D rgbTexture2D;

        [Header("Mesh generator")]
        [SerializeField] MeshGenerator meshGenerator;
        [SerializeField] new Camera camera;

        ComputeBuffer depthDataBuffer;
        byte[] depthDataArray = null;

        void Update()
        {
            nuitrack.ColorFrame colorFrame = NuitrackManager.ColorFrame;
            nuitrack.DepthFrame depthFrame = NuitrackManager.DepthFrame;

            if (colorFrame == null || depthFrame == null || frameTimestamp == depthFrame.Timestamp)
                return;

            frameTimestamp = depthFrame.Timestamp;

            if (meshGenerator.Mesh == null)
                meshGenerator.Generate(depthFrame.Cols, depthFrame.Rows);

            UpdateRGB(colorFrame);
            UpdateHeightMap(depthFrame);
            FitMeshIntoFrame(depthFrame);
        }

        void UpdateRGB(nuitrack.ColorFrame frame)
        {
            if (rgbTexture2D == null)
            {
                rgbTexture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
                meshGenerator.Material.SetTexture("_MainTex", rgbTexture2D);
            }

            rgbTexture2D.LoadRawTextureData(frame.Data, frame.DataSize);
            rgbTexture2D.Apply();
        }

        void FitMeshIntoFrame(nuitrack.DepthFrame frame)
        {
            float frameAspectRatio = (float)frame.Cols / frame.Rows;
            float targetAspectRatio = camera.aspect < frameAspectRatio ? camera.aspect : frameAspectRatio;

            float vAngle = camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
            float scale = Vector3.Distance(meshGenerator.transform.position, camera.transform.position) * Mathf.Tan(vAngle) * targetAspectRatio;

            meshGenerator.transform.localScale = new Vector3(scale * 2, scale * 2, 1);
        }

        void UpdateHeightMap(nuitrack.DepthFrame frame)
        {
            if (depthDataBuffer == null)
            {
                //We put the source data in the buffer, but the buffer does not support types
                //that take up less than 4 bytes(instead of ushot(Int16), we specify uint(Int32))
                depthDataBuffer = new ComputeBuffer(frame.DataSize / 2, sizeof(uint));
                meshGenerator.Material.SetBuffer("_DepthFrame", depthDataBuffer);

                meshGenerator.Material.SetInt("_textureWidth", frame.Cols);
                meshGenerator.Material.SetInt("_textureHeight", frame.Rows);

                depthDataArray = new byte[frame.DataSize];
            }

            Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
            depthDataBuffer.SetData(depthDataArray);

            meshGenerator.Material.SetFloat("_maxDepthSensor", FrameUtils.DepthToTexture.MaxSensorDepth);
            meshGenerator.transform.localPosition = Vector3.forward * FrameUtils.DepthToTexture.MaxSensorDepth;

            Vector3 localCameraPosition = meshGenerator.transform.InverseTransformPoint(camera.transform.position);
            meshGenerator.Material.SetVector("_CameraPosition", localCameraPosition);
        }

        private void OnDestroy()
        {
            if (rgbTexture2D != null)
                Destroy(rgbTexture2D);

            if (depthDataBuffer != null)
                depthDataBuffer.Release();
        }
    }
}
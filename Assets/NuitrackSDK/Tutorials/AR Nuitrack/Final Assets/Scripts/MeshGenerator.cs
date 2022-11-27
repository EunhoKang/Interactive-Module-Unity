using UnityEngine;
using System.Collections.Generic;


namespace NuitrackSDK.Tutorials.ARNuitrack
{
    [AddComponentMenu("NuitrackSDK/Tutorials/AR Nuitrack/Mesh Generator")]
    public class MeshGenerator : MonoBehaviour
    {
        [SerializeField] MeshRenderer meshRenderer;
        [SerializeField] MeshFilter meshFilter;

        Mesh mesh;
        List<Vector3> points = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        public Material Material
        {
            get
            {
                return meshRenderer.material;
            }
        }

        public Mesh Mesh
        {
            get
            {
                return mesh;
            }
        }

        public void Generate(int width, int height)
        {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            GeneratePointsAndUVS(width, height);
            GenerateTriangles(width, height);

            meshFilter.mesh = mesh;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        void GeneratePointsAndUVS(int width, int height)
        {
            float aspectRatio = (float)width / height;

            for (int h = 0; h < height + 1; h++)
            {
                for (int w = 0; w < width + 1; w++)
                {
                    float wi = (float)w / width;
                    float he = (float)h / height;

                    Vector2 uv = new Vector2(wi, 1 - he);
                    uvs.Add(uv);

                    if (aspectRatio > 1)
                        he = (he - 0.5f) / aspectRatio + 0.5f;  // he = he / aspectRatio + (0.5f - 0.5f / aspectRatio); // he = he / aspectRatio + (1 - 1 / aspectRatio) / 2
                    else
                        wi = aspectRatio * (wi - 0.5f) + 0.5f;  // wi = wi * aspectRatio + (0.5f - 0.5f * aspectRatio);

                    Vector3 point = new Vector3(wi - 0.5f, he - 0.5f, 0);
                    points.Add(point);
                }
            }

            mesh.SetVertices(points);
            mesh.SetUVs(0, uvs);
        }

        void GenerateTriangles(int width, int height)
        {
            int totalWidth = width + 1;

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    triangles.Add(totalWidth * h + w);
                    triangles.Add(totalWidth * (h + 1) + w);
                    triangles.Add(totalWidth * (h + 1) + (w + 1));

                    triangles.Add(totalWidth * (h + 1) + (w + 1));
                    triangles.Add(totalWidth * h + (w + 1));
                    triangles.Add(totalWidth * h + w);
                }
            }

            mesh.SetTriangles(triangles, 0);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0)), transform.TransformPoint(new Vector3(0.5f, 0.5f, 0)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(0.5f, 0.5f, 0)), transform.TransformPoint(new Vector3(0.5f, -0.5f, 0)));

            Gizmos.DrawLine(transform.TransformPoint(new Vector3(0.5f, -0.5f, 0)), transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0)), transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0)));
        }
    }
}
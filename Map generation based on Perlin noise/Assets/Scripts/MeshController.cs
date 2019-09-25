using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshController
{
    public static MeshDetails GenerateMesh(float[,] noiseArea, float multiplier, AnimationCurve curve, int lod)
    {
        int resolution = noiseArea.GetLength(0);
        float topLeftX = (resolution - 1) / -2f;
        float topLeftZ = (resolution - 1) / 2f;
        int lodIncrement = lod == 0 ? 1 : lod * 2;
        int verticesInRow = (resolution - 1) / lodIncrement + 1;
        MeshDetails meshDetails = new MeshDetails(resolution);
        int currentVertexIndex = 0;
        AnimationCurve mapHeightCurve = new AnimationCurve(curve.keys);

        for(int yIndex = 0; yIndex < resolution; yIndex += lodIncrement)
        {
            for(int xIndex = 0; xIndex < resolution; xIndex += lodIncrement)
            {
                meshDetails.vertices[currentVertexIndex] = new Vector3(topLeftX + xIndex, mapHeightCurve.Evaluate(noiseArea[xIndex, yIndex]) * multiplier, topLeftZ - yIndex);
                meshDetails.uvs[currentVertexIndex] = new Vector2(xIndex / (float)resolution, yIndex / (float)resolution);

                if(xIndex < resolution - 1 && yIndex < resolution - 1)
                {
                    meshDetails.AddTriangle(currentVertexIndex, currentVertexIndex + verticesInRow + 1, currentVertexIndex + verticesInRow);
                    meshDetails.AddTriangle(currentVertexIndex + verticesInRow + 1, currentVertexIndex, currentVertexIndex + 1);
                }

                currentVertexIndex++;
            }
        }

        return meshDetails;
    }
}

public class MeshDetails
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    private int currentTriangleIndex;

    public MeshDetails(int resolution)
    {
        vertices = new Vector3[resolution * resolution];
        triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        uvs = new Vector2[resolution * resolution];
    }

    public void AddTriangle(int vertexIndexA, int vertexIndexB, int vertexIndexC)
    {
        triangles[currentTriangleIndex] = vertexIndexA;
        triangles[currentTriangleIndex + 1] = vertexIndexB;
        triangles[currentTriangleIndex + 2] = vertexIndexC;

        currentTriangleIndex += 3;
    }

    public Mesh BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }
}
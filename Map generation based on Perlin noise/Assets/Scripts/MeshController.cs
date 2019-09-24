using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshController
{
    public static MeshDetails GenerateMesh(float[,] noiseArea)
    {
        int resolution = noiseArea.GetLength(0);
        float topLeftX = (resolution - 1) / -2f;
        float topLeftZ = (resolution - 1) / 2f;
        MeshDetails meshDetails = new MeshDetails(resolution);
        int currentVertexIndex = 0;

        for(int yIndex = 0; yIndex < resolution; yIndex++)
        {
            for(int xIndex = 0; xIndex < resolution; xIndex++)
            {
                meshDetails.vertices[currentVertexIndex] = new Vector3(topLeftX + xIndex, noiseArea[xIndex, yIndex], topLeftZ - yIndex);
                meshDetails.uvs[currentVertexIndex] = new Vector2(xIndex / (float)resolution, yIndex / (float)resolution);

                if(xIndex < resolution - 1 && yIndex < resolution - 1)
                {
                    meshDetails.AddTriangle(currentVertexIndex, currentVertexIndex + resolution + 1, currentVertexIndex + resolution);
                    meshDetails.AddTriangle(currentVertexIndex + resolution + 1, currentVertexIndex, currentVertexIndex + 1);
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
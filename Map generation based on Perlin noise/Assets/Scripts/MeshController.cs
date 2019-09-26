using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshController
{
    public static MeshDetails GenerateMesh(float[,] noiseArea, float multiplier, AnimationCurve curve, int lod)
    {
        int lodIncrement = lod == 0 ? 1 : lod * 2;

        int frontierResolution = noiseArea.GetLength(0);
        int meshResolution = frontierResolution - 2 * lodIncrement;
        int meshResolutionUnnormalized = frontierResolution - 2;

        float topLeftX = (meshResolutionUnnormalized - 1) / -2f;
        float topLeftZ = (meshResolutionUnnormalized - 1) / 2f;

        int verticesInRow = (meshResolution - 1) / lodIncrement + 1;

        MeshDetails meshDetails = new MeshDetails(verticesInRow);
        AnimationCurve mapHeightCurve = new AnimationCurve(curve.keys);

        int[,] vertexIndexes = new int[frontierResolution, frontierResolution];
        int meshVertexIndex = 0;
        int frontierVertexIndex = -1;

        for(int yIndex = 0; yIndex < frontierResolution; yIndex += lodIncrement)
        {
            for(int xIndex = 0; xIndex < frontierResolution; xIndex += lodIncrement)
            {
                bool isBorder = yIndex == 0 || xIndex == 0 || yIndex == frontierResolution - 1 || xIndex == frontierResolution - 1;

                if(isBorder)
                {
                    vertexIndexes[xIndex, yIndex] = frontierVertexIndex;
                    frontierVertexIndex--;
                }
                else
                {
                    vertexIndexes[xIndex, yIndex] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for(int yIndex = 0; yIndex < frontierResolution; yIndex += lodIncrement)
        {
            for(int xIndex = 0; xIndex < frontierResolution; xIndex += lodIncrement)
            {
                int vertexIndex = vertexIndexes[xIndex, yIndex];

                Vector2 uv = new Vector2((xIndex - lodIncrement) / (float)meshResolution, (yIndex - lodIncrement) / (float)meshResolution);
                Vector3 vertexCoords = new Vector3(topLeftX + uv.x * meshResolutionUnnormalized, mapHeightCurve.Evaluate(noiseArea[xIndex, yIndex]) * multiplier, topLeftZ - uv.y * meshResolutionUnnormalized);

                meshDetails.AddVertex(vertexCoords, uv, vertexIndex);

                if(xIndex < frontierResolution - 1 && yIndex < frontierResolution - 1)
                {
                    int vertexAIndex = vertexIndexes[xIndex, yIndex];
                    int vertexBIndex = vertexIndexes[xIndex + lodIncrement, yIndex];
                    int vertexCIndex = vertexIndexes[xIndex, yIndex + lodIncrement];
                    int vertexDIndex = vertexIndexes[xIndex + lodIncrement, yIndex + lodIncrement];

                    meshDetails.AddTriangle(vertexAIndex, vertexDIndex, vertexCIndex);
                    meshDetails.AddTriangle(vertexDIndex, vertexAIndex, vertexBIndex);
                }

                vertexIndex++;
            }
        }

        meshDetails.BuildNormals();

        return meshDetails;
    }
}

public class MeshDetails
{
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private Vector3[] frontierVertices;
    private int[] frontierTriangles;
    private Vector3[] normals;

    private int currentTriangleIndex;
    private int currentFrontierTriangleIndex;

    public MeshDetails(int resolution)
    {
        vertices = new Vector3[resolution * resolution];
        triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        uvs = new Vector2[resolution * resolution];

        frontierVertices = new Vector3[resolution * 4 + 4];
        frontierTriangles = new int[24 * resolution];
    }

    public void AddTriangle(int vertexIndexA, int vertexIndexB, int vertexIndexC)
    {
        if(vertexIndexA < 0 || vertexIndexB < 0 || vertexIndexC < 0)
        {
            frontierTriangles[currentFrontierTriangleIndex] = vertexIndexA;
            frontierTriangles[currentFrontierTriangleIndex + 1] = vertexIndexB;
            frontierTriangles[currentFrontierTriangleIndex + 2] = vertexIndexC;

            currentFrontierTriangleIndex += 3;
        }
        else
        {
            triangles[currentTriangleIndex] = vertexIndexA;
            triangles[currentTriangleIndex + 1] = vertexIndexB;
            triangles[currentTriangleIndex + 2] = vertexIndexC;

            currentTriangleIndex += 3;
        }
    }

    public void AddVertex(Vector3 vertexCoords, Vector2 uv, int vertexIndex)
    {
        if(vertexIndex < 0)
        {
            frontierVertices[-vertexIndex - 1] = vertexCoords;
        }
        else
        {
            vertices[vertexIndex] = vertexCoords;
            uvs[vertexIndex] = uv;
        }
    }

    public void BuildNormals()
    {
        normals = RecomputeNormals();
    }

    public Mesh BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }

    private Vector3[] RecomputeNormals()
    {
        Vector3[] normals = new Vector3[vertices.Length];
        int trianglesAmount = triangles.Length / 3;

        for(int index = 0; index < trianglesAmount; index++)
        {
            int triangleIndex = index * 3;

            int vertexAIndex = triangles[triangleIndex];
            int vertexBIndex = triangles[triangleIndex + 1];
            int vertexCIndex = triangles[triangleIndex + 2];

            Vector3 normal = TriangleNormal(vertexAIndex, vertexBIndex, vertexCIndex);

            normals[vertexAIndex] += normal;
            normals[vertexBIndex] += normal;
            normals[vertexCIndex] += normal;
        }

        int frontierTrianglesAmount = frontierTriangles.Length / 3;

        for(int index = 0; index < frontierTrianglesAmount; index++)
        {
            int triangleIndex = index * 3;

            int vertexAIndex = frontierTriangles[triangleIndex];
            int vertexBIndex = frontierTriangles[triangleIndex + 1];
            int vertexCIndex = frontierTriangles[triangleIndex + 2];

            Vector3 normal = TriangleNormal(vertexAIndex, vertexBIndex, vertexCIndex);
            
            if(vertexAIndex > 0)
            {
                normals[vertexAIndex] += normal;
            }
            if (vertexBIndex > 0)
            {
                normals[vertexBIndex] += normal;
            }
            if (vertexCIndex > 0)
            {
                normals[vertexCIndex] += normal;
            }
        }

        for(int index = 0; index < normals.Length; index++)
        {
            normals[index].Normalize();
        }

        return normals;
    }

    private Vector3 TriangleNormal(int indexA, int indexB, int indexC)
    {
        Vector3 vertexA = indexA < 0 ? frontierVertices[-indexA - 1] : vertices[indexA];
        Vector3 vertexB = indexB < 0 ? frontierVertices[-indexB - 1] : vertices[indexB];
        Vector3 vertexC = indexC < 0 ? frontierVertices[-indexC - 1] : vertices[indexC];

        Vector3 edgeAB = vertexB - vertexA;
        Vector3 edgeAC = vertexC - vertexA;

        return Vector3.Cross(edgeAB, edgeAC).normalized;
    }
}
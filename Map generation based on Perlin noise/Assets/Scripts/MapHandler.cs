using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapHandler : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DisplayMap(Texture2D texture)
    {
        // To avoid clicking Play button all the time I could apply the texture to the shared material to see effect in the editor
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width , 1, texture.height);
    }

    public void DisplayMesh(MeshDetails meshDetails, Texture2D texture)
    {
        meshFilter.sharedMesh = meshDetails.BuildMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}

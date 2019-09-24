using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapHandler : MonoBehaviour
{
    public Renderer renderer;

    public void DisplayMap(Texture2D texture)
    {

        // To avoid clicking Play button all the time I could apply the texture to the shared material to see effect in the editor
        renderer.sharedMaterial.mainTexture = texture;
        renderer.transform.localScale = new Vector3(texture.width , 1, texture.height);
    }
}

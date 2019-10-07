using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapHandler : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public enum DisplayMode { Noise, Mesh, Falloff };

    public NoiseAreaSettings noiseAreaSettings;
    public AreaSettings areaSettings;
    public TextureAsset textureAsset;
    public Material areaMaterial;

    [Space(10)]

    public DisplayMode displayMode;
    [Range(0, AreaSettings.availableLODS - 1)] public int previevLOD = 1;

    [Space(10)]

    public bool autoUpdate = true;

    private void OnValidate()
    {
        if (areaSettings != null)
        {
            areaSettings.onDataUpdate -= OnDataUpdate;
            areaSettings.onDataUpdate += OnDataUpdate;
        }

        if (noiseAreaSettings != null)
        {
            noiseAreaSettings.onDataUpdate -= OnDataUpdate;
            noiseAreaSettings.onDataUpdate += OnDataUpdate;
        }

        if (textureAsset != null)
        {
            textureAsset.onDataUpdate -= OnTextureUpdate;
            textureAsset.onDataUpdate += OnTextureUpdate;
        }
    }

    public void DisplayInEditor()
    {
        textureAsset.AttachToMaterial(areaMaterial);
        textureAsset.RefreshHeights(areaMaterial, noiseAreaSettings.minHeight, noiseAreaSettings.maxHeight);

        NoiseArea noiseArea = NoiseController.BuildNoiseArea(areaSettings.verticesPerLine, noiseAreaSettings, Vector3.zero);

        if (displayMode == DisplayMode.Noise)
        {
            DisplayMap(TextureController.GenerateFromNoise(noiseArea));
        }
        else if (displayMode == DisplayMode.Mesh)
        {
            DisplayMesh(MeshController.GenerateMesh(noiseArea.values, previevLOD, areaSettings));
        }
        else if (displayMode == DisplayMode.Falloff)
        {
            DisplayMap(TextureController.GenerateFromNoise(new NoiseArea(FalloffController.GenerateFalloffArea(areaSettings.verticesPerLine), 0, 1)));
        }
    }

    public void DisplayMap(Texture2D texture)
    {
        // To avoid clicking Play button all the time I could apply the texture to the shared material to see effect in the editor
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width , 1, texture.height) / 10f;

        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DisplayMesh(MeshDetails meshDetails)
    {
        meshFilter.sharedMesh = meshDetails.BuildMesh();

        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    private void OnDataUpdate()
    {
        if (!Application.isPlaying)
        {
            DisplayInEditor();
        }
    }

    private void OnTextureUpdate()
    {
        textureAsset.AttachToMaterial(areaMaterial);
    }
}

using UnityEngine;

public class MapHandler : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public enum DisplayMode { Noise, Mesh, Falloff };

    public AreaNoiseDetails areaNoiseDetails;
    public AreaDetails areaDetails;
    public TextureDetails textureDetails;
    public Material areaMaterial;

    [Space(10)]

    public DisplayMode displayMode;
    [Range(0, AreaDetails.availableLODs - 1)] public int previevLOD = 1;

    [Space(10)]

    public bool autoUpdate = true;

    private void OnValidate()
    {
        if (areaDetails != null)
        {
            areaDetails.onDataUpdate -= OnDataUpdate;
            areaDetails.onDataUpdate += OnDataUpdate;
        }

        if (areaNoiseDetails != null)
        {
            areaNoiseDetails.onDataUpdate -= OnDataUpdate;
            areaNoiseDetails.onDataUpdate += OnDataUpdate;
        }

        if (textureDetails != null)
        {
            textureDetails.onDataUpdate -= OnTextureUpdate;
            textureDetails.onDataUpdate += OnTextureUpdate;
        }
    }

    public void DisplayInEditor()
    {
        textureDetails.AttachToMaterial(areaMaterial);
        textureDetails.RefreshHeights(areaMaterial, areaNoiseDetails.minHeight, areaNoiseDetails.maxHeight);

        AreaNoise noiseArea = NoiseController.BuildNoiseArea(areaDetails.verticesPerLine, areaNoiseDetails, Vector2.zero);

        if (displayMode == DisplayMode.Noise)
        {
            DisplayMap(TextureController.GenerateFromNoise(noiseArea));
        }
        else if (displayMode == DisplayMode.Mesh)
        {
            DisplayMesh(MeshController.BuildMesh(noiseArea.area, previevLOD, areaDetails));
        }
        else if (displayMode == DisplayMode.Falloff)
        {
            DisplayMap(TextureController.GenerateFromNoise(new AreaNoise(FalloffController.GenerateFalloffArea(areaDetails.verticesPerLine), 0, 1)));
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
        textureDetails.AttachToMaterial(areaMaterial);
    }
}

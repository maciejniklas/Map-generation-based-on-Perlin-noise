using System.Collections.Generic;
using UnityEngine;

public class AreasController : MonoBehaviour
{
    private const float playerDistanceToUpdate = 25f;
    private const float sqrPlayerDistanceToUpdate = playerDistanceToUpdate * playerDistanceToUpdate;

    public Transform player;
    public Material material;
    public int colliderLODInedx;
    public LODDetails[] lodDetails;
    public AreaNoiseDetails areaNoiseDetails;
    public AreaDetails areaDetails;
    public TextureDetails textureDetails;

    private float areaResolution;
    private int visibleAreas;
    private Dictionary<Vector2, Area> areasCollection = new Dictionary<Vector2, Area>();
    private Vector2 lastPlayerPosition;

    public Vector2 playerPosition;
    private List<Area> visibleAreasCollection = new List<Area>();

    private void Start()
    {
        textureDetails.AttachToMaterial(material);
        textureDetails.RefreshHeights(material, areaNoiseDetails.minHeight, areaNoiseDetails.maxHeight);

        areaResolution = areaDetails.resolution;
        float viewRange = lodDetails[lodDetails.Length - 1].distance;
        visibleAreas = Mathf.RoundToInt(viewRange / areaResolution);

        UpdateVisibleAreas();
    }

    private void Update()
    {
        playerPosition = new Vector2(player.position.x, player.position.z);

        if(playerPosition != lastPlayerPosition)
        {
            foreach(Area area in visibleAreasCollection)
            {
                area.UpdateCollider();
            }
        }

        if((lastPlayerPosition - playerPosition).sqrMagnitude > sqrPlayerDistanceToUpdate)
        {
            lastPlayerPosition = playerPosition;
            UpdateVisibleAreas();
        }
    }

    private void OnAreaVisibilityChange(Area area, bool visibility)
    {
        if(visibility)
        {
            visibleAreasCollection.Add(area);
        }
        else
        {
            visibleAreasCollection.Remove(area);
        }
    }

    private void UpdateVisibleAreas()
    {
        HashSet<Vector2> updatedAreasCoords = new HashSet<Vector2>();

        for(int index = visibleAreasCollection.Count - 1; index >= 0; index--)
        {
            updatedAreasCoords.Add(visibleAreasCollection[index].coords);
            visibleAreasCollection[index].UpdateArea();
        }

        int areaCoordX = Mathf.RoundToInt(playerPosition.x / areaResolution);
        int areaCoordY = Mathf.RoundToInt(playerPosition.y / areaResolution);

        for(int yIndex = -visibleAreas; yIndex <= visibleAreas; yIndex++)
        {
            for(int xIndex = -visibleAreas; xIndex <= visibleAreas; xIndex ++)
            {
                Vector2 areaCoords = new Vector2(areaCoordX + xIndex, areaCoordY + yIndex);

                if(!updatedAreasCoords.Contains(areaCoords))
                {
                    if (areasCollection.ContainsKey(areaCoords))
                    {
                        areasCollection[areaCoords].UpdateArea();
                    }
                    else
                    {
                        Area area = new Area(areaCoords, transform, material, lodDetails, colliderLODInedx, areaNoiseDetails, areaDetails, player);
                        areasCollection.Add(areaCoords, area);
                        area.onVisibilityChange += OnAreaVisibilityChange;
                        area.Load();
                    }
                }
            }
        }
    }
}

[System.Serializable]
public struct LODDetails
{
    [Range(0, AreaDetails.availableLODs - 1)] public int lod;
    public float distance;

    public float sqrDistance
    {
        get
        {
            return distance * distance;
        }
    }
};
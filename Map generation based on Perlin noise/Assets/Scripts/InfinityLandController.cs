using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinityLandController : MonoBehaviour
{
    private const float playerDistanceToUpdate = 25f;
    private const float sqrPlayerDistanceToUpdate = playerDistanceToUpdate * playerDistanceToUpdate;
    private const float playerPositionToColliderGeneration = 5;

    public Transform player;
    public Material material;
    public int colliderLODInedx;
    public LODDetails[] lodDetails;

    private int areaResolution;
    private int visibleAreas;
    private Dictionary<Vector2, Area> areasCollection = new Dictionary<Vector2, Area>();
    private Vector2 lastPlayerPosition;

    public static Vector2 playerPosition;
    public static float viewRange;
    private static MapController mapController;
    private static List<Area> visibleAreasCollection = new List<Area>();

    private void Start()
    {
        mapController = FindObjectOfType<MapController>();
        areaResolution = mapController.resolution - 1;
        viewRange = lodDetails[lodDetails.Length - 1].distance;
        visibleAreas = Mathf.RoundToInt(viewRange / areaResolution);

        UpdateVisibleAreas();
    }

    private void Update()
    {
        playerPosition = new Vector2(player.position.x, player.position.z) / mapController.areaAsset.scale;

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

    private void UpdateVisibleAreas()
    {
        HashSet<Vector2> updatedAreasCoords = new HashSet<Vector2>();

        for(int index = visibleAreasCollection.Count - 1; index >= 0; index--)
        {
            updatedAreasCoords.Add(visibleAreasCollection[index].coord);
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
                        areasCollection.Add(areaCoords, new Area(areaCoords, areaResolution, transform, material, lodDetails, colliderLODInedx));
                    }
                }
            }
        }
    }

    public class Area
    {
        public Vector2 coord;

        private Vector2 position;
        private GameObject instance;
        private Bounds bounds;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private LODDetails[] lodDetails;
        private LODMesh[] lodMeshes;
        private MapDetails mapDetails;
        private bool received;
        private int previousLODIndex = -1;
        private MeshCollider collider;
        private int colliderLODIndex;
        private bool hasCollider;

        public Area(Vector2 coords, int resolution, Transform parent, Material material, LODDetails[] lodDetails, int colliderLODIndex)
        {
            position = coords * resolution;

            Vector3 position3D = new Vector3(position.x, 0, position.y);

            instance = new GameObject("Area");
            instance.transform.position = position3D * mapController.areaAsset.scale;
            instance.transform.parent = parent;

            bounds = new Bounds(position, Vector2.one * resolution);

            SetVisible(false);

            mapController.RequestMapDetails(OnMapDetailsReceived, position);

            meshRenderer = instance.AddComponent<MeshRenderer>();
            meshFilter = instance.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            instance.transform.localScale = Vector3.one * mapController.areaAsset.scale;

            this.lodDetails = lodDetails;
            lodMeshes = new LODMesh[lodDetails.Length];

            for(int index = 0; index < lodDetails.Length; index++)
            {
                lodMeshes[index] = new LODMesh(lodDetails[index].lod);
                lodMeshes[index].update += UpdateArea;

                if(index == colliderLODIndex)
                {
                    lodMeshes[index].update += UpdateCollider;
                }
            }

            this.colliderLODIndex = colliderLODIndex;
            collider = instance.AddComponent<MeshCollider>();

            this.coord = coords;
        }

        public bool IsVisible()
        {
            return instance.activeSelf;
        }
        
        private void OnMapDetailsReceived(MapDetails mapDetails)
        {
            this.mapDetails = mapDetails;
            received = true;

            UpdateArea();
        }

        private void OnMeshDetailsReceived(MeshDetails meshDetails)
        {
            meshFilter.mesh = meshDetails.BuildMesh();
        }

        public void SetVisible(bool visible)
        {
            instance.SetActive(visible);
        }

        public void UpdateArea()
        {
            if(received)
            {
                float distanceToPlayer = Mathf.Sqrt(bounds.SqrDistance(playerPosition));

                bool wasVisible = IsVisible();
                bool visible = distanceToPlayer <= viewRange;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int index = 0; index < lodDetails.Length; index++)
                    {
                        if (distanceToPlayer > lodDetails[index].distance)
                        {
                            lodIndex = index + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];

                        if (lodMesh.received)
                        {
                            meshFilter.mesh = lodMesh.mesh;
                            previousLODIndex = lodIndex;
                        }
                        else if (!lodMesh.requested)
                        {
                            lodMesh.RequestMesh(mapDetails);
                        }
                    }

                    visibleAreasCollection.Add(this);
                }

                if(wasVisible != visible)
                {
                    if(visible)
                    {
                        visibleAreasCollection.Add(this);
                    }
                    else
                    {
                        visibleAreasCollection.Remove(this);
                    }

                    SetVisible(visible);
                }
            }
        }

        public void UpdateCollider()
        {
            if(!hasCollider)
            {
                float sqrPlayerDistanceToBound = bounds.SqrDistance(playerPosition);

                if (sqrPlayerDistanceToBound < lodDetails[colliderLODIndex].sqrDistance && !lodMeshes[colliderLODIndex].requested)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(mapDetails);
                }

                if (sqrPlayerDistanceToBound < playerPositionToColliderGeneration * playerPositionToColliderGeneration && lodMeshes[colliderLODIndex].received)
                {
                    collider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasCollider = true;
                } 
            }
        }
    }

    private class LODMesh
    {
        public Mesh mesh;
        public bool requested;
        public bool received;
        private int lod;
        public System.Action update;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        private void OnMeshDetailsReceived(MeshDetails meshDetails)
        {
            mesh = meshDetails.BuildMesh();
            received = true;
            update();
        }

        public void RequestMesh(MapDetails mapDetails)
        {
            requested = true;
            mapController.RequestMeshDetails(OnMeshDetailsReceived, mapDetails, lod);
        }
    }

    [System.Serializable]
    public struct LODDetails
    {
        [Range(0, MeshController.availableLODS - 1)] public int lod;
        public float distance;

        public float sqrDistance
        {
            get
            {
                return distance * distance;
            }
        }
    };
}
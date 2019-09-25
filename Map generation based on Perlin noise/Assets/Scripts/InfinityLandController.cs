using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinityLandController : MonoBehaviour
{
    private const float playerDistanceToUpdate = 25f;
    private const float sqrPlayerDistanceToUpdate = playerDistanceToUpdate * playerDistanceToUpdate;

    public Transform player;
    public Material material;
    public LODDetails[] lodDetails;

    private int areaResolution;
    private int visibleAreas;
    private Dictionary<Vector2, Area> areasCollection = new Dictionary<Vector2, Area>();
    private List<Area> lastUpdateareasCollection = new List<Area>();
    private Vector2 lastPlayerPosition;

    public static Vector2 playerPosition;
    public static float viewRange;
    private static MapController mapController;

    private void Start()
    {
        areaResolution = MapController.resolution - 1;
        viewRange = lodDetails[lodDetails.Length - 1].distance;
        visibleAreas = Mathf.RoundToInt(viewRange / areaResolution);
        mapController = FindObjectOfType<MapController>();

        UpdateVisibleAreas();
    }

    private void Update()
    {
        playerPosition = new Vector2(player.position.x, player.position.z);

        if((lastPlayerPosition - playerPosition).sqrMagnitude > sqrPlayerDistanceToUpdate)
        {
            lastPlayerPosition = playerPosition;
            UpdateVisibleAreas();
        }
    }

    private void UpdateVisibleAreas()
    {
        for(int index = 0; index < lastUpdateareasCollection.Count; index++)
        {
            lastUpdateareasCollection[index].SetVisible(false);
        }
        lastUpdateareasCollection.Clear();

        int areaCoordX = Mathf.RoundToInt(playerPosition.x / areaResolution);
        int areaCoordY = Mathf.RoundToInt(playerPosition.y / areaResolution);

        for(int yIndex = -visibleAreas; yIndex <= visibleAreas; yIndex++)
        {
            for(int xIndex = -visibleAreas; xIndex <= visibleAreas; xIndex ++)
            {
                Vector2 areaCoords = new Vector2(areaCoordX + xIndex, areaCoordY + yIndex);

                if(areasCollection.ContainsKey(areaCoords))
                {
                    areasCollection[areaCoords].UpdateArea();

                    if(areasCollection[areaCoords].IsVisible())
                    {
                        lastUpdateareasCollection.Add(areasCollection[areaCoords]);
                    }
                }
                else
                {
                    areasCollection.Add(areaCoords, new Area(areaCoords, areaResolution, transform, material, lodDetails));
                }
            }
        }
    }

    public class Area
    {
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


        public Area(Vector2 coords, int resolution, Transform parent, Material material, LODDetails[] lodDetails)
        {
            position = coords * resolution;

            Vector3 position3D = new Vector3(position.x, 0, position.y);

            instance = new GameObject("Area");
            instance.transform.position = position3D;
            instance.transform.parent = parent;

            bounds = new Bounds(position, Vector2.one * resolution);

            SetVisible(false);

            mapController.RequestMapDetails(OnMapDetailsReceived, position);

            meshRenderer = instance.AddComponent<MeshRenderer>();
            meshFilter = instance.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            this.lodDetails = lodDetails;
            lodMeshes = new LODMesh[lodDetails.Length];

            for(int index = 0; index < lodDetails.Length; index++)
            {
                lodMeshes[index] = new LODMesh(lodDetails[index].lod, UpdateArea);
            }
        }

        public bool IsVisible()
        {
            return instance.activeSelf;
        }
        
        private void OnMapDetailsReceived(MapDetails mapDetails)
        {
            this.mapDetails = mapDetails;
            received = true;

            Texture2D texture = TextureController.GenerateFromColors(mapDetails.mapColors, MapController.resolution);
            meshRenderer.material.mainTexture = texture;

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
                }

                SetVisible(visible);
            }
        }
    }

    private class LODMesh
    {
        public Mesh mesh;
        public bool requested;
        public bool received;
        private int lod;
        private System.Action update;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            update = updateCallback;
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
        public int lod;
        public float distance;
    };
}
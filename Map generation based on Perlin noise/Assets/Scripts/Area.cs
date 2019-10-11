using UnityEngine;

public class Area
{
    private const float playerDistanceToColliderGeneration = 5;

    public Vector2 coords;
    public event System.Action<Area, bool> onVisibilityChange;

    private Vector2 center;
    private GameObject instance;
    private Bounds bounds;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private LODDetails[] lodDetails;
    private LODMesh[] lodMeshes;
    private AreaNoise areaNoise;
    private bool received;
    private int previousLODIndex = -1;
    private MeshCollider collider;
    private int colliderLODIndex;
    private bool hasCollider;
    private AreaNoiseDetails areaNoiseDetails;
    private AreaDetails areaDetails;
    private Transform player;
    private float viewRange;

    private Vector2 playerPosition
    {
        get
        {
            return new Vector2(player.position.x, player.position.z);
        }
    }

    public Area(Vector2 coords, Transform parent, Material material, LODDetails[] lodDetails, int colliderLODIndex, AreaNoiseDetails areaNoiseDetails, AreaDetails areaDetails, Transform player)
    {
        this.areaNoiseDetails = areaNoiseDetails;
        this.areaDetails = areaDetails;

        center = coords * areaDetails.resolution / areaDetails.scale;

        Vector3 position = coords * areaDetails.resolution;

        instance = new GameObject("Area" + parent.childCount);
        instance.transform.position = new Vector3(position.x, 0, position.y);
        instance.transform.parent = parent;

        bounds = new Bounds(position, Vector2.one * areaDetails.resolution);

        SetVisible(false);

        meshRenderer = instance.AddComponent<MeshRenderer>();
        meshFilter = instance.AddComponent<MeshFilter>();
        meshRenderer.material = material;

        this.lodDetails = lodDetails;
        lodMeshes = new LODMesh[lodDetails.Length];

        for (int index = 0; index < lodDetails.Length; index++)
        {
            lodMeshes[index] = new LODMesh(lodDetails[index].lod);
            lodMeshes[index].update += UpdateArea;

            if (index == colliderLODIndex)
            {
                lodMeshes[index].update += UpdateCollider;
            }
        }

        this.colliderLODIndex = colliderLODIndex;
        collider = instance.AddComponent<MeshCollider>();

        this.coords = coords;
        this.player = player;
        viewRange = lodDetails[lodDetails.Length - 1].distance;
    }

    public bool IsVisible()
    {
        return instance.activeSelf;
    }

    private void OnAreaNoiseReceived(object areaNoise)
    {
        this.areaNoise = (AreaNoise)areaNoise;
        received = true;

        UpdateArea();
    }

    private void OnMeshDetailsReceived(MeshDetails meshDetails)
    {
        meshFilter.mesh = meshDetails.BuildMesh();
    }

    public void Load()
    {
        ThreadHandler.RequestDetails(() => NoiseController.BuildNoiseArea(areaDetails.verticesPerLine, areaNoiseDetails, center), OnAreaNoiseReceived);
    }

    public void SetVisible(bool visible)
    {
        instance.SetActive(visible);
    }

    public void UpdateArea()
    {
        if (received)
        {
            float distanceToPlayer = Mathf.Sqrt(bounds.SqrDistance(playerPosition));

            bool wasVisible = IsVisible();
            bool visible = distanceToPlayer <= viewRange;

            if (visible)
            {
                int lodIndex = 0;

                for (int index = 0; index < lodDetails.Length - 1; index++)
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
                        lodMesh.RequestMesh(areaNoise, areaDetails);
                    }
                }
            }

            if (wasVisible != visible)
            {
                SetVisible(visible);
                onVisibilityChange(this, visible);
            }
        }
    }

    public void UpdateCollider()
    {
        if (!hasCollider)
        {
            float sqrPlayerDistanceToBound = bounds.SqrDistance(playerPosition);

            if (sqrPlayerDistanceToBound < lodDetails[colliderLODIndex].sqrDistance && !lodMeshes[colliderLODIndex].requested)
            {
                lodMeshes[colliderLODIndex].RequestMesh(areaNoise, areaDetails);
            }

            if (sqrPlayerDistanceToBound < playerDistanceToColliderGeneration * playerDistanceToColliderGeneration && lodMeshes[colliderLODIndex].received)
            {
                collider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                hasCollider = true;
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

        private void OnMeshDetailsReceived(object meshDetails)
        {
            mesh = ((MeshDetails)meshDetails).BuildMesh();
            received = true;
            update();
        }

        public void RequestMesh(AreaNoise noiseArea, AreaDetails areaSettings)
        {
            requested = true;
            ThreadHandler.RequestDetails(() => MeshController.BuildMesh(noiseArea.area, lod, areaSettings), OnMeshDetailsReceived);
        }
    }
}

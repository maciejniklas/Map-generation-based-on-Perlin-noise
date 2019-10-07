using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area
{
    private const float playerPositionToColliderGeneration = 5;

    public Vector2 coords;
    public event System.Action<Area, bool> onVisibilityChange;

    private Vector2 center;
    private GameObject instance;
    private Bounds bounds;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private LODDetails[] lodDetails;
    private LODMesh[] lodMeshes;
    private NoiseArea noiseArea;
    private bool received;
    private int previousLODIndex = -1;
    private MeshCollider collider;
    private int colliderLODIndex;
    private bool hasCollider;
    private NoiseAreaSettings noiseAreaSettings;
    private AreaSettings areaSettings;
    private Transform player;
    private float viewRange;

    private Vector2 playerPosition
    {
        get
        {
            return new Vector2(player.position.x, player.position.z);
        }
    }

    public Area(Vector2 coords, Transform parent, Material material, LODDetails[] lodDetails, int colliderLODIndex, NoiseAreaSettings noiseAreaSettings, AreaSettings areaSettings, Transform player)
    {
        this.noiseAreaSettings = noiseAreaSettings;
        this.areaSettings = areaSettings;

        center = coords * areaSettings.resolution / areaSettings.scale;

        Vector3 position = coords * areaSettings.resolution;

        instance = new GameObject("Area");
        instance.transform.position = new Vector3(position.x, 0, position.y);
        instance.transform.parent = parent;

        bounds = new Bounds(position, Vector2.one * areaSettings.resolution);

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

    private void OnNoiseAreaReceived(object noiseArea)
    {
        this.noiseArea = (NoiseArea)noiseArea;
        received = true;

        UpdateArea();
    }

    private void OnMeshDetailsReceived(MeshDetails meshDetails)
    {
        meshFilter.mesh = meshDetails.BuildMesh();
    }

    public void Load()
    {
        ThreadHandler.RequestDetails(() => NoiseController.BuildNoiseArea(areaSettings.verticesPerLine, noiseAreaSettings, center), OnNoiseAreaReceived);
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
                        lodMesh.RequestMesh(noiseArea, areaSettings);
                    }
                }
            }

            if (wasVisible != visible)
            {
                SetVisible(visible);
                if (onVisibilityChange != null)
                {
                    onVisibilityChange(this, visible);
                }
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
                lodMeshes[colliderLODIndex].RequestMesh(noiseArea, areaSettings);
            }

            if (sqrPlayerDistanceToBound < playerPositionToColliderGeneration * playerPositionToColliderGeneration && lodMeshes[colliderLODIndex].received)
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

        public void RequestMesh(NoiseArea noiseArea, AreaSettings areaSettings)
        {
            requested = true;
            ThreadHandler.RequestDetails(() => MeshController.GenerateMesh(noiseArea.values, lod, areaSettings), OnMeshDetailsReceived);
        }
    }
}

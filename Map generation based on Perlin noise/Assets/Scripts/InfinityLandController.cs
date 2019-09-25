﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinityLandController : MonoBehaviour
{
    public const float viewRange = 450f;

    public Transform player;

    private int areaResolution;
    private int visibleAreas;
    private Dictionary<Vector2, Area> areasCollection = new Dictionary<Vector2, Area>();
    private List<Area> lastUpdateareasCollection = new List<Area>();

    public static Vector2 playerPosition;

    private void Start()
    {
        areaResolution = MapController.resolution - 1;
        visibleAreas = Mathf.RoundToInt(viewRange / areaResolution);
    }

    private void Update()
    {
        playerPosition = new Vector2(player.position.x, player.position.z);

        UpdateVisibleAreas();
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
                    areasCollection.Add(areaCoords, new Area(areaCoords, areaResolution, transform));
                }
            }
        }
    }

    public class Area
    {
        private Vector2 position;
        private GameObject instance;
        private Bounds bounds;

        public Area(Vector2 coords, int resolution, Transform parent)
        {
            position = coords * resolution;

            Vector3 position3D = new Vector3(position.x, 0, position.y);

            instance = GameObject.CreatePrimitive(PrimitiveType.Plane);
            instance.transform.position = position3D;
            instance.transform.localScale = Vector3.one * resolution / 10f;
            instance.transform.parent = parent;

            bounds = new Bounds(position, Vector2.one * resolution);

            SetVisible(false);
        }

        public bool IsVisible()
        {
            return instance.activeSelf;
        }

        public void SetVisible(bool visible)
        {
            instance.SetActive(visible);
        }

        public void UpdateArea()
        {
            float distanceToPlayer = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
            bool visible = distanceToPlayer <= viewRange;

            SetVisible(visible);
        }
    }
}
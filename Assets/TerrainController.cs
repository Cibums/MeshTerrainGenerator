using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    public static TerrainController ins;

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }
    }

    public static float seed = 0.0f;
    public static float waterLevel = 28;
    public float zoom = 10;
    public int worldSize = 10;
    public int xSize = 200;
    public int zSize = 200;
    public GameObject waterPrefab;
    public GameObject treePrefab;
    public ColorGradiant waterColor;
    public WorldLayer[] worldLayers;
    public GameObject terrainTilePrefab;

    private void Start()
    {
        CreateWorld();
    }

    public void CreateWorld()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                Instantiate(terrainTilePrefab, new Vector3(x * xSize, 0, z * zSize), Quaternion.identity);
            }
        }
    }
}

[System.Serializable]
public class WorldLayer
{
    public ColorGradiant color;
    public int height;
}
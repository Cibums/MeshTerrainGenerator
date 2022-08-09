using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshTerrainGenerator : MonoBehaviour
{
    public bool waterCalculated = false;

    Mesh mesh;

    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;

    //public float waterHeight = 28;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //Debug.Log("Temperature: " + GetTemperatureAt());

        StartCoroutine(CreateShape());
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
    }

    private Color GetLayerColor(float y)
    {
        foreach (WorldLayer layer in TerrainController.ins.worldLayers)
        {
            if (y <= layer.height * TerrainController.ins.zoom)
            {
                return layer.color.GetColorByTemperature(GetTemperatureAt());
            }
        }

        Debug.Log("White");
        return Color.white;
    }

    private float GetTemperatureAt()
    {
        return Mathf.Abs(TerrainController.seed / TerrainController.ins.zoom) / 2857.1428571428571428571428571429f;
    }

    private IEnumerator CreateShape()
    {
        if (TerrainController.seed == 0.0f)
        {
            TerrainController.seed = UnityEngine.Random.Range(-100000.0f * TerrainController.ins.zoom, 100000.0f * TerrainController.ins.zoom);
        }
        
        Texture2D texture = new Texture2D(TerrainController.ins.xSize, TerrainController.ins.zSize);
        vertices = new Vector3[(TerrainController.ins.xSize + 1) * (TerrainController.ins.zSize + 1)];
        uv = new Vector2[(TerrainController.ins.xSize + 1) * (TerrainController.ins.zSize + 1)];

        for (int index = 0, z = 0; z <= TerrainController.ins.zSize; z++)
        {
            for (int x = 0; x <= TerrainController.ins.xSize; x++)
            {
                //float y1 = Mathf.PerlinNoise(x * (0.0005f / zoom) + seed, z * (0.0005f / zoom)) * 500f * zoom;
                float y2 = Mathf.PerlinNoise((x + transform.position.x) * (.5f / TerrainController.ins.zoom) + TerrainController.seed, (z + transform.position.z) * (.5f / TerrainController.ins.zoom)) * .5f * TerrainController.ins.zoom;
                float y3 = Mathf.PerlinNoise((x + transform.position.x) * (0.01f / TerrainController.ins.zoom) + TerrainController.seed, (z + transform.position.z) * (.01f / TerrainController.ins.zoom)) * 50.0f * TerrainController.ins.zoom;
                float y4 = Mathf.PerlinNoise((x + transform.position.x) * (0.05f / TerrainController.ins.zoom) + TerrainController.seed, (z + transform.position.z) * (0.05f / TerrainController.ins.zoom)) * 10.0f * TerrainController.ins.zoom;

                float y = y2 + y3 + y4;
                texture.SetPixel(x,z,GetLayerColor(y));

                Debug.Log(y);
                Debug.Log(GetLayerColor(y));


                int rnd = UnityEngine.Random.Range(0,100);

                if (rnd < 1 && y > 30 * TerrainController.ins.zoom)
                {
                    Instantiate(TerrainController.ins.treePrefab, new Vector3(x + transform.position.x, y, z + transform.position.z), Quaternion.Euler(0, UnityEngine.Random.Range(0,359),0)).transform.localScale = new Vector3(0.1f * TerrainController.ins.zoom, 0.1f * TerrainController.ins.zoom, 0.1f * TerrainController.ins.zoom);
                }

                vertices[index] = new Vector3(x,y,z);
                uv[index] = new Vector2(x, z);
                index++;
            }
        }

        texture.Apply();

        triangles = new int[TerrainController.ins.xSize * TerrainController.ins.zSize * 6];
        int vert = 0;
        int tris = 0;

        for (int x = 0; x < TerrainController.ins.xSize; x++)
        {
            for (int z = 0; z < TerrainController.ins.zSize; z++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + TerrainController.ins.xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + TerrainController.ins.xSize + 1;
                triangles[tris + 5] = vert + TerrainController.ins.xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
            UpdateMesh();
            yield return new WaitForSeconds(0.01f);
        }

        if (!waterCalculated)
        {
            Transform water = Instantiate(TerrainController.ins.waterPrefab, new Vector3(transform.position.x + TerrainController.ins.xSize / 2, 28, transform.position.z + TerrainController.ins.zSize / 2), Quaternion.identity).transform;
            water.localScale = new Vector3(TerrainController.ins.xSize / 10, 1, TerrainController.ins.zSize / 10);
            Vector3 orgPos = water.transform.position;
            TerrainController.waterLevel = orgPos.y * TerrainController.ins.zoom;
            water.GetComponent<Renderer>().material.color = TerrainController.ins.waterColor.GetColorByTemperature(GetTemperatureAt());
            water.position = new Vector3(water.position.x, TerrainController.waterLevel, water.position.z);

            waterCalculated = true;
        }
        
        GetComponent<Renderer>().material.mainTextureScale = new Vector2(1.0f/ TerrainController.ins.xSize, 1.0f/ TerrainController.ins.zSize);
        GetComponent<Renderer>().material.mainTexture = texture;
    }
}

[System.Serializable]
public class ColorGradiant
{
    public Color coldColor;
    public Color warmColor;

    public Color GetColorByTemperature(float temp) 
    {
        float percentageWarm = temp / 35.0f * 100.0f;

        //Debug.Log("R: "+ (warmColor.r * percentageWarm / 255.0f) + "\n G: " + (warmColor.g * percentageWarm / 255.0f) + "\n B: " + (warmColor.b * percentageWarm / 255.0f));

        Color newColor = new Color((warmColor.r * percentageWarm + coldColor.r * (100.0f - percentageWarm) / 255.0f),
                                (warmColor.g * percentageWarm + coldColor.g * (100.0f - percentageWarm) / 255.0f),
                                (warmColor.b * percentageWarm + coldColor.b * (100.0f - percentageWarm) / 255.0f),
                                (warmColor.a * percentageWarm + coldColor.a * (100.0f - percentageWarm) / 255.0f));

        return newColor;
    }
}
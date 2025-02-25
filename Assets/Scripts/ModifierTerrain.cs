using UnityEngine;

public class TerrainPainter : MonoBehaviour
{
    public Terrain terrain;
    public float brushSize = 5f; // Tamanho do pincel
    public float brushStrength = 0.002f; // Intensidade da pintura

    void Update()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) // Botão esquerdo ou direito do mouse
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    ModifyTerrain(hit.point, Input.GetMouseButton(0));
                }
            }
        }
    }

    void ModifyTerrain(Vector3 worldPoint, bool raise)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        // Converte posição do mundo para índices no mapa de alturas
        int heightmapWidth = terrainData.heightmapResolution;
        int heightmapHeight = terrainData.heightmapResolution;
        
        float relativeX = (worldPoint.x - terrainPos.x) / terrainData.size.x;
        float relativeZ = (worldPoint.z - terrainPos.z) / terrainData.size.z;

        int x = Mathf.RoundToInt(relativeX * heightmapWidth);
        int z = Mathf.RoundToInt(relativeZ * heightmapHeight);

        int brushSizeInPixels = Mathf.RoundToInt(brushSize * heightmapWidth / terrainData.size.x);
        
        // Obtém a altura atual do terreno
        float[,] heights = terrainData.GetHeights(x, z, brushSizeInPixels, brushSizeInPixels);

        // Modifica a altura dentro do raio do pincel
        for (int i = 0; i < brushSizeInPixels; i++)
        {
            for (int j = 0; j < brushSizeInPixels; j++)
            {
                float distance = Vector2.Distance(new Vector2(i, j), new Vector2(brushSizeInPixels / 2, brushSizeInPixels / 2));
                if (distance < brushSizeInPixels / 2)
                {
                    float heightChange = (raise ? 1 : -1) * brushStrength;
                    heights[i, j] += heightChange;
                }
            }
        }

        // Aplica a modificação
        terrainData.SetHeights(x, z, heights);
    }
}

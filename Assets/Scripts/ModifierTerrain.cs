using UnityEngine;

public class RuntimeTerrainPainter : MonoBehaviour
{
    public Terrain terrain;
    public Texture2D brushTexture;
    public GameObject brushIndicatorPrefab; // Prefab do brush visual
    private GameObject brushIndicator;
    public float maxHeight = 0.5f; // Defina o limite máximo (valor entre 0 e 1, sendo 1 o máximo do Terrain)
    public float minHeight = 0.0f; // Caso queira limitar também a profundidade

    public float brushSize = 10f;
    public float brushStrength = 0.005f;
    private float[,] originalHeights;
    private float[,,] originalSplatmap; // Armazena o splatmap original
    public int selectedTextureIndex = 0; // Índice da textura ativa no Terrain Layer
    public GameObject objectPrefab; // Objeto a ser instanciado
    public bool placeObjects = false; // Ativar/Desativar colocação de objetos

    void Start()
    {
        if (!Application.isPlaying) return; 

        if (terrain == null) terrain = Terrain.activeTerrain;

        TerrainData terrainData = terrain.terrainData;
        originalHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        // Salvar o splatmap original para restaurá-lo depois
        originalSplatmap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        // Criar o brush visual
        if (brushIndicatorPrefab != null)
        {
            brushIndicator = Instantiate(brushIndicatorPrefab);
            brushIndicator.SetActive(false); // Começa invisível
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                // Ajustar tamanho do brush visual corretamente
                float adjustedBrushSize = (brushSize / terrain.terrainData.heightmapResolution) * terrain.terrainData.size.x;

                // Atualiza a posição do brush visual
                brushIndicator.SetActive(true);
                brushIndicator.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                brushIndicator.transform.localScale = new Vector3(adjustedBrushSize * 0.1f, 1, adjustedBrushSize * 0.1f);
                
                // Aplica a modificação quando o botão do mouse é pressionado
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1)){
                    ApplyBrush(hit.point, Input.GetMouseButton(0));
                }
                
                if (Input.GetMouseButton(2)) {
                    ApplyTextureBrush(hit.point); // Pintar textura
                }

                // 🛠️ Adiciona um objeto ao clicar com o botão direito do mouse
                if (placeObjects && Input.GetKey("z") && objectPrefab != null)
                {
                    Instantiate(objectPrefab, hit.point, Quaternion.identity);
                }

            }
        }
        else
        {
            brushIndicator.SetActive(false);
        }
    }

void ApplyTextureBrush(Vector3 worldPoint)
{
    TerrainData terrainData = terrain.terrainData;
    Vector3 terrainPos = terrain.transform.position;

    int alphamapWidth = terrainData.alphamapWidth;
    int alphamapHeight = terrainData.alphamapHeight;

    float relativeX = (worldPoint.x - terrainPos.x) / terrainData.size.x;
    float relativeZ = (worldPoint.z - terrainPos.z) / terrainData.size.z;
    int x = Mathf.RoundToInt(relativeX * alphamapWidth);
    int z = Mathf.RoundToInt(relativeZ * alphamapHeight);

    int brushSizeInPixels = Mathf.RoundToInt(brushSize * alphamapWidth / terrainData.size.x);

    int xStart = Mathf.Clamp(x - brushSizeInPixels / 2, 0, alphamapWidth - brushSizeInPixels);
    int zStart = Mathf.Clamp(z - brushSizeInPixels / 2, 0, alphamapHeight - brushSizeInPixels);

    int width = Mathf.Clamp(brushSizeInPixels, 1, alphamapWidth - xStart);
    int height = Mathf.Clamp(brushSizeInPixels, 1, alphamapHeight - zStart);

    float[,,] splatmap = terrainData.GetAlphamaps(xStart, zStart, width, height);
    int numTextures = splatmap.GetLength(2); // Número de texturas do terreno

    for (int i = 0; i < width; i++)
    {
        for (int j = 0; j < height; j++)
        {
            float brushValue = brushTexture.GetPixelBilinear(i / (float)width, j / (float)height).a;

            for (int t = 0; t < numTextures; t++)
            {
                splatmap[i, j, t] = (t == selectedTextureIndex) ? brushValue : (splatmap[i, j, t] * (1 - brushValue));
            }
        }
    }

    terrainData.SetAlphamaps(xStart, zStart, splatmap);
}


    void ApplyBrush(Vector3 worldPoint, bool raise)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int heightmapWidth = terrainData.heightmapResolution;
        int heightmapHeight = terrainData.heightmapResolution;

        float relativeX = (worldPoint.x - terrainPos.x) / terrainData.size.x;
        float relativeZ = (worldPoint.z - terrainPos.z) / terrainData.size.z;
        int x = Mathf.RoundToInt(relativeX * heightmapWidth);
        int z = Mathf.RoundToInt(relativeZ * heightmapHeight);

        int brushSizeInPixels = Mathf.RoundToInt(brushSize * heightmapWidth / terrainData.size.x);

        int xStart = Mathf.Clamp(x - brushSizeInPixels / 2, 0, heightmapWidth - brushSizeInPixels);
        int zStart = Mathf.Clamp(z - brushSizeInPixels / 2, 0, heightmapHeight - brushSizeInPixels);
        
        int width = Mathf.Clamp(brushSizeInPixels, 1, heightmapWidth - xStart);
        int height = Mathf.Clamp(brushSizeInPixels, 1, heightmapHeight - zStart);

        float[,] heights = terrainData.GetHeights(xStart, zStart, width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float brushValue = brushTexture.GetPixelBilinear(i / (float)width, j / (float)height).a;
                float heightChange = brushValue * (raise ? brushStrength : -brushStrength);
                heights[i, j] = Mathf.Clamp(heights[i, j] + heightChange, minHeight, maxHeight);
            }
        }

        terrainData.SetHeights(xStart, zStart, heights);
    }

    void OnApplicationQuit()
    {
        if (terrain != null && originalHeights != null)
        {
            terrain.terrainData.SetHeights(0, 0, originalHeights);
        }

        if (terrain != null && originalSplatmap != null)
        {
            terrain.terrainData.SetAlphamaps(0, 0, originalSplatmap);
        }
    }
}

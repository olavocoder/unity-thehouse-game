using UnityEngine;
using System;
using System.Text.RegularExpressions;

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
    private bool isEnabledBrush = false;
    public float maxDistance = 5.0f;

    void Start(){
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

    void Update(){
        if (!Application.isPlaying) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Input.GetKeyDown("x")) isEnabledBrush = !isEnabledBrush; // Ativar/Desativar o brush

        if (Physics.Raycast(ray, out hit) && isEnabledBrush){
            if (hit.collider.CompareTag("Terrain") ){
                // Ajustar tamanho do brush visual corretamente
                float adjustedBrushSize = (brushSize / terrain.terrainData.heightmapResolution) * terrain.terrainData.size.x;
                
                if(SphereCollisor() == true){
                    ChangeChildrenmaterials(Color.red, brushIndicator);
                }else{
                    ChangeChildrenmaterials(Color.blue, brushIndicator);
                }

                // Atualiza a posição do brush visual
                brushIndicator.SetActive(true);
                brushIndicator.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                brushIndicator.transform.localScale = new Vector3(adjustedBrushSize * 0.1f, 1, adjustedBrushSize * 0.1f);

                // 🛠️ Adiciona um objeto ao clicar com o botão direito do mouse
                if (SphereCollisor() == false && placeObjects && Input.GetKeyDown("z") && objectPrefab != null)
                {
                    ApplyBrush(hit.point, Input.GetMouseButton(0));
                    ApplyBrush(hit.point, Input.GetMouseButton(0), "texture"); // Pintar textura
                    Instantiate(objectPrefab, hit.point, brushIndicator.transform.rotation);
                    isEnabledBrush = false;
                }

                if(Input.GetKeyDown("c")){
                    brushIndicator.transform.Rotate(0,90,0);
                }

            }

        }else{
            brushIndicator.SetActive(false);
        }
    }

    bool SphereCollisor(){
        Collider[] hits = Physics.OverlapSphere(brushIndicator.transform.position, maxDistance);
        string pattern = @"House_Green_Prefab";

        foreach (Collider hit in hits)
        {
            if(Regex.IsMatch(hit.gameObject.name, pattern)){
                return true;
            }
        }

        return false;
    }

    void ChangeChildrenmaterials(Color colorValue, GameObject itemObj){
        foreach(Renderer rend in itemObj.GetComponentsInChildren<Renderer>()){
            rend.material = new Material(rend.material);
            rend.material.color = colorValue;
        }
    }

    void ApplyBrush(Vector3 worldPoint, bool raise, string type = "height"){
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int heightmapWidth = type == "height" ? terrainData.heightmapResolution : terrainData.alphamapWidth;
        int heightmapHeight = type == "height"  ? terrainData.heightmapResolution : terrainData.alphamapHeight;

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

        float[,,] splatmap = new float[0, 0, 0];
        int numTextures = 0;

        if(type == "texture"){
            splatmap = terrainData.GetAlphamaps(xStart, zStart, width, height);
            numTextures = splatmap.GetLength(2); // Número de texturas do terreno
        }

        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                float brushValue = brushTexture.GetPixelBilinear(i / (float)width, j / (float)height).a;
                
                if(type == "height"){
                    float heightChange = brushValue * (raise ? brushStrength : -brushStrength);
                    heights[i, j] = Mathf.Clamp(heights[i, j] + heightChange, minHeight, maxHeight);
                }

                if(type == "texture"){
                    for (int t = 0; t < numTextures; t++){
                        splatmap[i, j, t] = (t == selectedTextureIndex) ? brushValue : (splatmap[i, j, t] * (1 - brushValue));
                    }
                }

            }
        }

        if(type == "height"){
            terrainData.SetHeights(xStart, zStart, heights);
        }else{ 
            terrainData.SetAlphamaps(xStart, zStart, splatmap);
        }
    }
    void OnDrawGizmos()
    {
        if (brushIndicator == null) return; // Evita erro se o objeto não estiver definido

        // Define a cor do gizmo (verde para visibilidade)
        Gizmos.color = Color.green;

        // Desenha uma esfera na posição do objeto com o raio do OverlapSphere
        Gizmos.DrawWireSphere(brushIndicator.transform.position, maxDistance); // Altere o raio conforme necessário
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

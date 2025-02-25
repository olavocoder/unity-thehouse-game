using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SavePerson : MonoBehaviour
{
    public GameObject objectToSave;
    public string folderPath;
    public string prefabName = "NewPrefab";

    public void SaveAsPrefab(){
        // Verifica se o GameObject existe
        if (objectToSave == null)
        {
            Debug.LogError("Nenhum GameObject foi definido para salvar.");
            return;
        }
        
        // Certifique-se de que a pasta "Assets/Textures" exista
        string texturesFolderPath = "Assets/Textures";
        if (!AssetDatabase.IsValidFolder(texturesFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Textures");
            Debug.Log($"Pasta criada: {texturesFolderPath}");
        }

        // Cria a pasta se ela não existir
        string meshesFolderPath = "Assets/Meshes";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parentFolder = System.IO.Path.GetDirectoryName(folderPath);
            string newFolderName = System.IO.Path.GetFileName(folderPath);
            AssetDatabase.CreateFolder(parentFolder, newFolderName);
            Debug.Log($"Pasta criada: {folderPath}");
        }
        
        SaveMeshes(objectToSave, meshesFolderPath);

        // Certifica-se de que todos os materiais estão salvos como assets
        Renderer[] renderers = objectToSave.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (!AssetDatabase.Contains(material))
                {
                    // Salva o material na pasta "Assets/Materials"
                    string materialPath = $"Assets/Materials/{material.name}.mat";
                    AssetDatabase.CreateAsset(material, materialPath);
                    Debug.Log($"Material salvo em: {materialPath}");
                }
                 // Salva as texturas associadas ao material
                SaveTexturesFromMaterial(material, texturesFolderPath);
            }
        }

        // Define o caminho completo do arquivo prefab
        string fullPath = $"{folderPath}/{prefabName}.prefab";

        // Salva o GameObject como um prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(objectToSave, fullPath);

        if (prefab != null)
        {
            Debug.Log($"Prefab salvo com sucesso em: {fullPath}");
        }
        else
        {
            Debug.LogError("Erro ao salvar o prefab.");
        }
    }

    private void SaveTexturesFromMaterial(Material material, string texturesFolderPath)
    {
        Shader shader = material.shader;
        for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
        {
            if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                Texture texture = material.GetTexture(propertyName);

                if (texture is Texture2D texture2D && !AssetDatabase.Contains(texture2D))
                {
                    string texturePath = $"{texturesFolderPath}/{texture2D.name}.png";
                    if (!System.IO.File.Exists(texturePath))
                    {
                        Texture2D readableTexture = MakeTextureReadable(texture2D);
                        byte[] textureBytes = readableTexture.EncodeToPNG();
                        System.IO.File.WriteAllBytes(texturePath, textureBytes);
                        AssetDatabase.ImportAsset(texturePath);
                        Debug.Log($"Textura salva em: {texturePath}");
                    }

                    Texture importedTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
                    material.SetTexture(propertyName, importedTexture);
                }
            }
        }
    }

    private Texture2D MakeTextureReadable(Texture2D texture)
    {
        // Verifica se a textura já é legível
        if (texture.isReadable)
        {
            return texture;
        }

        // Cria uma cópia da textura como legível
        RenderTexture tempRT = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(texture, tempRT);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tempRT;

        Texture2D readableTexture = new Texture2D(texture.width, texture.height, texture.format, false);
        readableTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tempRT);

        return readableTexture;
    }

        private void SaveMeshes(GameObject rootObject, string meshesFolderPath)
    {
        SkinnedMeshRenderer[] skinnedMeshRenderers = rootObject.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            if (skinnedMeshRenderer.sharedMesh != null)
            {
                Mesh originalMesh = skinnedMeshRenderer.sharedMesh;

                // Verifica se a malha já é um asset no projeto
                if (!AssetDatabase.Contains(originalMesh))
                {
                    // Cria um caminho para salvar a malha
                    string meshPath = $"{meshesFolderPath}/{originalMesh.name}.asset";

                    // Clona a malha original para salvar como asset
                    Mesh meshCopy = Instantiate(originalMesh);
                    AssetDatabase.CreateAsset(meshCopy, meshPath);
                    Debug.Log($"Malha salva em: {meshPath}");

                    // Atualiza o SkinnedMeshRenderer para usar a nova malha
                    skinnedMeshRenderer.sharedMesh = meshCopy;
                }
            }
        }
    }
}

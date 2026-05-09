using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteToPrefabGenerator : EditorWindow
{
    private GameObject basePrefab;

    private string spriteFolderPath = "Assets/Sprites/Items";
    private string outputPrefabFolder = "Assets/Prefabs/Generated";

    [MenuItem("Tools/Sprite Prefab Generator")]
    public static void ShowWindow()
    {
        GetWindow<SpriteToPrefabGenerator>("Sprite Prefab Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Prefabs From Sprites", EditorStyles.boldLabel);

        basePrefab = (GameObject)EditorGUILayout.ObjectField(
            "Base Prefab",
            basePrefab,
            typeof(GameObject),
            false
        );

        spriteFolderPath = EditorGUILayout.TextField("Sprite Folder", spriteFolderPath);
        outputPrefabFolder = EditorGUILayout.TextField("Output Folder", outputPrefabFolder);

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Prefabs"))
        {
            GeneratePrefabs();
        }
    }

    private void GeneratePrefabs()
    {
        if (basePrefab == null)
        {
            Debug.LogError("Please assign a base prefab first.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(spriteFolderPath))
        {
            Debug.LogError("Sprite folder does not exist: " + spriteFolderPath);
            return;
        }

        if (!AssetDatabase.IsValidFolder(outputPrefabFolder))
        {
            Directory.CreateDirectory(outputPrefabFolder);
            AssetDatabase.Refresh();
        }

        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { spriteFolderPath });

        foreach (string guid in spriteGuids)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (sprite == null)
                continue;

            GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);

            Transform circleChild = prefabInstance.transform.Find("Circle");

            if (circleChild == null)
            {
                Debug.LogError("Could not find child object named Circle inside prefab: " + basePrefab.name);
                DestroyImmediate(prefabInstance);
                return;
            }

            SpriteRenderer spriteRenderer = circleChild.GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                Debug.LogError("Circle child does not have a SpriteRenderer.");
                DestroyImmediate(prefabInstance);
                return;
            }

            // Only change the sprite on the Circle child
            spriteRenderer.sprite = sprite;

            // Rename the generated prefab root
            prefabInstance.name = sprite.name;

            // Keep the child name unchanged
            circleChild.name = "Circle";

            string prefabPath = outputPrefabFolder + "/" + sprite.name + ".prefab";

            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            DestroyImmediate(prefabInstance);

            Debug.Log("Created prefab: " + prefabPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Prefab generation complete.");
    }
}
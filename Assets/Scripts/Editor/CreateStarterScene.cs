using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CreateStarterScene
{
    [MenuItem("Solar System/Create Starter Scene")]
    public static void CreateScene()
    {
        const string path = "Assets/Scenes/Main.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
        {
            EditorUtility.DisplayDialog("Starter scene exists",
                "Open Assets/Scenes/Main.unity. This command will not overwrite it.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorSettings.serializationMode = SerializationMode.ForceText;
        EditorSettings.externalVersionControl = "Visible Meta Files";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("SolarSystem");
        var refs = root.AddComponent<SolarSystemReferences>();
        refs.selectionService = root.AddComponent<SelectionService>();

        var sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sun.name = "Sun";
        sun.transform.SetParent(root.transform);
        sun.transform.localScale = Vector3.one * 3f;
        refs.sun = sun.transform;

        for (int i = 0; i < 8; i++)
        {
            var planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = ((PlanetId)i).ToString();
            planet.transform.SetParent(root.transform);
            planet.transform.position = new Vector3(5f + i * 3f, 0f, 0f);
            var body = planet.AddComponent<CelestialBody>();
            body.planetId = (PlanetId)i;
            refs.planets[i] = body;
        }

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        refs.mainCamera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(13f, 20f, -32f);
        cameraObject.transform.LookAt(new Vector3(13f, 0f, 0f));
        var lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        if (EditorSceneManager.SaveScene(scene, path))
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Created Main.unity and set it as the startup build scene.");
        }
    }
}

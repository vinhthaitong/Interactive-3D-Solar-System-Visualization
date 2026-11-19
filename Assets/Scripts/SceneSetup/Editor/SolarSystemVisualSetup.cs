using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Only runs from the Editor. No animation, input, selection or UI is added.
public static class SolarSystemVisualSetup
{
    public const string ScenePath = "Assets/Scenes/Main.unity";
    public static readonly string[] Names = {
        "Sun", "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune"
    };
    public static readonly float[] Diameters = { 6f, 1.2f, 1.9f, 2f, 1.5f, 4.5f, 3.8f, 2.9f, 2.8f };
    public static readonly float[] Radii = { 0f, 7f, 10f, 13.5f, 17f, 23f, 30f, 37f, 43f };
    static readonly float[] Angles = { 0f, 205f, 240f, 130f, 175f, 10f, 60f, 135f, 185f };

    [MenuItem("Solar System/Apply Thai Scene Visuals")]
    public static void ApplyMenu()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play mode before applying visuals.");
        if (EditorSceneManager.GetActiveScene().path != ScenePath)
            throw new InvalidOperationException("Open the existing Main.unity before applying visuals.");
        Apply();
    }

    // Command-line entry. Opens the existing scene; never creates/replaces it.
    public static void ApplyBatch()
    {
        EditorSceneManager.OpenScene(ScenePath);
        Apply();
        SceneVisualVerification.RunBatch();
    }

    public static void Apply()
    {
        var references = UnityEngine.Object.FindAnyObjectByType<SolarSystemReferences>();
        Require(references != null, "SolarSystemReferences is missing.");
        Require(references.gameObject.scene.path == ScenePath, "Only Main.unity is supported.");
        Require(references.sun != null && references.planets.Length == 8, "Existing body references are required.");
        Require(references.mainCamera != null && references.selectionService != null, "Camera/selection references are required.");
        var bodies = new Transform[9];
        bodies[0] = references.sun;
        for (int i = 0; i < 8; i++)
        {
            Require(references.planets[i] != null && (int)references.planets[i].planetId == i,
                "Planet references must be in PlanetId order; repair references before applying.");
            bodies[i + 1] = references.planets[i].transform;
        }
        for (int i = 0; i < bodies.Length; i++)
        {
            Require(bodies[i].name == Names[i], "Unexpected body name: " + bodies[i].name);
            Require(bodies[i].GetComponent<MeshRenderer>() != null && bodies[i].GetComponent<MeshFilter>() != null
                && bodies[i].GetComponent<SphereCollider>() != null, "Expected a sphere renderer/collider on " + Names[i]);
        }

        // Only the known starter layout is repositioned. Subsequent runs preserve
        // transforms, camera framing, lighting edits and extra teammate components.
        bool starterLayout = IsStarterLayout(bodies);
        bool starterCamera = Vector3.Distance(references.mainCamera.transform.position,
            new Vector3(13f, 20f, -32f)) < 0.001f
            && Quaternion.Angle(references.mainCamera.transform.rotation,
                Quaternion.LookRotation(new Vector3(0f, -20f, 32f))) < 0.01f
            && references.mainCamera.GetComponents<Component>().Length == 3;
        bool firstLighting = references.sun.Find("Sun Light") == null;
        Directory.CreateDirectory(PlanetTextureMaps.MaterialFolder);
        Directory.CreateDirectory("Assets/Meshes/CelestialBodies");
        Directory.CreateDirectory("Assets/Textures");
        Directory.CreateDirectory("Assets/Prefabs/CelestialBodies");
        AssetDatabase.Refresh();

        for (int i = 0; i < bodies.Length; i++)
        {
            var body = bodies[i];
            Undo.RecordObject(body, "Apply Thai visuals");
            if (starterLayout)
            {
                float angle = Angles[i] * Mathf.Deg2Rad;
                body.position = references.transform.position + new Vector3(
                    Mathf.Cos(angle) * Radii[i], 0f, Mathf.Sin(angle) * Radii[i]);
                body.localScale = Vector3.one * Diameters[i];
            }
            var material = GetMaterial(Names[i]);
            var renderer = body.GetComponent<MeshRenderer>();
            // Preserve an explicitly substituted material on a later run.
            if (renderer.sharedMaterial == null || renderer.sharedMaterial.name == "Default-Material"
                || renderer.sharedMaterial == material)
            {
                Undo.RecordObject(renderer, "Assign celestial material");
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }
        AddSunLight(references.sun);
        AddSaturnRings(bodies[6]);
        if (firstLighting)
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.25f, 0.27f, 0.32f);
            RenderSettings.reflectionIntensity = 0f;
            RenderSettings.fog = false;
            // Only retire the unmodified foundation light, never an arbitrary team light.
            var starterLight = GameObject.Find("Directional Light");
            if (starterLight != null && starterLight.GetComponent<Light>() != null
                && starterLight.GetComponent<Light>().type == LightType.Directional
                && Mathf.Approximately(starterLight.GetComponent<Light>().intensity, 1f)
                && starterLight.GetComponent<Light>().color == Color.white
                && Quaternion.Angle(starterLight.transform.rotation, Quaternion.Euler(50f, -30f, 0f)) < 0.01f
                && starterLight.GetComponents<Component>().Length == 2)
            {
                Undo.RecordObject(starterLight.GetComponent<Light>(), "Disable starter light");
                starterLight.GetComponent<Light>().enabled = false;
            }
        }
        if (starterLayout && starterCamera)
        {
            var camera = references.mainCamera;
            Undo.RecordObject(camera, "Frame solar system");
            Undo.RecordObject(camera.transform, "Frame solar system");
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.003f, 0.005f, 0.014f);
            camera.fieldOfView = 32f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 250f;
            camera.transform.position = references.sun.position + new Vector3(-8f, 62f, -82f);
            camera.transform.LookAt(references.sun.position + Vector3.left * 8f);
        }

        // Connect each existing body once. Existing prefab assets/instances are
        // never overwritten on rerun, so later teammate components stay local.
        for (int i = 0; i < bodies.Length; i++)
        {
            string path = "Assets/Prefabs/CelestialBodies/" + Names[i] + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                Require(!PrefabUtility.IsPartOfAnyPrefab(bodies[i]),
                    "Body already belongs to another prefab: " + Names[i]);
                PrefabUtility.SaveAsPrefabAssetAndConnect(bodies[i].gameObject, path, InteractionMode.AutomatedAction);
            }
            if (PrefabUtility.IsPartOfPrefabInstance(bodies[i]))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(bodies[i]);
                PrefabUtility.RecordPrefabInstancePropertyModifications(bodies[i].GetComponent<MeshRenderer>());
            }
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(references.gameObject.scene);
        Require(EditorSceneManager.SaveScene(references.gameObject.scene), "Main scene could not be saved.");
        Debug.Log("Thai visuals saved to Main.unity. Starter layout changed: " + starterLayout);
    }

    static bool IsStarterLayout(Transform[] bodies)
    {
        if (Vector3.Distance(bodies[0].localPosition, Vector3.zero) > 0.001f
            || Vector3.Distance(bodies[0].localScale, Vector3.one * 3f) > 0.001f) return false;
        for (int i = 1; i < bodies.Length; i++)
            if (Vector3.Distance(bodies[i].localPosition, new Vector3(5f + (i - 1) * 3f, 0f, 0f)) > 0.001f
                || Vector3.Distance(bodies[i].localScale, Vector3.one) > 0.001f) return false;
        return true;
    }

    static Material GetMaterial(string name)
    {
        string path = PlanetTextureMaps.MaterialFolder + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        var shader = Shader.Find(name == "Sun" ? "Unlit/Texture" : "Standard");
        Require(shader != null, "Required Built-In shader is missing.");
        material = new Material(shader) { name = name, mainTexture = PlanetTextureMaps.SurfaceTexture(name) };
        if (name != "Sun")
        {
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Glossiness", name == "Earth" ? 0.28f : 0.12f);
            // A restrained surface contribution keeps the night side readable.
            var nightMap = name == "Earth" ? AssetDatabase.LoadAssetAtPath<Texture2D>(
                PlanetTextureMaps.Folder + PlanetTextureMaps.NightFile) : null;
            material.SetTexture("_EmissionMap", nightMap != null ? nightMap : material.mainTexture);
            material.SetColor("_EmissionColor", Color.white * (nightMap != null ? 1f : 0.1f));
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            material.EnableKeyword("_EMISSION");
        }
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static void AddSunLight(Transform sun)
    {
        if (sun.Find("Sun Light") != null) return;
        var lightObject = new GameObject("Sun Light");
        Undo.RegisterCreatedObjectUndo(lightObject, "Create Sun light");
        lightObject.transform.SetParent(sun, false);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.94f, 0.84f);
        light.intensity = 1.7f;
        light.range = 150f;
        light.renderMode = LightRenderMode.ForcePixel;
        light.shadows = LightShadows.None;
    }

    static void AddSaturnRings(Transform saturn)
    {
        if (saturn.Find("Saturn Rings") != null) return;
        const string meshPath = PlanetTextureMaps.RingMeshPath;
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mesh == null)
        {
            mesh = CreateRingMesh();
            AssetDatabase.CreateAsset(mesh, meshPath);
        }
        const string materialPath = PlanetTextureMaps.MaterialFolder + "SaturnRings.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard")) { name = "SaturnRings" };
            material.color = new Color(0.83f, 0.72f, 0.48f);
            material.SetFloat("_Glossiness", 0.08f);
            material.SetColor("_EmissionColor", new Color(0.12f, 0.095f, 0.06f));
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            material.EnableKeyword("_EMISSION");
            AssetDatabase.CreateAsset(material, materialPath);
        }
        var rings = new GameObject("Saturn Rings");
        Undo.RegisterCreatedObjectUndo(rings, "Create Saturn rings");
        rings.transform.SetParent(saturn, false);
        rings.transform.localRotation = Quaternion.Euler(-18f, 0f, 12f);
        rings.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = rings.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        // No collider: rays reach the CelestialBody sphere, not the decorative rings.
    }

    static Mesh CreateRingMesh()
    {
        const int segments = 160;
        // Separate annuli leave a visible Cassini-style gap; radii are relative
        // to a unit-diameter planet and inherit Saturn's scale and future motion.
        float[] edges = { 0.64f, 0.86f, 0.9f, 1.08f };
        int frontCount = (segments + 1) * 4;
        var vertices = new Vector3[frontCount * 2];
        var triangles = new int[segments * 24];
        int t = 0;
        for (int ring = 0; ring < 2; ring++)
        {
            int offset = ring * (segments + 1) * 2;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                vertices[offset + i * 2] = direction * edges[ring * 2];
                vertices[offset + i * 2 + 1] = direction * edges[ring * 2 + 1];
                if (i == segments) continue;
                int a = offset + i * 2;
                int b = a + frontCount;
                int[] face = { a, a + 2, a + 1, a + 1, a + 2, a + 3,
                    b + 1, b + 2, b, b + 3, b + 2, b + 1 };
                foreach (int index in face) triangles[t++] = index;
            }
        }
        Array.Copy(vertices, 0, vertices, frontCount, frontCount);
        var mesh = new Mesh { name = "Saturn Double Sided Rings", vertices = vertices, triangles = triangles };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        PlanetTextureMaps.SetRingUVs(mesh);
        return mesh;
    }

    public static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

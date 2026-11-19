using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Supplied images stay unchanged. Materials and the existing ring mesh reuse
// their original asset GUIDs, so scene and prefab references update together.
public static class PlanetTextureMaps
{
    public const string Folder = "Assets/Textures/CelestialBodies/";
    public const string MaterialFolder = "Assets/Materials/CelestialBodies/";
    public const string RingMeshPath = "Assets/Meshes/CelestialBodies/SaturnRings.asset";
    public static readonly string[] PlanetFiles = {
        "2k_mercury.jpg", "2k_venus_atmosphere.jpg", "2k_earth_daymap.jpg", "2k_mars.jpg",
        "2k_jupiter.jpg", "2k_saturn.jpg", "2k_uranus.jpg", "2k_neptune.jpg"
    };
    public const string NightFile = "2k_earth_nightmap.jpg";
    public const string SunFile = "2k_sun.jpg";
    public const string RingFile = "2k_saturn_ring_alpha.png";

    public static string SurfacePath(string name)
    {
        if (name == "Sun") return Folder + SunFile;
        int index = Array.IndexOf(SolarSystemVisualSetup.Names, name) - 1;
        SolarSystemVisualSetup.Require(index >= 0, "Unknown celestial body: " + name);
        return Folder + PlanetFiles[index];
    }

    public static Texture2D SurfaceTexture(string name)
    {
        var supplied = AssetDatabase.LoadAssetAtPath<Texture2D>(SurfacePath(name));
        SolarSystemVisualSetup.Require(supplied != null, "Required texture is missing: " + SurfacePath(name));
        return supplied;
    }

    [MenuItem("Solar System/Apply Supplied Planet Maps")]
    public static void Apply()
    {
        SolarSystemVisualSetup.Require(!Application.isPlaying, "Stop Play mode before changing material assets.");
        AssetDatabase.Refresh();
        foreach (string file in PlanetFiles) ConfigureImport(file, false);
        ConfigureImport(NightFile, false);
        ConfigureImport(RingFile, true);
        ApplySunMap();

        for (int i = 0; i < PlanetFiles.Length; i++)
            ConfigureSurface(SolarSystemVisualSetup.Names[i + 1], Folder + PlanetFiles[i]);

        var rings = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "SaturnRings.mat");
        SolarSystemVisualSetup.Require(rings != null, "Apply Thai Scene Visuals first to create Saturn's rings.");
        Undo.RecordObject(rings, "Apply supplied ring map");
        rings.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + RingFile);
        rings.color = Color.white;
        rings.SetTexture("_EmissionMap", rings.mainTexture);
        rings.SetColor("_EmissionColor", new Color(0.1f, 0.1f, 0.1f));
        rings.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        rings.EnableKeyword("_EMISSION");
        rings.SetFloat("_Mode", 1f); // Standard Cutout: transparent gaps write no depth.
        rings.SetFloat("_Cutoff", 0.15f);
        rings.SetFloat("_SrcBlend", (float)BlendMode.One);
        rings.SetFloat("_DstBlend", (float)BlendMode.Zero);
        rings.SetFloat("_ZWrite", 1f);
        rings.SetOverrideTag("RenderType", "TransparentCutout");
        rings.EnableKeyword("_ALPHATEST_ON");
        rings.DisableKeyword("_ALPHABLEND_ON");
        rings.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        rings.renderQueue = (int)RenderQueue.AlphaTest;
        EditorUtility.SetDirty(rings);

        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(RingMeshPath);
        SolarSystemVisualSetup.Require(mesh != null, "Saturn's saved ring mesh is missing.");
        Undo.RecordObject(mesh, "Map radial ring texture");
        SetRingUVs(mesh);
        EditorUtility.SetDirty(mesh);
        AssetDatabase.SaveAssets();
        Debug.Log("Applied supplied maps to the Sun, eight planets, Earth city-light emission and Saturn rings.");
    }

    [MenuItem("Solar System/Apply Supplied Sun Map")]
    public static void ApplySunMap()
    {
        SolarSystemVisualSetup.Require(!Application.isPlaying, "Stop Play mode before changing material assets.");
        AssetDatabase.Refresh();
        ConfigureImport(SunFile, false);
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "Sun.mat");
        SolarSystemVisualSetup.Require(material != null, "The existing Sun material is missing.");
        Undo.RecordObject(material, "Apply supplied Sun map");
        // Keep the self-lit shader and the Sun's separate point light unchanged.
        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + SunFile);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        Debug.Log("Applied supplied Sun texture to the existing shared Sun material.");
    }

    public static void ApplySunBatch()
    {
        EditorSceneManager.OpenScene(SolarSystemVisualSetup.ScenePath);
        ApplySunMap();
        string before = SceneVisualVerification.AssetSnapshot();
        ApplySunMap();
        SolarSystemVisualSetup.Require(before == SceneVisualVerification.AssetSnapshot(),
            "Reapplying the Sun map changed saved assets.");
        Debug.Log("SUN MAP REPEATABILITY CHECK PASSED.");
        SceneVisualVerification.RunBatch();
    }

    static void ConfigureImport(string file, bool ring)
    {
        string path = Folder + file;
        SolarSystemVisualSetup.Require(File.Exists(path), "Supplied texture is missing: " + path);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        SolarSystemVisualSetup.Require(importer != null, "Texture could not be imported: " + path);
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.wrapModeU = ring ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
        importer.wrapModeV = TextureWrapMode.Clamp;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = true;
        if (ring)
        {
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
        }
        // Avoid touching importer files on an unchanged second run.
        AssetDatabase.WriteImportSettingsIfDirty(path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
    }

    static void ConfigureSurface(string name, string texturePath)
    {
        string path = MaterialFolder + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard")) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        Undo.RecordObject(material, "Apply supplied planet map");
        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        material.color = Color.white;
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Glossiness", name == "Earth" ? 0.22f : 0.08f);
        bool earth = name == "Earth";
        material.SetTexture("_EmissionMap", earth
            ? AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + NightFile) : material.mainTexture);
        material.SetColor("_EmissionColor", Color.white * (earth ? 1f : 0.1f));
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        material.EnableKeyword("_EMISSION");
        EditorUtility.SetDirty(material);
    }

    public static void SetRingUVs(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float radius = new Vector2(vertices[i].x, vertices[i].z).magnitude;
            // The supplied 2048x125 strip runs from inner to outer radius.
            // Both faces get the same radial mapping; existing geometry stays intact.
            uv[i] = new Vector2(Mathf.InverseLerp(0.64f, 1.08f, radius), 0.5f);
        }
        mesh.uv = uv;
    }

    public static void ApplyBatch()
    {
        EditorSceneManager.OpenScene(SolarSystemVisualSetup.ScenePath);
        Apply();
        string before = SceneVisualVerification.AssetSnapshot();
        Apply();
        SolarSystemVisualSetup.Require(before == SceneVisualVerification.AssetSnapshot(),
            "Reapplying supplied maps changed saved assets.");
        Debug.Log("SUPPLIED PLANET MAP REPEATABILITY CHECK PASSED.");
        SceneVisualVerification.RunBatch();
    }
}

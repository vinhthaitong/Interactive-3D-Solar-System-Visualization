using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Checks the contracts consumed by teammates, without implementing their features.
[InitializeOnLoad]
public static class SceneVisualVerification
{
    const string PlayKey = "ThaiVisuals.PlayCheck";
    const string StartKey = "ThaiVisuals.PlayStart";

    static SceneVisualVerification()
    {
        EditorApplication.update += PollPlayCheck;
    }

    [MenuItem("Solar System/Verify Thai Scene Visuals")]
    public static void Verify()
    {
        SolarSystemVisualSetup.Require(EditorSceneManager.GetActiveScene().path == SolarSystemVisualSetup.ScenePath,
            "Open Main.unity first.");
        var refs = UnityEngine.Object.FindAnyObjectByType<SolarSystemReferences>();
        Check(refs != null && refs.sun != null && refs.planets.Length == 8,
            "SolarSystem references and nine bodies");
        Check(refs.mainCamera != null && refs.selectionService != null, "Camera and SelectionService references");
        Check(refs.sun.GetComponent<CelestialBody>() == null, "Sun has no PlanetId");
        Check(UnityEngine.Object.FindObjectsByType<CelestialBody>().Length == 8,
            "Exactly eight CelestialBody components");
        Check(UnityEngine.Object.FindObjectsByType<SolarSystemReferences>().Length == 1,
            "Exactly one SolarSystemReferences");
        var bodies = new Transform[9];
        bodies[0] = refs.sun;
        for (int i = 0; i < 8; i++)
        {
            Check(refs.planets[i] != null && (int)refs.planets[i].planetId == i,
                "PlanetId array entry " + i);
            Check(refs.planets[i].BodyTransform == refs.planets[i].transform, "BodyTransform contract " + i);
            bodies[i + 1] = refs.planets[i].transform;
        }
        Physics.SyncTransforms();
        float previousRadius = 0f;
        float previousExtent = 0f;
        for (int i = 0; i < bodies.Length; i++)
        {
            Transform body = bodies[i];
            string name = SolarSystemVisualSetup.Names[i];
            Check(body.name == name && body.parent == refs.transform, name + " name/parent");
            Check(UnityEngine.Object.FindObjectsByType<Transform>()
                .Count(t => t.name == name) == 1, name + " exists exactly once");
            Check(body.GetComponent<SphereCollider>() != null && body.GetComponent<SphereCollider>().enabled,
                name + " sphere collider enabled");
            var renderer = body.GetComponent<MeshRenderer>();
            var material = renderer.sharedMaterial;
            Check(material != null && material.shader != null && material.shader.isSupported,
                name + " supported material/shader");
            Check(material.mainTexture != null && AssetDatabase.GetAssetPath(material.mainTexture)
                == PlanetTextureMaps.SurfacePath(name), name + " saved texture assigned");
            Check(AssetDatabase.GetAssetPath(material) == PlanetTextureMaps.MaterialFolder + name + ".mat",
                name + " reusable material assigned");
            // Runtime scene objects do not retain Editor prefab connections.
            if (!Application.isPlaying)
                Check(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(body.gameObject)
                    == "Assets/Prefabs/CelestialBodies/" + name + ".prefab", name + " connected prefab");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CelestialBodies/" + name + ".prefab");
            Check(prefab != null && prefab.GetComponent<SphereCollider>() != null
                && prefab.GetComponent<MeshRenderer>().sharedMaterial == material, name + " prefab contents");
            if (i > 0)
                Check(prefab.GetComponent<CelestialBody>() != null
                    && (int)prefab.GetComponent<CelestialBody>().planetId == i - 1, name + " prefab PlanetId");

            float radius = Vector3.Distance(body.position, refs.sun.position);
            float extent = body.lossyScale.x * (i == 6 ? 1.08f : 0.5f);
            if (i > 0)
            {
                Check(radius > previousRadius && radius - previousRadius > previousExtent + extent,
                    name + " full-orbit clearance including rings");
                var direction = (refs.mainCamera.transform.position - body.position).normalized;
                var ray = new Ray(body.position + direction * body.lossyScale.x * 2f, -direction);
                RaycastHit hit;
                Check(Physics.Raycast(ray, out hit, body.lossyScale.x * 3f)
                    && hit.collider.GetComponent<CelestialBody>() == refs.planets[i - 1],
                    name + " collider can be raycast to its CelestialBody");
            }
            previousRadius = radius;
            previousExtent = extent;
            var point = refs.mainCamera.WorldToViewportPoint(body.position);
            Check(point.z > 0f && point.x > 0.025f && point.x < 0.975f && point.y > 0.025f && point.y < 0.975f,
                name + " inside initial camera frame");
        }
        Check(bodies[0].lossyScale.x > bodies[5].lossyScale.x
            && bodies[5].lossyScale.x > bodies[3].lossyScale.x
            && bodies[6].lossyScale.x > bodies[3].lossyScale.x, "Illustrative size hierarchy");
        var rings = bodies[6].Find("Saturn Rings");
        Check(rings != null && rings.GetComponent<MeshFilter>().sharedMesh != null
            && rings.GetComponent<MeshRenderer>().sharedMaterial != null, "Saturn ring mesh and material");
        Check(rings.GetComponent<Collider>() == null, "Decorative rings do not block selection rays");
        Check(bodies[6].GetComponentsInChildren<MeshRenderer>().Length == 2, "Only one ring object");
        Check(rings.GetComponent<MeshFilter>().sharedMesh.normals.All(n => n.sqrMagnitude > 0.9f),
            "Ring front/back normals valid");
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(PlanetTextureMaps.Folder + PlanetTextureMaps.RingFile) != null)
        {
            var ringMaterial = rings.GetComponent<MeshRenderer>().sharedMaterial;
            Check(AssetDatabase.GetAssetPath(ringMaterial.mainTexture)
                == PlanetTextureMaps.Folder + PlanetTextureMaps.RingFile
                && ringMaterial.IsKeywordEnabled("_ALPHATEST_ON"), "Supplied ring texture and alpha cutout");
            var mesh = rings.GetComponent<MeshFilter>().sharedMesh;
            Check(mesh.uv.Length == mesh.vertexCount && mesh.uv.Any(p => p.x < 0.01f)
                && mesh.uv.Any(p => p.x > 0.99f), "Ring radial UVs span the supplied strip");
            var earthMaterial = bodies[3].GetComponent<MeshRenderer>().sharedMaterial;
            Check(AssetDatabase.GetAssetPath(earthMaterial.GetTexture("_EmissionMap"))
                == PlanetTextureMaps.Folder + PlanetTextureMaps.NightFile, "Earth night-map emission assigned");
        }
        var sunLight = refs.sun.GetComponentInChildren<Light>();
        Check(sunLight != null && sunLight.enabled && sunLight.type == LightType.Point
            && sunLight.range > previousRadius, "Sun point light covers every planet");
        Check(RenderSettings.skybox == null && RenderSettings.ambientMode == AmbientMode.Flat
            && refs.mainCamera.clearFlags == CameraClearFlags.SolidColor, "Dark space environment");
        Check(GraphicsSettings.defaultRenderPipeline == null, "Built-In Render Pipeline");
        Debug.Log("THAI VISUAL CHECKS PASSED (scene, IDs, materials, textures, prefabs, rings, lighting, clearance, raycasts, camera frame).");
    }

    public static void RunBatch()
    {
        EditorSceneManager.OpenScene(SolarSystemVisualSetup.ScenePath);
        Verify();
        // Normalize freshly edited prefab overrides before measuring a rerun.
        SolarSystemVisualSetup.Apply();
        string before = AssetSnapshot();
        var refs = UnityEngine.Object.FindAnyObjectByType<SolarSystemReferences>();
        var identities = refs.planets.ToArray();
        SolarSystemVisualSetup.Apply();
        Check(before == AssetSnapshot(), "Second setup preserves saved scene and asset bytes/GUIDs");
        Check(identities.SequenceEqual(refs.planets),
            "Second setup preserves existing planet component identities");
        VerifyPreservedOverrides();
        Check(before == AssetSnapshot(), "Compatibility probe restored the saved scene and all assets");
        EditorSceneManager.OpenScene(SolarSystemVisualSetup.ScenePath);
        Verify();
        Debug.Log("THAI REPEATABILITY AND SAVED-SCENE RELOAD CHECKS PASSED.");
        CaptureOverview();
        CaptureSurfaceDetails();
    }

    static void VerifyPreservedOverrides()
    {
        // Temporarily simulate later integration edits. Restore the exact saved
        // scene in finally, including if a preservation assertion fails.
        byte[] originalScene = File.ReadAllBytes(SolarSystemVisualSetup.ScenePath);
        try
        {
            var refs = UnityEngine.Object.FindAnyObjectByType<SolarSystemReferences>();
            var mercury = refs.planets[0];
            var extraComponent = mercury.gameObject.AddComponent<AudioSource>();
            var extraChild = new GameObject("Compatibility Probe");
            extraChild.transform.SetParent(mercury.transform, false);
            mercury.transform.position += Vector3.up * 0.25f;
            mercury.transform.localRotation = Quaternion.Euler(0f, 32f, 0f);
            Vector3 bodyPosition = mercury.transform.position;
            Quaternion bodyRotation = mercury.transform.localRotation;
            refs.mainCamera.transform.position += Vector3.right;
            Vector3 cameraPosition = refs.mainCamera.transform.position;
            refs.mainCamera.fieldOfView = 57f;
            var light = refs.sun.GetComponentInChildren<Light>();
            light.intensity = 0.8f;
            var customMaterial = refs.planets[1].GetComponent<MeshRenderer>().sharedMaterial;
            mercury.GetComponent<MeshRenderer>().sharedMaterial = customMaterial;
            SolarSystemVisualSetup.Apply();
            Check(mercury.GetComponent<AudioSource>() == extraComponent && extraChild != null
                && extraChild.transform.parent == mercury.transform, "Later components and children preserved");
            Check(mercury.transform.position == bodyPosition && mercury.transform.localRotation == bodyRotation,
                "Later body position and rotation preserved");
            Check(refs.mainCamera.transform.position == cameraPosition && refs.mainCamera.fieldOfView == 57f,
                "Later camera framing preserved");
            Check(light.intensity == 0.8f && mercury.GetComponent<MeshRenderer>().sharedMaterial == customMaterial,
                "Later lighting and material overrides preserved");
        }
        finally
        {
            File.WriteAllBytes(SolarSystemVisualSetup.ScenePath, originalScene);
            AssetDatabase.ImportAsset(SolarSystemVisualSetup.ScenePath, ImportAssetOptions.ForceSynchronousImport);
            EditorSceneManager.OpenScene(SolarSystemVisualSetup.ScenePath);
        }
    }

    public static string AssetSnapshot()
    {
        var paths = Directory.GetFiles("Assets/Materials", "*", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles("Assets/Textures", "*", SearchOption.AllDirectories))
            .Concat(Directory.GetFiles("Assets/Meshes", "*", SearchOption.AllDirectories))
            .Concat(Directory.GetFiles("Assets/Prefabs/CelestialBodies", "*", SearchOption.AllDirectories))
            .Concat(new[] { SolarSystemVisualSetup.ScenePath, SolarSystemVisualSetup.ScenePath + ".meta" })
            .OrderBy(p => p);
        using (var sha = SHA256.Create())
            return string.Join("\n", paths.Select(p => p + ":" + BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(p)))));
    }

    public static void CaptureOverview()
    {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
        {
            Debug.Log("No graphics device: rendering requires a separate graphics-enabled Unity run.");
            return;
        }
        Directory.CreateDirectory("Screenshots");
        var camera = UnityEngine.Object.FindAnyObjectByType<SolarSystemReferences>().mainCamera;
        var target = new RenderTexture(1920, 1080, 24);
        var oldTarget = camera.targetTexture;
        var oldActive = RenderTexture.active;
        var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            image.Apply();
            File.WriteAllBytes("Screenshots/SceneOverview.png", image.EncodeToPNG());
            Debug.Log("Rendered Main camera to Screenshots/SceneOverview.png using " + SystemInfo.graphicsDeviceType);
        }
        finally
        {
            camera.targetTexture = oldTarget;
            RenderTexture.active = oldActive;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(image);
        }
    }

    public static void PlayModeBatch()
    {
        EditorSceneManager.OpenScene(SolarSystemVisualSetup.ScenePath);
        SessionState.SetBool(PlayKey, true);
        SessionState.SetString(StartKey, DateTime.UtcNow.ToString("O"));
        EditorApplication.isPlaying = true;
    }

    public static void CaptureSurfaceDetails()
    {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
        Directory.CreateDirectory("Screenshots");
        var refs = UnityEngine.Object.FindAnyObjectByType<SolarSystemReferences>();
        var camera = refs.mainCamera;
        var oldPosition = camera.transform.position;
        var oldRotation = camera.transform.rotation;
        float oldFov = camera.fieldOfView;
        float oldAspect = camera.aspect;
        var oldTarget = camera.targetTexture;
        var oldActive = RenderTexture.active;
        var target = new RenderTexture(512, 512, 24);
        var sheet = new Texture2D(1536, 1536, TextureFormat.RGB24, false);
        var night = new Texture2D(512, 512, TextureFormat.RGB24, false);
        var renderers = UnityEngine.Object.FindObjectsByType<Renderer>();
        var hiddenBefore = renderers.Select(r => r.forceRenderingOff).ToArray();
        try
        {
            camera.targetTexture = target;
            camera.aspect = 1f;
            camera.fieldOfView = 32f;
            for (int i = 0; i < 9; i++)
            {
                var body = i == 0 ? refs.sun : refs.planets[i - 1].transform;
                for (int r = 0; r < renderers.Length; r++)
                    renderers[r].forceRenderingOff = hiddenBefore[r]
                        || (renderers[r].transform != body && !renderers[r].transform.IsChildOf(body));
                Vector3 direction = i == 0 ? new Vector3(0f, 0.3f, -1f)
                    : (refs.sun.position - body.position).normalized + Vector3.up * 0.3f;
                float distance = body.lossyScale.x * (i == 6 ? 5f : 3.2f);
                camera.transform.position = body.position + direction.normalized * distance;
                camera.transform.LookAt(body.position);
                camera.Render();
                RenderTexture.active = target;
                sheet.ReadPixels(new Rect(0, 0, 512, 512), i % 3 * 512, (2 - i / 3) * 512);
            }
            sheet.Apply();
            File.WriteAllBytes("Screenshots/PlanetSurfaces.png", sheet.EncodeToPNG());
            var earth = refs.planets[2].transform;
            for (int r = 0; r < renderers.Length; r++)
                renderers[r].forceRenderingOff = hiddenBefore[r]
                    || (renderers[r].transform != earth && !renderers[r].transform.IsChildOf(earth));
            camera.transform.position = earth.position + (earth.position - refs.sun.position).normalized
                * earth.lossyScale.x * 3.2f;
            camera.transform.LookAt(earth.position);
            camera.Render();
            RenderTexture.active = target;
            night.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
            night.Apply();
            File.WriteAllBytes("Screenshots/EarthNight.png", night.EncodeToPNG());
            Debug.Log("Rendered nine surface closeups and Earth's night hemisphere using the saved scene materials.");
        }
        finally
        {
            for (int r = 0; r < renderers.Length; r++) renderers[r].forceRenderingOff = hiddenBefore[r];
            camera.transform.SetPositionAndRotation(oldPosition, oldRotation);
            camera.fieldOfView = oldFov;
            camera.aspect = oldAspect;
            camera.targetTexture = oldTarget;
            RenderTexture.active = oldActive;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(sheet);
            UnityEngine.Object.DestroyImmediate(night);
        }
    }

    static void PollPlayCheck()
    {
        if (!SessionState.GetBool(PlayKey, false)) return;
        try
        {
            if (Application.isPlaying)
            {
                // Batch mode has no focused Game view. Keep frames advancing.
                Application.runInBackground = true;
                EditorApplication.isPaused = false;
                EditorApplication.QueuePlayerLoopUpdate();
            }
            if (Application.isPlaying && !EditorApplication.isCompiling && !EditorApplication.isUpdating
                && Time.frameCount >= 1)
            {
                Verify();
                CaptureOverview();
                SessionState.SetBool(PlayKey, false);
                Debug.Log("THAI PLAY MODE CHECK PASSED after " + Time.frameCount + " frames.");
                EditorApplication.Exit(0);
            }
            else if ((DateTime.UtcNow - DateTime.Parse(SessionState.GetString(StartKey, ""))).TotalSeconds > 120)
                throw new InvalidOperationException("Play check timeout. Playing=" + Application.isPlaying
                    + ", paused=" + EditorApplication.isPaused + ", frames=" + Time.frameCount);
        }
        catch (Exception error)
        {
            SessionState.SetBool(PlayKey, false);
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    static void Check(bool condition, string message)
    {
        SolarSystemVisualSetup.Require(condition, "Visual check failed: " + message);
        Debug.Log("PASS: " + message);
    }
}

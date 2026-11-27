using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class PlanetInfoSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string DatabasePath = "Assets/Data/PlanetInfoDatabase.asset";
    private const string PrefabPath = "Assets/Prefabs/UI/PlanetInformationUI.prefab";
    private static readonly Color White = new Color(0.94f, 0.97f, 1f);
    private static readonly Color Muted = new Color(0.61f, 0.73f, 0.82f);

    [MenuItem("Solar System/Apply Denny Planet Information UI")]
    public static void ApplyMenu()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Stop Play mode before applying the planet information UI.");
            return;
        }
        if (EditorSceneManager.GetActiveScene().path != ScenePath)
        {
            Debug.LogError("Open Assets/Scenes/Main.unity first.");
            return;
        }
        Apply();
    }

    public static void ApplyBatch()
    {
        EditorSceneManager.OpenScene(ScenePath);
        Apply();
    }

    private static void Apply()
    {
        var refs = Object.FindAnyObjectByType<SolarSystemReferences>();
        if (refs == null || refs.selectionService == null || refs.planets == null || refs.planets.Length != 8)
            throw new System.InvalidOperationException("Main needs its eight planet references and SelectionService.");

        var database = AssetDatabase.LoadAssetAtPath<PlanetInfoDatabase>(DatabasePath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<PlanetInfoDatabase>();
            database.planets = DefaultData();
            AssetDatabase.CreateAsset(database, DatabasePath);
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            var root = BuildUI(database);
            prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }

        var existing = GameObject.Find("Planet Information UI");
        if (existing == null)
        {
            existing = (GameObject)PrefabUtility.InstantiatePrefab(prefab, refs.gameObject.scene);
            existing.name = "Planet Information UI";
        }
        var panel = existing.GetComponent<PlanetInfoPanel>();
        if (panel == null) throw new System.InvalidOperationException("Planet UI prefab is missing PlanetInfoPanel.");
        // Prefabs cannot hold scene references. Keep this one override on the instance.
        var serialized = new SerializedObject(panel);
        serialized.FindProperty("solarSystem").objectReferenceValue = refs;
        serialized.FindProperty("database").objectReferenceValue = database;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.RecordPrefabInstancePropertyModifications(panel);

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var eventObject = new GameObject("UI EventSystem", typeof(EventSystem));
            eventObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(refs.gameObject.scene);
        if (!EditorSceneManager.SaveScene(refs.gameObject.scene))
            throw new System.InvalidOperationException("Could not save Main.unity.");
        Debug.Log("Denny planet information data and UI are ready in Main.unity.");
    }

    private static GameObject BuildUI(PlanetInfoDatabase database)
    {
        var root = new GameObject("Planet Information UI", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(PlanetInfoPanel));
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var panel = Rect(root.transform, "Information Card", new Vector2(1, 1),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -94), new Vector2(390, 585));
        panel.gameObject.AddComponent<Image>().color = new Color(0.035f, 0.075f, 0.13f, 0.94f);
        TextLabel(panel, "Eyebrow", "SOLAR SYSTEM  /  PLANET GUIDE", 16, Muted,
            new Vector2(22, -23), new Vector2(345, 28));
        var rule = Rect(panel, "Accent Line", new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 1), new Vector2(22, -62), new Vector2(346, 3));
        rule.gameObject.AddComponent<Image>().color = new Color(0.25f, 0.73f, 0.85f);

        var prompt = Rect(panel, "Empty State", new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 1), new Vector2(22, -105), new Vector2(346, 210));
        TextLabel(prompt, "Prompt Title", "Choose a planet", 30, White,
            Vector2.zero, new Vector2(346, 48));
        TextLabel(prompt, "Prompt Copy", "Use the planet buttons below. Camera selection will also update this guide when available.",
            20, Muted, new Vector2(0, -64), new Vector2(344, 118));

        var detail = Rect(panel, "Planet Details", new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 1), new Vector2(22, -90), new Vector2(346, 480));
        var name = TextLabel(detail, "Planet Name", "", 36, White,
            Vector2.zero, new Vector2(295, 49));
        var close = MakeButton(detail, "Close", "X", new Vector2(306, -2), new Vector2(36, 36), 18);
        var kind = TextLabel(detail, "Kind", "", 18, new Color(0.38f, 0.81f, 0.91f),
            new Vector2(0, -50), new Vector2(340, 31));
        var facts = TextLabel(detail, "Facts", "", 19, White,
            new Vector2(0, -110), new Vector2(346, 235));
        var desc = TextLabel(detail, "Description", "", 19, Muted,
            new Vector2(0, -355), new Vector2(345, 120));

        var toolbar = Rect(root.transform, "Planet Picker", new Vector2(0.5f, 0),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 25), new Vector2(1070, 96));
        toolbar.gameObject.AddComponent<Image>().color = new Color(0.035f, 0.075f, 0.13f, 0.94f);
        TextLabel(toolbar, "Picker Label", "CHOOSE A PLANET", 15, Muted,
            new Vector2(18, -8), new Vector2(250, 25));
        var buttons = new Button[8];
        for (int i = 0; i < buttons.Length; i++)
            buttons[i] = MakeButton(toolbar, SolarSystemVisualSetup.Names[i + 1],
                SolarSystemVisualSetup.Names[i + 1], new Vector2(13 + i * 132, -41),
                new Vector2(126, 43), 17);

        root.GetComponent<PlanetInfoPanel>().Configure(null, database, detail.gameObject,
            prompt.gameObject, name, kind, facts, desc, close, buttons);
        return root;
    }

    private static RectTransform Rect(Transform parent, string name, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Text TextLabel(Transform parent, string name, string value, int size,
        Color color, Vector2 position, Vector2 dimensions)
    {
        var rect = Rect(parent, name, new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 1), position, dimensions);
        var label = rect.gameObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = size;
        label.color = color;
        label.text = value;
        label.alignment = TextAnchor.UpperLeft;
        label.raycastTarget = false;
        return label;
    }

    private static Button MakeButton(Transform parent, string name, string caption,
        Vector2 position, Vector2 dimensions, int fontSize)
    {
        var rect = Rect(parent, name, new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 1), position, dimensions);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.10f, 0.18f, 0.28f, 0.96f);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        var text = TextLabel(rect, "Label", caption, fontSize, White,
            new Vector2(0, 0), dimensions);
        text.alignment = TextAnchor.MiddleCenter;
        return button;
    }

    private static PlanetInfo[] DefaultData()
    {
        return new[]
        {
            Info(PlanetId.Mercury, "Terrestrial planet", 4879, 57.9f, 88f,
                "The smallest planet and the closest to the Sun. Its heavily cratered surface records a long history of impacts."),
            Info(PlanetId.Venus, "Terrestrial planet", 12104, 108.2f, 224.7f,
                "A thick atmosphere hides Venus's surface. Its intense greenhouse effect makes it the hottest planet."),
            Info(PlanetId.Earth, "Terrestrial planet", 12756, 149.6f, 365.2f,
                "Our home planet has liquid water at the surface and is the only known world with life."),
            Info(PlanetId.Mars, "Terrestrial planet", 6792, 228f, 687f,
                "The red planet has a thin atmosphere, polar ice, and evidence that liquid water flowed there long ago."),
            Info(PlanetId.Jupiter, "Gas giant", 142984, 778.5f, 4331f,
                "The largest planet is a gas giant with broad cloud bands and the long-lived Great Red Spot storm."),
            Info(PlanetId.Saturn, "Gas giant", 120536, 1432f, 10747f,
                "Saturn is a gas giant best known for its extensive rings, made mostly of ice and rocky debris."),
            Info(PlanetId.Uranus, "Ice giant", 51118, 2867f, 30589f,
                "Uranus is an ice giant with a blue-green appearance and an unusually tilted rotation axis."),
            Info(PlanetId.Neptune, "Ice giant", 49528, 4515f, 59800f,
                "The most distant planet is a dark blue ice giant with powerful winds and dynamic storms.")
        };
    }

    private static PlanetInfo Info(PlanetId id, string kind, int diameter, float distance,
        float period, string description)
    {
        return new PlanetInfo { id = id, displayName = id.ToString(), kind = kind,
            diameterKm = diameter, meanDistanceMillionKm = distance,
            orbitalPeriodDays = period, description = description };
    }
}

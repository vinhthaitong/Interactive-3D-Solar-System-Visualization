using UnityEngine;
using UnityEngine.UI;

public class PlanetInfoPanel : MonoBehaviour
{
    [SerializeField] private SolarSystemReferences solarSystem;
    [SerializeField] private PlanetInfoDatabase database;
    [SerializeField] private GameObject details;
    [SerializeField] private GameObject emptyState;
    [SerializeField] private Text planetName;
    [SerializeField] private Text planetKind;
    [SerializeField] private Text facts;
    [SerializeField] private Text description;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button[] planetButtons;

    private SelectionService selection;
    private readonly Color idleColor = new Color(0.10f, 0.18f, 0.28f, 0.96f);
    private readonly Color activeColor = new Color(0.11f, 0.43f, 0.55f, 1f);

    public void Configure(SolarSystemReferences references, PlanetInfoDatabase source,
        GameObject detailRoot, GameObject promptRoot, Text nameLabel, Text kindLabel,
        Text factLabel, Text descriptionLabel, Button dismissButton, Button[] buttons)
    {
        solarSystem = references;
        database = source;
        details = detailRoot;
        emptyState = promptRoot;
        planetName = nameLabel;
        planetKind = kindLabel;
        facts = factLabel;
        description = descriptionLabel;
        closeButton = dismissButton;
        planetButtons = buttons;
    }

    private void Awake()
    {
        if (solarSystem == null) solarSystem = FindAnyObjectByType<SolarSystemReferences>();
        selection = solarSystem != null ? solarSystem.selectionService : null;

        if (planetButtons != null)
            for (int i = 0; i < planetButtons.Length; i++)
            {
                int index = i;
                if (planetButtons[i] != null)
                    planetButtons[i].onClick.AddListener(() => SelectByIndex(index));
            }

        if (closeButton != null) closeButton.onClick.AddListener(ClearSelection);
    }

    private void OnEnable()
    {
        if (selection == null && solarSystem != null) selection = solarSystem.selectionService;
        if (selection != null) selection.SelectionChanged += ShowPlanet;
        ShowPlanet(selection != null ? selection.CurrentSelection : null);
    }

    private void OnDisable()
    {
        if (selection != null) selection.SelectionChanged -= ShowPlanet;
    }

    private void SelectByIndex(int index)
    {
        if (selection == null || solarSystem == null || solarSystem.planets == null ||
            index < 0 || index >= solarSystem.planets.Length) return;
        selection.SelectPlanet(solarSystem.planets[index]);
    }

    private void ClearSelection()
    {
        if (selection != null) selection.SelectPlanet(null);
    }

    private void ShowPlanet(CelestialBody body)
    {
        PlanetInfo info = body != null && database != null ? database.Find(body.planetId) : null;
        bool hasInfo = info != null;
        if (details != null) details.SetActive(hasInfo);
        if (emptyState != null) emptyState.SetActive(!hasInfo);

        if (hasInfo)
        {
            if (planetName != null) planetName.text = info.displayName;
            if (planetKind != null) planetKind.text = info.kind;
            if (facts != null)
                facts.text = "DIAMETER\n" + info.diameterKm.ToString("N0") + " km\n\n"
                    + "MEAN DISTANCE FROM SUN\n" + info.meanDistanceMillionKm.ToString("N1")
                    + " million km\n\nORBITAL PERIOD\n" + info.orbitalPeriodDays.ToString("N1") + " Earth days";
            if (description != null) description.text = info.description;
        }

        if (planetButtons != null)
            for (int i = 0; i < planetButtons.Length; i++)
                if (planetButtons[i] != null && planetButtons[i].image != null)
                    planetButtons[i].image.color = body != null && (int)body.planetId == i
                        ? activeColor : idleColor;
    }
}

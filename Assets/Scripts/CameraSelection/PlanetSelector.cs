
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class PlanetSelector : MonoBehaviour
{
    [SerializeField] private SolarSystemReferences solarSystem;

    private Camera playerCamera;
    private SelectionService selectionService;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();

        // Find the shared solar system references
        if (solarSystem == null)
        {
            solarSystem =
                FindAnyObjectByType<SolarSystemReferences>();
        }

        if (solarSystem != null)
        {
            selectionService = solarSystem.selectionService;
        }

        if (selectionService == null)
        {
            Debug.LogWarning("SelectionService not found.");
        }
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || selectionService == null)
            return;

        // Detect a left mouse click
        if (!mouse.leftButton.wasPressedThisFrame)
            return;

        // Ignore clicks on UI elements
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        SelectPlanet(mouse.position.ReadValue());
    }

    private void SelectPlanet(Vector2 mousePosition)
    {
        // Create a ray from the camera through the mouse position
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);

        RaycastHit hit;

        // Check whether the ray hits an object
        if (Physics.Raycast(ray, out hit,
            playerCamera.farClipPlane))
        {
            CelestialBody planet =
                hit.collider.GetComponentInParent<CelestialBody>();

            if (planet != null)
            {
                selectionService.SelectPlanet(planet);

                Debug.Log("Selected planet: " + planet.planetId);

                return;
            }
        }

        // Clicking empty space clears the selection
        selectionService.SelectPlanet(null);
    }
}

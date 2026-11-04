using UnityEngine;

public class SolarSystemReferences : MonoBehaviour
{
    public Transform sun;
    // Mercury through Neptune, in PlanetId order.
    public CelestialBody[] planets = new CelestialBody[8];
    public Camera mainCamera;
    public SelectionService selectionService;
}

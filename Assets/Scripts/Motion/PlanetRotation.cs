
using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    public SolarSystemReferences solarSystem;
    // One scale: Earth spins at 50 degrees/second; Sun equator takes 25 days.
    public float sunRotationSpeed = -1.99166667f;
    private SimulationControls controls;

    void Awake()
    {
        controls = GetComponent<SimulationControls>();
    }

    private float MotionDeltaTime => controls != null
        ? controls.MotionDeltaTime : Time.deltaTime;

    // NASA rounded sidereal periods: speed = -50 * 23.9 / periodHours.
    // Negative Y spins with the orbits; Venus and Uranus are retrograde.
    // Mercury through Neptune.
    public float[] rotationSpeeds =
    {
        -0.848962774f, 0.204886412f, -50f, -48.5772358f,
        -120.707071f, -111.682243f, 69.4767442f, -74.2236025f
    };

    private Transform saturnSurface;
    private MeshRenderer saturnRenderer;

    void Start()
    {
        if (solarSystem == null) return;
        foreach (CelestialBody planet in solarSystem.planets)
        {
            if (planet == null || planet.GetComponent<MeshFilter>() == null)
                continue;
            // Keep the ring child and root collider on the original planet.
            if (planet.planetId != PlanetId.Saturn) continue;
            saturnRenderer = planet.GetComponent<MeshRenderer>();
            if (saturnRenderer == null) continue;
            GameObject surface = new GameObject("Rotating Surface");
            surface.transform.SetParent(planet.BodyTransform, false);
            surface.layer = planet.gameObject.layer;
            surface.AddComponent<MeshFilter>().sharedMesh = planet.GetComponent<MeshFilter>().sharedMesh;
            surface.AddComponent<MeshRenderer>().sharedMaterials = saturnRenderer.sharedMaterials;
            saturnRenderer.enabled = false;
            saturnSurface = surface.transform;
        }
    }

    void OnDestroy()
    {
        if (saturnRenderer != null) saturnRenderer.enabled = true;
        if (saturnSurface != null) Destroy(saturnSurface.gameObject);
    }

    void Update()
    {
        if (solarSystem == null)
            return;

        if (solarSystem.sun != null)
            solarSystem.sun.Rotate(Vector3.up, sunRotationSpeed * MotionDeltaTime, Space.Self);

        for (int i = 0; i < solarSystem.planets.Length; i++)
        {
            CelestialBody planet = solarSystem.planets[i];

            if (planet == null || i >= rotationSpeeds.Length)
                continue;

            Transform surface = i == (int)PlanetId.Saturn && saturnSurface != null
                ? saturnSurface : planet.BodyTransform;
            surface.Rotate(
                Vector3.up,
                rotationSpeeds[i] * MotionDeltaTime,
                Space.Self
            );
        }
    }
}

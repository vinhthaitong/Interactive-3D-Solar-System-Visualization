
using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    public SolarSystemReferences solarSystem;
    private SimulationControls controls;

    void Awake()
    {
        controls = GetComponent<SimulationControls>();
    }

    private float MotionDeltaTime => controls != null
        ? controls.MotionDeltaTime : Time.deltaTime;

    // NASA orbital periods: speed = 50 * 23.9 / (periodDays * 24).
    // Same accelerated clock as axial rotation; degrees per second.
    // Mercury through Neptune, in PlanetId order.
    public float[] orbitSpeeds =
    {
        0.565814394f, 0.221591752f, 0.136340818f, 0.0724769529f,
        0.0114965751f, 0.0046330759f, 0.00162776379f, 0.000832636566f
    };

    private float[] orbitAngles;
    private float[] orbitRadii;
    private float[] orbitHeights;

    void Start()
    {
        if (solarSystem == null || solarSystem.sun == null)
            return;

        int count = solarSystem.planets.Length;

        orbitAngles = new float[count];
        orbitRadii = new float[count];
        orbitHeights = new float[count];

        Vector3 center = solarSystem.sun.position;

        for (int i = 0; i < count; i++)
        {
            CelestialBody planet = solarSystem.planets[i];

            if (planet == null)
                continue;

            Vector3 offset = planet.BodyTransform.position - center;

            orbitRadii[i] = new Vector2(offset.x, offset.z).magnitude;
            orbitAngles[i] = Mathf.Atan2(offset.z, offset.x);
            orbitHeights[i] = offset.y;
        }
    }

    void Update()
    {
        if (orbitAngles == null || solarSystem == null ||
            solarSystem.sun == null)
            return;

        Vector3 center = solarSystem.sun.position;

        for (int i = 0; i < orbitAngles.Length; i++)
        {
            if (i >= orbitSpeeds.Length)
                break;

            CelestialBody planet = solarSystem.planets[i];

            if (planet == null)
                continue;

            orbitAngles[i] += orbitSpeeds[i] *
                              Mathf.Deg2Rad * MotionDeltaTime;

            float x = Mathf.Cos(orbitAngles[i]) * orbitRadii[i];
            float z = Mathf.Sin(orbitAngles[i]) * orbitRadii[i];

            planet.BodyTransform.position = center +
                new Vector3(x, orbitHeights[i], z);
        }
    }
}

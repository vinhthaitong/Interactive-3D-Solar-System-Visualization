
using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    public SolarSystemReferences solarSystem;

    // Orbital speeds in degrees per second.
    // Mercury through Neptune, in PlanetId order.
    public float[] orbitSpeeds =
    {
        36f, 26f, 20f, 16f,
        11f, 8f, 6f, 5f
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
                              Mathf.Deg2Rad * Time.deltaTime;

            float x = Mathf.Cos(orbitAngles[i]) * orbitRadii[i];
            float z = Mathf.Sin(orbitAngles[i]) * orbitRadii[i];

            planet.BodyTransform.position = center +
                new Vector3(x, orbitHeights[i], z);
        }
    }
}

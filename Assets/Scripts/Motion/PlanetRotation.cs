
using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    public SolarSystemReferences solarSystem;

    // Rotation speeds in degrees per second.
    // Mercury through Neptune.
    public float[] rotationSpeeds =
    {
        5f, -4f, 50f, 48f,
        100f, 90f, -30f, 70f
    };

    void Update()
    {
        if (solarSystem == null)
            return;

        for (int i = 0; i < solarSystem.planets.Length; i++)
        {
            CelestialBody planet = solarSystem.planets[i];

            if (planet == null || i >= rotationSpeeds.Length)
                continue;

            planet.BodyTransform.Rotate(
                Vector3.up,
                rotationSpeeds[i] * Time.deltaTime,
                Space.Self
            );
        }
    }
}


using UnityEngine;

public class OrbitPath : MonoBehaviour
{
    public SolarSystemReferences solarSystem;

    public int segments = 128;
    public float lineWidth = 0.06f;

    private Material orbitMaterial;
    private Transform orbitRoot;

    void LateUpdate()
    {
        if (orbitRoot != null && solarSystem != null && solarSystem.sun != null)
            orbitRoot.position = solarSystem.sun.position;
    }

    void Start()
    {
        if (solarSystem == null || solarSystem.sun == null)
            return;

        orbitRoot = new GameObject("Orbit Paths").transform;
        orbitRoot.SetParent(transform, false);
        orbitRoot.position = solarSystem.sun.position;
        orbitMaterial = new Material(Shader.Find("Sprites/Default"));

        foreach (CelestialBody planet in solarSystem.planets)
        {
            if (planet == null)
                continue;

            CreateOrbit(planet);
        }
    }

    void CreateOrbit(CelestialBody planet)
    {
        Vector3 center = solarSystem.sun.position;
        Vector3 offset = planet.transform.position - center;
        float radius = new Vector2(offset.x, offset.z).magnitude;
        float height = offset.y;

        GameObject orbit = new GameObject(planet.name + " Orbit");
        orbit.transform.SetParent(orbitRoot, false);

        LineRenderer line = orbit.AddComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = segments;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.sharedMaterial = orbitMaterial;

        Color orbitColor = new Color(0.5f, 0.7f, 1f, 0.7f);
        line.startColor = orbitColor;
        line.endColor = orbitColor;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, height, z));
        }
    }

    void OnDestroy()
    {
        if (orbitRoot != null)
            Destroy(orbitRoot.gameObject);
        if (orbitMaterial != null)
            Destroy(orbitMaterial);
    }
}

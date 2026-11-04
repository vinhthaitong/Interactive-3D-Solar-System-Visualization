using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public PlanetId planetId;
    public Transform BodyTransform { get { return transform; } }
}

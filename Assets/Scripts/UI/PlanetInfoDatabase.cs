using System;
using UnityEngine;

[Serializable]
public class PlanetInfo
{
    public PlanetId id;
    public string displayName;
    public string kind;
    public int diameterKm;
    public float meanDistanceMillionKm;
    public float orbitalPeriodDays;
    [TextArea(3, 6)] public string description;
}

[CreateAssetMenu(menuName = "Solar System/Planet Information Database")]
public class PlanetInfoDatabase : ScriptableObject
{
    public PlanetInfo[] planets = new PlanetInfo[8];

    public PlanetInfo Find(PlanetId id)
    {
        if (planets == null) return null;
        foreach (PlanetInfo info in planets)
            if (info != null && info.id == id) return info;
        return null;
    }
}

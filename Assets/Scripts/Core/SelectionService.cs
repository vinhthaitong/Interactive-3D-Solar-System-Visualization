using System;
using UnityEngine;

public class SelectionService : MonoBehaviour
{
    public CelestialBody CurrentSelection { get; private set; }
    public event Action<CelestialBody> SelectionChanged;

    // Pass null to clear selection. Repeated selections do not raise an event.
    public void SelectPlanet(CelestialBody planet)
    {
        if (CurrentSelection == planet) return;
        CurrentSelection = planet;
        if (SelectionChanged != null) SelectionChanged(planet);
    }
}

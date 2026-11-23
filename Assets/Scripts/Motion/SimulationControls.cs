using UnityEngine;

// Changes only planetary motion; camera and information UI keep running.
public class SimulationControls : MonoBehaviour
{
    public bool paused;
    [Range(0.25f, 5f)] public float speed = 1f;
    public float MotionDeltaTime => paused ? 0f : Time.deltaTime * speed;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(12, 12, 230, 105), GUI.skin.box);
        GUILayout.Label("Planet motion");
        if (GUILayout.Button(paused ? "Resume" : "Pause"))
            paused = !paused;
        GUILayout.Label("Speed: " + speed.ToString("0.00") + "x");
        speed = GUILayout.HorizontalSlider(speed, 0.25f, 5f);
        GUILayout.EndArea();
    }
}

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Plotter))]
public class PlotterEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var script = (Plotter) target;

        if (GUILayout.Button("Plot Points"))
        {
            script.PlotPoints();
        }

        if (GUILayout.Button("Plot Buffer"))
        {
            script.PlotBuffer();
        }

        if (GUILayout.Button("Clear"))
        {
            script.Clear();
        }
    }
}

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubeGrid))]
public class CubeGridEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		CubeGrid grid = (CubeGrid)target;

		if (grid.Profiler != null) {
			if (GUILayout.Button("Run benchmark")) {
				grid.Profiler.Reset();
				grid.RunBenchmark();
			}

			GUILayout.Label($"[CPU]: {grid.Profiler.AverageCPUTimeInMs}ms");
			GUILayout.Label($"[GPU]: {grid.Profiler.AverageGPUTimeInMs}ms");
		}
	}

}

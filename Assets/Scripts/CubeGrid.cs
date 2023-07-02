using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CubeGrid : MonoBehaviour
{
	public CubeGridProfiler Profiler;

	[Header("Prefabs")]
	public Transform CubePrefab;

	[Header("Shaders")]
	public ComputeShader CubeShader;

	[Header("Cubes")]
	public int CubesPerAxis;
	public int Repetitions = 1000;

	private Transform[] _cubes;
	private float[] _cubesPositions;

	private ComputeBuffer _cubesPositionBuffer;

	[Header("Execution Mode")]
	public bool UseGPU;

	[Header("Benchmark")]
	public int numberOfExecutions = 10;

	private void Awake() {
		_cubesPositionBuffer = new ComputeBuffer(CubesPerAxis * CubesPerAxis, sizeof(float));

		Profiler = new CubeGridProfiler();
	}

	private void OnDestroy() {
		_cubesPositionBuffer.Release();
	}


	private void Start() {
		CreateGrid();
	}

	void CreateGrid() {
		_cubes = new Transform[CubesPerAxis * CubesPerAxis];
		_cubesPositions = new float[CubesPerAxis * CubesPerAxis];

		for (int x = 0, i = 0; x < CubesPerAxis; x++) {
			for (int z = 0; z < CubesPerAxis; z++, i++) {
				_cubes[i] = Instantiate(CubePrefab, transform);
				_cubes[i].transform.position = new Vector3(x, 0, z);
			}
		}

		StartCoroutine(UpdateCubeGrid());
        //UpdateCubeGrid();
	}

	IEnumerator UpdateCubeGrid() {
		while (true) {

			if (UseGPU) {
				UpdatePositionsGPU();
			}
			else {
				UpdatePositionsCPU();
			}

			for (int i = 0; i < _cubes.Length; i++) {
				_cubes[i].localPosition = new Vector3(_cubes[i].localPosition.x, _cubesPositions[i], _cubes[i].localPosition.z);
			}

			yield return new WaitForSeconds(0);
		}
	}

	void UpdatePositionsCPU() {
		for (int i = 0; i < _cubes.Length; i++) {
			for (int j = 0; j < Repetitions; j++) {
				_cubesPositions[i] = Random.Range(-1f, 1f);
			}
		}
	}

	void UpdatePositionsGPU() {
		CubeShader.SetBuffer(0, "_Positions", _cubesPositionBuffer);

		CubeShader.SetInt("_CubesPerAxis", CubesPerAxis);
		CubeShader.SetInt("_Repetitions", Repetitions);
		CubeShader.SetFloat("_Time", Time.deltaTime);

		int workgroups = Mathf.CeilToInt(CubesPerAxis / 8.0f);

		CubeShader.Dispatch(0, workgroups, workgroups, 1);

		_cubesPositionBuffer.GetData(_cubesPositions);

	}

	public void RunBenchmark() {
		StopAllCoroutines();

		Stopwatch stopWatch = new Stopwatch();

		// CPU
		for (int i = 0; i < numberOfExecutions; i++) {
			stopWatch.Reset();
			stopWatch.Start();
			UpdatePositionsCPU();
			stopWatch.Stop();
			Profiler.AddCPUCall(stopWatch.ElapsedMilliseconds);
		}

		// GPU
		for (int i = 0; i < numberOfExecutions; i++) {
			stopWatch.Reset();
			stopWatch.Start();
			UpdatePositionsGPU();
			stopWatch.Stop();
			Profiler.AddGPUCall(stopWatch.ElapsedMilliseconds);
		}
		StartCoroutine(UpdateCubeGrid());
	}
}

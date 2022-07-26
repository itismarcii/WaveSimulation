using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderHandler : MonoBehaviour
{
    [Serializable]
    internal struct Wave
    {
        public float X, Z;
        [Range(0f, 1.9999f)]public float Amplitude;
        public float Weavelength;
    }
    
    [SerializeField] private ComputeShader ComputShader_WaterSimulation;
    private Material _Material;
    // [SerializeField] private Mesh Mesh;
    private MeshFilter MeshFilter;
    private ComputeBuffer ComputeBuffer_WaterSimulation;
    private int Resolution;
    [SerializeField] private float Speed;
    [SerializeField] private Wave WaveA, WaveB, WaveC, WaveD, WaveE;

    private static readonly int
        PositionId = Shader.PropertyToID("_Positions"),
        ResolutionId = Shader.PropertyToID("_Resolution"),
        StepId = Shader.PropertyToID("_Step"),
        TimeId = Shader.PropertyToID("_Time"),
        WaveAId = Shader.PropertyToID("_WaveA"),
        WaveBId = Shader.PropertyToID("_WaveB"),
        WaveCId = Shader.PropertyToID("_WaveC"),
        WaveDId = Shader.PropertyToID("_WaveE"),
        WaveEId = Shader.PropertyToID("_WaveD");
    private float Step;

    void Awake()
    {
        MeshFilter = GetComponent<MeshFilter>();
        _Material = GetComponent<MeshRenderer>().material;
    }

    private void OnEnable()
    {
        var resolution = Math.Pow(MeshFilter.mesh.vertexCount,.5f);
        Resolution = (int)Math.Round(resolution);
        ComputeBuffer_WaterSimulation = new ComputeBuffer(Resolution * Resolution, 3 * sizeof(float));        //stride: Vector3 to float means 3 * sizeof float

        Setup();
    }

    private void Setup()
    {
        Step = 2f / Resolution;
        ComputShader_WaterSimulation.SetInt(ResolutionId, Resolution);

        ComputShader_WaterSimulation.SetVector(WaveAId, WaveToVector4(WaveA));
        ComputShader_WaterSimulation.SetVector(WaveBId, WaveToVector4(WaveB));
        ComputShader_WaterSimulation.SetVector(WaveCId, WaveToVector4(WaveC));
        ComputShader_WaterSimulation.SetVector(WaveDId, WaveToVector4(WaveD));
        ComputShader_WaterSimulation.SetVector(WaveEId, WaveToVector4(WaveE));
    }

    private void OnDisable()
    {
        ComputeBuffer_WaterSimulation.Release();
        ComputeBuffer_WaterSimulation = null;
    }

    private Vector4 WaveToVector4(Wave wave) => new Vector4(wave.X, wave.Z, wave.Amplitude, wave.Weavelength);
    

    private void UpdateFunctionOnGPU()
    {
        ComputShader_WaterSimulation.SetFloat(StepId, Step);
        ComputShader_WaterSimulation.SetFloat(TimeId, Time.fixedTime * Speed);
        ComputShader_WaterSimulation.SetBuffer(0, PositionId, ComputeBuffer_WaterSimulation);
        
        ComputShader_WaterSimulation.Dispatch(0, 8, 1, 8);

        _Material.SetBuffer(PositionId, ComputeBuffer_WaterSimulation);
        _Material.SetFloat(StepId, Step);

        Vector3[] data = new Vector3[ComputeBuffer_WaterSimulation.count];
        ComputeBuffer_WaterSimulation.GetData(data);
        BuildMesh(data);
    }

    private void FixedUpdate()
    {
        UpdateFunctionOnGPU();
    }

    private void BuildMesh(Vector3[] data)
    {
        var vertices = MeshFilter.mesh.vertices;
        var position = -Resolution / 2;
        var shift = new Vector3(position, 0, position);
        
        for (var i = 0; i < vertices.Length; i++)
            vertices[i] = data[i] + shift;

        MeshFilter.mesh.vertices = vertices;
    }

    public float GetWaveHeightAt(Vector3 transformPosition)
    {
        var vertices = MeshFilter.mesh.vertices;
        var closestVertex = vertices[0];
        var distance = Mathf.Infinity;
        var localScale = transform.localScale;
        
        foreach (var vertex in vertices)
        {
            var result = (new Vector3(
                vertex.x * localScale.x, 
                vertex.y * localScale.y, 
                vertex.z * localScale.z ) - transformPosition).sqrMagnitude;
            
            if(distance <= result) continue;
            closestVertex = vertex;
            distance = result;
        }
        return closestVertex.y;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderHandler : MonoBehaviour
{
    [SerializeField] private ComputeShader ComputShader_WaterSimulation;
    [SerializeField] private Material Material;
    [SerializeField] private Mesh Mesh;
    private ComputeBuffer ComputeBuffer_WaterSimulation;
    [SerializeField] private int Resolution;
    [SerializeField] private Vector4 WaveA, WaveB, WaveC;
    private static readonly int 
        PositionId = Shader.PropertyToID("_Positions"),
        ResolutionId = Shader.PropertyToID("_Resolution"),
        StepId = Shader.PropertyToID("_Step"),
        TimeId = Shader.PropertyToID("_Time"),
        WaveAId = Shader.PropertyToID("_WaveA"),
        WaveBId = Shader.PropertyToID("_WaveB"),
        WaveCId = Shader.PropertyToID("_WaveC");

    private void OnEnable()
    {
        ComputeBuffer_WaterSimulation = new ComputeBuffer(Resolution * Resolution, 3 * sizeof(float));        //stride: Vector3 to float means 3 * sizeof float
    }

    private void OnDisable()
    {
        ComputeBuffer_WaterSimulation.Release();
        ComputeBuffer_WaterSimulation = null;
    }

    private void UpdateFunctionOnGPU()
    {
        var step = 2f / Resolution;
        ComputShader_WaterSimulation.SetInt(ResolutionId, Resolution);
        ComputShader_WaterSimulation.SetFloat(StepId, step);
        ComputShader_WaterSimulation.SetFloat(TimeId, Time.time);
        ComputShader_WaterSimulation.SetVector(WaveAId, WaveA);
        ComputShader_WaterSimulation.SetVector(WaveBId, WaveB);
        ComputShader_WaterSimulation.SetVector(WaveCId, WaveC);
        
        
        ComputShader_WaterSimulation.SetBuffer(0, PositionId, ComputeBuffer_WaterSimulation);
        var groups = Mathf.CeilToInt(Resolution / 8f);
        ComputShader_WaterSimulation.Dispatch(0, 4, 4, 1);

        Material.SetBuffer(PositionId, ComputeBuffer_WaterSimulation);
        Material.SetFloat(StepId, step);
        
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / Resolution));
        Graphics.DrawMeshInstancedProcedural(Mesh, 0, Material, bounds, ComputeBuffer_WaterSimulation.count);
    }

    private void Update()
    {
        UpdateFunctionOnGPU();
    }
}

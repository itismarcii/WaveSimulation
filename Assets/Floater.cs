using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    private Rigidbody Rigidbody;
    public float DepthBeforeSubmerge = 1f;
    public float DisplacementAmount;
    private int FloaterCount;
    public float WaterDrag = .99f;
    public float WaterAngularDrag = .5f;
    public float Mass = 20;

    public ComputeShaderHandler ShaderHandler;
    
    private void Start()
    {
        Rigidbody = GetComponentInParent<Rigidbody>();
        FloaterCount = transform.parent.GetComponentsInChildren<Floater>().Length;
    }

    private void FixedUpdate()
    {
        if (!ShaderHandler) { Rigidbody.AddForce(Vector3.up * (Physics.gravity.y * Mass)); return; }

        var position = transform.position;
        Rigidbody.AddForceAtPosition(Physics.gravity  / FloaterCount, position, ForceMode.Acceleration);
        var waveHeight = ShaderHandler.GetWaveHeightAt(position);

        if (position.y >= waveHeight) return;
        
        var displacementMultiplier = Mathf.Clamp01((waveHeight - position.y) / DepthBeforeSubmerge) * DisplacementAmount;
        Rigidbody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), position, ForceMode.Acceleration);
        Rigidbody.AddForce(-Rigidbody.velocity * (displacementMultiplier * WaterDrag * Time.fixedDeltaTime),  ForceMode.VelocityChange);
        Rigidbody.AddTorque(-Rigidbody.angularVelocity * (displacementMultiplier * WaterAngularDrag * Time.fixedDeltaTime), ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            ShaderHandler = other.gameObject.GetComponent<ComputeShaderHandler>();
        }
        catch
        {
        }
    }
    
}

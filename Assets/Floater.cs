using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    private Rigidbody Rigidbody;
    private float DepthBeforeSubmerge = 1f;
    public float DisplacementAmount = 3f;
    public int floaterCount = 4;
    public float WaterDrag = .99f;
    public float WaterAngularDrag = .5f;

    public ComputeShaderHandler ShaderHandler;
    
    private void Start()
    {
        Rigidbody = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var position = transform.position;
        Rigidbody.AddForceAtPosition(Physics.gravity  / floaterCount, position, ForceMode.Acceleration);
        var waveHeight = ShaderHandler.GetWaveHeightAt(position);

        if (transform.position.y >= waveHeight) return;
        
        var displacementMultiplier = Mathf.Clamp01((waveHeight - transform.position.y) / DepthBeforeSubmerge) * DisplacementAmount;
        Rigidbody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), transform.position, ForceMode.Acceleration);
        Rigidbody.AddForce(-Rigidbody.angularVelocity * (displacementMultiplier * WaterDrag * Time.fixedDeltaTime),  ForceMode.VelocityChange);
        Rigidbody.AddTorque(Rigidbody.angularVelocity * (displacementMultiplier * WaterDrag * Time.fixedDeltaTime), ForceMode.VelocityChange);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    private Rigidbody Rigidbody;
    public float DepthBeforeSubmerge = 1f;
    private float DisplacementAmount;
    private int FloaterCount;
    public float WaterDrag = .99f;
    public float WaterAngularDrag = .5f;

    public ComputeShaderHandler ShaderHandler;
    
    private void Start()
    {
        Rigidbody = GetComponentInParent<Rigidbody>();
        DisplacementAmount = Math.Abs(transform.localPosition.y);
        FloaterCount = transform.parent.GetComponentsInChildren<Floater>().Length;
    }

    private void FixedUpdate()
    {
        if (!ShaderHandler) return;

        var position = transform.position;
        Rigidbody.AddForceAtPosition(Physics.gravity  / FloaterCount, position, ForceMode.Acceleration);
        var waveHeight = ShaderHandler.GetWaveHeightAt(position);

        if (position.y >= waveHeight) return;
        
        var displacementMultiplier = Mathf.Clamp01((waveHeight - position.y) / DepthBeforeSubmerge) * DisplacementAmount;
        Rigidbody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), position, ForceMode.Acceleration);
        Rigidbody.AddForce(displacementMultiplier * -Rigidbody.velocity * WaterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        //Rigidbody.AddForce(-Rigidbody.velocity * (displacementMultiplier * WaterDrag * Time.fixedDeltaTime),  ForceMode.VelocityChange);
        Rigidbody.AddTorque(displacementMultiplier * -Rigidbody.angularVelocity * WaterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        //Rigidbody.AddTorque(Rigidbody.angularVelocity * (displacementMultiplier * WaterAngularDrag * Time.fixedDeltaTime), ForceMode.VelocityChange);
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

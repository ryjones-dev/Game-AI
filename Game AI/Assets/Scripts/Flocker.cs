﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Flocker : MonoBehaviour
{
    [SerializeField]
    private Transform leader;

    [SerializeField]
    private float maxSpeed = 5.0f;

    [SerializeField, Tooltip("The distance the flocker should look for other flockers to flock with")]
    private float flockRadius = 5.0f;

    [SerializeField]
    private float separationWeight;

    [SerializeField]
    private float cohesionWeight;

    [SerializeField]
    private float alignmentWeight;

    private Rigidbody rbody;
    private Collider myCollider;

    private void Start()
    {
        rbody = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        Vector3 steeringForce = Vector3.zero;

        myCollider.enabled = false;
        Collider[] flockingColliders = Physics.OverlapSphere(transform.position, flockRadius, 1 << LayerMask.NameToLayer("Flocker"));
        myCollider.enabled = true;

        if(flockingColliders.Length > 0)
        {
            // Calculate the average flock direction
            Vector3 flockDirection = Vector3.zero;

            for (int i = 0; i < flockingColliders.Length; i++)
            {
                flockDirection += flockingColliders[i].transform.forward;

                // Separation - Flee each flocker based on distance
                float distance = Vector3.Distance(transform.position, flockingColliders[i].transform.position);
                steeringForce += ((transform.position - flockingColliders[i].transform.position).normalized * maxSpeed * (1 / distance) - rbody.velocity) * separationWeight;
            }

            flockDirection /= flockingColliders.Length;

            // Cohesion - seek the leader
            steeringForce += ((leader.position - transform.position).normalized * maxSpeed - rbody.velocity) * cohesionWeight;

            // Alignment - steer to be one with the flock
            steeringForce += (flockDirection.normalized * maxSpeed - rbody.velocity) * alignmentWeight;
        }

        Vector3 acceleration = steeringForce / rbody.mass;
        Vector3 velocity = acceleration * Time.deltaTime;
        velocity = new Vector3(velocity.x, 0, velocity.z);
        rbody.velocity = new Vector3(velocity.x, rbody.velocity.y, velocity.z);
        rbody.velocity = Vector3.ClampMagnitude(rbody.velocity, maxSpeed);

        if (rbody.velocity != Vector3.zero)
            transform.forward = Vector3.RotateTowards(transform.forward, new Vector3(rbody.velocity.x, 0, rbody.velocity.z), 0.1f, 1);
    }
}
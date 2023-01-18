using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine.Events;
using Unity.MLAgents.Sensors;

public class Drone : Agent
{
    protected float moveForce = 1;
    protected float rotSpeed = 360;

    protected Collider m_col;
    protected Rigidbody m_rigidBody;

    protected Vector3 moveAmount;
    protected Quaternion rotateAmount;

    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider, Collider> { }

    [Header("Trigger Callbacks")]
    public TriggerEvent onTriggerEnterEvent = new TriggerEvent();

   protected void Awake()
    {
        m_col = GetComponent<Collider>();
        m_rigidBody = GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        //7 observations overall

        //position and rotation observations 4 overall
        sensor.AddObservation(gameObject.transform.position);
        sensor.AddObservation(gameObject.transform.rotation.y);
        //velocity observations 3 overall
        sensor.AddObservation(m_rigidBody.velocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float moveY = actions.ContinuousActions[2];
        float rot = actions.ContinuousActions[3];

        rotateAmount = Quaternion.Euler(new Vector3(0, rot, 0) * rotSpeed * Time.fixedDeltaTime);
        moveAmount = transform.TransformDirection(new Vector3(moveX, moveY, moveZ)* moveForce);
    }

    protected void FixedUpdate()
    {
        m_rigidBody.MoveRotation((m_rigidBody.rotation * rotateAmount).normalized);
        m_rigidBody.AddForce(moveAmount, ForceMode.VelocityChange);
    }

    //calls the controllers on trigger event
    private void OnTriggerEnter(Collider other)
    {        
        onTriggerEnterEvent.Invoke(m_col, other);
    }

    protected void AdjustSpeed(string parameter_name) {
        float speedAdjust = Academy.Instance.EnvironmentParameters.GetWithDefault(parameter_name, 1.0f);
        moveForce *= speedAdjust;
        rotSpeed *= speedAdjust;
    }
}

    

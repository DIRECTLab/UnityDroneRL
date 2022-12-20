using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine.Events;

public class Defender : Agent
{
    float moveSpeed = 10;
    float rotSpeed = 360;


    private Collider m_col;


    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider, Collider> { }

    [Header("Trigger Callbacks")]
    public TriggerEvent onTriggerEnterEvent = new TriggerEvent();

    private void Awake()
    {
        m_col = GetComponent<Collider>();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float moveY = actions.ContinuousActions[2];
        float rot = actions.ContinuousActions[3];

        Vector3 rotation = new Vector3(0, rot, 0) * Time.deltaTime * rotSpeed;
        transform.Rotate(rotation, Space.World);

        var transformVector = new Vector3(moveX, moveY, moveZ) * Time.deltaTime * moveSpeed;
        transform.Translate(transformVector, Space.Self);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[3] = -Input.GetAxisRaw("Rotation");
        continuousActionsOut[2] = -Input.GetAxisRaw("UpDown");
        continuousActionsOut[1] = Input.GetAxisRaw("Vertical");
        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
    }

    //calls the controllers on trigger event
    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnterEvent.Invoke(m_col, other);
    }
}
   
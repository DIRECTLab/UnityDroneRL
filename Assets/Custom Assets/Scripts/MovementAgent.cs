using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MovementAgent : Agent
{

    float moveSpeed = 5;
    [SerializeField] private Transform targetTransform = null;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        /*actionsOut[1] = Input.GetAxisRaw("Horizontal");
        actionsOut[0] = -Input.GetAxisRaw("Vertical");*/
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
       /* float moveX = vectorActions[0];
        float moveZ = vectorActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;*/
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal")
        {
            SetReward(1f);
        }
        else if (other.tag == "Wall") { 
            SetReward(-1f);
        }
        EndEpisode();
    }
}

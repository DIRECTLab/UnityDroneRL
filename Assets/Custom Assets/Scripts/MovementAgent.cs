using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;

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

    public override void OnActionReceived(float[] vectorActions)
    {
        float moveX = vectorActions[0];
        float moveZ = vectorActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[1] = Input.GetAxisRaw("Horizontal");
        actionsOut[0] = -Input.GetAxisRaw("Vertical");
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

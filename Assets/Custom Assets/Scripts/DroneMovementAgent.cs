using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class DroneMovementAgent : Agent
{
    float moveSpeed = 5;
    [SerializeField] private Transform targetTransform = null;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
        targetTransform.localPosition = new Vector3(Random.Range(-9f, 9f), Random.Range(5, 19f), Random.Range(-9f, 9f));
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
        float moveY = vectorActions[2];
        transform.localPosition += new Vector3(moveX, moveY, moveZ) * Time.deltaTime * moveSpeed;

        SetReward(-Vector3.Distance(transform.localPosition, targetTransform.localPosition));
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[2] = -Input.GetAxisRaw("UpDown");
        actionsOut[1] = Input.GetAxisRaw("Horizontal");
        actionsOut[0] = -Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal")
        {
            SetReward(1f);
        }
        EndEpisode();
    }

}

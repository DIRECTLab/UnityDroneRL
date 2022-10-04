using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class VisionMovementAgent : Agent
{
    float moveSpeed = 5;
    float rotSpeed = 90;
    [SerializeField] private Transform targetTransform = null;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        float boundingArea = 1;
        float absoluteBounds = 7;

        targetTransform.localPosition = new Vector3(Random.Range(-absoluteBounds, absoluteBounds), 0, Random.Range(-absoluteBounds, absoluteBounds));

        //move the target away from the center
        if (-boundingArea < targetTransform.localPosition.x && targetTransform.localPosition.x < boundingArea) {
            float minAdition;
            float maxAdition;
            if (targetTransform.localPosition.x < 0)
            {
                maxAdition = -boundingArea - targetTransform.localPosition.x;
                minAdition = maxAdition - (absoluteBounds - boundingArea);
            }
            else
            {
                minAdition = boundingArea - targetTransform.localPosition.x;
                maxAdition = minAdition + (absoluteBounds - boundingArea);
            }

            targetTransform.localPosition += new Vector3(Random.Range(minAdition, maxAdition), 0, 0);
        }
        if (-boundingArea < targetTransform.localPosition.z && targetTransform.localPosition.z < boundingArea)
        {
            float minAdition;
            float maxAdition;
            if (targetTransform.localPosition.z < 0)
            {
                maxAdition = -boundingArea - targetTransform.localPosition.z;
                minAdition = maxAdition - (absoluteBounds - boundingArea);
            }
            else
            {
                minAdition = boundingArea - targetTransform.localPosition.z;
                maxAdition = minAdition + (absoluteBounds - boundingArea);
            }
            targetTransform.localPosition += new Vector3(0, 0, Random.Range(minAdition, maxAdition));
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
            float moveX = actions.ContinuousActions[0];
            float rot = actions.ContinuousActions[1];
        Vector3 rotation = new Vector3(0, rot, 0) * Time.deltaTime * rotSpeed;

            transform.Rotate(rotation, Space.World);
            transform.localPosition += transform.forward * moveX * Time.deltaTime * moveSpeed;

            SetReward(-Vector3.Distance(transform.localPosition, targetTransform.localPosition));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {

        var  continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxisRaw("Vertical");
        continuousActionsOut[1] = Input.GetAxisRaw("Horizontal");
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
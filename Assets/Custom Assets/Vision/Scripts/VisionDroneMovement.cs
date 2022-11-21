using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class VisionDroneMovement : Agent
{
    float moveSpeed = 5;
    float rotSpeed = 180;
    [SerializeField] private Transform targetTransform = null;

    //xz bound of safe box
    float boundingArea = 1;
    //farthest in xz goal can be
    float absoluteBound= 9;

    //y bound of safebox
    float yBoundingArea = 5;
    //farthest in y goal can be
    float yAbsoluteBound = 19;

    public bool withinBoundXZ(float loc) {
        return (-boundingArea < loc && loc < boundingArea && targetTransform.localPosition.y < yBoundingArea);
    }

    public float adjustXZ(float loc) {
        if (withinBoundXZ(loc))
        {
            float minAdition;
            float maxAdition;
            if (loc < 0)
            {
                maxAdition = -boundingArea - loc;
                minAdition = maxAdition - (absoluteBound - boundingArea);
            }
            else
            {
                minAdition = boundingArea - loc;
                maxAdition = minAdition + (absoluteBound - boundingArea);
            }

            return Random.Range(minAdition, maxAdition);
        }

        return 0;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        targetTransform.localPosition = new Vector3(Random.Range(-absoluteBound, absoluteBound), Random.Range(0, yAbsoluteBound), Random.Range(-absoluteBound, absoluteBound));

        //move the target away from the center
        if (withinBoundXZ(targetTransform.localPosition.x) && withinBoundXZ(targetTransform.localPosition.z))
        {
            targetTransform.localPosition += new Vector3(adjustXZ(targetTransform.localPosition.x), 0, adjustXZ(targetTransform.localPosition.z));
        }
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
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

        AddReward(0f);
        AddReward(-Vector3.Distance(transform.localPosition, targetTransform.localPosition));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {

        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[3] = -Input.GetAxisRaw("Rotation");
        continuousActionsOut[2] = -Input.GetAxisRaw("UpDown");
        continuousActionsOut[1] = Input.GetAxisRaw("Vertical");
        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal")
        {
            SetReward(10f);

        }
        EndEpisode();

    }

}
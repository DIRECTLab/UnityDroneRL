using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine.Events;

public class Defender : Drone
{

    private CoOpVisionController cont;

    private void Awake()
    {
        base.Awake();
        cont = GetComponentInParent<CoOpVisionController>();
    }

    

    public void FixedUpdate()
    {
        float shortestDistance = -1;
        float highestAngleVal = -1;

        foreach (CoOpVisionController.AttackerInfo a in cont.AttackerList)
        {
            Attacker attacker = a.Agent;

            float distance = Vector3.Distance(transform.localPosition, attacker.transform.localPosition);
            if (shortestDistance == -1 || distance < shortestDistance)
            {
                shortestDistance = distance;
            }

            float angleVal = utils.centerAngleWeight(attacker.transform.localPosition, 30, transform);
            if (highestAngleVal == -1 || angleVal > highestAngleVal)
            {
                highestAngleVal = angleVal;
            }
        }

        float maxDistance = Vector3.Distance(cont.bounds.extents, -cont.bounds.extents);
        if (highestAngleVal != -1 && shortestDistance != -1)
        {
            //Debug.Log("Distance " + shortestDistance);
            //Debug.Log("Distance Val " + weightedFlip(shortestDistance, maxDistance));
            AddReward(utils.weightedFlip(shortestDistance, maxDistance) * .5f);

            //Debug.Log("Angle " + highestAngleVal);
            AddReward(highestAngleVal * .5f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[3] = -Input.GetAxisRaw("Rotation");
        continuousActionsOut[2] = -Input.GetAxisRaw("UpDown");
        continuousActionsOut[1] = Input.GetAxisRaw("Vertical");
        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
    }
}
   
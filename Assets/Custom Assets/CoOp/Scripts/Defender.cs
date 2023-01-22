using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine.Events;
using Unity.MLAgents.Sensors;

public class Defender : Drone
{

    private CoOpVisionController cont;
    private BufferSensorComponent m_BufferSensor;

    private new void Awake()
    {
        base.Awake();
        cont = GetComponentInParent<CoOpVisionController>();
        m_BufferSensor = GetComponent<BufferSensorComponent>();
        AdjustSpeed("defender_speed");
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        foreach (Attacker attacker in cont.GetAttackers())
        {
            Vector3 pos = utils.addNoise(attacker.transform.localPosition);
            
            float[] floatPos = new float[3];
            floatPos[0] = pos.x;
            floatPos[1] = pos.y;
            floatPos[2] = pos.z;
            
            m_BufferSensor.AppendObservation(floatPos);
        }
    }

    //reward the defender for seeing and being close to an attacker
    public new void FixedUpdate()
    {
        base.FixedUpdate();

        float shortestDistance = -1;
        float highestAngleVal = -1;

        foreach (Attacker attacker in cont.GetAttackers())
        {           
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
            AddReward(utils.weightedFlip(shortestDistance, maxDistance) * .5f);

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
   
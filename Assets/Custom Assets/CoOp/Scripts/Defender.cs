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

    private void addDrone(Drone d) {
        Vector3 pos = utils.addNoise(d.transform.localPosition);

        float[] floatPos = new float[4];
        floatPos[0] = pos.x;
        floatPos[1] = pos.y;
        floatPos[2] = pos.z;
        floatPos[3] = d is Attacker? 1: 0; //if the drone is an attacker

        m_BufferSensor.AppendObservation(floatPos);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);


        //5 observations overall
        // 1 hot encoding of state defender or attacker 
        //position observations 3 overall
        foreach (Attacker attacker in cont.GetAttackers())
        {
            addDrone(attacker);
        }
        foreach (Defender defender in cont.GetDefenders()) {
            if (this != defender)
            {
                addDrone(defender);
            }
            /*else {
                Debug.Log("Me");
            }*/
        }

    }

    //punishes the defender for not seeing and being close to an attacker
    public new void FixedUpdate()
    {
        base.FixedUpdate();

        float shortestDistance = -1;
        float shortestAngle = -1;

        //gets the best distance and angle values
        //the angle value gets weighted between 0 and 1
        foreach (Attacker attacker in cont.GetAttackers())
        {           
            float distance = Vector3.Distance(transform.localPosition, attacker.transform.localPosition);
            if (shortestDistance == -1 || distance < shortestDistance)
            {
                shortestDistance = distance;
            }

            float angleVal = utils.angle(attacker.transform.localPosition, transform);
            if (shortestAngle == -1 || angleVal < shortestAngle)
            {
                shortestAngle = angleVal;
            }
        }
        float maxDistance = Vector3.Distance(cont.bounds.extents, -cont.bounds.extents);
        if (shortestAngle != -1 && shortestDistance != -1)
        {
            /*Debug.Log($"Shortest distance: {shortestDistance}");
            Debug.Log($"Shortest distance val: {-(shortestDistance / maxDistance)}");
            Debug.Log($"Shortest angle: {shortestAngle}");
            Debug.Log($"Shortest distance val: {utils.centerAngleWeight(shortestAngle, 30)}");*/
            AddReward( -(shortestDistance / maxDistance) * .5f);
            AddReward( utils.centerAngleWeight(shortestAngle, 30) * .5f);
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
   
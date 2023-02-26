using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine.Events;
using Unity.MLAgents.Sensors;

public class Defender : Drone
{

    public BufferSensorComponent m_BufferSensorAttack;
    public BufferSensorComponent m_BufferSensorDefend;

    private new void Awake()
    {
        base.Awake();
        AdjustSpeed("defender_speed");
    }

    private void addDrone(Drone d, BufferSensorComponent b) {
        Vector3 pos = utils.addNoise(d.transform.localPosition);
        pos = cont.NormalizePoint(pos);
/*        Debug.Log($"Position other: {pos}");
*/        
        float[] floatPos = new float[3];
        floatPos[0] = pos.x;
        floatPos[1] = pos.y;
        floatPos[2] = pos.z;
        b.AppendObservation(floatPos);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        /*Vector3 pos = utils.addNoise(transform.localPosition);
        pos = cont.NormalizePoint(pos);
        Debug.Log($"\n\n\n\n\n\n\nPosition: {pos}");*/

        //5 observations overall
        // 1 hot encoding of state defender or attacker 
        //position observations 3 overall
        foreach (Attacker attacker in cont.GetAttackers())
        {
            addDrone(attacker, m_BufferSensorAttack);
        }
        foreach (Defender defender in cont.GetDefenders()) {
            if (this != defender)
            {
                addDrone(defender, m_BufferSensorDefend);
            }
        }
    }

    //punishes the defender for not seeing and being close to an attacker
    public new void FixedUpdate()
    {
        base.FixedUpdate();

       /* float shortestDistance = -1;
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
            *//*Debug.Log($"Shortest distance: {shortestDistance}");
            Debug.Log($"Shortest distance val: {-(shortestDistance / maxDistance)}");
            Debug.Log($"Shortest angle: {shortestAngle}");
            Debug.Log($"Shortest distance val: {utils.centerAngleWeight(shortestAngle, 30)}");*//*
            AddReward( -(shortestDistance / maxDistance) *.5f  /cont.MaxEnvironmentSteps);
            AddReward( utils.centerAngleWeight(shortestAngle, 30)* .5f / cont.MaxEnvironmentSteps);
        }*/
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[3] = -Input.GetAxisRaw("Rotation");
        continuousActionsOut[2] = -Input.GetAxisRaw("UpDown");
        continuousActionsOut[1] = Input.GetAxisRaw("Vertical");
        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
    }

    protected void OnTriggerEnter(Collider other)
    {
        //reward for colliding with atttacker
        /*if (other.tag == CoOpVisionSettings.attackTag) { 
            AddReward(1); 
        }

        //punishment for running into the wall ot other defenders
        else if (other.tag == CoOpVisionSettings.wallTag || other.tag == CoOpVisionSettings.defendTag) AddReward(-1);*/

        base.OnTriggerEnter(other);

    }
}
   
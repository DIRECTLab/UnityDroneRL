using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Attacker : Drone
{
    private new void Awake()
    {
        base.Awake();
        AdjustSpeed("attacker_speed");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        var pos = utils.addNoise(cont.AttackerGoal.transform.localPosition);
        pos = cont.NormalizePoint(pos);

        sensor.AddObservation(pos);
    }

    public new void FixedUpdate()
    {
        base.FixedUpdate();

        //as long as the attacker is alive, reward it
        //AddReward(.25f);
    }

    new protected void OnTriggerEnter(Collider other)
    {
        //reward for escaping
        if (other.tag == CoOpVisionSettings.goaltag) AddReward(1);
        //punishment for running into the walls
        else if (other.tag == CoOpVisionSettings.wallTag) AddReward(-1);

        base.OnTriggerEnter(other);
    }
}

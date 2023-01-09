using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;

public class Attacker : Drone
{
    private new void Awake()
    {
        base.Awake();
        AdjustSpeed("attacker_speed");
    }
    public void FixedUpdate()
    {
        //as long as the attacker is alive, reward it
        AddReward(.25f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine.Events;

public class Defender : Agent
{
    float moveSpeed = 10;
    float rotSpeed = 360;

    private CoOpVisionController cont;
    private Collider m_col;

    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider, Collider> { }

    [Header("Trigger Callbacks")]
    public TriggerEvent onTriggerEnterEvent = new TriggerEvent();

    private void Awake()
    {
        cont = GetComponentInParent<CoOpVisionController>();
        m_col = GetComponent<Collider>();
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

        
    }

    //flips a value around another value
    //flipping 1 around 2 gives 3
    //flipping 2 around 6 gives 10
    private float flip(float val, float flipLoc) { 
        float dif = flipLoc - val;

        return val + (dif * 2);
    }

    //flips and weights a val between 0 and 1 (assuming its between 0 and the bound to begin with
    private float weightedFlip(float val, float bound)
    {
        return flip(val, (bound / 2)) / bound;
    }

    //returns 1 if centered, and decreases towards 0 as angle approaches the bounds
    public float centerAngleWeight(Vector3 local_pos, float angle_bound) {
        Vector3 targetDir = local_pos - transform.localPosition;
        float angle = Vector3.Angle(targetDir, transform.forward);

        //if angle is within  degrees bound 
        if (angle <= angle_bound) {
            return weightedFlip(angle, angle_bound);
        }

        return 0;
    }

    public void FixedUpdate()
    {
        float shortestDistance = -1;
        float highestAngleVal = -1;

        foreach (CoOpVisionController.AttackerInfo a in cont.AttackerList) {
            Attacker attacker = a.Agent;

            float distance = Vector3.Distance(transform.localPosition, attacker.transform.localPosition);
            if (shortestDistance == -1 || distance < shortestDistance)
            {
                shortestDistance = distance;
            }

            float angleVal = centerAngleWeight(attacker.transform.localPosition, 30);
            if (highestAngleVal == -1 || angleVal > highestAngleVal) { 
                highestAngleVal = angleVal;
            }
        }

        float maxDistance = Vector3.Distance(cont.bounds.extents, -cont.bounds.extents) ;
        if (highestAngleVal != -1 && shortestDistance != -1)
        {
            //Debug.Log("Distance " + shortestDistance);
            //Debug.Log("Distance Val " + weightedFlip(shortestDistance, maxDistance));
            AddReward(weightedFlip(shortestDistance, maxDistance) * .5f);

            //Debug.Log("Angle " + highestAngleVal);
            AddReward(highestAngleVal *  .5f);
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

    //calls the controllers on trigger event
    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnterEvent.Invoke(m_col, other);
    }
}
   
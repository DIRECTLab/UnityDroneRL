using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class CoOpVisionController : MonoBehaviour
{
    [System.Serializable]
    public class DefenderInfo
    {
        public Defender Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }
    public List<DefenderInfo> DefenderList = new List<DefenderInfo>();

    [System.Serializable]
    public class AttackerInfo
    {
        public Attacker Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }
    public List<AttackerInfo> AttackerList = new List<AttackerInfo>();

    //the max reset steps and the reset timer
    public int MaxEnvironmentSteps = 25000;
    private int m_ResetTimer;

    private SimpleMultiAgentGroup m_DefenderGroup;

    // Start is called before the first frame update
    void Start()
    {
        m_DefenderGroup = new SimpleMultiAgentGroup();
        foreach (var defender in DefenderList) { 
            defender.StartingPos = defender.Agent.transform.position;
            defender.StartingRot = defender.Agent.transform.rotation;
            defender.Rb = defender.Agent.GetComponent<Rigidbody>();
             
            m_DefenderGroup.RegisterAgent(defender.Agent);
        }

        ResetScene();
    }

    private void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_DefenderGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        //Hurry Up Penalty
        m_DefenderGroup.AddGroupReward(-0.5f / MaxEnvironmentSteps);
    }

   //invoked by defenders upon collision with object
    public void DefenderCollision(Collider collider, Collider collided) {
        if (collided.tag == "Goal")
        {
            m_DefenderGroup.AddGroupReward(10);
            m_DefenderGroup.EndGroupEpisode();
            ResetScene();
        }
    }

    public void ResetScene() { 
        m_ResetTimer = 0;

        foreach (var defender in DefenderList) {
            var pos = defender.StartingPos;
            var rot = defender.StartingRot;

            defender.Agent.transform.SetPositionAndRotation(pos, rot);

            defender.Rb.velocity = Vector3.zero;
            defender.Rb.angularVelocity = Vector3.zero;
        }
    }
}

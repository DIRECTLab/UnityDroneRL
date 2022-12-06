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
    }
    public List<AttackerInfo> AttackerList = new List<AttackerInfo>();

    //the max reset steps and the reset timer
    public int MaxEnvironmentSteps = 25000;
    private int m_ResetTimer;

    private int m_NumberOfRemainingDefenders;


    [HideInInspector]
    public Bounds bounds;

    private SimpleMultiAgentGroup m_DefenderGroup;

    // Start is called before the first frame update
    void Start()
    {
        //gets the bounds then disables bounds collider so it doesnt get in the way
        bounds = gameObject.GetComponent<Collider>().bounds;
        gameObject.GetComponent<Collider>().enabled = false;

        //sets up defender group
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

        //if out of time
        if ( (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0))
        {
            m_DefenderGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        //Hurry Up Penalty
        m_DefenderGroup.AddGroupReward(-0.5f / MaxEnvironmentSteps);
    }

   //invoked by defenders upon collision with object
    public void DefenderCollision(Collider collider, Collider collided) {
        //if collide with attacker, win
        if (collided.tag == "Attacker")
        {
            m_DefenderGroup.AddGroupReward(10);
            m_DefenderGroup.EndGroupEpisode();
            ResetScene();
        }
        //if collide with wall, remove self
        else if (collided.tag == "Wall")
        {
            Remove(collider);
        }
        //if collide with other defender remove both of them
        else if (collided.tag == "Defender") {
            Remove(collider);
            Remove(collided);
        }
    }

    public void Remove(Collider collider)
    {
        GameObject go = collider.gameObject;

        if (go.activeSelf) { 
            m_NumberOfRemainingDefenders--;

            if (m_NumberOfRemainingDefenders <= 0)
            {
                m_DefenderGroup.EndGroupEpisode();
                ResetScene();
            }
            else { 
                go.SetActive(false);
            }
        }
    }

    //get a random spawn location inside the areas bounds
    public Vector3 GetRandomSpawnPos()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {

            var marginMultiplier = .9f;
            var randomPosX = Random.Range(-bounds.extents.x * marginMultiplier, bounds.extents.x * marginMultiplier);
            var randomPosY = Random.Range(1, bounds.extents.y * 2 * marginMultiplier);
            var randomPosZ = Random.Range(-bounds.extents.z * marginMultiplier, bounds.extents.z * marginMultiplier);
            randomSpawnPos = new Vector3(randomPosX, randomPosY, randomPosZ);

            if (Physics.CheckBox(randomSpawnPos, new Vector3(1.5f, 1f, 1.5f), orientation: Quaternion.identity,  layerMask : (Physics.DefaultRaycastLayers << 3) ) == false)
            {
                foundNewSpawnLocation = true;
            }

        }
        return randomSpawnPos;
    }

    public void ResetScene() { 
        m_ResetTimer = 0;
        m_NumberOfRemainingDefenders = DefenderList.Count;

        foreach (var defender in DefenderList) {
            var pos = defender.StartingPos;
            var rot = defender.StartingRot;

            defender.Agent.transform.SetPositionAndRotation(pos, rot);

            defender.Rb.velocity = Vector3.zero;
            defender.Rb.angularVelocity = Vector3.zero;

            if (!defender.Agent.gameObject.activeSelf)
            {
                defender.Agent.gameObject.SetActive(true);
                m_DefenderGroup.RegisterAgent(defender.Agent);
            }
        }

        foreach (var attacker in AttackerList) {
            var pos = GetRandomSpawnPos();
            attacker.Agent.transform.position = pos;
        }
    }
}

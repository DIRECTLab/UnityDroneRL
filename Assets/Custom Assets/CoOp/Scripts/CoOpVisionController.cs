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

    public GameObject resultDisplay;
    private Renderer m_resultRenderer;
    private Material m_standardResultMat;

    public Collider colliderBounds;
    [HideInInspector] public Bounds bounds;

    private int m_NumberOfRemainingDefenders;
    private SimpleMultiAgentGroup m_DefenderGroup;

    private int m_NumberOfRemainingAttackers;
    private SimpleMultiAgentGroup m_AttackerGroup;


    private CoOpVisionSettings m_CoOpSettings;

    //the max reset steps and the reset timer
    [HideInInspector] public int MaxEnvironmentSteps;
    private int m_ResetTimer;

    // Start is called before the first frame update
    void Start()
    {
        m_CoOpSettings = FindObjectOfType<CoOpVisionSettings>();
        MaxEnvironmentSteps = m_CoOpSettings.MaxEnvironmentSteps;

        //gets the bounds then disables bounds collider so it doesnt get in the way
        bounds = colliderBounds.bounds;
        gameObject.GetComponent<Collider>().enabled = false;

        m_resultRenderer = resultDisplay.GetComponent<Renderer>();
        m_standardResultMat = m_resultRenderer.material;

        //sets up defender group
        m_DefenderGroup = new SimpleMultiAgentGroup();
        foreach (var defender in DefenderList) { 
            defender.StartingPos = defender.Agent.transform.localPosition;
            defender.StartingRot = defender.Agent.transform.localRotation;
            defender.Rb = defender.Agent.GetComponent<Rigidbody>();
             
            m_DefenderGroup.RegisterAgent(defender.Agent);
        }

        m_AttackerGroup = new SimpleMultiAgentGroup();
        foreach (var attacker in AttackerList) { 
            attacker.StartingPos = attacker.Agent.transform.localPosition;
            attacker.StartingRot = attacker.Agent.transform.localRotation;
            attacker.Rb = attacker.Agent.GetComponent<Rigidbody>();
            m_AttackerGroup.RegisterAgent(attacker.Agent);
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
            m_AttackerGroup.GroupEpisodeInterrupted();
            setVisualLog(m_standardResultMat);
            ResetScene();
        }

        if (m_NumberOfRemainingAttackers <= 0 || m_NumberOfRemainingDefenders <= 0) {
            //if the attackers are all gone,  then set status for defenders to 1
            int defendersSucceed = m_NumberOfRemainingAttackers <= 0 ? 1: -1;

            if (defendersSucceed < 0)
            {
                setVisualLog(m_CoOpSettings.attackerWin);
            }
            //if defenders succeed they get +1 and attackers get -1
            //if defenders fail they get -1 and attackers get +1
            m_DefenderGroup.AddGroupReward(defendersSucceed);
            m_AttackerGroup.AddGroupReward(-defendersSucceed);


            m_DefenderGroup.EndGroupEpisode();
            m_AttackerGroup.EndGroupEpisode();

            ResetScene();
        }

        //Hurry Up Penalty
        m_DefenderGroup.AddGroupReward(-0.25f / MaxEnvironmentSteps);

        //Stay Alive Reward
        m_AttackerGroup.AddGroupReward(0.25f / MaxEnvironmentSteps);

    }

   //invoked by defenders upon collision with object
    public void Collision(Collider collider, Collider collided) {
        //if collide with wall attacker or defender, remove self
         if (collided.tag == CoOpVisionSettings.wallTag || collided.tag == CoOpVisionSettings.attackTag || collided.tag == CoOpVisionSettings.defendTag)
        {
            //visual loggin system
            if (collider.tag == CoOpVisionSettings.attackTag)
            {
                if (collided.tag == CoOpVisionSettings.defendTag)
                {
                    setVisualLog(m_CoOpSettings.defenderWin);
                }
                else
                {
                    setVisualLog(m_CoOpSettings.attackerCrash);
                }
            }
            Remove(collider);
        }
    }


    public void Remove(Collider collider)
    {
        GameObject obj = collider.gameObject;

        if (obj.activeSelf) {
            if (collider.tag == CoOpVisionSettings.attackTag)
            {
                m_NumberOfRemainingAttackers --;
            }
            else if (collider.tag == CoOpVisionSettings.defendTag)
            {
                m_NumberOfRemainingDefenders--;
            }
            obj.SetActive(false);
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

    private void setVisualLog(Material material)
    {
        m_resultRenderer.material = material;
    }

    public void ResetScene()
    {
        m_ResetTimer = 0;
        m_NumberOfRemainingDefenders = DefenderList.Count;
        m_NumberOfRemainingAttackers = AttackerList.Count;

        foreach (var defender in DefenderList)
        {
            var pos = defender.StartingPos;
            var rot = defender.StartingRot;

            defender.Agent.transform.localPosition = pos;
            defender.Agent.transform.localRotation = rot;

            defender.Rb.velocity = Vector3.zero;
            defender.Rb.angularVelocity = Vector3.zero;

            if (!defender.Agent.gameObject.activeSelf)
            {
                defender.Agent.gameObject.SetActive(true);
                m_DefenderGroup.RegisterAgent(defender.Agent);
            }
        }

        foreach (var attacker in AttackerList)
        {
            var pos = GetRandomSpawnPos();
            var rot = attacker.StartingRot;

            attacker.Agent.transform.localPosition = pos;
            attacker.Agent.transform.localRotation = rot;

            attacker.Rb.velocity = Vector3.zero;
            attacker.Rb.angularVelocity = Vector3.zero;

            if (!attacker.Agent.gameObject.activeSelf)
            {
                attacker.Agent.gameObject.SetActive(true);
                m_AttackerGroup.RegisterAgent(attacker.Agent);
            }
        }
    }

    public List<Attacker> GetAttackers() { 
        List<Attacker> attakers = new List<Attacker>();
        foreach (var attackerInfo in AttackerList) {
            var attacker = attackerInfo.Agent;
            if (attacker.enabled) { 
                attakers.Add(attacker);
            }
        }
        return attakers;
    }
    public List<Defender> GetDefenders()
    {
        List<Defender> defenders = new List<Defender>();
        foreach (var defenderInfo in DefenderList)
        {
            var defender = defenderInfo.Agent;
            if (defender.enabled)
            {
                defenders.Add(defender);
            }
        }
        return defenders;
    }
}

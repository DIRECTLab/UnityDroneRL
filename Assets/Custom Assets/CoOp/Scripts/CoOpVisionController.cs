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

    public GameObject AttackerGoal;

    public GameObject resultDisplay;
    private Renderer m_resultRenderer;
    private Material m_standardResultMat;

    public Collider colliderBounds;
    [HideInInspector] public Bounds bounds;

    private int m_maxDefenders;
    private int m_NumberOfRemainingDefenders;
    private SimpleMultiAgentGroup m_DefenderGroup;

    private int m_defenderInterceptCount;
    private int m_escapedAttackersCount;

    private int m_maxAttackers;
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
        if ((m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0))
        {
            m_DefenderGroup.GroupEpisodeInterrupted();
            m_AttackerGroup.GroupEpisodeInterrupted();
            setVisualLog(m_standardResultMat);
            ResetScene();
        }

        
        //if defenders intercepted the attackers
        if (m_defenderInterceptCount > 0 && m_NumberOfRemainingAttackers <= 0)
        {
            setVisualLog(m_CoOpSettings.defenderWin);

            m_AttackerGroup.AddGroupReward(0);
            m_DefenderGroup.AddGroupReward(1);
            m_AttackerGroup.EndGroupEpisode();
            m_DefenderGroup.EndGroupEpisode();
            ResetScene();
        }
        //if any attacker managed to escape
        else if (m_escapedAttackersCount > 0)
        {
            setVisualLog(m_CoOpSettings.attackerWin);

            m_AttackerGroup.AddGroupReward(1);
            m_DefenderGroup.AddGroupReward(0);
            m_AttackerGroup.EndGroupEpisode();
            m_DefenderGroup.EndGroupEpisode();
            ResetScene();
        }
        //if one side has completely crashed on their own
        //for crashed teams call EndGroupEpisode, for surviving teams
        //call end group episode
        else if (m_NumberOfRemainingAttackers <= 0 || m_NumberOfRemainingDefenders <= 0)
        {
            setVisualLog(m_CoOpSettings.attackerCrash);

            //if there are still attackers alive, then all the defenders crashed
            if (m_NumberOfRemainingAttackers > 0)
            {
                m_AttackerGroup.GroupEpisodeInterrupted();
                m_DefenderGroup.EndGroupEpisode();
            }
            //if there are still defenders alive, then all the attackers crashed
            else if (m_NumberOfRemainingDefenders > 0)
            {
                m_AttackerGroup.EndGroupEpisode();
                m_DefenderGroup.GroupEpisodeInterrupted();
            }
            //if both teams completely crashed
            else
            {
                m_AttackerGroup.EndGroupEpisode();
                m_DefenderGroup.EndGroupEpisode();
            }

            ResetScene();           
        }

        //Stay Alive Reward
        //m_DefenderGroup.AddGroupReward(0.25f / MaxEnvironmentSteps);
        //m_AttackerGroup.AddGroupReward(0.25f / MaxEnvironmentSteps);

    }

   //invoked by defenders upon collision with object
    public void Collision(Collider collider, Collider collided) {
        //attacker collides with defender
        if (collider.tag == CoOpVisionSettings.attackTag && collided.tag == CoOpVisionSettings.defendTag)
        {
            m_defenderInterceptCount++;
        }
        //if attacker collides with the goal
        else if (collider.tag == CoOpVisionSettings.attackTag && collided.tag == CoOpVisionSettings.goaltag)
        {
            m_escapedAttackersCount++;
        }

        //if object collides with wall, attacker or defender
        if (collided.tag == CoOpVisionSettings.wallTag || collided.tag == CoOpVisionSettings.attackTag || collided.tag == CoOpVisionSettings.defendTag)
        {
            
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

    private float normalizeDimension(float val, float extant) { 
        return Mathf.Clamp(val, -extant, extant) / extant;
    }

    public Vector3 NormalizePoint(Vector3 point) { 
        point.x = normalizeDimension(point.x, bounds.extents.x);
        point.y = normalizeDimension(point.y - bounds.extents.y, bounds.extents.y);
        point.z = normalizeDimension(point.z, bounds.extents.z);
        return point;
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

            if (Physics.CheckBox(randomSpawnPos, new Vector3(2.2f, 1f, 2.2f), orientation: Quaternion.identity,  layerMask : (Physics.DefaultRaycastLayers << 3) ) == false)
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
        m_maxDefenders = m_NumberOfRemainingDefenders;

        m_defenderInterceptCount = 0;
        m_escapedAttackersCount = 0;

        m_NumberOfRemainingAttackers = AttackerList.Count;
        m_maxAttackers = m_NumberOfRemainingAttackers;

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

        AttackerGoal.transform.localPosition = GetRandomSpawnPos();
    }

    public List<Attacker> GetAttackers() { 
        List<Attacker> attakers = new List<Attacker>();
        foreach (var attackerInfo in AttackerList) {
            var attacker = attackerInfo.Agent;
            if (attacker.isActiveAndEnabled) { 
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
            if (defender.isActiveAndEnabled)
            {
                defenders.Add(defender);
            }
        }
        return defenders;
    }
}

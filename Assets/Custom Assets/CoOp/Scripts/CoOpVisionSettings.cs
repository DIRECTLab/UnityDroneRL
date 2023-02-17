using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CoOpVisionSettings : MonoBehaviour
{
    public const string wallTag = "Wall";
    public const string attackTag = "Attacker";
    public const string defendTag = "Defender";
    public const string goaltag = "Goal";

    //the max reset steps and the reset timer
    public int MaxEnvironmentSteps = 25000;

    public Material defenderWin;
    public Material attackerCrash;
    public Material attackerWin;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

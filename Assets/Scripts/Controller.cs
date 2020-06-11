using MLAgents.Policies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public BehaviorParameters bp;
    public Rigidbody droneRb;
    public AgentScript agentScprit;
    public GameObject drone;
    // Start is called before the first frame update
    void Start()
    {
        //bp.behaviorType = BehaviorType.HeuristicOnly;
        //droneRb.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
 
    }
}

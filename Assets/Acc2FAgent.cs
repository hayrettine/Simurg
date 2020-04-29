using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using System;

public class Acc2FAgent : Agent
{



    Rigidbody rBody;
    Transform tf;
    Vector3 currentVelocity;
    Vector3 desiredVelocity;
    public GameObject rotor1;
    public GameObject rotor2;
    public GameObject rotor3;
    public GameObject rotor4;
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        tf = GetComponent<Transform>();
        currentVelocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void AgentReset()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        tf.rotation = Quaternion.identity;
        tf.position = new Vector3(0, 20, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        desiredVelocity = currentVelocity + new Vector3(UnityEngine.Random.value * 2 - 1, UnityEngine.Random.value * 2 - 1, UnityEngine.Random.value * 2 - 1);
        sensor.AddObservation(tf.rotation);
        if ((desiredVelocity - currentVelocity).magnitude != 0)
            sensor.AddObservation((desiredVelocity - currentVelocity) / (desiredVelocity - currentVelocity).magnitude);
        else
            sensor.AddObservation(Vector3.zero);

        if (currentVelocity.magnitude != 0)
            sensor.AddObservation(currentVelocity / currentVelocity.magnitude);
        else
            sensor.AddObservation(Vector3.zero);
    }

    public override void AgentAction(float[] vectorAction)
    {
        float action1 = Mathf.Clamp(vectorAction[0], 0f, 1f);
        float action2 = Mathf.Clamp(vectorAction[1], 0f, 1f);
        float action3 = Mathf.Clamp(vectorAction[2], 0f, 1f);
        float action4 = Mathf.Clamp(vectorAction[3], 0f, 1f);

        rBody.AddForceAtPosition(tf.up * 10f * action1, rotor1.transform.position);
        rBody.AddForceAtPosition(tf.up * 10f * action2, rotor2.transform.position);
        rBody.AddForceAtPosition(tf.up * 10f * action3, rotor3.transform.position);
        rBody.AddForceAtPosition(tf.up * 10f * action4, rotor4.transform.position);

        currentVelocity = rBody.velocity;
        Debug.Log(currentVelocity);
        AddReward(-CalculateReward());
    }

    private float CalculateReward()
    {
        return (desiredVelocity - currentVelocity).magnitude;
    }
}

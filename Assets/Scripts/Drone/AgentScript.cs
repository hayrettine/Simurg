using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.IO;
using MLAgents.Sensors;
//using MLAgents.Sensor;
public class AgentScript : Agent
{

    public Rigidbody rBody;
    public Transform tf;
    public Rigidbody targetBody;
    public Transform targetTransform;
    public Camera camera;
    public int resWidth = 800;
    public int resHeight = 600;
    int max;
    float[] last_action;    
    public GameObject platform;
    public Transform plane;
 
    void Start()
    {
        last_action = new float[2];
        
    }
    

    public override void AgentReset()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        //tf.position = new Vector3(Random.RandomRange(-15, 15), 20f, Random.RandomRange(-15, 15));
        tf.position = new Vector3(plane.position.x, plane.position.y + 20f, plane.position.z);

        //TODO platformun resetleneceği kısım. targetTransform platformun tranformu. [Sinan]
        //TODO platformu (0,0.5,0) noktasında oluşturmamız yeterli şuan. Sonra random'a çekeriz.
        //targetTransform.position = new Vector3(Random.Range(-5, 5), 0.5f, Random.Range(-5, 5));
        //targetTransform.position = new Vector3(0, 0.5f, 0);

        platform.GetComponent<PlatformMovement>().ResetPlatform();



    }

 
    public override void CollectObservations(VectorSensor sensor)
    {
      sensor.AddObservation(punishmentForCenter());
      sensor.AddObservation(new Vector2(rBody.velocity.x / 20, rBody.velocity.z / 20));
    }

    public override void AgentAction(float[] vectorAction)
    {
        rBody.velocity += new Vector3(vectorAction[0], 0, vectorAction[1]);
        AddReward(-punishmentForCenter().magnitude);

        if (max == 0)//|| tf.position.y < 0.3)
        {
            Debug.Log("Done");
            AddReward(-punishmentForCenter().magnitude);
            Done();
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[2];
        //action[0] = Input.GetAxis("Horizontal");
        //action[0] = Input.GetAxis("Vertical");
        Debug.Log("heur");
        return action;
    }

    public Vector2 punishmentForCenter()
    {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Color32[] asd = screenShot.GetPixels32();
        int max_index = 0;
        int min_index = resHeight * resWidth;
        for (int i = 0; i < asd.Length; i++)
        {
            if (asd[i].r < 50)
            {
                if (i > max_index)
                {
                    max_index = i;
                }
                if (i < min_index)
                {
                    min_index = i;
                }
            }
        }
        max = max_index;
        Destroy(rt);
        Destroy(screenShot);

        int columnOfMax = max_index % resWidth;
        int columnOfMin = min_index % resWidth;
        int rowOfMax = max_index / resWidth;
        int rowOfMin = min_index / resWidth;

        float horizontalDistance2Center = ((resWidth / 2) - ((float)(columnOfMax + columnOfMin) / 2)) / (resWidth / 2);
        float verticalDistance2Center = ((resHeight / 2) - ((float)(rowOfMax + rowOfMin) / 2)) / (resHeight / 2);
        return new Vector2(horizontalDistance2Center, verticalDistance2Center);
    }


    

}

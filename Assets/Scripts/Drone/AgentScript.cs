using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.IO;
using MLAgents.Sensors;
using MLAgents.Policies;
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
    

    int currentBlack = 0;

 
    void Start()
    {
        last_action = new float[3];
        
    }
    

    public override void AgentReset()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        //tf.position = new Vector3(Random.RandomRange(-15, 15), 20f, Random.RandomRange(-15, 15));
        tf.position = new Vector3(plane.position.x, plane.position.y + 35f, plane.position.z);
     

        //TODO platformun resetleneceği kısım. targetTransform platformun tranformu. [Sinan]
        //TODO platformu (0,0.5,0) noktasında oluşturmamız yeterli şuan. Sonra random'a çekeriz.
        //targetTransform.position = new Vector3(Random.Range(-5, 5), 0.5f, Random.Range(-5, 5));
        //targetTransform.position = new Vector3(0, 0.5f, 0);

        platform.GetComponent<PlatformMovement>().ResetPlatform();

        currentBlack = 0;

       

    }

 
    public override void CollectObservations(VectorSensor sensor)
    {
      sensor.AddObservation(punishmentForCenter());
      
      sensor.AddObservation(currentBlack / (resWidth * resHeight));
      currentBlack = 0;
      sensor.AddObservation(new Vector3(rBody.velocity.x / 20, rBody.velocity.y / 10, rBody.velocity.z / 20));
    }

    public override void AgentAction(float[] vectorAction)
    {
        float actionY = Mathf.Clamp(vectorAction[1], 0f, 1f);
        rBody.velocity += new Vector3(vectorAction[0], actionY, vectorAction[2]);
        transform.rotation = Quaternion.identity;
        transform.Rotate(vectorAction[2] * 20, 0 , vectorAction[0] * 20);
        AddReward(-punishmentForCenter().magnitude);
    
        
        AddReward(currentBlack / (resHeight * resWidth));
        
        if (max == 0)//|| tf.position.y < 0.3)
        {

           // Debug.Log("max çalıştı");
            //Debug.Log(punishmentForCenter());

            //punishmentForCenter();
            AddReward(-100);
            Done();
        }
        if(currentBlack < 50)
        {
            AddReward(-100);
            Done();
        }
        if(currentBlack / (resWidth * resHeight) >= 0.95)
        {
            AddReward(150);
            AddReward(rBody.velocity.y);
            Debug.Log(rBody.velocity);
            
            Done();

        }
        currentBlack = 0;
    }

    public override float[] Heuristic()
    {
        var action = new float[3];
        //action[0] = Input.GetAxis("Horizontal");
        //action[0] = Input.GetAxis("Vertical");
        
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
            if (asd[i].r != 255)
            {
                if (i > max_index)
                {
                    max_index = i;
                }
                if (i < min_index)
                {
                    min_index = i;
                }
                currentBlack++;
            }
        }

    
        max = max_index;
       // Debug.Log(max);
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

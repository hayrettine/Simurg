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

    Rigidbody rBody;
    Transform tf;
    public GameObject platform;
    public Transform plane;
    Color32[] cameraImage;
    
     
    public Camera camera;
    public int resWidth = 800;
    public int resHeight = 600;

 
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        tf = GetComponent<Transform>();

       
    }
    

    public override void AgentReset()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        tf.position = new Vector3(plane.position.x, plane.position.y + 35f, plane.position.z);
        platform.GetComponent<PlatformMovement>().ResetPlatform();
    }

 
    public override void CollectObservations(VectorSensor sensor)
    {
      cameraImage = GetImageFromCamera();
      sensor.AddObservation(PixelDistanceForObservation());
      sensor.AddObservation(GetBlackPixelCount() / (resWidth * resHeight));
      sensor.AddObservation(new Vector3(rBody.velocity.x / 20, rBody.velocity.y / 20, rBody.velocity.z / 20));
    }

    public override void AgentAction(float[] vectorAction)
    {
        transform.rotation = Quaternion.identity;
        transform.Rotate(vectorAction[2] * 20, 0, vectorAction[0] * 20);

        float actionY = Mathf.Clamp(vectorAction[1], 0f, 1f);
        rBody.velocity += new Vector3(vectorAction[0], actionY, vectorAction[2]);

        int currentBlackPixelCount = GetBlackPixelCount();
       

        AddReward(PunishmentForCenter());

        AddReward(-1 + ((float)currentBlackPixelCount / (resHeight * resWidth)));
 


        if (currentBlackPixelCount < 50)
        {
            AddReward(-100);
            Done();
        }
        if(currentBlackPixelCount / (resWidth * resHeight) >= 0.95)
        {
            AddReward(150);
            AddReward(rBody.velocity.y);
            Debug.Log(rBody.velocity);
            Done();
        }
       

    }

    public float PunishmentForCenter()
    {
        int[] featuresFromImage = GetPositionOFBlackPixelsCenter();
        return CalculateCenterOfBlackPixelsFromMaxMinValue(featuresFromImage).magnitude * -1;
    }

    public Vector2 PixelDistanceForObservation()
    {
        int[] featuresFromImage = GetPositionOFBlackPixelsCenter();
        return CalculateCenterOfBlackPixelsFromMaxMinValue(featuresFromImage);
    }

    Vector2 CalculateCenterOfBlackPixelsFromMaxMinValue(int[] featuresFromImage)
    {

        int columnOfMax = featuresFromImage[0];
        int rowOfMax = featuresFromImage[1];
        int columnOfMin = featuresFromImage[2];        
        int rowOfMin = featuresFromImage[3];

        //Normalized distance values 
        float horizontalDistanceFromCenter = ((resWidth / 2) - ((float)(columnOfMax + columnOfMin) / 2)) / (resWidth / 2);
        float verticalDistanceFromCenter = ((resHeight / 2) - ((float)(rowOfMax + rowOfMin) / 2)) / (resHeight / 2);

        return new Vector2(horizontalDistanceFromCenter, verticalDistanceFromCenter);
    }

    Color32[] GetImageFromCamera()
    {
        //Get Image(RGB24) From camera 
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        //Set Image Texture format to RGB24
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        //Set camera resolution height and resolution weight
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; 
        //Get pixels from Texture
        Color32[] imageOfCamera = screenShot.GetPixels32();

        //Destroy objects for out of memory
        Destroy(rt);
        Destroy(screenShot);

        return imageOfCamera;
    }

    int[] GetPositionOFBlackPixelsCenter()
    {
        int max_index = 0;
        int min_index = resHeight * resWidth;
        for (int i = 0; i < cameraImage.Length; i++)
        {
            if (cameraImage[i].r != 255)
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


        // Debug.Log(max);
        int columnOfMax = max_index % resWidth;
        int columnOfMin = min_index % resWidth;
        int rowOfMax = max_index / resWidth;
        int rowOfMin = min_index / resWidth;

        int[] resultFeatures = { columnOfMax, rowOfMax, columnOfMin, rowOfMin };
        return resultFeatures;

    }

    int GetBlackPixelCount()
    {
        int blackPixelCount = 0;
        for (int i = 0; i < cameraImage.Length; i++)
        {
            if (cameraImage[i].r != 255)
            {
                blackPixelCount++;
            }
        }
        return blackPixelCount;
    }






}

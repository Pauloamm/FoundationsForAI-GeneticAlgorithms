using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.U2D.Path;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class Movement : MonoBehaviour
{

    public delegate void OnDestroy();
    public event OnDestroy OnDestroyEvent;
    
    struct Wisker
    {
        public Vector3 direction;
        public float distanceToObject;
    }

   
    // w1a + w2b + w3c + w4ab + w5ac + w6bc + w7a2 + w8b2 + w9c2 = angle to turn 
    //private const int numberOfWeights = 9;
    
    //Weights
    public float[] wiskersWeights;
        
        
        
    private float timeAlive = 0f;
    public float GetAliveTime => timeAlive;
    
    public float speed = 1f;
    
    public float maxAngle = 90f;
    public float wiskerSize = 0.5f;

    private Dictionary<char, Wisker> wiskersDict;
    
    
   
    //Wiskers
    private Wisker frontWiskerDirection;
    private Wisker leftWiskerDirection;
    private Wisker rightWiskerDirection;
    
    // Start is called before the first frame update
    void Start()
    {

        // wiskersWeights = new float[9];
        // for (int index = 0; index < wiskersWeights.Length; index++)
        // {
        //     //wiskersWeights[index] = 1;
        //     Random rg = new Random();
        //     wiskersWeights[index] = (float)rg.NextDouble()*2f -1;
        // }

       
        
        
        wiskersDict = new Dictionary<char, Wisker>();
        UpdateWiskers();
        
        wiskersDict.Add('a',leftWiskerDirection);
        wiskersDict.Add('b',frontWiskerDirection);
        wiskersDict.Add('c',rightWiskerDirection);

    }
    void Update()
    {
        timeAlive += Time.deltaTime;
        
        MovePosition();
        UpdateWiskers();
        
        
        transform.up = Quaternion.Euler(0, 0, AngleToTurn()) * transform.up;
    }

    void MovePosition()
    {
        this.transform.position += transform.up * Time.deltaTime * speed;
    }

    void UpdateWiskers()
    {
        frontWiskerDirection.direction =  transform.up;
        leftWiskerDirection.direction =  (Quaternion.Euler(0, 0, maxAngle / 2) * transform.up)  ;
        rightWiskerDirection.direction =  (Quaternion.Euler(0, 0, -maxAngle / 2) * transform.up) ;
    }


    float SendRaycast(Vector3 wiskerToRaycast)
    {
        RaycastHit hit;

        if (!Physics.Raycast(transform.position, wiskerToRaycast, out hit, wiskerSize)) return 0;
        
        return hit.distance;
        
 
    }

    
    
     float AngleToTurn()
     {
         float a = SendRaycast(wiskersDict['a'].direction);
         float b = SendRaycast(wiskersDict['b'].direction);
         float c = SendRaycast(wiskersDict['c'].direction);


         return wiskersWeights[0] * a +
                wiskersWeights[1] * b +
                wiskersWeights[2] * c +
                wiskersWeights[3] * a * b +
                wiskersWeights[4] * a * c +
                wiskersWeights[5] * b * c +
                wiskersWeights[6] * a * a +
                wiskersWeights[7] * b * b +
                wiskersWeights[8] * c * c ;
         
     }
    
     private void OnTriggerEnter(Collider other)
     {
         if (!other.CompareTag("Wall")) return;
         
         OnDestroyEvent?.Invoke();
         gameObject.SetActive(false);
     }
     
     //DEBUG----------------------------------------------------
     
     
     bool isObjectHittingWisker(Vector3 wiskerDirection)
     {
         float currentDist = SendRaycast(wiskerDirection);

         return currentDist > 0 && currentDist < wiskerSize;

     }
    private void OnDrawGizmos()
    {
        Vector3 currentPosition = this.transform.position;
        
        if(isObjectHittingWisker(frontWiskerDirection.direction)) Gizmos.color = Color.red;
        else Gizmos.color = Color.green;
        Gizmos.DrawLine(currentPosition, currentPosition + frontWiskerDirection.direction * wiskerSize);

        if(isObjectHittingWisker(leftWiskerDirection.direction)) Gizmos.color = Color.red;
        else Gizmos.color = Color.green;
        Gizmos.DrawLine(currentPosition, currentPosition + leftWiskerDirection.direction* wiskerSize);
        
        
        if(isObjectHittingWisker(rightWiskerDirection.direction)) Gizmos.color = Color.red;
        else Gizmos.color = Color.green;
        Gizmos.DrawLine(currentPosition, currentPosition + rightWiskerDirection.direction* wiskerSize);

    }

    public void NormalizeTimeAlive(float totalPopulationTime)
    {
        this.timeAlive = this.timeAlive / totalPopulationTime;
    }
   
}

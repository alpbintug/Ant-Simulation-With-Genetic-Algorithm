using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    private float timer;
    public List<Vector3> PathToTake;
    public float DestroyTimer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DestroyTimer -= Time.deltaTime;
        DestroyTimer = DestroyTimer < float.MinValue/2 ? -10 : DestroyTimer;
        if(DestroyTimer <= 0 && DestroyTimer>-10)
        {
            Destroy(this.gameObject);
        }
    }
}

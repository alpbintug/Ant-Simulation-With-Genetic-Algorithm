using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    private float timer;
    public List<Vector3> PathToTake;
    public float DestroyTimer = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timer += Time.deltaTime;

        if(timer > DestroyTimer && DestroyTimer > 0)
        {
            Destroy(this.gameObject);
        }
    }
}

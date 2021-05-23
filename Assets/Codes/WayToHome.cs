using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayToHome : MonoBehaviour
{
    private float timer;
    public float DestroyTimer = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer > DestroyTimer)
        {
            Destroy(this.gameObject);
        }
    }
}

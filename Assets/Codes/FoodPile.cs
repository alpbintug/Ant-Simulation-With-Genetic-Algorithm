using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPile : MonoBehaviour
{
    [Range(0f,10000f)]
    public float FoodCount = 100f;
    GameObject Food;
    // Start is called before the first frame update
    void Start()
    {
        Food = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Food.transform.localScale = new Vector3(FoodCount / 20, FoodCount / 20);
        if (FoodCount == 0)
            Destroy(Food);
    }
}

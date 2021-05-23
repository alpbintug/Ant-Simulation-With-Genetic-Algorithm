using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : MonoBehaviour
{
    [Range(0.5f, 10f)]
    public float HormoneRange = 1.5f;

    [Range(10f, 200f)]
    public float HormonePermanency = 30f;

    [Range(0.1f, 2f)]
    public float SensorRange = 0.5f;

    [Range(65f, 165f)]
    public float AngleOfVision = 75f;

    [Range(1f, 10f)]
    public float RangeOfVision = 3f;

    [Range(2f, 20f)]
    public float Velocity = 5f;

    [Range(0.1f, 2f)]
    public float CarryingCapacity = 0.5f;

    [Range(0.1f, 2f)]
    public float CostPerAnt = 1f;

    private GameObject AntObject;

    public GameObject borders;

    public GameObject WayToHome;


    private float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        AntObject = this.gameObject;
        Vector3 angles = AntObject.transform.eulerAngles;
        float angle = Random.Range(-180f, 180f);
        angles.z = angle;
        AntObject.transform.eulerAngles = angles;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 angles = AntObject.transform.eulerAngles;
        float angle = Random.Range(-5f, 5f);
        angles.z += angle;
        if (!isInMap())
            angles.z += Random.Range(1f,3f)*90;
        AntObject.transform.eulerAngles = angles;
        AntObject.transform.position += AntObject.transform.up * Time.deltaTime * Velocity;
        timer += Time.deltaTime;
        if (timer > 1)
        {
            timer = 0;
            GameObject wth = GameObject.Instantiate(WayToHome);
            wth.SetActive(true);
            wth.transform.position = this.transform.position;
            wth.transform.localScale = new Vector3(HormoneRange, HormoneRange);
        }
    }

    private bool isInMap()
    {
        return Mathf.Abs(AntObject.transform.position.x) < borders.transform.localScale.x && Mathf.Abs(AntObject.transform.position.y) < borders.transform.localScale.y;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class ColonyCenter : MonoBehaviour
{
    public List<GameObject> Ants;
    public GameObject Ant;
    private GameObject Colony;
    public GameObject text;
    public GameObject food;
    public GameObject textTimer;
    public int StartingPopulation = 5;
    public float FoodStored = 0;
    public float TargetFood = 500;
    private GameObject bestAnt;
    private GameObject secondBestAnt;
    private float bestTiming = float.MaxValue;
    private float secondBestTiming = float.MaxValue;
    private float currentTiming = 0;
    private int genCount = 0;
    private float foodCount;
    private float lastGenTiming;

    private const int SEEKING_FOOD = 0;
    private const int RETURNING_TO_BASE = 1;
    private const int RETURNING_TO_FOOD = 2;
    private const int CAN_SEE_FOOD = 3;
    // Start is called before the first frame update
    void Start()
    {
        Colony = this.gameObject;
        AddAnts();
        bestAnt = GameObject.Instantiate(Ants[0]);
        bestAnt.SetActive(false);
        foodCount = food.GetComponent<FoodPile>().FoodCount;
        secondBestAnt = GameObject.Instantiate(Ants[0]);
        secondBestAnt.SetActive(false);
        Ant _ant = Ants[0].GetComponent<Ant>();
        text.GetComponent<Text>().text = "Generation: " + genCount
            + "\nSensor Range:  " + _ant.SensorRange + ", Hormone Permanency: " + _ant.HormonePermanency
            + "\nAngle of Vision: " + _ant.AngleOfVision + ", Range of Vision: " + _ant.RangeOfVision
            + "\nVelocity: " + _ant.Velocity + ", Carrying Capacity: " + _ant.CarryingCapacity;
    }

    private void AddAnts()
    {
        for (int i = 0; i < StartingPopulation; i++)
        {
            Ants.Add(GameObject.Instantiate(Ant));
            Ants[i].SetActive(true);
            Ants[i].name = "Ant " + i;
            Ants[i].transform.position = this.transform.position;
        }
    }
    private void RemoveAnts()
    {
        foreach (GameObject ant in Ants)
        {
            Destroy(ant);
        }
        Ants.Clear();
    }
    private void GenerateNextGen()
    {
        Ant bestParent = bestAnt.GetComponent<Ant>();
        Ant secondBestParent = secondBestAnt.GetComponent<Ant>();
        float cutPoint = Random.Range(0, 1);
        float check = Random.Range(0, 1.1f);
        if (check > 1)
        {
            float val = Ant.GetComponent<Ant>().AngleOfVision;
            Ant.GetComponent<Ant>().AngleOfVision += Random.Range(-4f, 5f);
            Debug.Log("Mutation on Angle of Vision. Old value: "+val
                +" New value: " + Ant.GetComponent<Ant>().AngleOfVision);
        }
        else
        {
            Ant.GetComponent<Ant>().AngleOfVision = 
                check > cutPoint ? 
                bestParent.AngleOfVision : secondBestParent.AngleOfVision;
        }
        check = Random.Range(0, 1.1f);
        if (check > 1)
        {
            float val = Ant.GetComponent<Ant>().CarryingCapacity;
            Ant.GetComponent<Ant>().CarryingCapacity += Random.Range(-0.4f, 0.5f);
            Debug.Log("Mutation on Carrying Capacity. Old value: " + val + " New value: " + Ant.GetComponent<Ant>().CarryingCapacity);
        }
        else
        {
            Ant.GetComponent<Ant>().CarryingCapacity = check > cutPoint ? bestParent.CarryingCapacity : secondBestParent.CarryingCapacity;
        }
        check = Random.Range(0, 1.1f);
        if (check > 1)
        {
            float val = Ant.GetComponent<Ant>().RangeOfVision;
            Ant.GetComponent<Ant>().RangeOfVision += Random.Range(-4f, 5f);
            Debug.Log("Mutation on Range of Vision. Old value: " + val + " New value: " + Ant.GetComponent<Ant>().RangeOfVision);
        }
        else
        {
            Ant.GetComponent<Ant>().RangeOfVision = check > cutPoint ? bestParent.RangeOfVision : secondBestParent.RangeOfVision;
        }
        check = Random.Range(0, 1.1f);
        if (check > 1)
        {
            float val = Ant.GetComponent<Ant>().Velocity;
            Ant.GetComponent<Ant>().Velocity += Random.Range(-2f, 2f);
            Debug.Log("Mutation on Velocity. Old value: " + val + " New value: " + Ant.GetComponent<Ant>().Velocity);
        }
        else
        {
            Ant.GetComponent<Ant>().Velocity = check > cutPoint ? bestParent.Velocity : secondBestParent.Velocity;
        }
        check = Random.Range(0, 1.1f);
        if (check > 1)
        {
            float val = Ant.GetComponent<Ant>().SensorRange;
            Ant.GetComponent<Ant>().SensorRange += Random.Range(-2f,2f);
            Debug.Log("Mutation on Sensor Range. Old value: " + val + " New value: " + Ant.GetComponent<Ant>().SensorRange);
        }
        else
        {
            Ant.GetComponent<Ant>().SensorRange = check > cutPoint ? bestParent.SensorRange : secondBestParent.SensorRange;
        }
        check = Random.Range(0, 1.1f);
        if (check > 1)
        {
            float val = Ant.GetComponent<Ant>().HormonePermanency;
            Ant.GetComponent<Ant>().HormonePermanency += Random.Range(-3f, 4f);
            Debug.Log("Mutation on Hormone Permanency. Old value: " + val + " New value: " + Ant.GetComponent<Ant>().HormonePermanency);
        }
        else
        {
            Ant.GetComponent<Ant>().HormonePermanency = check > cutPoint ? bestParent.HormonePermanency : secondBestParent.HormonePermanency;
        }

    }
    // Update is called once per frame
    void Update()
    {
        NextGenCalculation();   
    }
    private void NextGenCalculation()
    {
        currentTiming += Time.deltaTime;
        textTimer.GetComponent<Text>().text = "Time: " + currentTiming + "\nCurrent food: " + FoodStored + "\nTarget food: " + TargetFood;
        if (FoodStored >= TargetFood)
        {
            GameObject[] path = GameObject.FindGameObjectsWithTag("Path");
            foreach (var item in path)
            {
                if (item.GetComponent<Waypoint>().DestroyTimer > 0)
                    Destroy(item);
            }
            FoodStored = 0;
            food.GetComponent<FoodPile>().FoodCount = foodCount;
            if (currentTiming < bestTiming)
            {
                secondBestTiming = bestTiming;
                secondBestAnt = bestAnt;
                bestTiming = currentTiming;
                bestAnt = GameObject.Instantiate(Ants[0]);
                bestAnt.SetActive(false);

            }
            else if (currentTiming < secondBestTiming)
            {
                secondBestTiming = currentTiming;
                secondBestAnt = GameObject.Instantiate(Ants[0]);
                secondBestAnt.SetActive(false);
            }
            genCount++;
            GenerateNextGen();
            RemoveAnts();
            AddAnts();
            Ant _ant = Ants[0].GetComponent<Ant>();
            text.GetComponent<Text>().text = "Generation: " + genCount
                + "\nSensor Range:  " + _ant.SensorRange + ", Hormone Permanency: " + _ant.HormonePermanency
                + "\nAngle of Vision: " + _ant.AngleOfVision + ", Range of Vision: " + _ant.RangeOfVision
                + "\nVelocity: " + _ant.Velocity + ", Carrying Capacity: " + _ant.CarryingCapacity
                + "\n\nBest gen time: " + bestTiming + (secondBestTiming < float.MaxValue ? ", Second best gen time: " + secondBestTiming : "")
                + "\nLast gen time: " + currentTiming;
            currentTiming = 0;
        }
    }
}

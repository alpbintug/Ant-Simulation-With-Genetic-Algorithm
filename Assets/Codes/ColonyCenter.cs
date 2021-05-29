using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColonyCenter : MonoBehaviour
{
    public List<GameObject> Ants;
    public GameObject Ant;
    private GameObject Colony;
    public int StartingPopulation = 5;
    public float FoodStored = 0;

    private const int SEEKING_FOOD = 0;
    private const int RETURNING_TO_BASE = 1;
    private const int RETURNING_TO_FOOD = 2;
    private const int CAN_SEE_FOOD = 3;
    // Start is called before the first frame update
    void Start()
    {
        Colony = this.gameObject;
        for (int i = 0; i < StartingPopulation; i++)
        {
            Ants.Add(GameObject.Instantiate(Ant));
            Ants[i].SetActive(true);
            Ants[i].transform.position = this.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //PathFinder();
    }
    private void PathFinder()
    {
        int i;
        List<GameObject> FoodSeekers = Ants.FindAll(g => g.GetComponent<Ant>().STATUS == SEEKING_FOOD);
        List<GameObject> FoodCarriers = Ants.FindAll(g => (g.GetComponent<Ant>().STATUS == SEEKING_FOOD || g.GetComponent<Ant>().STATUS == RETURNING_TO_FOOD));

        if (FoodSeekers.Count == 0 || FoodCarriers.Count == 0)
            return;
        foreach (GameObject seeker in FoodSeekers)
        {
            Ant antSeeker = seeker.GetComponent<Ant>();
            foreach(GameObject carrier in FoodCarriers)
            {
                Ant antCarrier = carrier.GetComponent<Ant>();
                for(i = 0; i < antCarrier.movesFromFood.Count && antSeeker.STATUS != RETURNING_TO_FOOD; i++)
                {
                    if (Vector3.Distance(antCarrier.movesFromFood[i], antSeeker.transform.position) < antSeeker.SensorRange)
                    {
                        antSeeker.movesFromFood = antCarrier.movesFromFood.GetRange(0,i);
                        antSeeker.STATUS = RETURNING_TO_FOOD;
                    }
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColonyCenter : MonoBehaviour
{
    public List<GameObject> Ants;
    public GameObject Ant;
    private GameObject Colony;
    public int StartingPopulation = 5;
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
        
    }
}

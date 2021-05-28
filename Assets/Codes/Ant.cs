using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Ant : MonoBehaviour
{
    #region GENES OF THE ANT
    [Range(0.5f, 10f)]
    public float HormoneRange = 1.5f;

    [Range(10f, 200f)]
    public float HormonePermanency = 30f;

    [Range(1f, 20f)]
    public float SensorRange = 5f;

    [Range(45f, 165f)]
    public float AngleOfVision = 75f;

    [Range(10f, 100f)]
    public float RangeOfVision = 30f;

    [Range(2f, 20f)]
    public float Velocity = 5f;

    [Range(0.1f, 2f)]
    public float CarryingCapacity = 0.5f;

    [Range(0.1f, 2f)]
    public float CostPerAnt = 1f;
    #endregion

    #region SETTING UP VARIABLES
    #region GAMEOBJECTS
    private GameObject AntObject;
    public GameObject borders;
    public GameObject WayToHome;
    private GameObject FoodSource;
    #endregion
    #region VALUES
    private float currentFood = 0;
    private float timer = 0;
    private float angle;
    private Vector3 angles;
    #endregion
    #region MOVES
    public List<Vector3> movesFromHome;
    public List<Vector3> movesFromFood;
    private Vector3 targetWaypoint;
    public List<Vector3> moves;
    #endregion
    #endregion

    #region STATUS VARIABLES
    private const int SEEKING_FOOD = 0;
    private const int RETURNING_TO_BASE = 1;
    private const int RETURNING_TO_FOOD = 2;
    private const int CAN_SEE_FOOD = 3;
    private int STATUS;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        STATUS = SEEKING_FOOD;
        moves = new List<Vector3>();
        movesFromFood = new List<Vector3>();
        movesFromHome = new List<Vector3>();
        AntObject = this.gameObject;
        angles = AntObject.transform.eulerAngles;
        angle = Random.Range(-180f, 180f);
        angles.z = angle;
        AntObject.transform.eulerAngles = angles;
    }

    // Update is called once per frame
    void Update()
    {
        DecideMove();
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer > 0.1)
        {
            timer = 0;
            SaveMoves();
        }
    }

    #region MOVEMENT FUNCTIONS
    /// <summary>
    /// Function that will decide the next move of the ant
    /// </summary>
    private void DecideMove()
    {
        switch (STATUS)
        {
            case SEEKING_FOOD:
                SeekFood();
                break;
            case RETURNING_TO_BASE:
                ReturnToBase();
                break;
            case CAN_SEE_FOOD:
                MoveToFood();
                break;
            case RETURNING_TO_FOOD:
                ReturnToFood();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Function to call after the ant starts carrying food.
    /// </summary>
    private void ReturnToBase()
    {
        //WE HAVE TO PICK ONE OF THE WAYPOINTS LEFT FROM ANY ANT (WAY TO HOME) WITHIN OUR SENSOR RANGE
        //Then we start to move to the points stored in the move list of way to home
        //We can pick the shortest path within our sensor range
        //Upon reaching to the base, we reset our way to home list
        //Debug.Log(movesFromHome.Count);
        if (movesFromHome.Count>0 && Vector3.Distance(transform.position, targetWaypoint) < 1)
        {
            targetWaypoint = movesFromHome[movesFromHome.Count - 1];
            targetWaypoint.x += Random.Range(-5f, 5f);
            targetWaypoint.y += Random.Range(-5f, 5f);
            movesFromFood.Add(targetWaypoint);
            movesFromHome.RemoveAt(movesFromHome.Count - 1);
            FaceVector3(targetWaypoint);
        }
        else if (movesFromHome.Count == 0)
        {
            STATUS = RETURNING_TO_FOOD;
        }
        MoveForward();


    }

    /// <summary>
    /// Function to call after the ant deposits the food it has been carrying to the colony base.
    /// This function will help the ant to return to a food deposit
    /// </summary>
    private void ReturnToFood()
    {
        //After returning to base, we have to search for "WAY TO FOOD" waypoint within our sensor range
        //Then we start to move to the points stored in the move list of way to food
        //If there is no food at the destination, we just reset WAY TO FOOD list
        //Then set the STATUS to SEEKING_FOOD
    }

    /// <summary>
    /// Function to make the ant to move towards the visible food.
    /// Essentially, ant should be facing towards to the food, which is guaranteed in SeekFood function
    /// </summary>
    private void MoveToFood()
    {
        //We are facing the food, therefore we can just move to it until we touch it
        //Then we have to take a piece of it and set the STATUS to RETURN_TO_BASE
        MoveForward();
        float dist = Vector3.Distance(AntObject.transform.position, FoodSource.transform.position);
        if (dist <= FoodSource.transform.localScale.x/2)
        {
            if (FoodSource.GetComponent<FoodPile>().FoodCount >= CarryingCapacity)
            {
                currentFood = CarryingCapacity;
            }
            else
            {
                currentFood = FoodSource.GetComponent<FoodPile>().FoodCount;
            }
            FoodSource.GetComponent<FoodPile>().FoodCount -= currentFood;
            STATUS = RETURNING_TO_BASE;
            Collider2D[] AntsWithinRange = Physics2D.OverlapCircleAll(transform.position, SensorRange, LayerMask.GetMask("Ant"));
            foreach (Collider2D _Ant in AntsWithinRange)
            {
                if (_Ant.gameObject.GetComponent<Ant>().movesFromHome.Count < movesFromHome.Count && _Ant.gameObject.GetComponent<Ant>().movesFromHome.Count > 0)
                {
                    movesFromHome = _Ant.gameObject.GetComponent<Ant>().movesFromHome;
                }

            }
            movesFromFood = new List<Vector3>();
            targetWaypoint = transform.position;
        }
        
    }
    /// <summary>
    /// Makes the current object face the given Vector3 (Point in 3d space)
    /// </summary>
    /// <param name="target">Target point in 3D space</param>
    private void FaceVector3(Vector3 target)
    {
        Vector3 diff = target - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }
    ///<summary> Function to make the ant randomly move or face an visible food.</summary>
    private void SeekFood()
    {

        Transform targetFood = FindVisibleFood();
        if (targetFood != transform)
        {
            STATUS = CAN_SEE_FOOD;
            FaceVector3(targetFood.position);
        }
        else
        {
            angles = AntObject.transform.eulerAngles;
            angle = Random.Range(-5f, 5f);
            angles.z += angle;
            if (!IsInMap())
            {
                angles.z += Random.Range(1f, 2f) * 90;
                AntObject.transform.eulerAngles = angles;
                MoveForward();

            }
            AntObject.transform.eulerAngles = angles;
        }
        MoveForward();
    }

    private void MoveForward()
    {
        transform.position += transform.up * Time.deltaTime * Velocity;
    }
    /// <summary>
    /// Function to be called every 0.1 seconds to save what the ant has been doing.
    /// This is the only way I could store 
    /// </summary>
    private void SaveMoves()
    {
        moves.Add(this.transform.position);
        if (STATUS == RETURNING_TO_BASE)
            movesFromFood.Add(this.transform.position);
        else
            movesFromHome.Add(this.transform.position);
        if (movesFromFood.Count > HormonePermanency / 0.1)
            movesFromFood.RemoveAt(0);
        if (movesFromHome.Count > HormonePermanency / 0.1)
            movesFromHome.RemoveAt(0);
    }
    #endregion

    #region OTHER CALCULATIONS

    /// <summary>
    /// Checks if the ant is in the map
    /// </summary>
    /// <returns>True if ant is in the map, else, returns false</returns>
    private bool IsInMap()
    {
        return Mathf.Abs(AntObject.transform.position.x) < borders.transform.localScale.x && Mathf.Abs(AntObject.transform.position.y) < borders.transform.localScale.y;
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees -= transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0 );
    }

    /// <summary>
    /// Searches for Visible Food withing the Field of View of the ant
    /// </summary>
    /// <returns>Returns the position of the food, if one is visible, if not, returns position of the ant.</returns>
    private Transform FindVisibleFood()
    {
        Collider2D[] foodCol = Physics2D.OverlapCircleAll(transform.position, RangeOfVision, LayerMask.GetMask("FoodLayer"));

        Vector3 foodPlace;
        Vector3 distVector;
        float degreeDiff;
        foreach (Collider2D _food in foodCol)
        {
            foodPlace = _food.transform.position;
            distVector = (foodPlace - this.transform.position).normalized;
            degreeDiff = Vector3.Angle(distVector, transform.up);
            FoodSource = _food.gameObject;
            if (Mathf.Abs(degreeDiff) < AngleOfVision / 2)
            {
                return _food.transform;
            }
        }
        return transform;
    }
    #endregion
}

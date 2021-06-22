using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Ant : MonoBehaviour
{
    #region GENES OF THE ANT

    public string AntName;
    [Range(6f, 200f)]
    public float HormonePermanency = 100f;

    [Range(1f, 200f)]
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
    public GameObject WayToFood;
    private GameObject FoodSource;
    private GameObject ColonyCenter;
    #endregion
    #region VALUES
    private float currentFood = 0;
    private float angle;
    private Vector3 angles;
    private Vector3 lastPosition;
    #endregion
    #region LAYERS
    LayerMask LayerWayToHome;
    LayerMask LayerAnts;
    LayerMask LayerWayToFood;
    LayerMask LayerFood;
    #endregion
    #region MOVES
    public List<Vector3> movesFromHome;
    public List<Vector3> movesFromFood;
    public Vector3 targetWaypoint;
    public List<Vector3> moves;
    #endregion
    #endregion
    #region STATUS VARIABLES
    private const int WANDERING = 0;
    private const int CARRYING_FOOD = 1;
    private const int FOLLOWING_FOOD_SCENT = 2;
    private const int CAN_SEE_FOOD = 3;
    public int STATUS;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        LayerWayToHome = LayerMask.GetMask("WayToHome");
        LayerAnts = LayerMask.GetMask("Ant");
        LayerWayToFood = LayerMask.GetMask("WayToFood");
        LayerFood = LayerMask.GetMask("FoodLayer");
        ColonyCenter = GameObject.Find("Colony");
        STATUS = WANDERING;
        moves = new List<Vector3>();
        movesFromFood = new List<Vector3>();
        movesFromHome = new List<Vector3>();
        moves.Add(transform.position);
        AntObject = this.gameObject;
        angles = AntObject.transform.eulerAngles;
        angle = Random.Range(-180f, 180f);
        angles.z = angle;
        AntObject.transform.eulerAngles = angles;
        lastPosition = transform.position;
        SaveMoves();
    }

    // Update is called once per frame
    void Update()
    {
        DecideMove();
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, lastPosition)>SensorRange-1)
        {
            SaveMoves();
            lastPosition = transform.position;
        }
    }

    #region MOVEMENT FUNCTIONS
    /// <summary>
    /// Function that will decide the next move of the ant
    /// </summary>
    public void DecideMove()
    {
        switch (STATUS)
        {
            case WANDERING:
                Wander();
                break;
            case CARRYING_FOOD:
                CarryFood();
                break;
            case CAN_SEE_FOOD:
                MoveToFood();
                break;
            case FOLLOWING_FOOD_SCENT:
                FollowScent();
                break;
            default:
                break;
        }
    }
    #region MOVING STATES
    /// ///<summary> Function to make the ant randomly move or face an visible food.</summary>
    private void Wander()
    {
        Transform targetFood = FindVisibleFood();
        if (targetFood != transform)
        {
            STATUS = CAN_SEE_FOOD;
            FaceVector3(targetFood.position);
        }
        else if (FindWaypoint())
        {
            STATUS = FOLLOWING_FOOD_SCENT;
            targetWaypoint = movesFromFood[movesFromFood.Count - 1];
            FaceVector3(targetWaypoint);
        }
        else
        {
            angles = AntObject.transform.eulerAngles;
            angle = Random.Range(-5f, 5f);
            angles.z += angle;
            if (!IsInMap())
            {
                angles.z += 180;
                AntObject.transform.eulerAngles = angles;
                MoveForward();

            }
            AntObject.transform.eulerAngles = angles;
        }
        MoveForward();
    }
    /// <summary>
    /// Function to call after the ant starts carrying food.
    /// </summary>
    private void CarryFood()
    {
        //WE HAVE TO PICK ONE OF THE WAYPOINTS LEFT FROM ANY ANT (WAY TO HOME) WITHIN OUR SENSOR RANGE
        //Then we start to move to the points stored in the move list of way to home
        //We can pick the shortest path within our sensor range
        //Upon reaching to the base, we reset our way to home list
        MoveForward();
        if (movesFromHome.Count > 0 && Vector3.Distance(transform.position, targetWaypoint) < 1)
        {
            targetWaypoint = movesFromHome[movesFromHome.Count - 1];
            FaceVector3(targetWaypoint);
            movesFromHome.RemoveAt(movesFromHome.Count - 1);
        }
        if (Vector3.Distance(ColonyCenter.transform.position, transform.position) < ColonyCenter.transform.localScale.x)
        {
            ColonyCenter.GetComponent<ColonyCenter>().FoodStored += currentFood;
            currentFood = 0;
            STATUS = FOLLOWING_FOOD_SCENT;
            if (!FindWaypoint()) {
                movesFromFood = new List<Vector3>();
                STATUS = WANDERING;
            }
            movesFromHome = new List<Vector3>();
        }
        if(Vector3.Distance(transform.position, targetWaypoint) > SensorRange + 5)
        {
            movesFromFood = new List<Vector3>();
            STATUS = WANDERING;
        }
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
        if(FoodSource == null)
        {
            STATUS = WANDERING;
            ClearFoodScents();
            return;
        }
        float dist = Vector3.Distance(AntObject.transform.position, FoodSource.transform.position);
        if (dist <= FoodSource.transform.localScale.x / 2)
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
            GenerateWaypoint();
            STATUS = CARRYING_FOOD;
            FindWaypoint();
            targetWaypoint = transform.position;
            movesFromFood = new List<Vector3>();
        }
    }    
    /// <summary>
    /// Function to call after the ant deposits the food it has been carrying to the colony base.
    /// This function will help the ant to return to a food deposit
    /// </summary>
    private void FollowScent()
    {
        //After returning to base, we have to search for "WAY TO FOOD" waypoint within our sensor range
        //Then we start to move to the points stored in the move list of way to food
        //If there is no food at the destination, we just reset WAY TO FOOD list
        //Then set the STATUS to SEEKING_FOOD
        MoveForward();
        Transform food = FindVisibleFood();
        if (movesFromFood.Count > 0 && Vector3.Distance(transform.position, targetWaypoint) < 1)
        {
            targetWaypoint = movesFromFood[movesFromFood.Count - 1];
            FaceVector3(targetWaypoint);
            movesFromFood.RemoveAt(movesFromFood.Count - 1);
        }
        else if((food == transform && movesFromFood.Count ==0))
        {
            ClearFoodScents();
            STATUS = WANDERING;
        }
        if (food != transform)
        {
            STATUS = CAN_SEE_FOOD;
            FaceVector3(food.position);
        }
    }
    #endregion
    /// <summary>
    /// Makes the object to move forward based on its angle.
    /// </summary>
    private void MoveForward()
    {
        transform.position += transform.up *  Velocity * Time.deltaTime*5;
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
    /// <summary>
    /// ///////////
    /// BURAYI DÜZENLEYECEKSİN SONRA
    /// ///////////
    /// </summary>
    private void GenerateWaypoint()
    {
        Collider2D[] BaseWaypoints = Physics2D.OverlapCircleAll(transform.position, SensorRange, STATUS == CARRYING_FOOD ? LayerWayToFood : LayerWayToHome);
        if (BaseWaypoints.Length == 0)
        {
            GameObject waypoint = GameObject.Instantiate(STATUS == CARRYING_FOOD ? WayToFood : WayToHome);
            waypoint.transform.position = transform.position;
            waypoint.GetComponent<Waypoint>().PathToTake = STATUS == CARRYING_FOOD ? movesFromFood.ToList() : movesFromHome.ToList();
            waypoint.GetComponent<Waypoint>().DestroyTimer = HormonePermanency;
            BaseWaypoints = new Collider2D[1];
            BaseWaypoints[0] = waypoint.GetComponent<Collider2D>();
        }
    }
    private bool ClearFoodScents()
    {
        Collider2D[] BaseWaypoints = Physics2D.OverlapCircleAll(transform.position, SensorRange,LayerWayToFood);
        bool res = BaseWaypoints.Length > 0;
        foreach (var wp in BaseWaypoints)
        {
            Destroy(wp.gameObject);
        }
        return res;
    }
    private bool FindWaypoint()
    {
        Collider2D[] BaseWaypoints = Physics2D.OverlapCircleAll(transform.position, SensorRange, STATUS == CARRYING_FOOD ? LayerWayToHome : LayerWayToFood);
        float Closest = float.MaxValue;
        Collider2D closestCollider = null;
        foreach (Collider2D collider2D in BaseWaypoints)
        {
            if (Vector3.Distance(collider2D.transform.position, transform.position) < Closest)
            {
                Closest = Vector3.Distance(collider2D.transform.position, transform.position);
                movesFromHome = STATUS == CARRYING_FOOD ? collider2D.gameObject.GetComponent<Waypoint>().PathToTake.ToList(): movesFromHome;
                movesFromFood = STATUS == CARRYING_FOOD ? movesFromFood: collider2D.gameObject.GetComponent<Waypoint>().PathToTake.ToList();
                closestCollider = collider2D;
            }
        }
        if (closestCollider != null)
            closestCollider.gameObject.GetComponent<Waypoint>().DestroyTimer = HormonePermanency;
        return BaseWaypoints.Length > 0;
    }
    /// <summary>
    /// Function to be called every 0.1 seconds to save what the ant has been doing.
    /// This is the only way I could store 
    /// </summary>
    public void SaveMoves(bool surprassDistance = false)
    {
        if(STATUS == CARRYING_FOOD)
        {
            //IF Returning to base, we are following movesFromHome and adding to movesFromFood
            if (movesFromFood.Count == 0)
            {
                movesFromFood.Add(transform.position);
                GenerateWaypoint();
            }
            else if(Vector3.Distance(transform.position,movesFromFood[movesFromFood.Count-1])>SensorRange-1f)
            {
                movesFromFood.Add(transform.position);
                GenerateWaypoint();
            }
        }
        else if(STATUS == FOLLOWING_FOOD_SCENT)
        {
            //If returning to food, we are following movesFromFood, and adding to movesFromHome
            if (movesFromHome.Count == 0)
            {
                movesFromHome.Add(transform.position);
            }
            else if (Vector3.Distance(transform.position, movesFromHome[movesFromHome.Count - 1]) > SensorRange - 1f)
            {
                movesFromHome.Add(transform.position);
            }
        }
        else if(STATUS == CAN_SEE_FOOD)
        {
            //If can see food, we are following the food collider, and adding to movesFromHome
            if (movesFromHome.Count == 0)
            {
                movesFromHome.Add(transform.position);
            }
            else if (Vector3.Distance(transform.position, movesFromHome[movesFromHome.Count - 1]) > SensorRange - 1f)
            {
                movesFromHome.Add(transform.position);
            }
        }
        else
        {
            //If seeking food, we are adding to movesFromHome and randomly wandering
            if (movesFromHome.Count == 0)
            {
                movesFromHome.Add(transform.position);
            }
            else if (Vector3.Distance(transform.position, movesFromHome[movesFromHome.Count - 1]) > SensorRange - 1f)
            {
                movesFromHome.Add(transform.position);
            }
        }
        moves.Add(transform.position);
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
        Collider2D[] foodCol = Physics2D.OverlapCircleAll(transform.position, RangeOfVision, LayerFood);

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
    private bool CanSeeColony()
    {
        Vector3 distVec = (ColonyCenter.transform.position - transform.position).normalized;
        float degreeDiff = Vector3.Angle(distVec, transform.up);
        return Mathf.Abs(degreeDiff) < AngleOfVision / 2 && distVec.magnitude < (RangeOfVision+ColonyCenter.transform.localScale.x/2);
    }
    #endregion
}

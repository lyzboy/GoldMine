using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to create a character data object.
/// </summary>
public class Character
{

    /// <summary>
    /// An enum value of a job the characer can have
    /// </summary>
    public enum Jobs
    {
        Mechanic,
        Excavator,
        Loader,
        Hauler,
        Universal
    }

    /// <summary>
    /// The character's first name
    /// </summary>
    string firstName;
    /// <summary>
    /// The characters last name
    /// </summary>
    string lastName;
    /// <summary>
    /// The walking speed of the character
    /// </summary>
    float speed = 5;

    /// <summary>
    /// Used to reduce the speed for idling tasks
    /// </summary>
    public bool IsIdle;

    /// <summary>
    /// The characters current job
    /// </summary>
    Jobs job;
    /// <summary>
    /// The value of the characters current job
    /// </summary>
    public Jobs Job { get { return job; } set { job = value; } }


    /// <summary>
    /// The skill of the character when operating vehicles
    /// </summary>
    float operatingSkill = 1;
    /// <summary>
    /// The skill of the character when it comes to repairing vehicles
    /// </summary>
    float maintenance = 1;

    /// <summary>
    /// The x position of the character
    /// </summary>
    float x;
    /// <summary>
    /// The y position of the character
    /// </summary>
    float y;

    Vehicle currentVehicle = null;
    bool isInVehicle = false;

    /// <summary>
    /// Used to tell if character is currently in the vehicle
    /// </summary>
    public bool IsInVehicle { get { return isInVehicle;  } set { isInVehicle = value; } }

    /// <summary>
    /// The current vehicle of the character
    /// </summary>
    public Vehicle CurrentVehicle { get { return currentVehicle; } 
        set {
            if (value == null)
            {
                currentVehicle = null;
                isInVehicle = false;
            }
            else
            {
                currentVehicle = value;
            }
        } 
    }

    /// <summary>
    /// shows with the character should be visually hidden on screen
    /// </summary>
    bool hidden = false;

    /// <summary>
    /// Returns if the character should be hidden on screen
    /// </summary>
    public bool IsHidden { 
        get {
            return hidden;
        } 
        set { 
            hidden = value;
            cbCharacterChanged(this);

        }
    }

    /// <summary>
    /// Reference to the map this character is in
    /// </summary>
    Map map;

    public Map GetMap { get { return map; } }

    /// <summary>
    /// The AStar agent for this character
    /// </summary>
    AStarAgent agent;

    /// <summary>
    /// The task system work AI for this character
    /// </summary>
    WorkerAI workAI;
    public WorkerAI GetWorkerAI { get { return workAI; } }


    /// <summary>
    /// The action the character should take when it arrives at it's destination
    /// </summary>
    Action actionOnArrival;
    /// <summary>
    /// The value to reduce the characters movement speed by when transitioning between depths
    /// </summary>
    float divisor = 1;

    /// <summary>
    /// 
    /// </summary>
    public bool IsSelected { get; protected set; }


    /// <summary>
    /// The current head number of the character to show visually
    /// </summary>
    int headNum = 0;
    public int HeadNum {
        get {
            return headNum;
        }
        set {
            headNum = value;
        }
    }

    /// <summary>
    /// The current body number of the character to show visually
    /// </summary>
    int bodyNum = 0;
    public int BodyNum {
        get {
            return bodyNum;
        }
        set {
            bodyNum = value;
        }
    }


    /// <summary>
    /// Action delegate to call when the character has been changed, typically signals visual changes
    /// </summary>
    Action<Character> cbCharacterChanged;


    /// <summary>
    /// Create a character
    /// </summary>
    /// <param name="x">X position to spawn the character</param>
    /// <param name="y">Y position to spawn the character</param>
    /// <param name="map">The map the character will be in</param>
    /// <param name="ts">A global task system to apply to the character</param>
    /// <param name="firstName">The first name of the character, If not declared a random one will be assigned</param>
    /// <param name="lastName">The last name of the character, If not declared a random one will be assigned</param>
    public Character(float x, float y, Map map, TaskSystem ts, string firstName = "Default", string lastName = "Default")
    {
        if(firstName == "Default")
        {
            GenerateFirstName();
        }

        if(lastName == "Default")
        {
            GenerateLastName();
        }
        this.map = map;

        //Nav = new AStar(map.gridMap);
        agent = new AStarAgent(map.gridMap);
        workAI = new WorkerAI(this, ts);

        speed = UnityEngine.Random.Range(2f, 4f);

        job = Jobs.Universal;

        this.x = x;
        this.y = y;
        SetDestination(x, y);
        //cbCharacterChanged(this);

    }

    /// <summary>
    /// Create a character
    /// </summary>
    /// <param name="map">The map the character will be in</param>
    /// <param name="ts">A global task system to apply to the character</param>
    /// <param name="firstName">The first name of the character, If not declared a random one will be assigned</param>
    /// <param name="lastName">The last name of the character, If not declared a random one will be assigned</param>
    public Character(float x, float y, Map map, TaskSystem ts, Jobs aJob, string firstName = "Default", string lastName = "Default")
    {
        if (firstName == "Default")
        {
            GenerateFirstName();
        }

        if (lastName == "Default")
        {
            GenerateLastName();
        }
        this.map = map;

        //Nav = new AStar(map.gridMap);
        agent = new AStarAgent(map.gridMap);
        workAI = new WorkerAI(this, ts);

        speed = UnityEngine.Random.Range(2f, 4f);

        job = aJob;


        this.x = x;
        this.y = y;
        SetDestination(x, y);
        //cbCharacterChanged(this);

    }

    /// <summary>
    /// Method the is executed every game tick
    /// </summary>
    /// <param name="delta"></param>
    public void Update(float delta)
    {
        MoveCharacter(delta);
        workAI.Update();
    }

    /// <summary>
    /// Called each tick to move the character to it's destination
    /// </summary>
    /// <param name="delta"></param>
    void MoveCharacter(float delta)
    {
        float calculatedSpeed = speed;
        if (IsIdle)
        {
            calculatedSpeed = speed / 2;
        }
        Vector2 pos = agent.MoveAgent(delta, this.x, this.y, calculatedSpeed, map);
        x = pos.x;
        y = pos.y;
        cbCharacterChanged(this);
        
    }

    public bool ArrivedAtTarget()
    {
        return agent.AtDestination;
    }


    /// <summary>
    /// Set the A* agents destination. If spawning or teleporting, needs to be set after initializing of x and y
    /// </summary>
    /// <param name="x">X of target position</param>
    /// <param name="y">Y of target position</param>
    /// <param name="onArrivedAtPosition">Action to complete once arrived at the position</param>
    public void SetDestination(float x, float y, Action onArrivedAtPosition = null)
    {
        agent.SetDestination(this.x, this.y, x, y, map, onArrivedAtPosition);
    }


    public void PlayCompletedAnimation(Action onCompleted = null)
    {
        onCompleted();
    }

    public void SetSelected(bool isSelected)
    {
        this.IsSelected = isSelected;
        cbCharacterChanged(this);
    }


    /// <summary>
    /// Teleports a character to the provided x,y coord
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void TeleportTo(int x, int y)
    {
        this.x = x;
        this.y = y;
        SetDestination(x, y);
    }


    /// <summary>
    /// Generates a first name for a character
    /// </summary>
    void GenerateFirstName()
    {
        int num = UnityEngine.Random.Range(0, GameManager.Instance.characterFirstNames.Length);
        firstName = GameManager.Instance.characterFirstNames[num];
    }


    /// <summary>
    /// Generates a last name for the character
    /// </summary>
    void GenerateLastName()
    {
        int num = UnityEngine.Random.Range(0, GameManager.Instance.characterLastNames.Length);
        lastName = GameManager.Instance.characterLastNames[num];
    }

    public Vector2 GetPos()
    {
        return new Vector2(x, y);
    }

    public string GetFirstName
    {
        get{ return firstName; }
    }

    public string GetLastName
    {
        get{ return lastName;}
    }

    public string GetName
    {
        get { return $"{firstName} {lastName}"; }
    }


    /// <summary>
    /// Register a callback to the character changed event
    /// </summary>
    /// <param name="callback"></param>
    public void RegisterCharacterChangedCallback(Action<Character> callback)
    {
        cbCharacterChanged += callback;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// The TaskStateMachine class is used to manage the state of a task and holds all references to the
/// the task associated with it.
/// </summary>
public class TaskStateMachine
{
    /// <summary>
    /// Reference to the current task
    /// </summary>
    private ITaskStrategy taskStrategy;

    public String taskStrategyName;

    public TaskStateMachine(ITaskStrategy taskStrategy)
    {
        this.taskStrategy = taskStrategy;
        taskStrategyName = this.taskStrategy.GetType().ToString();
    }
    /// <summary>
    /// A method that is called periodically, typically on each frame update, to check the state of the task.
    /// </summary>
    public void Execute()
    {
        taskStrategy.Execute();
    }

    public void Reset()
    {
        taskStrategy.Reset();
    }

    /// <summary>
    /// This method returns a boolean value indicating whether the task is currently being executed.
    /// </summary>
    /// <returns></returns>
    public bool IsCompleted()
    {
        return taskStrategy.IsCompleted();
    }
    /// <summary>
    /// This method returns a boolean value indicating whether the task is currently being executed.
    /// </summary>
    /// <returns></returns>
    public bool IsExecuting()
    {
        return taskStrategy.IsExecuting();
    }
    /// <summary>
    /// This method returns a boolean value indicating whether the task cannot be completed at the 
    /// current time due to certain circumstances. True would mean the task did not complete.
    /// </summary>
    /// <returns></returns>
    public bool IsBlocked()
    {
        return taskStrategy.IsBlocked();
    }

    public string GetErrorMessage()
    {
        return taskStrategy.ErrorMessage;
    }

    public override string ToString()
    {
        return $"{taskStrategyName}";
    }
}



/// <summary>
/// Interface for all tasks to have approriate methods and fields
/// </summary>
public interface ITaskStrategy
{
    /// <summary>
    /// This method is called to execute the task strategy. The specific actions taken by the task 
    /// strategy will depend on the type of task it is associated with.
    /// </summary>
    public void Execute();
    /// <summary>
    /// This method resets the state of the task strategy to its initial state. This method is typically 
    /// called when the task strategy is being reused or when it is being discarded and no longer needed.
    /// </summary>
    public void Reset();
    /// <summary>
    /// This method returns a boolean value indicating whether the task has been completed.
    /// </summary>
    /// <returns></returns>
    public bool IsCompleted();
    /// <summary>
    /// This method returns a boolean value indicating whether the task is currently being executed.
    /// </summary>
    /// <returns></returns>
    public bool IsExecuting();
    /// <summary>
    /// This method returns a boolean value indicating whether the task cannot be completed at the 
    /// current time due to certain circumstances. True would mean the task did not complete.
    /// </summary>
    /// <returns></returns>
    public bool IsBlocked();
    /// <summary>
    /// Used to track whether the Execute() method has been called or not.
    /// </summary>
    public bool HasExecutionBegun { get; set; }
    /// <summary>
    /// Error to display to the player when a task has a problem
    /// </summary>
    public string ErrorMessage { get; }
}

/// <summary>
/// This interface is used on any task that needs the closest character to complete it, where characters apply for positioning
/// </summary>
public interface IGlobalTask
{
    public Character Character { get; set; }
    public Vector2 Destination { get; }
    public bool IsEmergencyTask { get; set; }
}


/// <summary>
/// Task that allows a character to appear to be idle by walking to certain random points
/// </summary>
public class IdleTask : ITaskStrategy
{
    private Character character;

    Vector2 destination;

    bool isComplete;

    int count = 0;
    int numberOfMoves = 3;

    bool hasBegun = false;
    public bool HasExecutionBegun { get => hasBegun; set => hasBegun = value; }
    private string errorMessage;
    public string ErrorMessage { get => errorMessage; }

    /// <summary>
    /// Creates an idle task
    /// </summary>
    /// <param name="character">That character that should be completing the idle task</param>
    /// <param name="numOfWaypoints">The number of waypoints the character should seek out</param>
    public IdleTask(Character character, int numOfWaypoints = 3)
    {
        this.character = character;
        numberOfMoves = numOfWaypoints;
        errorMessage = "The idle task can not be completed";
    }

    public void Execute()
    {
        if (!hasBegun)
        {
            //TODO setup as a list of move to random tasks
            character.IsIdle = true;
            hasBegun = true;
            Debug.Log("Setting idle");
            destination = GenerateRandomDestination();
            character.SetDestination(destination.x, destination.y);
        }
        if (character.ArrivedAtTarget())
        {
            count++;
            if (count >= numberOfMoves)
            {
                Debug.Log("idle complete");
                character.IsIdle = false;
                isComplete = true;
            }
            else
            {
                destination = GenerateRandomDestination();
                character.SetDestination(destination.x, destination.y);
            }
        }
    }




    public bool IsBlocked()
    {
        return false;
    }

    public bool IsCompleted()
    {
        return isComplete;
    }

    public bool IsExecuting()
    {
        return !isComplete;
    }

    public void Reset()
    {
        isComplete = false;
        hasBegun = false;

        destination = GenerateRandomDestination();
    }

    /// <summary>
    /// Used to create a random destination
    /// </summary>
    /// <param name="maxRange">The max tiles away from the starting tile</param>
    /// <returns></returns>
    Vector2 GenerateRandomDestination(int maxRange = 5)
    {

        // create a position to go to
        Vector2 pos = Vector2.zero;
        Vector2 charPos = character.GetPos();

        // generate a random seed
        long seed = DateTime.Now.Ticks;
        // set the RNG to the seed
        UnityEngine.Random.InitState((int)seed);
        Vector2 right = new Vector2(
            UnityEngine.Random.Range((int)charPos.x - 1, (int)charPos.x + maxRange),
            charPos.y);
        Vector2 left = new Vector2(
            UnityEngine.Random.Range((int)charPos.x - 1, (int)charPos.x - maxRange),
            charPos.y);
        Vector2 up = new Vector2(
            charPos.x,
            UnityEngine.Random.Range((int)charPos.y + 1, (int)charPos.y + maxRange));
        Vector2 down = new Vector2(
            charPos.x,
            UnityEngine.Random.Range((int)charPos.y - 1, (int)charPos.y - maxRange));
        Vector2 upRight = new Vector2(
            UnityEngine.Random.Range((int)charPos.x + 1, (int)charPos.x + maxRange),
            UnityEngine.Random.Range((int)charPos.y + 1, (int)charPos.y + maxRange));
        Vector2 downRight = new Vector2(
            UnityEngine.Random.Range((int)charPos.x + 1, (int)charPos.x + maxRange),
            UnityEngine.Random.Range((int)charPos.y - 1, (int)charPos.y - maxRange));
        Vector2 upLeft = new Vector2(
            UnityEngine.Random.Range((int)charPos.x - 1, (int)charPos.x - maxRange),
            UnityEngine.Random.Range((int)charPos.y + 1, (int)charPos.y + maxRange));
        Vector2 downLeft = new Vector2(
            UnityEngine.Random.Range((int)charPos.x - 1, (int)charPos.x - maxRange),
            UnityEngine.Random.Range((int)charPos.y - 1, (int)charPos.y - maxRange));


        // generate a random seed
        seed = DateTime.Now.Ticks;
        // set the RNG to the seed
        UnityEngine.Random.InitState((int)seed);
        int chosenNum = UnityEngine.Random.Range(1, 9);

        switch (chosenNum)
        {
            case 1:
                pos = right;
                break;
            case 2:
                pos = left;
                break;
            case 3:
                pos = up;
                break;
            case 4:
                pos = down;
                break;
            case 5:
                pos = upRight;
                break;
            case 6:
                pos = upLeft;
                break;
            case 7:
                pos = downRight;
                break;
            case 8:
                pos = downLeft;
                break;
            default:
                pos = charPos;
                break;
        }
; return pos;
    }
}

/// <summary>
/// A task that, using the a* agent, has the object move to a provided location
/// </summary>
public class MoveToPositionTask : ITaskStrategy
{
    /// <summary>
    /// The target for the A* agent to move to
    /// </summary>
    private Vector2 targetPosition;

    /// <summary>
    /// the character this task is assigned to
    /// </summary>
    public Character character;

    private bool atTarget = false;

    private bool destinationSet = false;

    public bool HasExecutionBegun { set => destinationSet = value; get => destinationSet; }
    string errorMessage;
    public string ErrorMessage => errorMessage;

    public MoveToPositionTask(Vector2 targetPosition, Character character)
    {
        this.targetPosition = targetPosition;
        this.character = character;
        errorMessage = $"Unable to move to {character.GetName} position.";
    }

    public void Reset()
    {
        atTarget = false;
        HasExecutionBegun = false;
    }

    public void Execute()
    {
        if (!HasExecutionBegun)
        {
            HasExecutionBegun = true;
            character.SetDestination((int)targetPosition.x, (int)targetPosition.y);
        }
        if (character.ArrivedAtTarget())
        {
            atTarget = true;
        }
    }

    public bool IsCompleted()
    {
        return atTarget;
    }

    public bool IsExecuting()
    {
        return !atTarget;
    }

    public bool IsBlocked()
    {
        return false;
    }
}


/// <summary>
/// A task that has a character toggle the power to a piece of equipment
/// </summary>
public class TogglePowerTask : ITaskStrategy, IGlobalTask
{
    /// <summary>
    /// The washplant to toggle power of 
    /// </summary>
    private Equipment equipment;
    /// <summary>
    /// The character to have move to the washplant
    /// </summary>
    private Character character;
    public Character Character { get => character; set => character = value; }

    private bool destinationSet = false;

    public bool HasExecutionBegun { get => destinationSet; set => destinationSet = value; }

    private bool isBlocked = false;

    /// <summary>
    /// flag for checking if task is complete
    /// </summary>
    private bool complete = false;

    private Vector2 destination;
    public Vector2 Destination { get => destination; }

    private bool isEmergency = false;
    public bool IsEmergencyTask { get => isEmergency; set => isEmergency = value; }
    string errorMessage = "Unable to complete toggle power task";
    public string ErrorMessage => throw new NotImplementedException();

    MoveToPositionTask moveTask;

    public TogglePowerTask(Equipment equipment, bool isEmergency)
    {
        this.equipment = equipment;
        destination = equipment.GetPos;
        moveTask = new MoveToPositionTask(equipment.GetPos, character);

    }
    public void SetCharacter(Character ch)
    {
        character = ch;
    }

    public void Execute()
    {
        if (character != null)
        {
            moveTask.Execute();

            if (moveTask.IsCompleted())
            {
                IPowerOn eq = equipment;
                eq.PowerOn();
                complete = true;
            }
            if (moveTask.IsBlocked())
            {
                errorMessage = $"{character.GetName} is unable to complete to reach the equipment {equipment.GetName}";
                isBlocked = true;
            }
        }
    }

    public bool IsCompleted()
    {
        return complete;
    }

    public bool IsExecuting()
    {
        return !complete;
    }

    public void Reset()
    {
        moveTask = new MoveToPositionTask(equipment.GetPos, character);
        complete = false;

    }

    public bool IsBlocked()
    {
        return isBlocked;
    }
}

/// <summary>
/// Task to move a character to a destination inside a vehicle
/// </summary>
public class MoveInVehicleTask : ITaskStrategy
{
    private bool atTarget = false;

    private bool destinationSet = false;
    public bool HasExecutionBegun { get => destinationSet; set => destinationSet = value; }
    string errorMessage = "Unable to move in vehicle";
    public string ErrorMessage => errorMessage;

    Vehicle vehicle;
    Vector2 target;
    Character character;

    bool isBlocked = false;

    public MoveInVehicleTask(Character character, Vector2 target)
    {
        this.vehicle = character.CurrentVehicle;
        this.target = target;
        this.character = character;
    }

    public void Execute()
    {
        if (vehicle.Destination != target)
        {
            if (vehicle != null)
            {
                vehicle.SetDestination(target.x, target.y);
            }
            else
            {
                isBlocked = true;
            }
        }
    }

    public bool IsCompleted()
    {
        return vehicle.ArrivedAtTarget();
    }

    public bool IsExecuting()
    {
        return !vehicle.ArrivedAtTarget();
    }

    public void Reset()
    {
        atTarget = false;
        HasExecutionBegun = false;
        isBlocked = false;
    }

    public bool IsBlocked()
    {
        return isBlocked;
    }
}

/// <summary>
/// Task to get character out of vehicle
/// </summary>
public class GetOutOfVehicleTask : ITaskStrategy
{
    Character character;
    bool complete = false;
    bool blocked = false;
    public bool HasExecutionBegun { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    string errorMessage = "Unable to get out of vehicle";
    public string ErrorMessage => errorMessage;

    public GetOutOfVehicleTask(Character character)
    {
        this.character = character;
    }

    public void Execute()
    {
        if (character.CurrentVehicle != null)
        {
            character.CurrentVehicle.GetOutOfVehicle(character);
        }
        else if (character.IsInVehicle)
        {
            character.CurrentVehicle.GetOutOfVehicle(character);
        }
        complete = true;
    }

    public bool IsBlocked()
    {
        return blocked;
    }

    public bool IsCompleted()
    {
        return complete;
    }

    public bool IsExecuting()
    {
        return !complete;
    }

    public void Reset()
    {
        complete = false;
        blocked = false;
    }
}

/// <summary>
/// Task to move a character towards a vehicle and then get in said vehicle
/// </summary>
public class GetInVehicleTask : ITaskStrategy
{
    Character character;
    Vehicle vehicle;

    bool executionBegun;
    public bool HasExecutionBegun { get => executionBegun; set => executionBegun = value; }

    string errorMessage = "Unable to get into vehicle";
    public string ErrorMessage => errorMessage;

    bool isBlocked = false;
    bool isComplete = false;


    public GetInVehicleTask(Character character, Vehicle vehicle)
    {
        this.character = character;
        this.vehicle = vehicle;
        this.vehicle.ReserveVehicle(this.character);
    }

    public void Execute()
    {
        if (character == null)
        {
            isBlocked = true;
            errorMessage = $"There is no character assigned to the get in vehicle task.";
            return;
        }
        if (vehicle == null)
        {
            isBlocked = true;
            errorMessage = $"{character.GetName} doesn't have a vehicle to get into";
            return;
        }
        if (character.CurrentVehicle == vehicle && character.IsInVehicle)
        {
            isComplete = true;
        }
        else
        {

            if (!HasExecutionBegun)
            {
                if (vehicle.isReserved())
                {
                    if(vehicle.Character != character)
                    {
                        vehicle.ReserveVehicle(character);
                    }
                }
                HasExecutionBegun = true;
                if (character.CurrentVehicle != vehicle) character.CurrentVehicle = vehicle;
                character.SetDestination(vehicle.x, vehicle.y);
                Debug.Log("Vehicle set as destination");
            }
            if (character.ArrivedAtTarget())
            {
                Debug.Log("Character at vehicle");
                vehicle.GetInVehicle(character);
                isComplete = true;
            }
        }

    }

    public bool IsBlocked()
    {
        return isBlocked;
    }

    public bool IsCompleted()
    {
        return isComplete;
    }

    public bool IsExecuting()
    {
        return !isComplete;
    }

    public void Reset()
    {
        if (character.IsInVehicle) character.CurrentVehicle.GetOutOfVehicle(character);
        isComplete = false;
        isBlocked = false;
        HasExecutionBegun = false;
    }
}

/// <summary>
/// Task that will drive player in a vehicel to target position/tile
/// </summary>
public class DriveExcavatorToDigTileTask : ITaskStrategy
{
    Character character;
    Excavator ex;
    bool hasBegun = false;
    public bool HasExecutionBegun { get => hasBegun; set => hasBegun = value; }

    bool isComplete = false;
    bool isBlocked = false;

    string errorMessage = "Unable to drive vehicle to tile.";
    public string ErrorMessage => errorMessage;

    public DriveExcavatorToDigTileTask(Character character, Excavator ex)
    {
        this.character = character;
        this.ex = ex;
    }

    public void Execute()
    {
        if (!HasExecutionBegun)
        {
            // if there are dig areas
            if (character.GetMap.digAreas.Count > 0)
            {
                Tile tile;

                tile = character.GetMap.FindDiggableTile(ex);
                ex.TargetTile = tile;


                // Find a position that is in range of the tile but not on top of it.

                // Calculate the direction from the start tile to the goal tile
                Vector2 direction = ex.GetPos - tile.GetPos;

                // Normalize the direction vector
                direction = direction.normalized;

                if (direction.x < 0)
                {
                    direction.x = -1;
                }
                else if (direction.x > 0)
                {
                    direction.x = 1;
                }

                if (direction.y < 0)
                {
                    direction.y = -1;
                }
                else if (direction.y > 0)
                {
                    direction.y = 1;
                }

                // Multiply the normalized direction vector by the desired distance
                Vector2 offset = direction * (ex.BoomRange / 2);

                // Add the offset to the goal tile's position to get the final position
                Vector2 finalPosition = tile.GetPos + offset;
                finalPosition.x = (int)finalPosition.x;
                finalPosition.y = (int)finalPosition.y;



                if (ex.TargetTile != null)
                {
                    if (ex.TargetTileInRange() && ex.Destination == finalPosition)
                    {
                        isComplete = true;
                    }
                    else
                    {
                        ex.SetDestination(finalPosition.x, finalPosition.y);
                    }
                } else
                {
                    isBlocked = true;
                    errorMessage = $"There is not target tile set for {character.GetName} driving {ex.GetName}";
                }

            }
            else
            {
                //character.CurrentVehicle.GetOutOfVehicle(character);
                errorMessage = $"There are currently no dig areas created. Please create a dig area to continue dig operations.";
                isBlocked = true;
            }
        }

        if (ex.ArrivedAtTarget())
        {
            isComplete = true;
        }
    }

    public bool IsBlocked()
    {
        return isBlocked;
    }

    public bool IsCompleted()
    {
        return isComplete;
    }

    public bool IsExecuting()
    {
        return !isComplete;
    }

    public void Reset()
    {
        isBlocked = false;
        isComplete = false;
        HasExecutionBegun = false;
    }
}

/// <summary>
/// Task to rotate the boom of an excavator
/// </summary>
public class RotateExcavatorBoomTask : ITaskStrategy
{

    Excavator ex;
    bool hasBegun = false;
    public bool HasExecutionBegun { get => hasBegun; set => hasBegun = value; }
    string errorMessage = "Unable to rotate excavator cab.";
    bool isBlocked = false;
    public string ErrorMessage => errorMessage;

    public RotateExcavatorBoomTask(Excavator ex)
    {
        this.ex = ex;
    }

    public void Execute()
    {
        if (!HasExecutionBegun)
        {
            ex.SetBoomRotationToTargetTile();
        }

        if(ex.TargetTile == null)
        {
            errorMessage = $"{ex.Character} driving {ex.GetName} has no target tile";
            isBlocked = true;
        }
    }

    public bool IsBlocked()
    {
        return isBlocked == true;
    }

    public bool IsCompleted()
    {
        return ex.CabRotationComplete;
    }

    public bool IsExecuting()
    {
        return !ex.CabRotationComplete;
    }

    public void Reset()
    {
        HasExecutionBegun = false;
    }
}

/// <summary>
/// Task to dig with an excavator
/// </summary>
public class DigWithExcavtorTask : ITaskStrategy
{
    Excavator ex;
    Character character;
    bool hasBegun = false;
    public bool HasExecutionBegun { get => hasBegun; set => hasBegun = value; }

    string errorMessage = "Unable to dig with excavator";
    public string ErrorMessage => errorMessage;

    bool isComplete = false;
    bool isBlocked = false;

    public DigWithExcavtorTask(Character character, Excavator ex)
    {
        this.character = character;
        this.ex = ex;
    }
    public void Execute()
    {
        if (character.IsInVehicle)
        {
            if (ex.IsDirtInBucket)
            {
                isComplete = true;
            }
            else
            {
                if (ex.DigTile(ex.TargetTile))
                {

                isComplete = true;
                } else
                {
                    errorMessage = $"{character.GetName} operating {ex.GetName} unable to dig. Probably already has dirt";
                    isBlocked = false;
                }
            }
        }
    }

    public bool IsBlocked()
    {
        return isBlocked;
    }

    public bool IsCompleted()
    {
        return isComplete;
    }

    public bool IsExecuting()
    {
        return !isComplete;
    }

    public void Reset()
    {
        isComplete = false;
        isBlocked = false;
        HasExecutionBegun = false;
    }
}

public class UnloadExcavatorBucketTask : ITaskStrategy
{
    Character character;
    Excavator ex;
    public bool HasExecutionBegun { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    string errorMessage = "Unable to unload dirt";
    bool isComplete = false;
    bool isBlocked = false;
    public string ErrorMessage => errorMessage;

    public UnloadExcavatorBucketTask(Character character, Excavator ex)
    {
        this.character = character;
        this.ex = ex;
    }
    public void Execute()
    {
        DumpTruck dt = character.GetMap.IsDumpTruckInRange(ex);
        if (dt != null)
        {
            if (ex.DumpBucket(dt))
            {
                isComplete = true;
            }
            else
            {
                if (!dt.isReserved())
                {
                    errorMessage = $"There is currently no one operating a Dump Truck in this dig area.";
                    isBlocked = true;
                }
            }
        }
        else
        {
            if (!character.GetMap.IsAnyDumpTruckReserved())
            {
                character.CurrentVehicle.GetOutOfVehicle(character);
                errorMessage=$"There is currently no one operating Dump Trucks.";
                isBlocked = true;
            }
        }
    }

    public bool IsBlocked()
    {
        return isBlocked;
    }

    public bool IsCompleted()
    {
        return isComplete;
    }

    public bool IsExecuting()
    {
        return !isComplete;
    }

    public void Reset()
    {
        isComplete=false;
        isBlocked=false;
    }
}

/// <summary>
/// Task to complete the operation of a dump truck
/// </summary>
public class OperateDumpTruckTask : ITaskStrategy
{
    Character character;
    private bool isComplete = false;
    private bool hasBegun = false;
    public bool HasExecutionBegun { get => hasBegun; set => hasBegun = value; }
    string errorMessage = "Unable to operate dump truck";
    public string ErrorMessage => errorMessage;

    private bool isBlocked = false;

    public OperateDumpTruckTask(Character character)
    {
        this.character = character;
    }

    public void Execute()
    {


        if (character.IsInVehicle)
        {
            if (character.CurrentVehicle.GetType().Equals(typeof(DumpTruck)))
            {
                AdditionalExecution(character.CurrentVehicle as DumpTruck);
            }
            else
            {
                character.CurrentVehicle.GetOutOfVehicle(character);
            }
        }

        if (character.GetMap.ReserveDumpTruck(character))
        {
            character.SetDestination(character.CurrentVehicle.GetPos.x, character.CurrentVehicle.GetPos.y, () =>
            {
                character.CurrentVehicle.GetInVehicle(character, () =>
                {
                    AdditionalExecution(character.CurrentVehicle as DumpTruck);
                });
            });
        }
        else
        {
            // notify player that no excavator is available
            UIController.Instance.Notify($"{character.GetName} cannot operate at the dig site because there are no available dump trucks.");
            isBlocked = true;
        }

    }

    /// <summary>
    /// Part of this code can only run once.
    /// </summary>
    /// <param name="dt"></param>
    private void AdditionalExecution(DumpTruck dt)
    {
        if (!HasExecutionBegun)
        {
            HasExecutionBegun = true;
            if (character.GetMap.digAreas.Count > 0)
            {
                if (!dt.IsBedFull())
                {
                    Vehicle targetV = character.GetMap.GetDigExcavator();
                    if (targetV == null)
                    {
                        Tile tile = character.GetMap.digAreas.Values.First();
                        character.CurrentVehicle.SetDestination(tile.GetPos.x, tile.GetPos.y, () =>
                        {
                            // notify player that no excavator is available
                            UIController.Instance.Notify($"No excavator at dig site.");
                            isBlocked = true;
                        });
                    }
                    else // there is an excavator at dig area
                    {
                        if (Vector2.Distance(character.CurrentVehicle.GetPos, targetV.GetPos) < 7)
                        {
                            Reset();
                        }
                        else
                        {
                            character.CurrentVehicle.SetDestination(targetV.GetPos.x, targetV.GetPos.y + 6, () =>
                            {
                                Reset();
                            });
                        }
                    }
                }
                else // bed is full
                {
                    if (dt.DirtTypeInBed() == DirtType.Gravel)
                    {
                        if (character.GetMap.payDirtAreas.Count > 0)
                        {
                            Tile payTile = character.GetMap.payDirtAreas.Values.First();
                            dt.SetDestination(payTile.GetPos.x, payTile.GetPos.y, () =>
                            {
                                dt.DumpBed(payTile);
                                isComplete = true;
                            });
                        }
                        else// no pay area
                        {
                            UIController.Instance.Notify($"There are no pay areas to unload the dump truck.");
                            isBlocked = true;
                        }
                    }
                    else
                    {
                        if (character.GetMap.wasteAreas.Count > 0)
                        {
                            Tile wastTile = character.GetMap.wasteAreas.Values.First();
                            dt.SetDestination(wastTile.GetPos.x, wastTile.GetPos.y, () =>
                            {
                                dt.DumpBed(wastTile);
                                isComplete = true;
                            });
                        }
                        else// no pay area
                        {
                            UIController.Instance.Notify($"There are no pay areas to unload the dump truck.");
                            isBlocked = true;
                        }
                    }
                }
            }
            else
            {
                character.CurrentVehicle.GetOutOfVehicle(character, () =>
                {
                    // notify player that no excavator is available
                    UIController.Instance.Notify($"There are currently no dig areas created. Please create a dig area to continue dig operations.");
                    isBlocked = true;
                });
            }
        }
    }

    public bool IsBlocked()
    {
        return isBlocked;
    }

    public bool IsCompleted()
    {
        return isComplete;
    }

    public bool IsExecuting()
    {
        return !isComplete;
    }

    public void Reset()
    {
        isComplete = false;
        HasExecutionBegun = false;
        isBlocked = false;
    }
}



/// <summary>
/// Task to complete the operation of an excavator in the dig area
/// </summary>
public class WorkInDigAreaTask : ITaskStrategy
{
    private bool isComplete = false;
    private bool isBlocked = false;
    private bool hasBegun = false;
    public bool HasExecutionBegun { get => hasBegun; set => hasBegun = value; }
    string errorMessage = "Unable to operate in dig area";
    public string ErrorMessage => errorMessage;

    Character character;
    Excavator ex;
    // The Queue of strategies to complete
    ITaskStrategy[] taskStrategies;
    int step = 0;


    public WorkInDigAreaTask(Character character)
    {
        this.character = character;
        this.character.GetMap.ReserveExcavator(this.character);
        ex = this.character.CurrentVehicle as Excavator;

        taskStrategies = new ITaskStrategy[] {
            new GetInVehicleTask(this.character, ex),
            new DriveExcavatorToDigTileTask(this.character, ex),
            new RotateExcavatorBoomTask(ex),
            new DigWithExcavtorTask(this.character, ex),
            new RotateExcavatorBoomTask(ex),
            new UnloadExcavatorBucketTask(this.character, ex)
        };
    }

    public void Execute()
    {
        if (taskStrategies[step].IsBlocked())
        {
            isBlocked = true;
            errorMessage = taskStrategies[step].ErrorMessage;
            return;
        }
        if (taskStrategies[step].IsCompleted())
        {
            step++;
            if(step >= taskStrategies.Length)
            {
                isComplete = true;
                return;
            }
            Debug.Log($"{character.GetName} on step {step}");
            return;
        }
        taskStrategies[step].Execute();
    }

    public bool IsBlocked()
    {
        return isBlocked;
    }

    public bool IsCompleted()
    {
        return isComplete;
    }

    public bool IsExecuting()
    {
        return !isComplete;
    }

    public void Reset()
    {
        step = 0;
        isBlocked = false;
        isComplete = false;
        for (int i = 0; i < taskStrategies.Length; i++)
        {
            taskStrategies[i].Reset();
        }
    }
}



/// <summary>
/// This class is used to create a task system. The class is used as a global class in which workers can apply to complete tasks
/// </summary>
public class TaskSystem
{
    /// <summary>
    /// List of emergency tasks that need to be complete
    /// </summary>
    Queue<TaskStateMachine> emergencyTasks = new Queue<TaskStateMachine>();
    Queue<TaskStateMachine> globalTasks = new Queue<TaskStateMachine>();

    public void Update(float delta)
    {
        ProcessEmergencyTasks();
    }

    /// <summary>
    /// Function that goes throught the emergency task queue and assigns them if there are available characters.
    /// </summary>
    void ProcessEmergencyTasks()
    {
        while (emergencyTasks.Count > 0)
        {
            // Get the closest character assigned to the task
            PollDistance(emergencyTasks.Peek());
            // set reference to interface to obtain character value
            IGlobalTask task = emergencyTasks.Peek() as IGlobalTask;
            // if the character has been assigned.
            if (task.Character != null)
            {
                // dequeue the task and assing to character
                task.Character.GetWorkerAI.SetEmergencyTask(emergencyTasks.Dequeue());
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// Finds closest character and assigns them emergency task
    /// </summary>
    /// <param name="task"></param>
    void PollDistance(TaskStateMachine task)
    {
        // change the task paramerter as an IGlobalTask to get interface's parameters
        IGlobalTask globalTask = task as IGlobalTask;
        // get a references to all availbel characters
        List<Character> characters = GameManager.Instance.GetCurrentMap().characters;
        foreach (Character character in characters)
        {
            // get a refernce to the current searching character's current task
            TaskStateMachine currentTask = character.GetWorkerAI.GetCurrentTask;
            // if the current task is already a global task and a possible emergency task
            if (currentTask is IGlobalTask)
            {
                // cast the current task to check if it is an emergency task
                IGlobalTask currentGlobalTask = currentTask as IGlobalTask;
                // if the current task is an emergency task
                if (currentGlobalTask.IsEmergencyTask)
                {
                    // do not use this character because it is completing an emergency task already
                    continue;
                }
            }
            // if the character is not assigned
            if (globalTask.Character == null)
            {
                // assign a default character
                globalTask.Character = character;
            }
            // if there is a character assigned, compare the current character is closer than the assigned
            else if (Vector2.Distance(character.GetPos(), globalTask.Destination) <
                Vector2.Distance(globalTask.Character.GetPos(), globalTask.Destination))
            {
                // assing current character to task
                globalTask.Character = character;
            }
        }
    }

    /// <summary>
    /// Used to add an emergency task to the queue
    /// </summary>
    /// <param name="taskStrategy"></param>
    public void AddEmergencyTask(TaskStateMachine taskStrategy)
    {
        IGlobalTask task = taskStrategy as IGlobalTask;
        task.IsEmergencyTask = true;
        emergencyTasks.Enqueue(taskStrategy);
    }

    public void AddGlobalTask(TaskStateMachine taskStrategy)
    {
        globalTasks.Enqueue(taskStrategy);
    }

    public TaskStateMachine RequestTask(Character character)
    {
        if (globalTasks.Count > 0)
        {
            Debug.Log("getting global task");
            return globalTasks.Dequeue();
        }
        else
        {
            TaskStateMachine tsm = null;
            switch (character.Job)
            {
                case Character.Jobs.Excavator:
                    WorkInDigAreaTask widat = new WorkInDigAreaTask(character);
                    tsm = new TaskStateMachine(widat);
                    break;
                case Character.Jobs.Hauler:
                    OperateDumpTruckTask odtt = new OperateDumpTruckTask(character);

                    tsm = new TaskStateMachine(odtt);
                    break;
                default:
                    throw new ArgumentException("Character job not implemented");

            }
            return tsm;
        }
    }

    /// <summary>
    /// used to get an idle task
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    public TaskStateMachine RequestIdle(Character character)
    {
        IdleTask idleTask = new IdleTask(character);
        TaskStateMachine tsm = new TaskStateMachine(idleTask);
        return tsm;
    }

}
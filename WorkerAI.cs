using System.Collections.Generic;

/// <summary>
/// Class used to hand all AI for characters including movement, task
/// management, and state
/// </summary>
public class WorkerAI
{
    // field to store the current task state machine
    private TaskStateMachine currentTask;
    public TaskStateMachine GetCurrentTask { get => currentTask; }

    private Queue<TaskStateMachine> abandonedTasksQueue;

    /// <summary>
    /// Reference to this AI's character
    /// </summary>
    private Character character;

    /// <summary>
    /// Reference to the global task system.
    /// </summary>
    private TaskSystem taskSystem;

    public WorkerAI(Character character, TaskSystem ts)
    {
        this.character = character;
        abandonedTasksQueue = new Queue<TaskStateMachine>();
        currentTask = null;
        taskSystem = ts;

    }

    
    public void Update()
    {
        // need to check for a manual task
        if(currentTask != null)
        {
            if (currentTask.IsBlocked())
            {
                InGameConsole.i.Log($"{character.GetName}'s task is blocked");
                InGameConsole.i.Log($"{character.GetName}'s task: {currentTask.GetErrorMessage()}");
                UIController.Instance.Notify(currentTask.GetErrorMessage() + "\nBeginning idle task");
                //Debug.Log("Abandoning task");
                currentTask.Reset();
                abandonedTasksQueue.Enqueue(currentTask);
                currentTask = taskSystem.RequestIdle(character);
                return;
            }
            else if (currentTask.IsCompleted())
            {
                //Debug.Log("Task is complete");
                InGameConsole.i.Log($"{character.GetName}'s task is complete");
                currentTask = null;
                return;
            } else
            {
                currentTask.Execute();
            }
        } else
        {
            if (abandonedTasksQueue.Count > 0)
            {
                currentTask = abandonedTasksQueue.Dequeue();
                currentTask.Reset();
            }
            else
            {
                // request new task
                currentTask = taskSystem.RequestTask(character);
                InGameConsole.i.Log($"{character.GetName}'s task is {currentTask.ToString()}");
            }
        }
    }

    public void SetEmergencyTask(TaskStateMachine emergencyTask)
    {
        abandonedTasksQueue.Enqueue(currentTask);
        currentTask = emergencyTask;
    }

}

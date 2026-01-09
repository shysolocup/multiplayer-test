using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;

[GlobalClass]
public partial class TaskLib : Singleton<TaskLib>
{
    private static SceneTree tree;

    public override void _Ready()
    {
        base._Ready();
        tree = GetTree();
    }
    
    /// <summary>
    /// Waits for a given time
    /// </summary>
    /// <param name="time">time in milliseconds to wait for</param>
    public async Task Sleep(float time)
    {
        SceneTreeTimer t = tree.CreateTimer(time);
        await ToSignal(t, SceneTreeTimer.SignalName.Timeout);
        t.Dispose();
    }


    /// <summary>
    /// Waits for a given time and runs function in a new thread
    /// </summary>
    /// <param name="time">time in milliseconds to wait for</param>
    public CancellationTokenSource Delay(float time, Func<CancellationToken, Task> callback)
    {

        var cts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            SceneTreeTimer t = tree.CreateTimer(time);

            await ToSignal(t, SceneTreeTimer.SignalName.Timeout);
            await callback(cts.Token);

            t.Dispose();
        });

        return cts;
    }

    /// <summary>
    /// Waits for a given time and runs function in a new thread
    /// </summary>
    /// <param name="time">time in milliseconds to wait for</param>
    public void Delay(float time, Func<Task> callback) => Task.Run(async () =>
        {
            SceneTreeTimer t = tree.CreateTimer(time);

            await ToSignal(t, SceneTreeTimer.SignalName.Timeout);
            await callback();

            t.Dispose();
        });

    /// <summary>
    /// Spawns a new cancellable threaded task
    /// </summary>
    /// <param name="callback">The callback to run</param>
    /// <returns>CancellationTokenSource to cancel the task</returns>
    public CancellationTokenSource Spawn(Func<CancellationToken, Task> callback)
    {
        var cts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            await callback(cts.Token);
        });

        return cts;
    }

    /// <summary>
    /// Spawns a new cancellable threaded task
    /// </summary>
    /// <param name="callback">The callback to run</param>
    /// <returns>CancellationTokenSource to cancel the task</returns>
    public void Spawn(Func<Task> callback) => Task.Run(async () =>
        {
            await callback();
        });
}
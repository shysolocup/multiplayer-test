
using System;
using Godot;

public class OptimalEvent : GodotObject
{

    public static OptimalEvent operator + (OptimalEvent a, Action action) {
        a.Event += (EventDelegate)Delegate.CreateDelegate(typeof(EventDelegate), action.Target, action.Method);
        return a;
    }
    public static OptimalEvent operator - (OptimalEvent a, Action action) {
        a.Event -= (EventDelegate)Delegate.CreateDelegate(typeof(EventDelegate), action.Target, action.Method);
        return a;
    }

    public delegate void EventDelegate();
    private EventDelegate EventHandler;

    private void EventFunction()
        => EventHandler?.Invoke();

    private Callable EventCall => new(this, nameof(EventFunction));

    public event EventDelegate Event {
        add
        {
            if (EventHandler is null)
                self.Connect(signalBase, EventCall);

            EventHandler += value;
        }

        remove
        {
            EventHandler -= value;

            if (EventHandler is not null) return;

            self.Disconnect(signalBase, EventCall);
        }
    }

    private GodotObject self;
    private StringName signalBase;

    public OptimalEvent(GodotObject self, StringName signalBase)
    {
        this.self = self;
        this.signalBase = signalBase;
    }
}
using System;
using System.Collections.Generic;


// Represents valid state transitions in a FSM.  We need to be able to compare them, so
// implementing IEquatable interface.
// NOTE: When .NET support is updated in Unity this class can probably be replaced by Tuple<T, T>.
class StateTransition<T> : IEquatable<StateTransition<T>> {
    readonly T currentState;
    readonly T nextState;

    public StateTransition(T curr, T next) {
        this.currentState = curr;
        this.nextState = next;
    }

    // This class is to be used as a key in a Dictionary so we should override this.
    public override int GetHashCode() {
        // Overflow is ok, let it wrap!
        unchecked {            
            int hash = (7 * 13 + currentState.GetHashCode());
            hash = hash * 13 + nextState.GetHashCode();            
            return hash;
        }
    }

    public bool Equals(StateTransition<T> other) {
        if(ReferenceEquals(this, other))
            return true;
        return other != null && 
               this.currentState.Equals(other.currentState) &&
               this.nextState.Equals(other.nextState);
    }

    public override string ToString() {
        return currentState.ToString() + " -> " + nextState.ToString();
    }
}

// Generic FSM
public class FiniteStateMachine<T> {

    // Every transition has the ability to execute an action before and/or after the state 
    // being changed.
    // NOTE: By having the before/after methods be associated with the transition it allows 
    //       more flexibility with actions executed.  We may not always want to execute the 
    //       same code when changing to/from a state from different states.
    private class TransitionActions {
        readonly Action before;
        readonly Action after;
        readonly Action<EventArgs> announce;

        public TransitionActions(Action before, Action after, Action<EventArgs> announce) {
            this.before = before;
            this.after = after;
            this.announce = announce;
        }

        public Action Before {
            get { return this.before; }
        }

        public Action After {
            get { return this.after; }
        }

        public Action<EventArgs> Announce {
            get { return this.announce; }
        }
    }

    Dictionary<StateTransition<T>, TransitionActions> transitions;
    T currState;
    T prevState;

    public event EventHandler StateChanged;

#region Getters and Setters
    public T CurrentState {
        get;
        private set;
    }

    public T PrevState {
        get;
        private set;
    }
#endregion

    public FiniteStateMachine() {
        this.transitions = new Dictionary<StateTransition<T>, TransitionActions>();
    }

    // Attempt to transition to a new state.
    public void ChangeState(T newState) {
        var t = new StateTransition<T>(this.CurrentState, newState);
        TransitionActions actions;
        // If this transition has been defined, proceed.
        if(this.transitions.TryGetValue(t, out actions)) {
            if(actions.Before != null)
                actions.Before();
                
            this.PrevState = this.CurrentState;
            this.CurrentState = newState;

            // Notify listeners that state has changed.
            this.OnStateChanged(EventArgs.Empty);

            if(actions.Announce != null)
                actions.Announce(EventArgs.Empty);

            if(actions.After != null)
                actions.After();
        } else {
            UnityEngine.Debug.Log("[FSM] Warning! ChangeState: Transition Not Defined - " + t);
        }
    }

    // Add a transition to dictionary.
    public void AddTransition(T s1, T s2, Action before, Action after, Action<EventArgs> announce) {
        var t = new StateTransition<T>(s1, s2);
        // If it already exists, don't add it again.
        if(transitions.ContainsKey(t)) {
            UnityEngine.Debug.Log("[FSM] Warning! AddTransition: Transition Already Exists - " + t);
            return;
        }

        this.transitions.Add(t, new TransitionActions(before, after, announce));
    }

    // Add a transition to dictionary without before and after actions.
    public void AddTransition(T s1, T s2, Action<EventArgs> announce) {
        this.AddTransition(s1, s2, null, null, announce);
    }

    // Remove a transition from dictionary.
    public void DeleteTransition(T s1, T s2) {
        var t = new StateTransition<T>(s1, s2);
        if(!transitions.Remove(t)) {
            UnityEngine.Debug.Log("[FSM] Warning! DeleteTransition: Transition not found - " + t);
        }
    }

    virtual protected void OnStateChanged(EventArgs e) {
        // Ensure things are safer with multiple threads.
        var changed = StateChanged;
        if(changed != null)
            changed(this, e);
    }
}

using Godot;
using System;

public partial class State : Node
{
    public State_Machine state_machine;

    public virtual void Enter(PlayerMovementState previous_state)
    {
        // Code for entering the state
        ////Globals.debug.debug_err("Base State Class Enter() Function");
    }

    public virtual void Exit()
    {
        // Code for exiting the state
        ////Globals.debug.debug_err("Base State Class Exit() Function");
    }

    public virtual void Update(float delta)
    {
        // Code for updating the state
    }

    public virtual void PhysicsUpdate(float delta)
    {
        // Code for physics update in the state
    }
}

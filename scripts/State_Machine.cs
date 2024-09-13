using Godot;
using System;

public partial class State_Machine : Node
{
    [Export] public PlayerMovementState CURRENT_STATE;  // Current active state
    [Export] public PlayerMovementState PREVIOUS_STATE;
    [Export] public AnimationPlayer animationPlayer;
    public bool enabled = false;
    public Godot.Collections.Array<State> states = new Godot.Collections.Array<State>();  // Array to hold all states

    public override void _Ready()
    {
       // Adding to Debug
        Globals.debug.add_debug_property("Position", 0);
        Globals.debug.add_debug_property("Current Player State", "Null");
        Globals.debug.add_debug_property("Previous Player State", "Null");
        Globals.debug.add_debug_property("Weapon Loaded", "");
        Globals.debug.add_debug_property("Animation", "");
        Globals.debug.add_debug_property("FOV", 0);
        Globals.debug.add_debug_property("Slide Timer", 0);
    }

    //_on_PLAYER_READY
    public void InitializeStateMachine(Player player)
    {
        //GD.Print("Running State Machine");  
        foreach (Node child in GetChildren())
        {
            if (child is PlayerMovementState state)
            {
                //GD.Print("Player State Added: " + state.Name);
                states.Add(state);
                state.state_machine = this;

                // Initialize state with player data
                state.InitializeState(player);
            }          
            else
            {
                Globals.debug.debug_err("State machine contains incompatible child node");
            }
        }

        enabled = true;
        // If a CURRENT_STATE is set, enter it
        OnStateTransition("IdlePlayerState");
    }

    public override void _Process(double delta)
    {
        // Call the Update method of the current state
        if (enabled) CURRENT_STATE?.Update((float)delta);

        // Update Debug
        Globals.debug.update_debug_property("Animation", animationPlayer.CurrentAnimation);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Call the PhysicsUpdate method of the current state
        if (enabled) CURRENT_STATE?.PhysicsUpdate((float)delta);
    }

    public void OnStateTransition(StringName newStateName)
    {
        // Find the new state by name
        foreach (PlayerMovementState state in states)
        {
            if (state.Name == newStateName)
            {
                TransitionToState(state);
                break;
            }
        }
    }

    private void TransitionToState(PlayerMovementState newState)
    {
        if (CURRENT_STATE != null)
        {
            Globals.debug.update_debug_property("Previous Player State", CURRENT_STATE.Name);
            PREVIOUS_STATE = CURRENT_STATE;
            CURRENT_STATE.Exit();  // Call exit on the current state
        }

        if (newState != null)
        {
            newState.Enter(CURRENT_STATE);  // Call enter on the new state
            CURRENT_STATE = newState;
            Globals.debug.update_debug_property("Current Player State", CURRENT_STATE.Name);
        }
    }
}

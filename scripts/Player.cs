using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

public enum Team
{
	Red, // Can be only damaged by None or blue
	Blue, // Can be only damaged by None or Red
	None // Can be damaged by anyone
}

public partial class Player : CharacterBody3D
{
    public PlayerInfo player_info; // for networking
    [Export] public float health = 100.0f;
    [Export(PropertyHint.Range, "1,100")]
    public float MouseSensitivity { get; set; } = 50.0f;
    [Export] public float default_speed { get; set; } = 5.0f;
    [Export] public float sprint_speed { get; set; } = 7.0f;
    [Export] public float crouch_speed { get; set; } = 3.0f;
    [Export] public float slide_speed { get; set; } = 6.0f;
    [Export] public float ACCELERATION { get; set; } = 0.1f;
    [Export] public float DECELERATION { get; set; } = 0.25f;
    [Export] public float JumpVelocity { get; set; } = 5.8f;
    [Export] public float Jump_Input_Multiplier { get; set; } = 0.85f;
    [Export] public float slide_tilt { get; set; } = 0.89f;
    [Export] public float camera_fov { get; set; } = 75.0f;
    [Export] public int interact_distance { get; set; } = 3;
    [Export(PropertyHint.Range, "5,10,0.1")] public float CrouchAnimSpeed { get; set; } = 7.0f;
    [Export] public NodePath CameraNodePath { get; set; }
    [Export] public NodePath AnimationPlayerPath { get; set; }
    [Export] public NodePath ShapeCastPath { get; set; }
    [Export] public WeaponControllerSingleMesh WEAPON_CONTROLLER;
    [Export] public HealthComponent healthComponent { get; set; }
    [Export] public State_Machine stateMachine { get; set; }
    [Export] public PlayerUI player_ui;
    [Export] public float _speed;
    public int Money { get; set; } = 100;
    public float gravity = 12.0f;
    public Vector3 gravity_vector;
    public Vector3 _rotation = Vector3.Zero;

    private bool _isCrouching = false;
    private bool _isCrouchKeyHeld = false;

    public Camera3D _camera;
    private AnimationPlayer _animationPlayer;
    private ShapeCast3D _crouchShapeCast;

    // Raycasting Variables
    public PhysicsRayQueryParameters3D raycast;
    public PhysicsDirectSpaceState3D space_state;
    public Godot.Collections.Dictionary weapon_raycast_result;
    public Godot.Collections.Dictionary interact_raycast_result;
    public ImmediateMesh raycast_mesh;  // Mesh for the raycast line

    // Interaction
    public Node current_interactable;

    // Multiplayer
    public MultiplayerSynchronizer multiplayerSynchronizer;

    [Signal] public delegate void PlayerReadyEventHandler();

    public override void _Ready()
    {
        GD.Print("Loading Player");

        InitializeNetworkComponents();
        InitializePlayerComponents();

        // Spawn Points
        GlobalPosition = new Vector3(GD.Randf() * 20, 10 , 10); 

        EmitSignal(SignalName.PlayerReady);
    }

    private void InitializeNetworkComponents()
    {
        // Set current player as multiplayer authority
        player_info = Globals.localPlayerInfo;
        multiplayerSynchronizer = GetNode<MultiplayerSynchronizer>("%MultiplayerSynchronizer");
        multiplayerSynchronizer.SetMultiplayerAuthority(int.Parse(Name));
    }
    private void InitializePlayerComponents()
    {
        // Setup Player UI
        player_ui.UpdateUI("Health", "Health: " + health);
        player_ui.UpdateUI("Money", "$" + Money);
        // Setting current speed to the default_speed set for player and gravity vector
        _speed = default_speed;


        _camera = GetNode<Camera3D>(CameraNodePath);
        _camera.Fov = camera_fov;
        // Raycasting
        space_state = _camera.GetWorld3D().DirectSpaceState;
        _animationPlayer = GetNode<AnimationPlayer>(AnimationPlayerPath);
        WEAPON_CONTROLLER.InitializeWeaponController(this);

        // Needed for Networked Player Not Extras
        if (multiplayerSynchronizer.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;
        _crouchShapeCast = GetNode<ShapeCast3D>(ShapeCastPath);
        stateMachine.InitializeStateMachine(this);
        gravity_vector = new Vector3(0.0f, gravity, 0.0f);
    }

    public override void _Input(InputEvent @event)
    {
        if (multiplayerSynchronizer.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;
            if (@event is InputEventKey eventKey)
            {
                if (eventKey.Pressed && eventKey.Keycode == Key.Escape)
                {
                    if (Input.MouseMode == Input.MouseModeEnum.Captured)
                    {
                        Input.MouseMode = Input.MouseModeEnum.Visible;
                    }
                    else
                    {
                        GetTree().Quit();
                    }
                }
            }

            if (@event is InputEventMouseMotion eventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                HandleMouseInput(eventMouseMotion);
            }

            // Handle mouse click to recapture the mouse
            if (@event is InputEventMouseButton eventMouseButton)
            {
                if (eventMouseButton.Pressed && Input.MouseMode == Input.MouseModeEnum.Visible)
                {
                    // Capture the mouse again when clicking inside the window
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                }
            }
        
    }

    public void Die()
    {
        GD.Print("Player is Dead ID: " + Name);
    }

    public Godot.Collections.Dictionary shoot_raycast(int distance, float recoil_offset_x = 0.0f, float recoil_offset_y = 0.0f, bool debug_raycost_dot = false)
    {
        space_state = _camera.GetWorld3D().DirectSpaceState;
        
        // Get the viewport rectangle
        Rect2 visible_rect = GetViewport().GetVisibleRect();

        // Calculate the center of the screen
        Vector2 screen_center = visible_rect.Size / 2;

        Vector3 origin = _camera.ProjectRayOrigin(screen_center);

        // Apply recoil offset by moving the screen center upwards
        screen_center.Y -= recoil_offset_y;  // Moves the center up for recoil effect
        screen_center.X -= recoil_offset_x; // Moves the recoil left or right

        // Creating Ray Begining and End
        Vector3 end = origin + _camera.ProjectRayNormal(screen_center) * distance; // Distanced Traveled
            
        // Shooting Ray
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin,end);
        query.CollideWithBodies = true;

        // Gives us information of what collided
        Godot.Collections.Dictionary result = space_state.IntersectRay(query);

        return result;
    }


    public void run_raycast()
    {
        interact_raycast_result = shoot_raycast(interact_distance);

        // Handle Interactions
        if (interact_raycast_result.ContainsKey("collider")) // check to see if there is an object in interactable distance
        {
            var collider = (Node)interact_raycast_result["collider"]; // set it to collider if there is 
            if (IsInstanceValid(collider) && collider != current_interactable && collider.HasSignal(InteractionComponent.SignalName.OnInteract)) // if the collider is a new one and not the old one
            {
                if (IsInstanceValid(current_interactable))
                {
                    current_interactable?.EmitSignal(InteractionComponent.SignalName.NotInRange);
                    player_ui.GetUIElement("Interact").Visible = false;
                }
                current_interactable = collider;
                current_interactable?.EmitSignal(InteractionComponent.SignalName.InRange);
                player_ui.GetUIElement("Interact").Visible = true;
            }
        }
        else if (IsInstanceValid(current_interactable))
        {
            current_interactable?.EmitSignal(InteractionComponent.SignalName.NotInRange);
            current_interactable = null;
            player_ui.GetUIElement("Interact").Visible = false;
        }
        else
        {
            current_interactable = null;
            player_ui.GetUIElement("Interact").Visible = false;
        }

        // Shoot weapon raycast
        weapon_raycast_result = shoot_raycast(1000, WEAPON_CONTROLLER.recoil_offset.X, WEAPON_CONTROLLER.recoil_offset.Y); //Regular distance for Weapon raycast

        if (weapon_raycast_result.ContainsKey("position"))
        {
            Vector3 raycast_end_position = (Vector3)weapon_raycast_result["position"];
            DrawRaycastDot(raycast_end_position); 
        }
        
    }

    private void DrawRaycastDot(Vector3 raycast_end_position)
    {
        // Create a sphere or small dot to represent the hit marker
        var red_dot = new MeshInstance3D();
        red_dot.Mesh = new SphereMesh { Radius = 0.05f, Height = 0.1f};  // Small sphere as the dot
        red_dot.MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(1, 0, 0) };  // Red color
        // Add the red dot to the world
        GetTree().Root.GetChild(0).AddChild(red_dot);

        // Set the position of the red dot at the hit location
        red_dot.GlobalPosition = raycast_end_position;
        GetTree().CreateTimer(0.01f).Timeout += () => { red_dot.QueueFree(); };

    }

    public void interact()
    {
        if (current_interactable != null)
        {
            //Globals.debug.debug_message("Interacting With: " + current_interactable.Name);
            current_interactable.EmitSignal(InteractionComponent.SignalName.OnInteract);
        }
    }

    private void HandleMouseInput(InputEventMouseMotion eventMouseMotion)
    {
        float internalSensitivity = MouseSensitivity / 10000.0f;

        _rotation.Y -= eventMouseMotion.Relative.X * internalSensitivity;
        _rotation.X -= eventMouseMotion.Relative.Y * internalSensitivity;
        _rotation.X = Mathf.Clamp(_rotation.X, -1.5f, 1.5f);

        Rotation = new Vector3(0, _rotation.Y, 0);
        if (_camera != null)
        {
            _camera.Rotation = new Vector3(_rotation.X, 0, 0);
        }
    }

    public void UpdateGravity(float delta)
    {
        Velocity -= gravity_vector * delta;
    }

    public void Damage(float damage)
    {
        health -= damage;

        if (health <= 0.0f)
        {
            //Player is dead
            Globals.debug.debug_message("Player is dead");
        }
    }

    public void AddMoney(int _money)
    {
        Money += _money;
        player_ui.UpdateUI("Money", "$" + Money);
    }

    public void UpdateInput(float speed, float acceleration, float deceleration, float input_multiplier)
    {
        if (multiplayerSynchronizer.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;
            
            _camera.MakeCurrent();

            Vector3 velocity = Velocity;

            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
            Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized() *  input_multiplier;

            if (direction != Vector3.Zero)
            {
                velocity.X = Mathf.Lerp(velocity.X, direction.X * speed, acceleration); 
                velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * speed, acceleration); 
            }
            else
            {
                velocity.X = Mathf.MoveToward(velocity.X, 0, deceleration);
                velocity.Z = Mathf.MoveToward(velocity.Z, 0, deceleration);
            }

            Velocity = velocity;   
    }

    public void UpdateVelocity()
    {
        MoveAndSlide();
    }
}

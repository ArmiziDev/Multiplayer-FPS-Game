using Godot;
using System;

public enum Team
{
	Red, // Can be only damaged by None or blue
	Blue, // Can be only damaged by None or Red
	None // Can be damaged by anyone
}

public partial class Player : CharacterBody3D
{
    public PlayerInfo player_info; // for networking
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
    [Export] public State_Machine stateMachine { get; set; }
    [Export] public PlayerUserInterface player_user_interface;
    [Export] public float _speed;

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
    public Vector3 raycast_bullet_end;

    // Interaction
    public Node current_interactable;

    // Multiplayer
    public MultiplayerSynchronizer multiplayerSynchronizer;
    public PlayerNetworkingCalls playerNetworkingCalls { get; set; }

    [Signal] public delegate void PlayerReadyEventHandler();

    public override void _Ready()
    {
        // We Initalized player_info when we spawned them in the worldsetup script
        GD.Print("Loading Player");

        InitializeNetworkComponents();
        InitializePlayerComponents();

        // Spawn Points
        GlobalPosition = new Vector3(GD.Randf() * 20, 10 , 10);

        EmitSignal(SignalName.PlayerReady);
    }

    private void InitializeNetworkComponents()
    {
        playerNetworkingCalls = GetNode<PlayerNetworkingCalls>("%PlayerRpcCalls");

        // Set current player as multiplayer authority
        multiplayerSynchronizer = GetNode<MultiplayerSynchronizer>("%MultiplayerSynchronizer");
        multiplayerSynchronizer.SetMultiplayerAuthority(int.Parse(Name));
    }
    private void InitializePlayerComponents()
    {
        // Setting current speed to the default_speed set for player and gravity vector
        _speed = default_speed;

        //Setting player color
        var player_mesh = GetNode<MeshInstance3D>("%Placeholder Mesh");
        StandardMaterial3D player_material = new StandardMaterial3D();

        switch (player_info.player_team)
        {
            case (Team.Red):
                player_material.AlbedoColor = new Color(1, 0, 0);
                break;
            case (Team.Blue):
                player_material.AlbedoColor = new Color(0, 0, 1);
                break;
            case (Team.None):
                player_material.AlbedoColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
                break;
        }

        // Apply the material to the player's mesh
        player_mesh.SetMaterialOverride(player_material);

        _camera = GetNode<Camera3D>(CameraNodePath);
        _camera.Fov = camera_fov;

        // Raycasting
        space_state = _camera.GetWorld3D().DirectSpaceState;
        _animationPlayer = GetNode<AnimationPlayer>(AnimationPlayerPath);
        WEAPON_CONTROLLER.InitializeWeaponController(this);

        // Needed for Networked Player Not Extras
        if (multiplayerSynchronizer.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;
        Globals.localPlayerInfo = player_info;
        Globals.localPlayer = this;

        _crouchShapeCast = GetNode<ShapeCast3D>(ShapeCastPath);
        stateMachine.InitializeStateMachine(this);
        gravity_vector = new Vector3(0.0f, gravity, 0.0f);

        // Adding User Interface
        PackedScene playerUIScene = (PackedScene)ResourceLoader.Load("res://scenes/player_user_interface.tscn");

        player_user_interface = (PlayerUserInterface)playerUIScene.Instantiate();

        // Add the UI to the scene tree, usually under the root or a UI-related node
        GetTree().Root.AddChild(player_user_interface);

        player_user_interface.reticle().SetPlayer(this);
        player_user_interface.playerUI().UpdateUI("Health", "Health " + player_info.health);
        player_user_interface.playerUI().UpdateUI("Money", "$" + player_info.money);
        player_user_interface.playerUI().UpdateUI("DisplayName", player_info.Name);

        player_user_interface.debug().add_debug_property("Global Local Player Team: ", Globals.localPlayerInfo.player_team);
        player_user_interface.debug().add_debug_property("Player Team: ", player_info.player_team);
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
                if (eventMouseButton.Pressed && Input.MouseMode == Input.MouseModeEnum.Visible && !Globals.PlayerUI.playerUI().is_buymenu_open())
                {
                    // Capture the mouse again when clicking inside the window
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                }
            }
        
    }

    public void Damage(int _damage, PlayerInfo sender_player)
    {
        // sender is the player sending the damage
        // player_info is the player currently being damaged
        PlayerInfo damaged_player = player_info;
        playerNetworkingCalls.Damage(_damage, damaged_player, sender_player);
    }

    public void PlayerDie()
    {
        // Game Manager Handles Player Death
        Globals.gameManager.HandlePlayerDead(this); // reciever, sender
    }

    public void ResetAttributes()
    {
        player_info.health = 100;
        WEAPON_CONTROLLER.WEAPON_TYPE.current_ammo = WEAPON_CONTROLLER.WEAPON_TYPE.magazine_capacity; // reset ammo
        
        player_user_interface?.playerUI().UpdateUI("Health", "Health: " + player_info.health);
        player_user_interface?.playerUI().UpdateUI("Ammo", WEAPON_CONTROLLER.WEAPON_TYPE.current_ammo + " / " + WEAPON_CONTROLLER.WEAPON_TYPE.magazine_capacity);
    }

    public Godot.Collections.Dictionary shoot_raycast(int distance, float raycast_offset_x = 0.0f, float raycast_offset_y = 0.0f, bool weapon_shooting = false)
    {
        space_state = _camera.GetWorld3D().DirectSpaceState;
        
        // Get the viewport rectangle
        Rect2 visible_rect = GetViewport().GetVisibleRect();

        // Calculate the center of the screen
        Vector2 screen_center = visible_rect.Size / 2;

        Vector3 origin = _camera.ProjectRayOrigin(screen_center);

        // Apply recoil offset by moving the screen center upwards
        screen_center.X -= raycast_offset_x; // Moves the recoil left or right
        screen_center.Y -= raycast_offset_y;  // Moves the center up for recoil effect

        // Creating Ray Begining and End
        Vector3 end = origin + _camera.ProjectRayNormal(screen_center) * distance; // Distanced Traveled
            
        // Shooting Ray
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin,end);
        query.CollideWithBodies = true;

        // Gives us information of what collided
        Godot.Collections.Dictionary result = space_state.IntersectRay(query);

        if (weapon_shooting)
        {
            raycast_bullet_end = end;
        }

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
                    player_user_interface.playerUI().GetUIElement("Interact").Visible = false;
                }
                current_interactable = collider;
                current_interactable?.EmitSignal(InteractionComponent.SignalName.InRange);
                player_user_interface.playerUI().GetUIElement("Interact").Visible = true;
            }
        }
        else if (IsInstanceValid(current_interactable))
        {
            current_interactable?.EmitSignal(InteractionComponent.SignalName.NotInRange);
            current_interactable = null;
            player_user_interface.playerUI().GetUIElement("Interact").Visible = false;
        }
        else
        {
            current_interactable = null;
            player_user_interface.playerUI().GetUIElement("Interact").Visible = false;
        }

        // Shoot weapon raycast
        weapon_raycast_result = shoot_raycast(1000, WEAPON_CONTROLLER.raycast_offset.X, WEAPON_CONTROLLER.raycast_offset.Y, true); //Regular distance for Weapon raycast

        if (Globals.PlayerUI.debug().Visible) // red dot for debugging purposes
        {
            if (weapon_raycast_result.ContainsKey("position"))
            {
                Vector3 raycast_end_position = (Vector3)weapon_raycast_result["position"];
                DrawRaycastDot(raycast_end_position);
            }
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

    public void AddMoney(int _money)
    {
        player_info.money += _money;
        player_user_interface.playerUI().UpdateUI("Money", "$" + player_info.money);
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

    // Networking Player RPC Calls Handling
    public void _on_player_rpc_player_damage_update(PlayerInfo reciever, PlayerInfo sender)
    {
        if (reciever == Globals.localPlayerInfo) // if the reciever of the damage is the localPlayer we want to blood splatter
        {
            Globals.PlayerUI.playerUI().ShowBloodSplatter();
        }

        if (player_info.health <= 0)
        {
            PlayerDie();
            Globals.PlayerUI.playerUI().AddPlayerKill(sender, player_info);
        }

        Globals.PlayerUI.playerUI().UpdateUI("Health", "Health " + Globals.localPlayerInfo.health);
        Globals.PlayerUI.playerUI().UpdateUI("Money", "$" + Globals.localPlayerInfo.money);
    }
}

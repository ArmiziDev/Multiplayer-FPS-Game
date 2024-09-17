 using Godot;
using System;
using System.Runtime.CompilerServices;


[Tool]
public partial class WeaponControllerSingleMesh : Node3D
{
    [Export]
    public Weapons WEAPON_TYPE { get; set; }
    
    [Export] public float sway_speed = 1.2f;

    //[Export] public Node3D recoil_node3D;
    [Export] public MeshInstance3D weapon_mesh { get; set; }
    [Export] public MeshInstance3D weapon_shadow { get; set; }
    [Export] public MuzzleFlash muzzle_flash { get; set; }
    [Export] public Timer shoot_timer { get; set; }
    [Export] public Timer PulloutTimer { get; set; }

    public Player player;
    public WeaponRecoilPhysics weapon_recoil_physics;
    public bool can_shoot = true;
    public bool shooting = false;
    public bool reloading = false;
    public int StringName;

    public int current_loadout_index;

    private Vector2 mouse_movement;
    private float random_sway_x;
    private float random_sway_y;
    private float random_sway_amount;
    private float time = 0.0f;
    private float idle_sway_adjustment;
    private float idle_sway_rotation_strength;
    private Vector2 weapon_bob_amount = Vector2.Zero;
    public NoiseTexture2D sway_noise;
    public PackedScene raycast_bullet_hole;
    public PackedScene raycast_bullet_linecast;
    private bool currently_loading_weapon = false;
    private Random random;

    // Raycast Offset
    public Vector3 raycast_offset = Vector3.Zero;
    public Vector3 recoil_offset = Vector3.Zero;
    public Vector3 recoil_offset_target = Vector3.Zero;
    public Vector3 recoil_innacuracy_spread = Vector3.Zero;
    public float recoil_reset_lerp_factor;

    //Signals
    [Signal] public delegate void WeaponFiredEventHandler();

    public void InitializeWeaponController(Player player)
    {
        this.player = player;
        sway_noise = ResourceLoader.Load<NoiseTexture2D>("res://meshes/weapons/weapon_sway/weapon_sway1.tres");
        raycast_bullet_hole = ResourceLoader.Load<PackedScene>("res://scenes/raycast_bullet_hole.tscn");
        raycast_bullet_linecast = ResourceLoader.Load<PackedScene>("res://scenes/bullet_line_cast.tscn");
        
        weapon_mesh = GetNode<MeshInstance3D>("%WeaponMesh");
        weapon_shadow = GetNode<MeshInstance3D>("%WeaponShadow");
        shoot_timer = GetNode<Timer>("%ShootTimer");
        PulloutTimer = GetNode<Timer>("%PulloutTimer");
        
        weapon_recoil_physics = GetNode<WeaponRecoilPhysics>("%RecoilPosition");
        muzzle_flash = GetNode<MuzzleFlash>("%MuzzleFlash");

        random = new Random();

        LoadLoadout();
        InitializeComponents();
    }

    public void _on_PLAYER_READY()
    {
        current_loadout_index = 0;
        LoadWeapon();
    }

    private void InitializeComponents()
    {
        weapon_recoil_physics.InitializeWeaponRecoilPhysics(this);
        muzzle_flash.InitializeMuzzleFlash(this);
    }
    private void LoadLoadout()
    {
        player.player_info.loadout[0] = "AR-15";
        player.player_info.loadout[1] = "Desert Eagle";
        player.player_info.loadout[2] = "Hand";

        WEAPON_TYPE = Globals.weaponDictionary["Hand"];
    }
    public override void _Process(double delta)
    {
        ResetRecoilOffset((float)delta);

        raycast_offset = recoil_offset;
        raycast_offset += recoil_innacuracy_spread;
    }
    public override void _Input(InputEvent @event)
    {
        if (player.multiplayerSynchronizer.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;
        if (@event.IsActionPressed("primary_weapon") && current_loadout_index != 0)
        {
            current_loadout_index = 0;
            LoadWeapon();
            player.playerNetworkingCalls.UpdateLoadout(player);
        }
        if (@event.IsActionPressed("secondary_weapon") && current_loadout_index != 1)
        {
            current_loadout_index = 1;
            LoadWeapon();
            player.playerNetworkingCalls.UpdateLoadout(player);
        }
        if (@event.IsActionPressed("tertiary_weapon") && current_loadout_index != 2)
        {
            current_loadout_index = 2;
            LoadWeapon();
            player.playerNetworkingCalls.UpdateLoadout(player);
        }
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            mouse_movement = eventMouseMotion.Relative;
        }
    }

    public void OnWeaponPickedUp(Weapons _weapon)
    {
        switch(_weapon.weapon_type)
        {
            case (Weapons.WeaponType.Gun):
                // If Weapon Is A Gun Proceed
                switch (_weapon.gun_class)
                {
                    case (Weapons.GunClass.Pistol): // Secondary
                        if (player.player_info.loadout[1] != "Hand")
                        {
                            DropCurrentWeapon();
                        }
                        player.player_info.loadout[1] = _weapon.name;
                        LoadWeapon();
                        break;
                    case (Weapons.GunClass.None): // Error
                        //Globals.debug.debug_err(_weapon.name + " Has No Class");
                        break;
                    default: // First Gun In Loadout (Rifle, SMG, Sniper, Shotgun)
                        if (player.player_info.loadout[0] != "Hand")
                        {
                            DropCurrentWeapon();
                        }
                        player.player_info.loadout[0] = _weapon.name;
                        LoadWeapon();
                        break;
                }
                break;
            case (Weapons.WeaponType.Melee):
                break;
            case (Weapons.WeaponType.Utility):
                break;
        }
        player.playerNetworkingCalls.UpdateLoadout(player);
    }

    public void DropWeapon()
    {
        player.playerNetworkingCalls.DropWeapon(player);
    }

    public void DropCurrentWeapon()
    {
        if (WEAPON_TYPE == null) return;
        if (WEAPON_TYPE.weapon_type == Weapons.WeaponType.Empty) return;

        // Load the weapon physics body scene
        WeaponPhysicsBody WeaponBody = (WeaponPhysicsBody)GD.Load<PackedScene>("res://scenes/weapon_physics_body.tscn").Instantiate();
        // Add the weapon to the root node or world node (the farthest down parent in the scene)
        GetTree().Root.GetChild(0).AddChild(WeaponBody);
        WeaponBody.SetWeapon(WEAPON_TYPE);
        WeaponBody.InitializeWeaponPhysicsBody(this);
        // Set the weapon type

        // Set the weapon's global transform to match the player's
        WeaponBody.GlobalTransform = GlobalTransform;

        // Calculate the position in front of the player
        Vector3 forwardDirection = -GlobalTransform.Basis.Z;  // Forward direction (negative Z axis)
        Vector3 dropPosition = GlobalTransform.Origin + forwardDirection * 2.0f;  // Drop 2 units in front of the player

        // Set a consistent Y (height) value for the drop position (e.g., 1.0f units above the ground)
        dropPosition.Y  = GlobalTransform.Origin.Y;  // Keep it at the player's current Y position

        // Set the weapon's position to the calculated drop position
        WeaponBody.GlobalTransform = new Transform3D(WeaponBody.GlobalTransform.Basis, dropPosition);

        // Optionally apply additional rotation or adjustments if needed
        WeaponBody.RotationDegrees = RotationDegrees;  // Keep the player's current rotation

        // Reset the player's current weapon type if needed
        WEAPON_TYPE = Globals.weaponDictionary["Hand"];

        player.player_info.loadout[current_loadout_index] = WEAPON_TYPE.name;

        player.playerNetworkingCalls.UpdateLoadout(player);
    }

    internal void DropWeaponKeepOriginal(int loadout_index)
    {
        if (player.player_info.loadout[loadout_index] == "Hand") return;

        // Load the weapon physics body scene
        WeaponPhysicsBody WeaponBody = (WeaponPhysicsBody)GD.Load<PackedScene>("res://scenes/weapon_physics_body.tscn").Instantiate();
        // Add the weapon to the root node or world node (the farthest down parent in the scene)
        GetTree().Root.GetChild(0).AddChild(WeaponBody);
        WeaponBody.SetWeapon(Globals.weaponDictionary[player.player_info.loadout[loadout_index]]);
        WeaponBody.InitializeWeaponPhysicsBody(this);
        // Set the weapon type

        // Set the weapon's global transform to match the player's
        WeaponBody.GlobalTransform = GlobalTransform;

        // Calculate the position in front of the player
        Vector3 forwardDirection = -GlobalTransform.Basis.Z;  // Forward direction (negative Z axis)
        Vector3 dropPosition = GlobalTransform.Origin + forwardDirection * 2.0f;  // Drop 2 units in front of the player

        // Set a consistent Y (height) value for the drop position (e.g., 1.0f units above the ground)
        dropPosition.Y = GlobalTransform.Origin.Y;  // Keep it at the player's current Y position

        // Set the weapon's position to the calculated drop position
        WeaponBody.GlobalTransform = new Transform3D(WeaponBody.GlobalTransform.Basis, dropPosition);

        // Optionally apply additional rotation or adjustments if needed
        WeaponBody.RotationDegrees = RotationDegrees;  // Keep the player's current rotation
    }

    public void LoadWeapon()
    {
        currently_loading_weapon = true;

        PulloutTimer.Stop();
        PulloutTimer.Start();

        WEAPON_TYPE = Globals.weaponDictionary[player.player_info.loadout[current_loadout_index]]; // Setting weapon to the current loadout when loading a weapon
        
        if (WEAPON_TYPE != null)
        {
            can_shoot = false; // Have to wait for pullout animation

            if (player.player_info.loadout[current_loadout_index] == "Hand")
            {
                Position = WEAPON_TYPE.position;
                RotationDegrees = WEAPON_TYPE.rotation;
            }
            else
            {
                StartPulloutAnimation();
            }

            if (weapon_mesh != null)
            {
                weapon_mesh.Mesh = WEAPON_TYPE.mesh;
            }
            else
            {
                //Globals.debug?.debug_err("weapon_mesh is null. Cannot set mesh.");
            }

            if (weapon_shadow != null)
            {
                weapon_shadow.Visible = WEAPON_TYPE.shadow;
            }
            else
            {
                //Globals.debug?.debug_err("weapon_shadow is null. Cannot set visibility.");
            }

            Scale = WEAPON_TYPE.scale;

            // Sway Animation
            idle_sway_adjustment = WEAPON_TYPE.idle_sway_adjustment;
            idle_sway_rotation_strength = WEAPON_TYPE.idle_sway_rotation_strength;
            random_sway_amount = WEAPON_TYPE.random_sway_amount;

            // Gun Stats
            if (shoot_timer != null)
            {
                shoot_timer.WaitTime = WEAPON_TYPE.time_between_bullets;
            }
            else
            {
                //Globals.debug?.debug_err("shoot_timer is null. Cannot set wait time.");
            }

            // Muzzle Flash
            if (muzzle_flash != null)
            {
                muzzle_flash.Position = WEAPON_TYPE.muzzle_flash_position;
            }
            else
            {
                //Globals.debug?.debug_err("muzzle_flash is null. Cannot set position.");
            }

            // Update Loadout UI
            player.player_user_interface?.playerUI().UpdateUI("Loadout1", "1: " + player.player_info.loadout[0]);
            player.player_user_interface?.playerUI().UpdateUI("Loadout2", "2: " + player.player_info.loadout[1]);
            player.player_user_interface?.playerUI().UpdateUI("Loadout3", "3: " + player.player_info.loadout[2]);

            player.player_user_interface?.playerUI().UpdateUI("Ammo", WEAPON_TYPE.current_ammo + " / " + WEAPON_TYPE.magazine_capacity);

            // Await and completion
            //await ToSignal(GetTree().CreateTimer(WEAPON_TYPE.pullout_time), "timeout"); // causing godot to crash!
            PulloutTimer.Start();
        }
        else
        {
            //Globals.debug?.debug_err("WEAPON_TYPE is null. Cannot load weapon.");
        }
    }

    private void _on_pullout_timer_timeout()
    {
        currently_loading_weapon = false;
        can_shoot = true;

        //Globals.debug.debug_message("Ending Weapon Position: " + Position);
        //Globals.debug.debug_message("Ending Weapon Rotation: " + Rotation);
    }

    private async void StartPulloutAnimation()
    {
        float pulloutDuration = 1.0f; // Duration of the animation in seconds
        float elapsedTime = 0.0f;     // Track the elapsed time

        // Set starting position and rotation (adjusted)
        Vector3 startPosition = WEAPON_TYPE.position;
        Vector3 startRotation = WEAPON_TYPE.rotation;

        startPosition.Y -= 1f;      // Start from below the final position
        startRotation.X -= 100.0f;     // Tilt the weapon up at the beginning

        // Final position and rotation (target values)
        Vector3 endPosition = WEAPON_TYPE.position;
        Vector3 endRotation = WEAPON_TYPE.rotation;

        //Globals.debug.debug_message("Original Weapon Position: " + startPosition);
        //Globals.debug.debug_message("Original Weapon Rotation: " + startRotation);

        while (elapsedTime < pulloutDuration)
        {
            if (player.player_info.loadout[current_loadout_index] == "Hand") break;
            // Calculate the interpolation factor (0 to 1)
            float t = elapsedTime / pulloutDuration;

            // Lerp the position and rotation
            Vector3 currentPosition = startPosition.Lerp(endPosition, t);
            Vector3 currentRotation = startRotation.Lerp(endRotation, t);

            // Apply the interpolated values to the weapon
            Position = currentPosition;
            RotationDegrees = currentRotation;

            // Increment elapsed time based on frame time
            elapsedTime += (float)GetProcessDeltaTime();

            // Wait until the next frame to continue the loop
            await ToSignal(GetTree(), "process_frame");
        }

        // Ensure the weapon reaches the final position and rotation
        Position = endPosition;
        RotationDegrees = endRotation;

        //Globals.debug.debug_message("Final Weapon Position: " + Position);
        //Globals.debug.debug_message("Final Weapon Rotation: " + Rotation);
    }


    public void sway_weapon(float delta, bool isIdle)
    {
        if (Engine.IsEditorHint() || WEAPON_TYPE == null) return;

        // Clamp Mouse Movement
        mouse_movement = mouse_movement.Clamp(WEAPON_TYPE.sway_min, WEAPON_TYPE.sway_max);

        if(isIdle)
        {
            // Player Movement Sway
            float sway_random = get_sway_noise();
            float sway_random_adjusted = sway_random * idle_sway_adjustment; // Adjust sway strength

            time += delta * (sway_speed + sway_random);
            random_sway_x = Mathf.Sin(time * 1.5f + sway_random_adjusted) / random_sway_amount;
            random_sway_y = Mathf.Sin(time - sway_random_adjusted) / random_sway_amount;

            // Mouse Movement Sway

            // Get current position and modify it
            Vector3 position = Position;
            position.X = Mathf.Lerp(position.X, WEAPON_TYPE.position.X - (mouse_movement.X * WEAPON_TYPE.sway_amount_position + random_sway_x)
            * delta, WEAPON_TYPE.sway_speed_position);
            position.Y = Mathf.Lerp(position.Y, WEAPON_TYPE.position.Y + (mouse_movement.Y * WEAPON_TYPE.sway_amount_position + random_sway_y)
            * delta, WEAPON_TYPE.sway_speed_position);
            Position = position;  // Assign modified position back to the property

            // Get current rotation and modify it
            Vector3 rotationDegrees = RotationDegrees;
            rotationDegrees.Y = Mathf.Lerp(rotationDegrees.Y, WEAPON_TYPE.rotation.Y + (mouse_movement.X * WEAPON_TYPE.sway_amount_rotation + 
            (random_sway_y * idle_sway_rotation_strength)) * delta, WEAPON_TYPE.sway_speed_rotation);
            rotationDegrees.X = Mathf.Lerp(rotationDegrees.X, WEAPON_TYPE.rotation.X - (mouse_movement.Y * WEAPON_TYPE.sway_amount_rotation + 
            (random_sway_x * idle_sway_rotation_strength)) * delta, WEAPON_TYPE.sway_speed_rotation);
            RotationDegrees = rotationDegrees;  // Assign modified rotation back to the property
        }
        else
        {
            // Get current position and modify it
            Vector3 position = Position;
            position.X = Mathf.Lerp(position.X, WEAPON_TYPE.position.X - (mouse_movement.X * WEAPON_TYPE.sway_amount_position + weapon_bob_amount.X)
            * delta, WEAPON_TYPE.sway_speed_position);
            position.Y = Mathf.Lerp(position.Y, WEAPON_TYPE.position.Y + (mouse_movement.Y * WEAPON_TYPE.sway_amount_position + weapon_bob_amount.Y)
            * delta, WEAPON_TYPE.sway_speed_position);
            Position = position;  // Assign modified position back to the property

            // Get current rotation and modify it
            Vector3 rotationDegrees = RotationDegrees;
            rotationDegrees.Y = Mathf.Lerp(rotationDegrees.Y, WEAPON_TYPE.rotation.Y + (mouse_movement.X * WEAPON_TYPE.sway_amount_rotation) 
            * delta, WEAPON_TYPE.sway_speed_rotation);
            rotationDegrees.X = Mathf.Lerp(rotationDegrees.X, WEAPON_TYPE.rotation.X - (mouse_movement.Y * WEAPON_TYPE.sway_amount_rotation) 
            * delta, WEAPON_TYPE.sway_speed_rotation);
            RotationDegrees = rotationDegrees;  // Assign modified rotation back to the property
        }
    }

    public void _weapon_bob(float delta, float bob_speed, float bhob_amount, float vbob_amount)
    {
        if (WEAPON_TYPE == null) return;
        time += delta;
        weapon_bob_amount.X = Mathf.Sin(time * bob_speed) * bhob_amount;
        weapon_bob_amount.Y = Mathf.Abs(Mathf.Cos(time * bob_speed) * vbob_amount);
    }

    private float get_sway_noise()
    {
        Vector3 player_position = Vector3.Zero;

        if (!Engine.IsEditorHint())
        {
            player_position = player.GlobalPosition;
        }

        return sway_noise.Noise.GetNoise2D(player_position.X, player_position.Y);
    }

    public void _visual_weapon_fire()
    {
        weapon_recoil_physics.add_recoil();
        muzzle_flash.add_muzzle_flash();
    }

    public void _attack(float delta)
    {
        //raycast_offset = Vector3.Zero;
        if (WEAPON_TYPE != null && WEAPON_TYPE.weapon_type == Weapons.WeaponType.Gun && can_shoot && WEAPON_TYPE.current_ammo > 0)
        {
            player.playerNetworkingCalls.Shoot(player);

            WEAPON_TYPE.current_ammo --;
            player.player_user_interface?.playerUI().UpdateUI("Ammo", WEAPON_TYPE.current_ammo + " / " + WEAPON_TYPE.magazine_capacity);

            shooting = true;
            can_shoot = false;

            shoot_timer.Start();

            //Sound
            //WEAPON_TYPE.FireSound.Play();

            if (player.weapon_raycast_result.ContainsKey("position") && player.weapon_raycast_result.ContainsKey("normal"))
            {
                if (Multiplayer.IsServer())
                {
                    networked_bullet_hole((Vector3)player.weapon_raycast_result["position"], (Vector3)player.weapon_raycast_result["normal"]);
                    networked_bullet_linecast(GlobalTransform, muzzle_flash.GlobalTransform.Origin, (Vector3)player.weapon_raycast_result["position"]);
                }
                else
                {
                    RpcId(1, nameof(networked_bullet_hole), (Vector3)player.weapon_raycast_result["position"], (Vector3)player.weapon_raycast_result["normal"]);
                    RpcId(1, nameof(networked_bullet_linecast), GlobalTransform, muzzle_flash.GlobalTransform.Origin, (Vector3)player.weapon_raycast_result["position"]);
                }
            }
            else
            {
                if (Multiplayer.IsServer())
                {
                    networked_bullet_linecast(GlobalTransform, muzzle_flash.GlobalTransform.Origin, player.raycast_bullet_end);
                }
                else
                {
                    RpcId(1, nameof(networked_bullet_linecast), GlobalTransform, muzzle_flash.GlobalTransform.Origin, player.raycast_bullet_end);
                }
            }

            if (player.weapon_raycast_result.ContainsKey("collider"))
            {
                var collider = (Node)player.weapon_raycast_result["collider"]; //set it to collider if there is 
                if (collider.HasMethod("Damage"))
                {
                    if (collider is Player)
                    {
                        Player hit_player = (Player)collider;
                        if(hit_player != player)
                        {
                            hit_player.Damage((int)WEAPON_TYPE.base_damage, player.player_info);
                        }
                    }
                }
            }

            add_recoil_offset(delta);
            add_weapon_innacuracy(delta);
        }
    }

    private void add_weapon_innacuracy(float delta)
    {
        // Multiplying player velocity by 15 for a dramatic effect to bullet spread when moving
        float player_movement_spread_multiplier = 15.0f;

        recoil_innacuracy_spread.X = GetRandomFloatInRange(-WEAPON_TYPE.bullet_spread, WEAPON_TYPE.bullet_spread);
        recoil_innacuracy_spread.Y = GetRandomFloatInRange(-WEAPON_TYPE.bullet_spread, WEAPON_TYPE.bullet_spread);

        // Cap the innacuracy to 50
        recoil_innacuracy_spread *= Mathf.Min(50, Mathf.Max(1, player.Velocity.Length() * player_movement_spread_multiplier));

        //Globals.debug.update_debug_property("Recoil Innacuracy", recoil_innacuracy_spread.Length());
    }

    private void add_recoil_offset(float delta)
    {
        // Adding gun recoil
        recoil_offset.Y += WEAPON_TYPE.recoil_amount_rotation.Y;
        // Currently Random Left and Right Movement
		recoil_offset.X += GetRandomFloatInRange(-WEAPON_TYPE.recoil_amount_rotation.X, WEAPON_TYPE.recoil_amount_rotation.X);
    }

    private void ResetRecoilOffset(float delta) 
    {
        if (WEAPON_TYPE == null) return;
        float recoil_reset_speed_amplifier = 1.0f;
        if (!shooting) recoil_reset_speed_amplifier = WEAPON_TYPE.recoil_reset_speed_amplifier;

        recoil_reset_lerp_factor = WEAPON_TYPE.recoil_reset_speed * recoil_reset_speed_amplifier;

        if (player.stateMachine.CURRENT_STATE.Name == "CrouchingPlayerState") recoil_reset_lerp_factor *= 2;

        //Globals.debug.update_debug_property("Lerp Factor", recoil_reset_lerp_factor);
        
        // Interpolate to 0,0,0 slowly, when not shooting we amplifiy
        recoil_offset = recoil_offset.Lerp(Vector3.Zero, WEAPON_TYPE.recoil_follow_speed * recoil_reset_lerp_factor * delta);
    }

    public async void _reload()
    {
        if (WEAPON_TYPE != null && !reloading && WEAPON_TYPE.current_ammo != WEAPON_TYPE.magazine_capacity)
        {
            reloading = true;
            can_shoot = false;
            await ToSignal(GetTree().CreateTimer(WEAPON_TYPE.reload_time), "timeout");
            WEAPON_TYPE.current_ammo = WEAPON_TYPE.magazine_capacity;

            reloading = false;
            can_shoot = true;
            player.player_user_interface?.playerUI().UpdateUI("Ammo", WEAPON_TYPE.current_ammo + " / " + WEAPON_TYPE.magazine_capacity);
        }
    }

    public void _on_shoot_timer_timeout()
    {
        can_shoot = true;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void networked_bullet_hole(Vector3 _pos, Vector3 _normal)
    {
        if (Multiplayer.IsServer())
        {
            Rpc(nameof(networked_bullet_hole), _pos, _normal);
        }
        Node3D instance = raycast_bullet_hole.Instantiate<Node3D>();
        GetTree().Root.AddChild(instance);
        instance.GlobalPosition = _pos; 
        instance.LookAt(instance.GlobalTransform.Origin + _normal, Vector3.Up);
        if (_normal != Vector3.Up && _normal != Vector3.Down)
        {
            instance.RotateObjectLocal(new Vector3(1, 0, 0), 90);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void networked_bullet_linecast(Transform3D playerGlobalTransform, Vector3 muzzleFlashOrigin, Vector3 target_position)
    {
        if (Multiplayer.IsServer())
        {
            Rpc(nameof(networked_bullet_linecast), playerGlobalTransform, muzzleFlashOrigin, target_position);
        }

        BulletLineCast bullet = raycast_bullet_linecast.Instantiate<BulletLineCast>();
        GetTree().Root.AddChild(bullet);

        // Align the bullet's rotation with the player's rotation
        bullet.GlobalTransform = playerGlobalTransform;  // Copy the player's global transform to the bullet

        // Calculate the forward direction from the player's transform
        Vector3 forwardDirection = -playerGlobalTransform.Basis.Z;  // Forward direction (negative Z axis)

        // Get the muzzle flash position relative to the world
        Vector3 muzzleFlashPositionWorld = muzzleFlashOrigin;

        // Add a small offset to the muzzle flash position in the forward direction
        float offsetDistance = 0.5f;  // Adjust this value based on how far ahead of the muzzle_flash you want the bullet to start
        Vector3 startPosition = muzzleFlashPositionWorld + forwardDirection * offsetDistance;  // Start just ahead of the muzzle flash

        // Set the start and target positions
        bullet.SetTarget(startPosition, target_position);  // Pass start and target positions
    }



    private float GetRandomFloatInRange(float min, float max)
    {
        return (float)(min + random.NextDouble() * (max - min));
    }
}

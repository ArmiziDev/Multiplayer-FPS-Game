using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public partial class PlayerUI : CanvasLayer
{
    private Dictionary<StringName, Control> _uiElements;
    private Panel scoreboard;
    private TextureRect bloodSplatter;
    private AnimationPlayer animationPlayer;
    private Control buyMenu;
    private Control buyMenuWeaponInfo;
    private Control escMenu;

    private VBoxContainer PlayerKillUIContainer;
    [Export] private PackedScene player_kill_ui;

    public override void _Ready()
    { 
        // Initialize the dictionary to hold UI elements
        _uiElements = new Dictionary<StringName, Control>();

        //Set Animation Player
        animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");

        // Get The Blood Splatter
        bloodSplatter = GetNode<TextureRect>("%BloodSplatter");

        // Set Kill UI Container
        PlayerKillUIContainer = GetNode<VBoxContainer>("PlayerKillUIContainer");

        // Add UI Elements
        AddUIElement("Health", GetNode<Label>("%Health"));
        AddUIElement("Ammo", GetNode<Label>("%Ammo"));
        AddUIElement("Interact", GetNode<Label>("%Interact"));
        AddUIElement("Money", GetNode<Label>("%Money"));
        AddUIElement("DisplayName", GetNode<Label>("%DisplayName"));
        GetUIElement("Interact").Visible = false;

        // Loadout
        AddUIElement("Loadout1", GetNode<Label>("%Loadout1"));
        AddUIElement("Loadout2", GetNode<Label>("%Loadout2"));
        AddUIElement("Loadout3", GetNode<Label>("%Loadout3"));

        // Score
        AddUIElement("RedTeamScore", GetNode<Label>("%RedTeamScore"));
        AddUIElement("BlueTeamScore", GetNode<Label>("%BlueTeamScore"));

        // Time Display
        AddUIElement("RoundTimeDisplay", GetNode<Label>("%RoundTimeDisplay"));

        scoreboard = GetNode<Panel>("%Scoreboard");
        scoreboard.Visible = false;

        buyMenu = GetNode<Control>("%BuyMenu");
        buyMenuWeaponInfo = buyMenu.GetNode<Control>("WeaponInfo");
        escMenu = GetNode<Control>("%EscMenu");
        escMenu.Visible = false;
        buyMenuWeaponInfo.Visible = false;
        buyMenu.Visible = false;

        escMenu.GetNode<HSlider>("%SensitivitySlider").GetNode<Label>("Value").Text = Globals.localPlayer.MouseSensitivity.ToString();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("scoreboard"))
        {
            scoreboard.Visible = true;
            switch (Globals.game_mode)
            {
                case 0: // 5v5s
                    show_redvsblue_scoreboard();
                    break;
                default:
                    show_regular_scoreboard();
                    break;
            }
        }
        if (@event.IsActionReleased("scoreboard"))
        {
            scoreboard.Visible = false;
        }
    }

    public void buy_menu()
    {
        if (!buyMenu.Visible && !escMenu.Visible)
        {
            buyMenu.Visible = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else if (escMenu.Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else
        {
            buyMenu.Visible = false;
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public void esc_menu()
    {
        escMenu.Visible = !escMenu.Visible;
        if (escMenu.Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            buyMenu.Visible = false;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public bool is_buymenu_open() { return buyMenu.Visible; }

    public void show_regular_scoreboard()
    {
        int current_player_count = 1;
        if (scoreboard.Visible)
        {
            foreach (PlayerInfo player in Globals.PLAYERS)
            {
                Label current_player_label = GetNode<Label>("%Player" + current_player_count.ToString());
                current_player_label.Text = player.Name + "           " + player.kills + "   /   " + player.deaths;

                current_player_count++;
            }
        }
    }

    public void show_redvsblue_scoreboard()
    {
        int redplayercount = 1;
        int blueplayercount = 6;
        if (scoreboard.Visible)
        {
            foreach (PlayerInfo player in Globals.PLAYERS)
            {
                if (player.player_team == Team.Red)
                {
                    Label current_player_label = GetNode<Label>("%Player" + redplayercount.ToString());
                    current_player_label.Text = player.Name + "           " + player.kills + "   /   " + player.deaths;

                    redplayercount++;
                }
                if (player.player_team == Team.Blue)
                {
                    Label current_player_label = GetNode<Label>("%Player" + blueplayercount.ToString());
                    current_player_label.Text = player.kills + "   /   " + player.deaths + "           " + player.Name;

                    blueplayercount++;
                }
            }
        }
    }

    // General method to update any UI element
    public void UpdateUI(StringName elementName, object value)
    {
        if (_uiElements.ContainsKey(elementName))
        {
            Control element = _uiElements[elementName];
            
            switch (element)
            {
                case Label label:
                    label.Text = $"{value}";
                    break;
                case TextureRect textureRect:
                    if (value is Texture2D texture)
                    {
                        textureRect.Texture = texture;
                    }
                    break;
                case ProgressBar progressBar:
                    if (value is float progress)
                    {
                        progressBar.Value = progress;
                    }
                    break;
                // Add more cases for different UI elements as needed
                default:
                    Globals.PlayerUI.debug().debug_err($"Unsupported UI element type for {elementName}.");
                    break;
            }
        }
        else
        {
            Globals.PlayerUI.debug().debug_err($"UI element {elementName} not found.");
        }
    }

    // Method to add new UI elements dynamically
    public void AddUIElement(StringName elementName, Control element)
    {
        if (element != null && !_uiElements.ContainsKey(elementName))
        {
            _uiElements.Add(elementName, element);
        }
    }
    // Method to get a UI element by its name
    public Control GetUIElement(StringName elementName)
    {
        if (_uiElements.ContainsKey(elementName))
        {
            return _uiElements[elementName];
        }
        else
        {
            //Globals.debug.debug_err($"UI element {elementName} not found.");
            return null;
        }
    }

    public void AddPlayerKill(PlayerInfo Killer, PlayerInfo Killed)
    {
        // Instantiate the player_kill_ui scene
        PlayerKillUi killUIInstance = (PlayerKillUi)player_kill_ui.Instantiate<Panel>();
        PlayerKillUIContainer.AddChild(killUIInstance);

        killUIInstance.SetPlayerKillUI(Killer, Killed);
    }

    public void ShowBloodSplatter()
    {
        animationPlayer.Play("BloodSplatter");
    }

    private void SetWeaponInfo(StringName weapon_name)
    {
        Weapons current_weapon = Globals.weaponDictionary[weapon_name];
        buyMenuWeaponInfo.GetNode<Label>("%WeaponName").Text = current_weapon.name;
        buyMenuWeaponInfo.GetNode<Label>("%WeaponDamage").Text = "Damage: " + current_weapon.base_damage;
        buyMenuWeaponInfo.GetNode<Label>("%WeaponAmmo").Text = "Ammo: " + current_weapon.magazine_capacity + " / " + current_weapon.magazine_capacity;
        buyMenuWeaponInfo.GetNode<Label>("%WeaponCost").Text = "$" + current_weapon.cost;
        buyMenuWeaponInfo.GetNode<Label>("%WeaponFireRate").Text = Math.Round(1.0f / current_weapon.time_between_bullets, 2) + " BPS";
        buyMenuWeaponInfo.GetNode<Label>("%WeaponRecoil").Text = "Recoil: " + Mathf.Round((current_weapon.recoil_amount_rotation.Y / 50.0f) * 100.0f) + "%";

        StringName weapon_class = "";

        switch (current_weapon.gun_class)
        {
            case Weapons.GunClass.Rifle:
                weapon_class = "Rifle";
                break;
            case Weapons.GunClass.Pistol:
                weapon_class = "Pistol";
                break;
            case Weapons.GunClass.SMG:
                weapon_class = "SMG";
                break;
            case Weapons.GunClass.Shotgun:
                weapon_class = "Shotgun";
                break;
        }

        buyMenuWeaponInfo.GetNode<Label>("%WeaponClass").Text = weapon_class;
    }

    private void on_buy_button(StringName weapon_name)
    {
        WeaponButtonPressed(weapon_name);
    }

    private void on_button_mouse_entered(StringName weapon_name)
    {
        buyMenuWeaponInfo.Visible = true;
        SetWeaponInfo(weapon_name);
    }

    private void on_button_mouse_exited(StringName weapon_name)
    {
        buyMenuWeaponInfo.Visible = false;
    }

    public void WeaponButtonPressed(StringName weapon_name)
    {
        Weapons bought_weapon = Globals.weaponDictionary[weapon_name];
        if (Globals.localPlayerInfo.money >= bought_weapon.cost)
        {
            Globals.localPlayerInfo.money -= bought_weapon.cost;
            Globals.PlayerUI.playerUI().UpdateUI("Money", "$" + Globals.localPlayerInfo.money);

            Globals.localPlayer.playerNetworkingCalls.BuyWeapon(Globals.localPlayer, weapon_name);
        }
        else
        {
            Globals.PlayerUI.debug().debug_err("Player can't afford " + weapon_name);
        }
    }

    private void _on_sensitivity_slider_value_changed(float value)
    {
        escMenu.GetNode<HSlider>("%SensitivitySlider").GetNode<Label>("Value").Text = value.ToString();

        Globals.localPlayer.MouseSensitivity = value;
    }

    private void _on_quit_game_button_pressed()
    {
        GetTree().Quit();
    }
}
 
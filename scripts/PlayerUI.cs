using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerUI : CanvasLayer
{
    private Dictionary<StringName, Control> _uiElements;
    private Panel scoreboard;

    public override void _Ready()
    {
        // Initialize the dictionary to hold UI elements
        _uiElements = new Dictionary<StringName, Control>();

        // Example: Adding a health label to the UI elements
        AddUIElement("Health", GetNode<Label>("%Health"));
        AddUIElement("Ammo", GetNode<Label>("%Ammo"));
        AddUIElement("Interact", GetNode<Label>("%Interact"));
        AddUIElement("Money", GetNode<Label>("%Money"));
        GetUIElement("Interact").Visible = false;

        // Loadout
        AddUIElement("Loadout1", GetNode<Label>("%Loadout1"));
        AddUIElement("Loadout2", GetNode<Label>("%Loadout2"));
        AddUIElement("Loadout3", GetNode<Label>("%Loadout3"));

        scoreboard = GetNode<Panel>("%Scoreboard");
        scoreboard.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("scoreboard"))
        {
            scoreboard.Visible = true;
            int redplayercount = 1;
            int blueplayercount = 1;
            if (scoreboard.Visible)
            {
                foreach(var player in Globals.PLAYERS)
                {
                    if(player.player_team == Team.Red)
                    {
                        GetNode<Label>("%Player" + redplayercount.ToString()).Text = player.Name;
                        redplayercount++;
                    }
                    if(player.player_team == Team.Blue)
                    {
                        GetNode<Label>("%Player" + blueplayercount.ToString()).Text = player.Name;
                        blueplayercount++;                        
                    }
                }
            }
        }
        if (@event.IsActionReleased("scoreboard"))
        {
            scoreboard.Visible = false;
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
                    Globals.debug.debug_err($"Unsupported UI element type for {elementName}.");
                    break;
            }
        }
        else
        {
            Globals.debug.debug_err($"UI element {elementName} not found.");
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
            Globals.debug.debug_err($"UI element {elementName} not found.");
            return null;
        }
    }
}

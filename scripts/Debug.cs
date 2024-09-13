using Godot;
using System;

public partial class Debug : PanelContainer
{
    [Export] VBoxContainer vBoxContainer;
    private Godot.Collections.Array<Label> debugLabels = new Godot.Collections.Array<Label>();

    public override void _Ready()
    {
        //Sets global reference to self in Globals Singleton
        Globals.debug = this;

        // Setting Initial Visibility To False
        Visible = true;

        // Add debug properties
        add_debug_property("FPS", 0);  // Initialize FPS as 0, will be updated later
    }

    public override void _Process(double delta)
    {
        if(Visible)
        {
            int fps = (int)Engine.GetFramesPerSecond();
            update_debug_property("FPS", fps);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("debug"))
        {
            Visible = !Visible;
        }
    }

    public void add_debug_property(string title, object value)
    {
        Label property = new Label();
        vBoxContainer.AddChild(property);
        property.Name = title;
        property.Text = $"{title}: {value}";

        // Add the label to the array for easy access
        debugLabels.Add(property);
    }

    public void update_debug_property(string title, object value)
    {
        bool is_in_debug = false;
        foreach (Label label in debugLabels)
        {
            if (label.Name == title)
            {
                is_in_debug = true;
                label.Text = $"{title}: {value}";
                break;
            }
        }
        if (!is_in_debug)
        {
            add_debug_property(title, value);
            update_debug_property(title, value);
        }
    }

    // New method to add a temporary red message
    public async void debug_err(string message, float duration = 3.0f)
    {
        GD.PrintErr(message);
        Label tempMessage = new Label();
        tempMessage.Text = message;
        tempMessage.Modulate = new Color(1, 0, 0);  // Set the text color to red
        vBoxContainer.AddChild(tempMessage);

        // Wait for the duration before removing the message
        await ToSignal(GetTree().CreateTimer(duration), "timeout");

        // Remove the message from the panel and free its memory
        vBoxContainer.RemoveChild(tempMessage);
        tempMessage.QueueFree();
    }

    public async void debug_message(string message, float duration = 3.0f)
    {
        //GD.PrintErr(message);
        Label tempMessage = new Label();
        tempMessage.Text = message;
        //tempMessage.Modulate = new Color(1, 0, 0);  // Set the text color to red
        vBoxContainer.AddChild(tempMessage);

        // Wait for the duration before removing the message
        await ToSignal(GetTree().CreateTimer(duration), "timeout");

        // Remove the message from the panel and free its memory
        vBoxContainer.RemoveChild(tempMessage);
        tempMessage.QueueFree();
    }


}

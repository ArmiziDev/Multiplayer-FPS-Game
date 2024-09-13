using Godot;
using Microsoft.VisualBasic;
using System;
using System.Reflection;

public partial class InteractionComponent : Node
{
	public enum InteractableType
	{
		Pickup,
		Lootable,
		Door,
		Damageable,
		Trigger,
		Custom
	}

	[Export] public InteractableType interactableType;
	[Export] public MeshInstance3D mesh;

	BaseMaterial3D highlight_material;
	public Node parent;
	[Signal] public delegate void InteractableComponentEventHandler();
	[Signal] public delegate void InRangeEventHandler();
    [Signal] public delegate void NotInRangeEventHandler();
    [Signal] public delegate void OnInteractEventHandler();
	[Signal] public delegate void PickupEventHandler();
	[Signal] public delegate void LootableEventHandler();
	[Signal] public delegate void TriggerEventHandler();
	[Signal] public delegate void CustomEventHandler();

	public override void _Ready()
	{
		parent = GetParent();
		parent.Connect("ready", new Callable(this, nameof(connect_parent)));
	}

    public void in_range()
    {
        mesh.MaterialOverlay = highlight_material;
    }

    public void not_in_range()
    {
        mesh.MaterialOverlay = null;
    }

	public void OnLoot()
	{
		Globals.debug.debug_message(parent.Name + ": Loot Triggered");
	}

	public void OnPickup()
	{
		MethodInfo pickupMethod = parent.GetType().GetMethod("Pickup");

		if (pickupMethod != null)
		{
			pickupMethod.Invoke(parent, null);
		}
		else
		{
			Globals.debug.debug_err("Parent Has No Pickup Function");
		}
	}

	public void OnTrigger()
	{
		Globals.debug.debug_message(parent.Name + ": Trigger Event Triggered");
	}

    public void on_interact()
    {
        Globals.debug.debug_message("On interact:: " + parent.Name);

		switch (interactableType)
		{
			case InteractableType.Pickup:
				OnPickup();
				//EmitSignal(InteractionComponent.SignalName.Pickup); // doesnt work
				break;
			case InteractableType.Lootable:
				OnLoot();
				//EmitSignal(InteractionComponent.SignalName.Lootable);
				break;
			case InteractableType.Trigger:
				OnTrigger();
				//EmitSignal(InteractionComponent.SignalName.Trigger);
				break;
		}
    }

	private void die()
	{
		parent.QueueFree();
	}

	public void set_default_mesh()
	{
		mesh = FindMeshInstance3D(parent);

		if (mesh == null)
		{
			GD.PrintErr(parent.Name + ": No MeshInstance3D found in parent node's children.");
		}
	}

	private MeshInstance3D FindMeshInstance3D(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is MeshInstance3D)
			{
				//Globals.debug.debug_message(parent.Name+ " Found MeshInstance3D: " + child.Name);
				return (MeshInstance3D)child;
			}

			// Recursively search in child nodes
			MeshInstance3D foundMesh = FindMeshInstance3D(child);
			if (foundMesh != null)
			{
				return foundMesh;
			}
		}
		return null;
	}

	public void connect_parent()
	{
		// Every Interactable has interactablecomponent Signal
		parent.AddUserSignal(InteractionComponent.SignalName.InteractableComponent);

		// Only Interactables that you need to interact with with E will need these signals
		if (interactableType == InteractableType.Pickup || 
			interactableType == InteractableType.Lootable)
		{
			parent.AddUserSignal(InteractionComponent.SignalName.InRange);
			parent.AddUserSignal(InteractionComponent.SignalName.NotInRange);
			parent.AddUserSignal(InteractionComponent.SignalName.OnInteract);

			parent.Connect(InteractionComponent.SignalName.InRange, new Callable(this, nameof(in_range)));
			parent.Connect(InteractionComponent.SignalName.NotInRange, new Callable(this, nameof(not_in_range)));
			parent.Connect(InteractionComponent.SignalName.OnInteract, new Callable(this, nameof(on_interact)));
		}

		// Now we set individual signals
		if(interactableType == InteractableType.Pickup)
		{
			parent.AddUserSignal(InteractionComponent.SignalName.Pickup);
			parent.Connect(InteractionComponent.SignalName.Pickup, new Callable(this, nameof(OnPickup)));
		}
		else if (interactableType == InteractableType.Lootable)
		{
			parent.AddUserSignal(InteractionComponent.SignalName.Lootable);
			parent.Connect(InteractionComponent.SignalName.Lootable, new Callable(this, nameof(OnLoot)));
		}
		else if (interactableType == InteractableType.Trigger)
		{
			parent.AddUserSignal(InteractionComponent.SignalName.Trigger);
			parent.Connect(InteractionComponent.SignalName.Trigger, new Callable(this, nameof(OnTrigger)));
		}

		// Find the mesh the parent is using if we haven't manually chosen one for highlighting
		if(mesh == null)
		{
			set_default_mesh();
		}

		highlight_material = (BaseMaterial3D)GD.Load("res://materials/interactable_highlight.tres");
	}
}

using Godot;
using System;

public partial class Main : Control
{
    private Node slotMachineScene;
    private Node blackJack;
    public override void _Ready() {
        slotMachineScene = ResourceLoader.Load<PackedScene>( "res://CatSlots/Control.tscn" ).Instantiate();
        blackJack = ResourceLoader.Load<PackedScene>( "res://BlackJack/BlackJack.tscn" ).Instantiate();
        
        GetNode<Button>( "HBoxContainer/Slots" ).Pressed += () => {
            switchScene( slotMachineScene );
        };
        GetNode<Button>( "HBoxContainer/BlackJack" ).Pressed += () => {
            switchScene( blackJack );
        };
    }
    private void switchScene( Node node ) {
        QueueFree();
        GetTree().Root.AddChild( node );
    }
}

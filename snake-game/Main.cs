using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Control
{
    public override void _Ready()
    {
        GetNode<Button>( "VBoxContainer/start" ).Pressed += () => {
            GetNode<Snake>( "snake" ).Start();
            GetNode<VBoxContainer>( "VBoxContainer" ).Visible = false;
        };
    }
    public void ShowButton( string text ) {
        GetNode<Label>( "VBoxContainer/Label" ).Text = text;
        GetNode<VBoxContainer>( "VBoxContainer" ).Visible = true;
    }
}
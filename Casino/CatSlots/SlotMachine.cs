using Godot;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
// Skibidi
public partial class SlotMachine : Control
{

    Label labelBalanceNum;
    int bet;
    bool spinStarted = false;
    float time = 0;
    int speed = 600;
    int[] score = [ 0, 0, 0 ];
    Userdata global;

    public override void _Ready()
    {
        labelBalanceNum = GetNode<Label>("Control/VBoxContainer/HBoxContainer3/LabelBalancenum");

        global = GetNode<Userdata>( "/root/Userdata" );
        global.readData();

        bet = bet > global.data.Balance ? global.data.Balance : 50;

        labelBalanceNum.Text = global.data.Balance.ToString();

        Button decreaseButton = GetNode<Button>( "Control/VBoxContainer/HBoxContainer2/MarginContainer/DecreaseBet" );
        Button increaseButton = GetNode<Button>( "Control/VBoxContainer/HBoxContainer2/MarginContainer2/IncreaseBet" );
        Button spinButton = GetNode<Button>( "Control/Button" );

        decreaseButton.Pressed += () => {
            if ( spinStarted ) return;
            changeBet( -10 );
        };
        increaseButton.Pressed += () => {
            if ( spinStarted ) return;
            changeBet( 10 );
        };

        spinButton.Pressed += () => {
            if ( spinStarted ) return;

            spinStarted = true;
            SetProcess( true );
        };

        GetNode<Label>( "Control/VBoxContainer/HBoxContainer2/LabelBetnum" ).Text = bet.ToString();

        SetProcess( false );

        Control panelOne = GetNode<Control>( "Control/VBoxContainer/HBoxContainer/Panel/Control" );
        Control panelTwo = GetNode<Control>( "Control/VBoxContainer/HBoxContainer/Panel2/Control" );
        Control panelThree = GetNode<Control>("Control/VBoxContainer/HBoxContainer/Panel3/Control");

        panelOne.Position = new Vector2( panelOne.Position.X, 128 * ( GD.Randi() % 7 ) );
        panelTwo.Position = new Vector2( panelTwo.Position.X, 128 * ( GD.Randi() % 7 ) );
        panelThree.Position = new Vector2( panelThree.Position.X, 128 * ( GD.Randi() % 7 ) );

        GetNode<Button>( "menu" ).Pressed += mainMenu;
    }   
    public override void _Process( double delta ) {
        CustomPanel panelOne = GetNode<CustomPanel>( "Control/VBoxContainer/HBoxContainer/Panel/Control" );
        CustomPanel panelTwo = GetNode<CustomPanel>( "Control/VBoxContainer/HBoxContainer/Panel2/Control" );
        CustomPanel panelThree = GetNode<CustomPanel>("Control/VBoxContainer/HBoxContainer/Panel3/Control");

        time += ( float ) delta * speed;

        if (time >= 0 && !panelOne.complete)
        {
            panelOne.Position = new Vector2(panelOne.Position.X, panelOne.Position.Y + (float)delta * speed);
            if (panelOne.Position.Y > 896) panelOne.Position = new Vector2(0, panelOne.Position.X);

            if (time >= 3000)
            {
                int randomNumber = (int)(GD.Randi() % 7);
                GD.Randomize(); // New

                panelOne.complete = true;
                panelOne.Position = new Vector2(panelOne.Position.X, 128 * randomNumber);

                score[0] = randomNumber;
            }
        }
        if ( time >= 300 && !panelTwo.complete ) {
            panelTwo.Position = new Vector2( panelTwo.Position.X, panelTwo.Position.Y + ( float ) delta * speed );
            if ( panelTwo.Position.Y > 896 ) panelTwo.Position = new Vector2( 0, panelTwo.Position.X ); 

            if ( time >= 3300 ) {
                int randomNumber = ( int ) ( GD.Randi() % 7 );
                GD.Randomize(); // New


                panelTwo.complete = true;
                panelTwo.Position = new Vector2( panelTwo.Position.X, 128 * randomNumber );

                score[ 1 ] = randomNumber;
            }
        }
        if ( time >= 600 && !panelThree.complete ) {
            panelThree.Position = new Vector2( panelThree.Position.X, panelThree.Position.Y + ( float ) delta * speed );
            if ( panelThree.Position.Y > 896 ) panelThree.Position = new Vector2( 0, panelThree.Position.X );


            if (time >= 3600)
            {
                int randomNumber = (int)(GD.Randi() % 7);
                GD.Randomize(); // New


                panelThree.Position = new Vector2(panelThree.Position.X, 128 * randomNumber);

                score[2] = randomNumber;

                SetProcess(false);
                panelOne.complete = false;
                panelTwo.complete = false;
                spinStarted = false;
                time = 0;

                int winning = 0;

                switch (score.Join(""))
                {
                    case "000":
                        winning = bet * 25;
                        break;
                    case "111":
                        winning = bet * 5;
                        break;
                    case "222":
                        winning = bet * 10;
                        break;
                    case "333":
                        winning = bet * 15;
                        break;
                    case "444":
                        winning = bet * 30;
                        break;
                    case "555":
                        winning = bet * 40;
                        break;
                    case "666":
                        winning = bet * 100;
                        break;
                    default:
                        int zeroScore = score.Select(num => num == 0 ? 1 : 0).Aggregate((a, b) => a + b);
                        if (zeroScore == 1) break;
                        if (zeroScore == 2)
                        {
                            winning = bet * 3;
                            break;
                        }

                        winning = -bet;
                        break;
                }

                GetNode<Label>("Control/Panel/VBoxContainer/HBoxContainer10/Label2").Text = (winning > 0 ? "+" : "") + winning.ToString();

                global.data.Balance += winning;

                labelBalanceNum.Text = global.data.Balance.ToString();

                global.writeData();

                bet = bet > global.data.Balance ? global.data.Balance : bet; 
                GetNode<Label>( "Control/VBoxContainer/HBoxContainer2/LabelBetnum" ).Text = bet.ToString();
            }
        }
    }
    public void changeBet( int value ) {
        bet = Math.Clamp( bet + value, 0, global.data.Balance );
        GetNode<Label>( "Control/VBoxContainer/HBoxContainer2/LabelBetnum" ).Text = bet.ToString();
    }
    private void mainMenu () {
        var main = ResourceLoader.Load<PackedScene>( "res://Main.tscn" ).Instantiate();

        QueueFree();
        GetTree().Root.AddChild( main );
    }
};


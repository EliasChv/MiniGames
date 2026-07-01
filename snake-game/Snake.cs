using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class Body {
    public Vector2 Location { get; set; }
    public Vector2 PreviousLocation { get; set; }
}

public partial class Snake : Control
{
    Vector2 BoardSize = new( 15, 15 );
    List<Body> SnakeBody = [];
    Vector2 Fruit = new();
    Vector2 ScreenSize = new();
    Vector2 Direction = Vector2.Zero;
    float time = 0;
    int tileSize = 32;
    [ Export ] Texture2D fruitSprite;

    public override void _Ready()
    {
        SnakeBody.Add( new() { Location = new( 3, 7 ) } );
        SnakeBody.Add( new() { Location = new( 2, 7 ) } );

        Fruit = new Vector2( 12, 7 );

        ScreenSize = DisplayServer.WindowGetSize();

        SetProcess( false );
    }

    public override void _Draw()
    {
        Vector2 center = ScreenSize / 2f - BoardSize * tileSize / 2f;

        for (int i = 0; i < BoardSize.X; i++) for (int j = 0; j < BoardSize.Y; j++)
            {
            DrawRect(new(center.X + i * tileSize, center.Y + j * tileSize, tileSize, tileSize), (i + j) % 2 == 0 ? new(0.41f, 0.41f, 0.41f) : new(0.25f, 0.25f, 0.25f));
            //DrawRect(new(center.X + i * tileSize, center.Y + j * tileSize, tileSize, tileSize), (i + j) % 2 == 0 ? new(0.22f, 0.59f, 0.20f) : new(0.13f, 0.30f, 0.13f));
            }

        foreach (Body body in SnakeBody)
        {
            DrawRect(new(center + body.Location * tileSize, tileSize, tileSize), new(1f, 1f, 1f));
            //DrawRect( new( center + body.Location * tileSize, tileSize, tileSize ), new(0f, 0f, 1f) );

        }

        //DrawTextureRect( fruitSprite, new( center + Fruit * tileSize, tileSize, tileSize ), true );
        DrawCircle( center + Fruit * tileSize + Vector2.One * tileSize / 2, tileSize / 2 * 0.75f, new(1f, 1f, 1f), true );
    }

    public override void _Process(double delta)
    {   
        time += ( float ) delta;

        if ( time > 0.25 && Direction != Vector2.Zero ) {
            time = 0;

            Vector2 UpdatedLocation = SnakeBody[ 0 ].Location + Direction;

            // foreach ( Body body in SnakeBody ) {
            //     if ( UpdatedLocation == body.Location ) {
            //         GetParent<Main>().ShowButton( "You hit yourself fucken dumbass" );
            //         SetProcess( false );
            //         return;
            //     }
            // }
    
            if ( UpdatedLocation.X < 0 || UpdatedLocation.X > ( BoardSize.X - 1 ) || UpdatedLocation.Y < 0 || UpdatedLocation.Y > ( BoardSize.Y - 1 ) ) {
                GetParent<Main>().ShowButton( "Game Over" );
                SetProcess( false );
                return;
            }

            SnakeBody[ 0 ].PreviousLocation = SnakeBody[ 0 ].Location;
            SnakeBody[ 0 ].Location = SnakeBody[ 0 ].Location + Direction;

            for ( int i = 0; i < SnakeBody.Count; i++ ) {
                if ( i == 0 ) continue;

                if ( SnakeBody[ i ].Location != SnakeBody[ i - 1 ].Location ) {
                    SnakeBody[ i ].PreviousLocation = SnakeBody[ i ].Location;
                    SnakeBody[ i ].Location = SnakeBody[ i - 1 ].PreviousLocation;
                }
            }

            if ( SnakeBody[ 0 ].Location == Fruit ) {
                SnakeBody.Add( new() { Location = SnakeBody[ SnakeBody.Count - 1 ].Location } );
                Fruit = new( GD.Randi() % BoardSize.X, GD.Randi() % BoardSize.Y );
            }
        }

        QueueRedraw();
    }

    public override void _Input(InputEvent @event)
    {
        if ( @event is InputEventKey keyEvent ) {
            switch ( keyEvent.Keycode ) {
                case Key.W:
                case Key.Up:
                    if ( Direction != Vector2.Down ) Direction = Vector2.Up;
                    break;
                case Key.A:
                case Key.Left:
                    if ( Direction != Vector2.Right ) Direction = Vector2.Left;
                    break;
                case Key.S:
                case Key.Down:
                    if ( Direction != Vector2.Up ) Direction = Vector2.Down;
                    break;
                case Key.D:
                case Key.Right:
                    if ( Direction != Vector2.Left ) Direction = Vector2.Right;
                    break;
            }
        }
    }

    public void Start() {
        SnakeBody = [];
        Fruit = new();
        Direction = Vector2.Zero;
        time = 0;

        SnakeBody.Add( new() { Location = new( 3, 7 ) } );
        SnakeBody.Add( new() { Location = new( 2, 7 ) } );

        Fruit = new Vector2( 12, 7 );

        SetProcess( true );
    }
}
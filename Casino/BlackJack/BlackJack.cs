using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class BlackJack : Control
{
    private class Card
    {
        public enum SymbolEnum { Spade, Clover, Heart, Diamond }
        public SymbolEnum Symbol { get; set; }
        public int Number { get; set; }
        public bool Hidden { get; set; }
        public Vector2 Position { get; set; }
        public int Value { get; set; }
        public Card setupCard( List<Card> cards, Vector2 position, bool hidden = false ) {
            int total = cards.Any() ? cards.Select( card => card.Value ).Aggregate( ( a, b ) => a + b ) : 0;
            int randomNumber = ( ( int ) ( GD.Randi() % 13 ) ) + 1;

            Hidden = hidden;
            Number = randomNumber;
            Symbol = ( Card.SymbolEnum ) ( GD.Randi() % 4 );
            Position = position;
            Value = randomNumber == 1 ? ( total > 10 ? 1 : 11 ) : randomNumber > 10 ? 10 : randomNumber;

            return this;
        }
    }
    private List<Card> DealerCards = new();
    private List<Card> UserCards = new();
    private bool Animating = true;
    private bool DealerAnimating = true;
    private bool FinishGame = false;
    [ Export ] public Texture2D background;
    [ Export ] public Texture2D cardback;
    [ Export ] public Texture2D[] symbolImages;
    [ Export ] public Font font;
    private Userdata global;
    private int bet;
    public override void _Ready() {
        global = GetNode<Userdata>("/root/Userdata");
        global.readData();

        bet = bet > global.data.Balance ? global.data.Balance : 50;

        GetNode<Label>("VBoxContainer2/HBoxContainer/Balancevalue").Text = global.data.Balance.ToString();
        GetNode<Label>("VBoxContainer2/HBoxContainer2/Betvalue").Text = bet.ToString();

        DealerCards.Add(new Card().setupCard(DealerCards, new(970, 70)));
        DealerCards.Add(new Card().setupCard(DealerCards, new(970, 70), true));

        UserCards.Add(new Card().setupCard(UserCards, new(102, 450)));
        UserCards.Add(new Card().setupCard(UserCards, new(102, 450)));

        GetNode<Label>("VBoxContainer/HBoxContainer2/DealerTot").Text = DealerCards.Select(card => card.Hidden ? 0 : card.Value).Aggregate((a, b) => a + b).ToString();
        GetNode<Label>("VBoxContainer/HBoxContainer3/UserTot").Text = UserCards.Select(card => card.Value).Aggregate((a, b) => a + b).ToString();

        GetNode<Button>("VBoxContainer/HBoxContainer/Hit").Pressed += () => {
            if (Animating) return;

            Animating = true;

            UserCards.Add(new Card().setupCard(UserCards, new(102, 450)));

            if (UserCards.Select(card => card.Value).Aggregate((a, b) => a + b) > 21) {
                Animating = false;
                endGame();
            }

            GetNode<Label>("VBoxContainer/HBoxContainer3/UserTot").Text = UserCards.Select(card => card.Value).Aggregate((a, b) => a + b).ToString();
        };
        GetNode<Button>("VBoxContainer/HBoxContainer/Stand").Pressed += () => { endGame(); };

        GetNode<Button>("VBoxContainer2/HBoxContainer3/Decrease").Pressed += () => {
            changeBet(-10);
        };
        GetNode<Button>("VBoxContainer2/HBoxContainer3/Increase").Pressed += () => {
            changeBet(10);
        };

        GetNode<Button>("Panel3/VBoxContainer/Menu").Pressed += mainMenu;

        GetNode<Button>("Panel3/VBoxContainer/Restart").Pressed += restart;
    }
    public override void _Process( double delta )
    {
        QueueRedraw();

        foreach ( var ( card, i ) in DealerCards.Select( ( card, i ) => ( card, i ) ) ) {
            Vector2 position = new( 702 - i * 90, 70 );

            card.Position = new( card.Position.X + ( card.Position.X > position.X ? -200 : 0 ) * ( float ) delta, card.Position.Y );

            if ( i == ( DealerCards.Count - 1 ) && card.Position.X <= position.X ) DealerAnimating = false;
        }
        foreach ( var ( card, i ) in UserCards.Select( ( card, i ) => ( card, i ) ) ) {
            Vector2 position = new( 450 + i * 90, 450 );

            card.Position = new( card.Position.X + ( card.Position.X < position.X ? 200 : 0 ) * ( float ) delta, card.Position.Y );

            if ( i == ( UserCards.Count - 1 ) && card.Position.X >= position.X ) Animating = false;
        }

        if (FinishGame && !DealerAnimating)
        {
            FinishGame = false;

            int dealerTotal = DealerCards.Select(card => card.Value).Aggregate((a, b) => a + b);
            int userTotal = UserCards.Select(card => card.Value).Aggregate((a, b) => a + b);

            if ( userTotal > 21 || (userTotal > 21 && userTotal < dealerTotal) || (dealerTotal>userTotal && dealerTotal < 21 && userTotal < 21 ) || (dealerTotal == 21 && dealerTotal > userTotal)) {
                global.data.Balance -= bet;
            }  
            else if ( dealerTotal > 21 || userTotal > dealerTotal ) {
                global.data.Balance += bet * 2;
            }

            global.writeData();

            GetNode<Panel>( "Panel3" ).Visible = true;
            GetNode<Label>( "Panel3/VBoxContainer/Label/" ).Text = userTotal > 21 || (userTotal > 21 && userTotal < dealerTotal) || ( dealerTotal>userTotal && dealerTotal < 21 && userTotal < 21 ) || (dealerTotal == 21 && dealerTotal > userTotal) ? "FUCKEN LOSER" : dealerTotal > 21 || userTotal > dealerTotal ? "YOU WON FUCKER 🖕" : "WOW ITS A FUCKING TIE 👔";

            GetNode<Label>( "VBoxContainer/HBoxContainer2/DealerTot" ).Text = dealerTotal.ToString();
            GetNode<Label>( "VBoxContainer2/HBoxContainer/Balancevalue" ).Text = global.data.Balance.ToString();
        }
    }
    public override void _Draw()
    {
        Vector2 cardSize = new( 80, 128 );

        foreach ( var ( card, i ) in DealerCards.Select( ( card, i ) => ( card, i ) ) ) {
            Color color = card.Symbol == Card.SymbolEnum.Heart || card.Symbol == Card.SymbolEnum.Diamond ? Colors.Red : Colors.Black;

            if ( card.Hidden ) {
                DrawTextureRectRegion( background, new( card.Position, cardSize ), new( new( 0, 0 ), cardSize ) );
                DrawTextureRectRegion( cardback, new( card.Position, cardSize ), new( new( 0, 0 ), cardSize ) );
                continue;
            }

            string cardCharacter = card.Number == 1 ? "A" : card.Number == 11 ? "J" : card.Number == 12 ? "Q" : card.Number == 13 ? "K" : card.Number.ToString();

            DrawTextureRectRegion( background, new( card.Position, cardSize ), new( new( 0, 0 ), cardSize ) );
            DrawTextureRectRegion( symbolImages[ ( int ) card.Symbol ], new( card.Position + new Vector2( 80, 128 ) / 2 - new Vector2( 15, 15 ), new( 30, 30 ) ), new( new( 0, 0 ), new( 32, 32 ) ) );
            
            Vector2 stringSize = font.GetStringSize( cardCharacter, HorizontalAlignment.Left, -1, 20 );
            
            DrawString( font, card.Position + new Vector2( 5, -8 ) + Vector2.Down * stringSize, cardCharacter, HorizontalAlignment.Left, -1, 20, color );
            DrawString( font, card.Position + cardSize + new Vector2( -5, -5 ) + Vector2.Left * stringSize, cardCharacter, HorizontalAlignment.Left, -1, 20, color );
        }

        foreach ( var ( card, i ) in UserCards.Select( ( card, i ) => ( card, i ) ) ) {
            Color color = card.Symbol == Card.SymbolEnum.Heart || card.Symbol == Card.SymbolEnum.Diamond ? Colors.Red : Colors.Black;

            if ( card.Hidden ) {
                DrawTextureRectRegion( background, new( card.Position, cardSize ), new( new( 0, 0 ), cardSize ) );
                DrawTextureRectRegion( cardback, new( card.Position, cardSize ), new( new( 0, 0 ), cardSize ) );
                continue;
            }

            string cardCharacter = card.Number == 1 ? "A" : card.Number == 11 ? "J" : card.Number == 12 ? "Q" : card.Number == 13 ? "K" : card.Number.ToString();

            DrawTextureRectRegion( background, new( card.Position, cardSize ), new( new( 0, 0 ), cardSize ) );
            DrawTextureRectRegion( symbolImages[ ( int ) card.Symbol ], new( card.Position + new Vector2( 80, 128 ) / 2 - new Vector2( 15, 15 ), new( 30, 30 ) ), new( new( 0, 0 ), new( 32, 32 ) ) );
            
            Vector2 stringSize = font.GetStringSize( cardCharacter, HorizontalAlignment.Left, -1, 20 );
            
            DrawString( font, card.Position + new Vector2( 5, -8 ) + Vector2.Down * stringSize, cardCharacter, HorizontalAlignment.Left, -1, 20, color );
            DrawString( font, card.Position + cardSize + new Vector2( -5, -5 ) + Vector2.Left * stringSize, cardCharacter, HorizontalAlignment.Left, -1, 20, color );
        }

        for ( int i = 0; i < 3; i++ ) {
            Vector2 position = new( 102 + i * 10, 450 );

            DrawTextureRectRegion( background, new( position, cardSize ), new( new( 0, 0 ), cardSize ) );
            DrawTextureRectRegion( cardback, new( position, cardSize ), new( new( 0, 0 ), cardSize ) );
        }

        for ( int i = 0; i < 3; i++ ) {
            Vector2 position = new( 970 - i * 10, 70 );

            DrawTextureRectRegion( background, new( position, cardSize ), new( new( 0, 0 ), cardSize ) );
            DrawTextureRectRegion( cardback, new( position, cardSize ), new( new( 0, 0 ), cardSize ) );
        }
    }
    public void changeBet( int value ) {
        bet = Math.Clamp( bet + value, 0, global.data.Balance );
        GetNode<Label>( "VBoxContainer2/HBoxContainer2/Betvalue" ).Text = bet.ToString();
    }
    private void endGame() {
        if ( Animating ) return;

        DealerAnimating = true;
        FinishGame = true;

        foreach ( Card card in DealerCards ) card.Hidden = false;

        for ( int i = 0; i < 10; i++ ) {
            if ( DealerCards.Select( card => card.Number ).Aggregate( ( a, b ) => a + b ) + 5 >= 21 ) break;
            
            DealerCards.Add( new Card().setupCard( DealerCards, new( 970, 70 ) ) );
        }
    }
    private void mainMenu () {
        var main = ResourceLoader.Load<PackedScene>( "res://Main.tscn" ).Instantiate();

        QueueFree();
        GetTree().Root.AddChild( main );
    }
    private void restart () {
        DealerCards = new();
        UserCards = new();

        DealerAnimating = true;
        Animating = true;

        DealerCards.Add(new Card().setupCard(DealerCards, new(970, 70)));
        DealerCards.Add(new Card().setupCard(DealerCards, new(970, 70), true));

        UserCards.Add(new Card().setupCard(UserCards, new(102, 450)));
        UserCards.Add(new Card().setupCard(UserCards, new(102, 450)));

        GetNode<Label>("VBoxContainer/HBoxContainer2/DealerTot").Text = DealerCards.Select(card => card.Hidden ? 0 : card.Value).Aggregate((a, b) => a + b).ToString();
        GetNode<Label>("VBoxContainer/HBoxContainer3/UserTot").Text = UserCards.Select(card => card.Value).Aggregate((a, b) => a + b).ToString();

        GetNode<Panel>( "Panel3" ).Visible = false;
    }
}


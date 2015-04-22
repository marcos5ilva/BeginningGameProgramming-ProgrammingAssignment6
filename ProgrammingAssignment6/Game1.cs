using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using XnaCards;

namespace ProgrammingAssignment6
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int WINDOW_WIDTH = 800;
        const int WINDOW_HEIGHT = 600;


        // max valid blackjack score for a hand
        const int MAX_HAND_VALUE = 21;

        // deck and hands
        Deck deck;
        List<Card> dealerHand = new List<Card>();
        List<Card> playerHand = new List<Card>();

        // hand placement
        const int TOP_CARD_OFFSET = 100;
        const int HORIZONTAL_CARD_OFFSET = 150;
        const int VERTICAL_CARD_SPACING = 125;

        // messages
        SpriteFont messageFont;
        const string SCORE_MESSAGE_PREFIX = "Score: ";
        Message playerScoreMessage;
        List<Message> messages = new List<Message>();

        // message placement
        const int SCORE_MESSAGE_TOP_OFFSET = 25;
        const int HORIZONTAL_MESSAGE_OFFSET = HORIZONTAL_CARD_OFFSET;
        Vector2 winnerMessageLocation = new Vector2(WINDOW_WIDTH / 2,
            WINDOW_HEIGHT / 2);

        // menu buttons
        Texture2D quitButtonSprite;
        List<MenuButton> menuButtons = new List<MenuButton>();

        // menu button placement
        const int TOP_MENU_BUTTON_OFFSET = TOP_CARD_OFFSET;
        const int QUIT_MENU_BUTTON_OFFSET = WINDOW_HEIGHT - TOP_CARD_OFFSET;
        const int HORIZONTAL_MENU_BUTTON_OFFSET = WINDOW_WIDTH / 2;
        const int VERTICAL_MENU_BUTTON_SPACING = 125;

        // use to detect hand over when player and dealer didn't hit
        bool playerHit = false;
        bool dealerHit = false;

        // game state tracking
        static GameState currentState = GameState.WaitingForPlayer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution and show mouse
			IsMouseVisible = true;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // create and shuffle deck
			deck = new Deck(Content, 0,0);
			deck.Shuffle();

            // first player card
			playerHand.Add(deck.TakeTopCard());


            // first dealer card
			dealerHand.Add(deck.TakeTopCard());

            // second player card
			playerHand.Add(deck.TakeTopCard());


            // second dealer 
			dealerHand.Add(deck.TakeTopCard());

			//checking if  cards are flipped over
			foreach (Card card in dealerHand)
			{
				if (card.FaceUp) 
					card.FlipOver ();
				if (!dealerHand [1].FaceUp)
					dealerHand [1].FlipOver ();
				

			}

			foreach (Card card in playerHand)
			{
				if(!card.FaceUp)
				{
					card.FlipOver();
				}
			}

			//setting the x and y for each card

			for (int i = 0; i < playerHand.Count; i++)
			{
				playerHand[i].X= HORIZONTAL_CARD_OFFSET; 
				playerHand[i].Y= i*HORIZONTAL_CARD_OFFSET+TOP_CARD_OFFSET;
			}

			for (int i = 0; i < dealerHand.Count; i++)
			{
				dealerHand [i].X = WINDOW_WIDTH - HORIZONTAL_CARD_OFFSET ; 
				dealerHand [i].Y = i * VERTICAL_CARD_SPACING + TOP_CARD_OFFSET;
			}



            // load sprite font, create message for player score and add to list
            messageFont = Content.Load<SpriteFont>("Arial24");
            playerScoreMessage = new Message(SCORE_MESSAGE_PREFIX + GetBlackjackScore(playerHand).ToString(),
                messageFont,
                new Vector2(HORIZONTAL_MESSAGE_OFFSET, SCORE_MESSAGE_TOP_OFFSET));
            messages.Add(playerScoreMessage);

            // load quit button sprite for later use
            quitButtonSprite = Content.Load<Texture2D>("quitbutton");

            // create hit button and add to list
			MenuButton hitButton = new MenuButton (Content.Load<Texture2D> ("hitbutton"),
				                   new Vector2 (HORIZONTAL_MENU_BUTTON_OFFSET,
					               TOP_MENU_BUTTON_OFFSET),
				                   GameState.PlayerHitting);
			menuButtons.Add(hitButton);
			
			
            // create stand button and add to list
			MenuButton standButton = new MenuButton (Content.Load<Texture2D> ("standbutton"),
				                     new Vector2 (HORIZONTAL_MENU_BUTTON_OFFSET,
					                 TOP_MENU_BUTTON_OFFSET + VERTICAL_MENU_BUTTON_SPACING),
				                     GameState.WaitingForDealer);
			menuButtons.Add(standButton );


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();


            // update menu buttons as appropriate

			if (currentState == GameState.WaitingForPlayer ||
			    currentState == GameState.DisplayingHandResults)
			{
				foreach (MenuButton menuButton in menuButtons)
				{
					menuButton.Update (Mouse.GetState ());
				}
			}

            // game state-specific processing

				switch (currentState) 
				{

					case GameState.PlayerHitting:

					// give another card to player 
					playerHand.Add (deck.TakeTopCard ());
					

					//checking if player card are Face Up

					foreach (Card card in playerHand) 
					{
						if (!card.FaceUp)
						{
							card.FlipOver ();
						}
					}

					//setting the x and y for each card

				for (int i = 0; i < playerHand.Count; i++)
				{
					playerHand [i].X = HORIZONTAL_CARD_OFFSET; 
					playerHand [i].Y = i * HORIZONTAL_CARD_OFFSET + TOP_CARD_OFFSET;
				}
					
					//setting the new player score
					
				playerScoreMessage.Text = SCORE_MESSAGE_PREFIX + GetBlackjackScore (playerHand).ToString (); 


				ChangeState (GameState.WaitingForDealer);


				//playerHit = true;

				break;

			case GameState.WaitingForDealer:
				if (GetBlackjackScore (dealerHand) <= 16)
				{
					ChangeState (GameState.DealerHitting);
				} 

				else
				{
					ChangeState(GameState.CheckingHandOver);
				}

				break;

			case GameState.DealerHitting:

				// give another card to dealer 
				dealerHand.Add (deck.TakeTopCard ());

				for (int i = 0; i < dealerHand.Count; i++) {
					dealerHand [i].X = WINDOW_WIDTH - HORIZONTAL_CARD_OFFSET;
					dealerHand [i].Y = i * HORIZONTAL_CARD_OFFSET + TOP_CARD_OFFSET;
				}

				ChangeState (GameState.CheckingHandOver);

				break;

			case GameState.CheckingHandOver:
				if(GetBlackjackScore (playerHand) > MAX_HAND_VALUE||
					GetBlackjackScore(dealerHand)> MAX_HAND_VALUE)
				{

					//checking if dealer card are Face Down and Flip Over
					foreach (Card card in dealerHand) 
					{
						if (!card.FaceUp)
						{
							card.FlipOver ();
						}
					}

					//setting the dealer  score message
					Message dealerScoreMessage = new Message(SCORE_MESSAGE_PREFIX + GetBlackjackScore(playerHand).ToString(),
						messageFont,
						new Vector2(WINDOW_WIDTH-HORIZONTAL_CARD_OFFSET, SCORE_MESSAGE_TOP_OFFSET));
					messages.Add(dealerScoreMessage);

					dealerScoreMessage.Text = SCORE_MESSAGE_PREFIX + GetBlackjackScore (dealerHand).ToString (); 

					//setting the winner message
					Message winnerMessage = new Message(SCORE_MESSAGE_PREFIX + GetBlackjackScore(playerHand).ToString(),
											messageFont,winnerMessageLocation);
					messages.Add(winnerMessage);
					
				}


				break;
			
				default:
				Console.WriteLine ("currentState = "+ currentState);
					break;
				}



            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Goldenrod);

            spriteBatch.Begin();

            // draw hands
			foreach (Card card in playerHand)
				card.Draw (spriteBatch);

			foreach (Card card in dealerHand)
				card.Draw (spriteBatch);

            // draw messages
			foreach (Message message in messages)
				message.Draw (spriteBatch);

            // draw menu buttons
			foreach (MenuButton menuButton in menuButtons)
				menuButton.Draw (spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Calculates the Blackjack score for the given hand
        /// </summary>
        /// <param name="hand">the hand</param>
        /// <returns>the Blackjack score for the hand</returns>
        private int GetBlackjackScore(List<Card> hand)
        {
            // add up score excluding Aces
            int numAces = 0;
            int score = 0;
            foreach (Card card in hand)
            {
                if (card.Rank != Rank.Ace)
                {
                    score += GetBlackjackCardValue(card);
                }
                else
                {
                    numAces++;
                }
            }

            // if more than one ace, only one should ever be counted as 11
            if (numAces > 1)
            {
                // make all but the first ace count as 1
                score += numAces - 1;
                numAces = 1;
            }

            // if there's an Ace, score it the best way possible
            if (numAces > 0)
            {
                if (score + 11 <= MAX_HAND_VALUE)
                {
                    // counting Ace as 11 doesn't bust
                    score += 11;
                }
                else
                {
                    // count Ace as 1
                    score++;
                }
            }

            return score;
        }

        /// <summary>
        /// Gets the Blackjack value for the given card
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>the Blackjack value for the card</returns>
        private int GetBlackjackCardValue(Card card)
        {
            switch (card.Rank)
            {
                case Rank.Ace:
                    return 11;
                case Rank.King:
                case Rank.Queen:
                case Rank.Jack:
                case Rank.Ten:
                    return 10;
                case Rank.Nine:
                    return 9;
                case Rank.Eight:
                    return 8;
                case Rank.Seven:
                    return 7;
                case Rank.Six:
                    return 6;
                case Rank.Five:
                    return 5;
                case Rank.Four:
                    return 4;
                case Rank.Three:
                    return 3;
                case Rank.Two:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Changes the state of the game
        /// </summary>
        /// <param name="newState">the new game state</param>
        public static void ChangeState(GameState newState)
        {
			Console.WriteLine ("Estado Corrente:" + currentState);
            currentState = newState;
			Console.WriteLine ("Novo Estado:" + newState);
        }
    }
}

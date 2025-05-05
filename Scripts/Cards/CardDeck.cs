using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class CardDeck : Node
{
	private List<Card> Cards { get; set; } = [];

	public override void _Ready()
	{
		base._Ready();
		InitializeFullCardDeck();
		// InitializeCustomCardDeck();
		InitializeCardDeckUI();
	}

	private void InitializeFullCardDeck()
	{
		// Add all Hearts
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Card{
				Rank = (Rank)i,
				Suit = Suit.Hearts
			});
		}

		// Add all Spades
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Card{
				Rank = (Rank)i,
				Suit = Suit.Spades
			});
		}

		// Add all Diamonds
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Card{
				Rank = (Rank)i,
				Suit = Suit.Diamonds
			});
		}

		// Add all Clubs
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Card{
				Rank = (Rank)i,
				Suit = Suit.Clubs
			});
		}
	}

	private void InitializeCustomCardDeck()
	{
		var customDeck = new List<Card>
		{
			new Card { Rank = Rank.Two, Suit = Suit.Hearts },
			new Card { Rank = Rank.Two, Suit = Suit.Spades },
			new Card { Rank = Rank.Two, Suit = Suit.Diamonds },
			new Card { Rank = Rank.Two, Suit = Suit.Clubs },
			new Card { Rank = Rank.Three, Suit = Suit.Hearts },
			new Card { Rank = Rank.Three, Suit = Suit.Spades },
			new Card { Rank = Rank.Three, Suit = Suit.Diamonds },
			new Card { Rank = Rank.Three, Suit = Suit.Clubs },
		};
		Cards = customDeck;
	}

	private void InitializeCardDeckUI()
	{
		string containersPath = "CardListPanelContainer/CardListHorizontalContainer/CardList/";
		Texture2D atlas = GD.Load<Texture2D>("res://Assets/RawImages/Cards.png");
		Vector2I tileSize = new Vector2I(16, 16);
		
		var containerHearts = GetNode<VBoxContainer>(containersPath + "CardListHearts");
		foreach (Card card in Cards.Where(c => c.Suit == Suit.Hearts))
		{
			TextureRect cardUI = CreateCardUI(card, atlas, tileSize);
			containerHearts.AddChild(cardUI);
		}
		
		var containerDiamonds = GetNode<VBoxContainer>(containersPath + "CardListDiamonds");
		foreach (Card card in Cards.Where(c => c.Suit == Suit.Diamonds))
		{
			TextureRect cardUI = CreateCardUI(card, atlas, tileSize);
			containerDiamonds.AddChild(cardUI);
		}
		
		var containerSpades = GetNode<VBoxContainer>(containersPath + "CardListSpades");
		foreach (Card card in Cards.Where(c => c.Suit == Suit.Spades))
		{
			TextureRect cardUI = CreateCardUI(card, atlas, tileSize);
			containerSpades.AddChild(cardUI);
		}
		
		var containerClubs = GetNode<VBoxContainer>(containersPath + "CardListClubs");
		foreach (Card card in Cards.Where(c => c.Suit == Suit.Clubs))
		{
			TextureRect cardUI = CreateCardUI(card, atlas, tileSize);
			containerClubs.AddChild(cardUI);
		}
	}

	private TextureRect CreateCardUI(Card card, Texture2D atlasTexture, Vector2I tileSize)
	{
		// Create AtlasTexture for the card
		AtlasTexture atlas = new AtlasTexture();
		atlas.Atlas = atlasTexture;

		// Calculate tile position
		int suitIndex = (int)card.Suit; // row
		int rankIndex = (int)card.Rank - 1; // column (Ace = 1, so subtract 1)

		Rect2 region = new Rect2(rankIndex * tileSize.X, suitIndex * tileSize.Y, tileSize.X, tileSize.Y);
		atlas.Region = region;

        TextureRect cardUI = new CardUI
        {
            Card = card,
            Texture = atlas,
            CustomMinimumSize = tileSize * 2,
            ExpandMode = TextureRect.ExpandModeEnum.KeepSize
        };

        return cardUI;
	}
}

public class Card {
	public Rank Rank { get; set; }

	public Suit Suit { get; set; }
}

public enum Rank { Two = 1, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

public enum Suit {
	Hearts,
	Spades,
	Diamonds,
	Clubs,
}

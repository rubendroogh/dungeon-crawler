using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a deck of playing cards.
/// Initially, it contains only the Two of each suit.
/// The deck can be expanded to include a full set of 52 cards if needed.
/// The deck is used to create a UI representation of the cards.
/// 
/// TODO: Make this into a proper manager like the rest.
/// </summary>
public partial class CardList : Node
{
	private List<Card> Cards { get; set; } = [];

	private static Vector2I TileSize = new Vector2I(16, 16);

	public override void _Ready()
	{
		base._Ready();
		InitializeCustomCardDeck();
		InitializeCardDeckUI();
	}

	/// <summary>
	/// Gets the icon texture for a specific card based on its rank and suit.
	/// </summary>
	/// <returns>A texture representing the card's icon.</returns>
	public static Texture2D GetCardIcon(Card card)
	{
		if (card == null)
		{
			GD.PrintErr("Card is null");
			return null;
		}

		Texture2D atlas = GD.Load<Texture2D>("res://Assets/RawImages/Cards.png");

		// Create AtlasTexture for the card
		AtlasTexture iconTexture = new AtlasTexture();
		iconTexture.Atlas = atlas;

		// Calculate tile position
		int suitIndex = (int)card.Suit; // row
		int rankIndex = (int)card.Rank - 1; // column (Ace = 1, so subtract 1)

		Rect2 region = new Rect2(rankIndex * TileSize.X, suitIndex * TileSize.Y, TileSize.X, TileSize.Y);
		iconTexture.Region = region;

		return iconTexture;
	}

	public void AddCardToDeck(Card card)
	{
		if (card == null)
		{
			GD.PrintErr("Card is null");
			return;
		}

		Cards.Add(card);

		// TODO: This can be a bit cleaner
		ResetCardUI();
		InitializeCardDeckUI();
	}

	/// <summary>
	/// Initializes a full card deck with all 52 cards.
	/// Probably not used for now, but useful for reference.
	/// </summary>
	private void InitializeFullCardDeck()
	{
		// Add all Hearts
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Card
			{
				Rank = (Rank)i,
				Suit = Suit.Hearts
			});
		}

		// Add all Spades
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Card
			{
				Rank = (Rank)i,
				Suit = Suit.Spades
			});
		}

		// Add all Diamonds
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Card
			{
				Rank = (Rank)i,
				Suit = Suit.Diamonds
			});
		}

		// Add all Clubs
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Card
			{
				Rank = (Rank)i,
				Suit = Suit.Clubs
			});
		}
	}

	/// <summary>
	/// Initializes a custom card deck with only the Two of each suit.
	/// </summary>
	private void InitializeCustomCardDeck()
	{
		var customDeck = new List<Card>
		{
			new Card { Rank = Rank.Two, Suit = Suit.Hearts },
			new Card { Rank = Rank.Two, Suit = Suit.Spades },
			new Card { Rank = Rank.Two, Suit = Suit.Diamonds },
			new Card { Rank = Rank.Two, Suit = Suit.Clubs },
		};
		Cards = customDeck;
	}

	/// <summary>
	/// Initializes the UI for the card deck.
	/// </summary>
	private void InitializeCardDeckUI()
	{
		string containersPath = "CardListPanelContainer/Margin/CardListHorizontalContainer/CardList/";

		var containerHearts = GetNode<VBoxContainer>(containersPath + "CardListHearts");
		foreach (Card card in Cards.Where(c => c.Suit == Suit.Hearts))
		{
			TextureRect cardUI = CreateCardUI(card);
			containerHearts.AddChild(cardUI);
		}

		var containerDiamonds = GetNode<VBoxContainer>(containersPath + "CardListDiamonds");
		foreach (Card card in Cards.Where(c => c.Suit == Suit.Diamonds))
		{
			TextureRect cardUI = CreateCardUI(card);
			containerDiamonds.AddChild(cardUI);
		}

		var containerSpades = GetNode<VBoxContainer>(containersPath + "CardListSpades");
		foreach (Card card in Cards.Where(c => c.Suit == Suit.Spades))
		{
			TextureRect cardUI = CreateCardUI(card);
			containerSpades.AddChild(cardUI);
		}

		var containerClubs = GetNode<VBoxContainer>(containersPath + "CardListClubs");
		foreach (Card card in Cards.Where(c => c.Suit == Suit.Clubs))
		{
			TextureRect cardUI = CreateCardUI(card);
			containerClubs.AddChild(cardUI);
		}
	}

	/// <summary>
	/// Resets the UI for the card list by clearing all displayed cards.
	/// </summary>
	private void ResetCardUI()
	{
		string containersPath = "CardListPanelContainer/Margin/CardListHorizontalContainer/CardList/";

		var containerHearts = GetNode<VBoxContainer>(containersPath + "CardListHearts");
		foreach (var child in containerHearts.GetChildren())
		{
			child.QueueFree();
		}

		var containerDiamonds = GetNode<VBoxContainer>(containersPath + "CardListDiamonds");
		foreach (var child in containerDiamonds.GetChildren())
		{
			child.QueueFree();
		}

		var containerSpades = GetNode<VBoxContainer>(containersPath + "CardListSpades");
		foreach (var child in containerSpades.GetChildren())
		{
			child.QueueFree();
		}

		var containerClubs = GetNode<VBoxContainer>(containersPath + "CardListClubs");
		foreach (var child in containerClubs.GetChildren())
		{
			child.QueueFree();
		}
	}

	/// <summary>
	/// Creates a TextureRect UI element for a card using an atlas texture.
	/// </summary>
	private TextureRect CreateCardUI(Card card)
	{
		var texture = GetCardIcon(card);
		TextureRect cardUI = new CardUI
		{
			Card = card,
			Texture = texture,
			CustomMinimumSize = TileSize * 2,
			ExpandMode = TextureRect.ExpandModeEnum.KeepSize
		};

		return cardUI;
	}
}

/// <summary>
/// Represents a playing card with a rank and suit, used to add mana charges to the selected spell.
/// </summary>
public class Card
{
	public Rank Rank { get; set; }

	public Suit Suit { get; set; }

	public Texture2D GetIcon()
	{
		return CardList.GetCardIcon(this);
	}
}

/// <summary>
/// Represents the rank of a card in a standard deck.
/// The ranks are ordered from Two (1) to Ace (14).
/// </summary>
public enum Rank { Two = 1, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

/// <summary>
/// Represents the suit of a card in a standard deck.
/// </summary>
public enum Suit
{
	Hearts,
	Diamonds,
	Clubs,
	Spades,
}

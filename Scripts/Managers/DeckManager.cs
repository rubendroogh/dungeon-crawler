using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a deck of blessings. These blessings are used to cast spells in the game.
/// Initially, it contains only the Two of each domain.
/// 
/// TODO: Make this into a proper manager like the rest.
/// </summary>
public partial class DeckManager : Node
{
	private List<Blessing> Cards { get; set; } = [];

	private static Vector2I TileSize = new Vector2I(16, 16);

	public override void _Ready()
	{
		base._Ready();
		InitializeCustomBlessingDeck();
		InitializeCardDeckUI();
	}

	/// <summary>
	/// Gets the icon texture for a specific card based on its rank and suit.
	/// </summary>
	/// <returns>A texture representing the card's icon.</returns>
	public static Texture2D GetCardIcon(Blessing card)
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
		int suitIndex = (int)card.Domain; // row
		int rankIndex = (int)card.Rank - 1; // column (Ace = 1, so subtract 1)

		Rect2 region = new Rect2(rankIndex * TileSize.X, suitIndex * TileSize.Y, TileSize.X, TileSize.Y);
		iconTexture.Region = region;

		return iconTexture;
	}

	/// <summary>
	/// Adds a blessing to the deck and updates the UI.
	/// </summary>
	public void AddBlessing(Blessing blessing)
	{
		if (blessing == null)
		{
			GD.PrintErr("Card is null");
			return;
		}

		Cards.Add(blessing);

		// TODO: This can be a bit cleaner
		ResetBlessingUI();
		InitializeCardDeckUI();
	}

	/// <summary>
	/// Initializes a full card deck with all 52 cards.
	/// Probably not used for now, but useful for reference.
	/// </summary>
	private void InitializeFullCardDeck()
	{
		// Add all Zer
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Blessing
			{
				Rank = (Rank)i,
				Domain = Domain.Zer
			});
		}

		// Add all Hamin
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Blessing
			{
				Rank = (Rank)i,
				Domain = Domain.Hamin
			});
		}

		// Add all Calina
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Blessing
			{
				Rank = (Rank)i,
				Domain = Domain.Calina
			});
		}

		// Add all Jaddis
		for (int i = 1; i < 14; i++)
		{
			Cards.Add(new Blessing
			{
				Rank = (Rank)i,
				Domain = Domain.Jaddis
			});
		}
	}

	/// <summary>
	/// Initializes a custom blessing deck with only the Two of each domain.
	/// </summary>
	private void InitializeCustomBlessingDeck()
	{
		var customDeck = new List<Blessing>
		{
			new Blessing { Rank = Rank.Two, Domain = Domain.Calina },
			new Blessing { Rank = Rank.Two, Domain = Domain.Hamin },
			new Blessing { Rank = Rank.Two, Domain = Domain.Jaddis },
			new Blessing { Rank = Rank.Two, Domain = Domain.Zer },
		};
		Cards = customDeck;
	}

	/// <summary>
	/// Initializes the UI for the card deck.
	/// </summary>
	private void InitializeCardDeckUI()
	{
		string containersPath = "Margin/CardListHorizontalContainer/CardList/";

		var containerHearts = GetNode<VBoxContainer>(containersPath + "CardListHearts");
		foreach (Blessing card in Cards.Where(c => c.Domain == Domain.Calina))
		{
			TextureRect cardUI = CreateBlessingUI(card);
			containerHearts.AddChild(cardUI);
		}

		var containerDiamonds = GetNode<VBoxContainer>(containersPath + "CardListDiamonds");
		foreach (Blessing card in Cards.Where(c => c.Domain == Domain.Hamin))
		{
			TextureRect cardUI = CreateBlessingUI(card);
			containerDiamonds.AddChild(cardUI);
		}

		var containerSpades = GetNode<VBoxContainer>(containersPath + "CardListSpades");
		foreach (Blessing card in Cards.Where(c => c.Domain == Domain.Jaddis))
		{
			TextureRect cardUI = CreateBlessingUI(card);
			containerSpades.AddChild(cardUI);
		}

		var containerClubs = GetNode<VBoxContainer>(containersPath + "CardListClubs");
		foreach (Blessing card in Cards.Where(c => c.Domain == Domain.Zer))
		{
			TextureRect cardUI = CreateBlessingUI(card);
			containerClubs.AddChild(cardUI);
		}
	}

	/// <summary>
	/// Resets the UI for the blessing list by clearing all displayed blessings.
	/// </summary>
	private void ResetBlessingUI()
	{
		string containersPath = "Margin/CardListHorizontalContainer/CardList/";

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
	/// Creates a TextureRect UI element for a blessing using an atlas texture.
	/// </summary>
	private TextureRect CreateBlessingUI(Blessing blessing)
	{
		var texture = GetCardIcon(blessing);
		TextureRect blessingUI = new BlessingUI
		{
			Card = blessing,
			Texture = texture,
			CustomMinimumSize = TileSize * 2,
			ExpandMode = TextureRect.ExpandModeEnum.KeepSize,
			Scale = new Vector2(2, 2)
		};

		return blessingUI;
	}
}

/// <summary>
/// Represents a blessing card with a rank and domain (which is its type).
/// This is used to cast spells in the game.
/// </summary>
public class Blessing
{
	public Rank Rank { get; set; }

	public Domain Domain { get; set; }

    public Texture2D GetIcon()
	{
		return DeckManager.GetCardIcon(this);
	}
}

/// <summary>
/// Represents the rank of a card in a standard deck.
/// The ranks are ordered from Two (1) to Ace (14).
/// </summary>
public enum Rank { Two = 1, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

/// <summary>
/// Represents the type of a blessing in a standard deck.
/// </summary>
public enum Domain
{
	/// <summary>
	/// Focused on power and resilience.
	/// </summary>
	Calina,
	
	/// <summary>
	/// Focused on wisdom and calm.
	/// </summary>
	Hamin,

	/// <summary>
	/// Focused on the element of surprise and deception.
	/// </summary>
	Jaddis,

	/// <summary>
	/// Focused on pest and decay.
	/// </summary>
	Zer,
}

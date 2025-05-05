using Godot;

/// <summary>
/// An entity that is a participant in a battle on either side.
/// </summary>
public partial class Character : Node
{
    public float CurrentHealth { get; set; }

    [Export]
    public CharacterData CharacterData { get; set; }

    private Sprite2D CharacterSprite { get; set; }

    private TextureProgressBar HealthBar { get; set; }

    public override void _Ready()
    {
        base._Ready();
        CurrentHealth = CharacterData.MaxHealth;
    }

    public void Setup(CharacterData characterData)
    {
        CharacterData = characterData;
        CurrentHealth = characterData.MaxHealth;

        CharacterSprite = GetNode<Sprite2D>("CharacterSprite");
        HealthBar = GetNode<TextureProgressBar>("HealthBar");
        HealthBar.MaxValue = characterData.MaxHealth;
        HealthBar.Value = CurrentHealth;
        CharacterSprite.Texture = characterData.Image;
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        UpdateHealthBar();

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > CharacterData.MaxHealth)
        {
            CurrentHealth = CharacterData.MaxHealth;
        }

        UpdateHealthBar();
    }

    private void Die()
    {
        ManagerRepository.BattleLogManager.AddToLog($"{CharacterData.Name} has died.");
    }

    public void UpdateHealthBar()
    {
        HealthBar.Value = CurrentHealth;
    }
}

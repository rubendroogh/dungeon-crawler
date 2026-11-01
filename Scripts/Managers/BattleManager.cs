using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

/// <summary>Owns the battle loop. Only the player main phase waits for input.</summary>
public partial class BattleManager : Node
{
    public static BattleManager Instance { get; private set; }
    public Dictionary<Character, bool> Characters { get; private set; } = new();
    public TurnPhase CurrentTurnPhase { get; private set; } = TurnPhase.Start;
    public int CastSpellsThisTurn { get; set; }
    public int Round { get; private set; }
    public bool IsBattleActive { get; private set; }
    public bool CanQueueSpells => IsBattleActive && CurrentTurnPhase == TurnPhase.Main
        && Characters.GetValueOrDefault(PlayerManager.Instance.GetPlayer());
    public event System.Action<bool> BattleEnded;
    private TaskCompletionSource<bool> EndMainPhase;
    private Label TurnLabel;

    public override void _Ready()
    {
        Instance = this;
        TurnLabel = GetTree().Root.FindChild("TurnLabel", true, false) as Label;
    }

    public void InitializeBattle(CharacterData opponentData = null)
    {
        if (IsBattleActive) return;
        OpponentManager.Instance.ClearOpponents();
        Characters = new() { { PlayerManager.Instance.GetPlayer(), true } };
        var player = PlayerManager.Instance.GetPlayer();
        player.ActionQueue.Clear();
        ActionManager.Instance.ClearSelectedSpell();
        ManaSourceManager.Instance.ResetAllMana();
        SpellQueueManager.Instance.UpdateSpellQueue();
        BattleLogManager.Instance.ClearLog();
        Round = 0;
        IsBattleActive = true;
        if (opponentData == null) OpponentManager.Instance.SpawnRandomOpponent();
        else OpponentManager.Instance.SpawnOpponent(opponentData);
        _ = RunBattleLoop();
    }

    public void StartNewTurnPhaseFrom(TurnPhase phase)
    {
        if (phase == TurnPhase.Main && CanQueueSpells)
        {
            // Lock input before resuming the asynchronous loop.
            CurrentTurnPhase = TurnPhase.Damage;
            ActionManager.Instance.ClearSelectedSpell();
            ManaSourceManager.Instance.DeselectAllMana();
            EndMainPhase?.TrySetResult(true);
        }
    }

    private async Task RunBattleLoop()
    {
        try
        {
            while (IsBattleActive && IsInsideTree())
            {
                Round++;
                foreach (var character in Characters.Keys.ToArray())
                {
                    if (character.IsDead) continue;
                    foreach (var key in Characters.Keys.ToArray()) Characters[key] = key == character;
                    CastSpellsThisTurn = 0;
                    SetPhase(TurnPhase.Start, character);
                    if (character.IsPlayer) ManaSourceManager.Instance.ResetAllMana();
                    await character.StartTurn();
                    if (await CheckBattleEnd()) return;

                    EndMainPhase = new TaskCompletionSource<bool>();
                    SetPhase(TurnPhase.Main, character);
                    if (character.IsPlayer) await EndMainPhase.Task;
                    else ((Enemy)character).ChooseAction(Characters.Keys.Where(c => !c.IsDead).ToList());

                    SetPhase(TurnPhase.Damage, character);
                    await character.ResolveQueue();
                    if (await CheckBattleEnd()) return;
                    SetPhase(TurnPhase.End, character);
                    await character.EndTurn();
                    if (await CheckBattleEnd()) return;
                    SetPhase(TurnPhase.PostEnd, character);
                }
            }
        }
        catch (Exception exception)
        {
            GD.PushError($"Battle failed: {exception}");
            IsBattleActive = false;
            BattleEnded?.Invoke(false);
        }
    }

    private void SetPhase(TurnPhase phase, Character character)
    {
        CurrentTurnPhase = phase;
        TurnLabel.Text = $"Round {Round} · {(character.IsPlayer ? "Your turn" : "Enemy turn")} · {phase}";
    }

    private async Task<bool> CheckBattleEnd()
    {
        var player = PlayerManager.Instance.GetPlayer();
        var victory = Characters.Keys.Where(c => !c.IsPlayer).All(c => c.IsDead);
        if (!player.IsDead && !victory) return false;
        foreach (var character in Characters.Keys) character.ActionQueue.Clear();
        await player.ClearAllEffects();
        ActionManager.Instance.ClearSelectedSpell();
        ManaSourceManager.Instance.ResetAllMana();
        SpellQueueManager.Instance.UpdateSpellQueue();
        IsBattleActive = false;
        BattleEnded?.Invoke(!player.IsDead);
        return true;
    }
}

public enum TurnPhase { Start, Main, Damage, End, PostEnd }

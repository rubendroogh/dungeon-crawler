# Priest — first playable MVP

A short descent into Alderwick for Mart to playtest. Based on the [Priest design brief](https://alike-address-88a.notion.site/Priest-1e2b94d5324680708cb4f72e58f552af).

## Play

Open `project.godot` in **Godot 4.4.1 .NET**, build the C# project, then press **F5**. The project targets .NET 8. The initial window is 1280 × 720, with a 1920 × 1080 reference viewport.

1. Enter your name. Leave personality traits at zero for a balanced Zer/Hamin build, or trade positive and negative points until the total is zero. The affinity preview shows your starting gods.
2. Start the game, then press Space, Enter, or click to continue the introduction.
3. Choose a route through the dungeon. You can visit both the shrine and archive before challenging the Keeper.
4. In combat, click a spell, check the automatically selected blessings, and click **Add to queue**. Repeat, then **Cast queue**. Spells resolve in order. **Clear queue** returns your blessings; **Pass turn** ends an empty turn.
5. Watch the enemy's next attack and spend some mana on wards to protect your health. Your three starting blessings refresh every player turn; health carries between fights. Choose one reward after each regular encounter.
6. Defeat the Keeper to finish. Victory and defeat both offer a fresh run.

Hover over spells for their cost, effects, and keywords. The Map tab shows the floor and visited rooms.

Extra mana is scarce: ordinary victories and the shrine do not grant blessings. The optional archive offers the floor's only permanent mana upgrade, chosen instead of a spell or recovery. A run therefore has three mana per turn, or four after claiming that upgrade. Dual-domain builds begin with two mana in their strongest domain and one in the other.

Every aligned domain grants a starting ward costing one mana of that domain. Hamin's Stone Ward grants 9 block, Jaddis's Umbral Shield 8, Zer's Grave Ward 6, and Calina's Flame Ward 5. Block stacks, absorbs damage after resistances, and expires at the start of your next turn. The health bar shows the current block pool. The Keeper alternates between 8 physical damage and 16 fire damage, making defense compete with attacks for your limited mana.

## Included

- Eight personality traits with negative counterparts, four divine domains, and single- or dual-domain starting builds.
- Queued spell combat, damage resistances, enemy intentions, temporary status effects, casting feedback, and a combat log.
- A seven-room branching floor: entrance, flooded threshold, shrine, archive, last lantern, boss, and exit.
- One-time healing, blessing and spell rewards; persistent health between encounters; victory, defeat, and replay.

This MVP uses a fixed floor and the existing art and spell resources. Procedural floors, saved runs, inventory, and a devotion system remain outside this playable slice. Balance is ready for playtesting rather than final tuning.

## Validation

```powershell
dotnet build --no-restore
# Substitute the path to your Godot .NET executable:
& 'path/to/Godot_console.exe' --headless --path . -- --mvp-test
```

The integration checks exercise all starting-domain combinations and their castable mana budgets, character creation, block absorption and expiry, status expiry, mouse spell selection, queue refunds, both dungeon branches, rewards, boss victories with three and four mana, the one-time blessing limit, defeat when ignoring block, and fresh replay after both outcomes. Additional boss runs check every single and mixed alignment using starting wards and three mana. They exit with code 1 on failure.

To also capture rendered screens, omit `--headless` and add `--rendering-method gl_compatibility` before `--`, and `--mvp-capture` after it. Captures go to `.godot/mvp-preview/`.

Add `--mvp-smoke` after `--` to check the core run, block rules, mana upgrade, defeat, and replay while skipping the additional boss run for each alignment.

Managers are owned by the main scene so replay recreates their state together with the player and UI.

## Editing the existing content and presentation

- `Resources/Dungeon/Alderwick.tres` owns the floor's room catalog, starting mana, blessing reward limit, reward spells, and shared text. Room resources under `Resources/Dungeon/Rooms` hold descriptions, map coordinates, icons, exits, encounter references, and whether a blessing reward is available.
- Enemy character resources under `Resources/Characters` reference their ordered attacks under `Resources/Actions`.
- `Scenes/SceneUI` contains the reusable exploration view, map, backdrop, and choice button. Shared palettes and dimensions are in `Resources/Themes/DungeonMapStyle.tres` and `DungeonBackdropStyle.tres`; map icons are in `Assets/Textures/UI/Map`.
- `Levels/MainScene.tscn` composes the screens and supplies exported node references. `Scripts/Debug/MvpPlaytest.cs` contains only the opt-in diagnostic playthrough and captures.

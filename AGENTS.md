# Priest: the video game

This Dungeon Crawler is an old-school first-person roguelike dungeon crawler where the player moves by selecting a direction to go into, with battles and events taking place along the way. The battling is deep and complex, and you should be able to create a wide variety of builds that feel different and unique. The battles feel puzzely and difficult.

In essence, it’s a deckbuilder, but the cards are disguised as spells and more interactive due to different ways to cast them, and the knock-on effects of casting them in a queue.

# Code standards

These standards follow the original project code, before the MVP additions (commit `0ebe6fe`). Representative examples are `SpellBookManager`, `CharacterCreationManager`, `ActionData`, `ComponentExposer`, and the spell and status-effect behaviours.

## Documentation

- Document all classes concisely and clearly with XML `/// <summary>` comments describing their responsibility.
- Document methods and members when their contract, lifecycle, units, or gameplay meaning needs explanation. Include parameter and return documentation where useful.
- Don't explain anything that a developer can easily make out from the code. Comments should explain intent, constraints, or a non-obvious decision.
- Keep documentation accurate when changing behaviour. Use `TODO` comments for specific unfinished work.

## Naming and formatting

- Use descriptive names and the project's gameplay vocabulary: blessings, domains, spells, queues, keywords, and status effects. Use the spelling `Behaviour` for behaviour classes and interfaces.
- Use PascalCase for classes, methods, properties, fields (including private fields), constants, and enum members. Use camelCase for parameters and local variables. Do not introduce underscore prefixes for private fields.
- Preserve Godot's required callback names, such as `_Ready`, `_GuiInput`, and `_Pressed`. Name event handlers `On...` and interfaces `I...`.
- Name script files after their primary class. Keep closely related helper types or enums together where this matches the existing files.
- Put opening braces on their own line for classes and methods. Prefer braced, multiline control-flow blocks and separate logical steps with blank lines.
- Match the surrounding file's indentation. The original code contains both tabs and spaces; avoid unrelated formatting changes.
- Use `var` when the type is clear, and explicit types where they improve readability. Target-typed `new()` and collection expressions such as `[]` are already established conventions.
- Keep code in UTF-8 and follow `.editorconfig`. Follow the namespace convention of the area being edited; the project contains both global types and file-scoped namespaces. Namespace migration is a separate refactor.

## Responsibilities and project structure

- Put system orchestration and shared state in focused managers under `Scripts/Managers`. Follow the existing singleton pattern where needed: `public static ... Instance { get; private set; }`, assigned in `_Ready`.
- Put interaction and presentation scripts under `Scripts/UI`, character logic under `Scripts/Characters`, and dungeon generation under `Scripts/Dungeon`.
- Keep spell/action logic, spell effects, keywords, and status-effect logic in their respective folders under `Scripts/Actions/Behaviours`.
- Extend the existing behaviour interfaces and base classes (`IActionBehaviour`, `ISpellBehaviour`, `DefaultSpellBehaviour`, and `BaseStatusEffectBehaviour`) for new effects. Register implementations in the relevant centralized lookup.
- Keep reusable helpers in focused extension classes under `Scripts/Utility`.
- Give methods one clear job. Extract named methods for initialization, signal wiring, state updates, and UI updates, following methods such as `InitializeCustomSignals` and `UpdateSpellListUI`.

## Centralized content and configuration

- Store content in centralized places. Keep editable spell definitions, character stats, personality traits, textures, sounds, and similar content in Godot resources under `Resources` and `Assets`.
- Separate resource data from runtime behaviour. Follow `ActionData` and `CharacterData` for data, and the existing behaviour classes for custom logic.
- Use `[GlobalClass]` for resource types that should be available in the editor. Use `[Export]`, `[ExportGroup]`, and property hints to make configuration clear in the Inspector.
- Use resource preloaders and the existing registries for available content and starting builds. Put a new definition in its appropriate resource or registry rather than duplicating it in consumers.
- Keep reusable UI layouts and visual components in `.tscn` scenes under `Scenes`; keep level composition under `Levels`. Instantiate reusable scenes for lists, queues, opponents, and effects.
- Reuse themes, style boxes, materials, and shaders under `Resources` for shared presentation. Centralize shared domain colors and formatting through helpers such as `DomainExtensions`.

## Godot nodes and UI references

- Declare Godot script classes `partial` and derive them from the appropriate Godot type.
- Use exported node, resource, and `PackedScene` references for editor-configured dependencies.
- Use `ComponentExposer` for reusable scenes that expose UI components. Define their names as constants in the appropriate `public static partial class Components` file under `Scripts/UI/Exposers`, then retrieve them with `GetComponent<T>`.
- Cache resolved node references instead of repeatedly searching the scene tree during updates. Keep scene-tree lookups in initialization or focused accessors.
- Initialize local node references in `_Ready`. Defer setup that depends on other managers being ready with `CallDeferred`, following the existing initialization pattern.
- Use signals and named handlers to connect interaction to state changes. Use Godot C# signals with `[Signal]` and an `...EventHandler` delegate when exposing a signal to the engine.
- Use `QueueFree()` for scene nodes that are being removed.

## State, errors, and asynchronous flow

- Prefer properties for runtime state and computed accessors for derived values. Use `private set` or `protected set` when callers should read state without changing it directly. Fields remain appropriate for exported configuration and existing data containers.
- Validate missing references, invalid targets, and unmet prerequisites before performing an action. Follow the existing guard-clause pattern with a clear return value or early return.
- Report unexpected setup or programming problems through Godot logging such as `GD.PrintErr`. Use `BattleLogManager` for player-facing combat events.
- Keep combat resolution in the existing action/queue/result pipeline. Use `ResolveResult`, `Damage`, and `DamageModifier` rather than implementing parallel damage calculations in UI code.
- Use `Task`/`Task<T>` for asynchronous operations that callers may need to await. Await operations whose completion determines the next gameplay step. Use an explicit `_ = ...` only when intentionally starting an independent operation.
- Use Godot signals, tweens, and the `NodeExtensions.Delay` helper for animation and scene timing. Keep gameplay waits non-blocking.
- Preserve virtual hooks and interface contracts when extending behaviours. The existing `.editorconfig` silences `CS1998` for asynchronous hooks whose default implementation has no work to await.

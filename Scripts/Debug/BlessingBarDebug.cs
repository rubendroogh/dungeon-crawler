using Godot;
using System;

public partial class BlessingBarDebug : TextureProgressBar
{
    [Export]
    private HBoxContainer BlessingsContainer;

    [Export]
    private PackedScene BlessingUIScene;

    public override void _Ready()
    {
        // TODO: Move the setup and instantiation to the AddBlessing method in ManaSourceManager.
        var b1 = BlessingUIScene.Instantiate<BlessingUI>().Setup(new Blessing
        (
            DungeonRPG.Blessings.Enums.Level.Minor,
            DungeonRPG.Blessings.Enums.Domain.Zer
        ));

        var b2 = BlessingUIScene.Instantiate<BlessingUI>().Setup(new Blessing
        (
            DungeonRPG.Blessings.Enums.Level.Major,
            DungeonRPG.Blessings.Enums.Domain.Calina
        ));

        var b3 = BlessingUIScene.Instantiate<BlessingUI>().Setup(new Blessing
        (
            DungeonRPG.Blessings.Enums.Level.Superior,
            DungeonRPG.Blessings.Enums.Domain.Jaddis
        ));

        var b4 = BlessingUIScene.Instantiate<BlessingUI>().Setup(new Blessing
        (
            DungeonRPG.Blessings.Enums.Level.Minor,
            DungeonRPG.Blessings.Enums.Domain.Hamin
        ));

        BlessingsContainer.AddChild(b1);
        BlessingsContainer.AddChild(b2);
        BlessingsContainer.AddChild(b3);
        BlessingsContainer.AddChild(b4);

        Managers.ManaSourceManager.AddBlessing(b1.Blessing);
        Managers.ManaSourceManager.AddBlessing(b2.Blessing);
        Managers.ManaSourceManager.AddBlessing(b3.Blessing);
        Managers.ManaSourceManager.AddBlessing(b4.Blessing);
    }
}

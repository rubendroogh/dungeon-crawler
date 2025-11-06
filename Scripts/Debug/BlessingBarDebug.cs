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
        var b1 = BlessingUIScene.Instantiate<BlessingUI>().Setup(new Blessing
        {
            Level = DungeonRPG.Blessings.Enums.Level.Minor,
            Domain = DungeonRPG.Blessings.Enums.Domain.Zer
        });

        var b2 = BlessingUIScene.Instantiate<BlessingUI>().Setup(new Blessing
        {
            Level = DungeonRPG.Blessings.Enums.Level.Major,
            Domain = DungeonRPG.Blessings.Enums.Domain.Calina
        });

        var b3 = BlessingUIScene.Instantiate<BlessingUI>().Setup(new Blessing
        {
            Level = DungeonRPG.Blessings.Enums.Level.Superior,
            Domain = DungeonRPG.Blessings.Enums.Domain.Jaddis
        });

        var b4 = BlessingUIScene.Instantiate<BlessingUI>().Setup(new Blessing
        {
            Level = DungeonRPG.Blessings.Enums.Level.Greater,
            Domain = DungeonRPG.Blessings.Enums.Domain.Hamin
        });

        BlessingsContainer.AddChild(b1);
        BlessingsContainer.AddChild(b2);
        BlessingsContainer.AddChild(b3);
        BlessingsContainer.AddChild(b4);
    }
}

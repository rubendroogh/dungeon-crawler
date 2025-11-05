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

        BlessingsContainer.AddChild(b1);
        BlessingsContainer.AddChild(b2);
    }
}

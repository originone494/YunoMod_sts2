using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace YunoMod.Scripts;

public partial class NYunoCharacter_Merchant : NMerchantCharacter
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PlayAnimation("default", loop: true);

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}

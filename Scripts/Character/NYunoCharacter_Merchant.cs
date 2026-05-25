using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace YunoMod.Scripts;

public partial class NYunoCharacter_Merchant : NMerchantCharacter
{
    public override void _Ready()
    {
        try
        {
            PlayAnimation("default", loop: true);
        }
        catch (InvalidOperationException) { }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}

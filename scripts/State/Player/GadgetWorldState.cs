using BmSDK.BmGame;

namespace Samuil1337.CharacterSwapping.State
{
    sealed class GadgetWorldState : IStateComponent
    {
        RFloatingRaft[] _rafts = [];

        public void CaptureState(SwitchContext ctx)
        {
            _rafts = [.. ctx.Rpp.IceRafts];
        }

        public void ApplyState(SwitchContext ctx)
        {
            ctx.Rpp.IceRafts = [.. _rafts];
        }
    }
}

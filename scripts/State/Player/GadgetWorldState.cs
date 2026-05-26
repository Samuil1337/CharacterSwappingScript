using BmSDK.BmGame;

namespace Samuil1337.CharacterSwapping.State
{
    sealed class GadgetWorldState : IStateComponent
    {
        RProjectile[] _projectiles = [];
        RFloatingRaft[] _rafts = [];

        public void CaptureState(SwitchContext ctx)
        {
            var rpp = ctx.Rpp;

            _projectiles = [.. rpp.SpawnedProjectiles];
            rpp.SpawnedProjectiles.Clear();
            _rafts = [.. rpp.IceRafts];
        }

        public void ApplyState(SwitchContext ctx)
        {
            var rpp = ctx.Rpp;

            rpp.SpawnedProjectiles.CopyFrom(_projectiles);
            rpp.IceRafts = [.. _rafts];
            foreach (var raft in _rafts)
            {
                raft.Player = rpp;
            }

            foreach (var trap in Game.FindObjects<RThugTrap>())
            {
                if (!trap.IsValid || trap.IsClassDefaultObject)
                {
                    continue;
                }

                trap.Owner = rpp;
            }
        }
    }
}

using BmSDK.BmGame;
using BmSDK.Engine;

namespace Samuil1337.CharacterSwapping.State.Player
{
    sealed class ForensicDeviceState : IStateComponent
    {
        bool _isInvestigateMode;
        Actor[]? _jammingActors;

        public void CaptureState(SwitchContext ctx)
        {
            var rpc = ctx.Rpc;
            _isInvestigateMode = rpc.bInvestigateMode;
            _jammingActors = rpc.CurrentForensicsDevice?.JammingActors?.ToArray();
        }

        public void ApplyState(SwitchContext ctx)
        {
            var rpc = ctx.Rpc;
            if (rpc.CurrentForensicsDevice is null)
            {
                return;
            }

            var device = rpc.CurrentForensicsDevice;
            if (_jammingActors is not null)
            {
                device.JammingActors = [.. _jammingActors];
            }
            else
            {
                foreach (var villain in Game.FindObjects<RPawnVillain>())
                {
                    if (!villain.IsValid || villain.IsClassDefaultObject)
                    {
                        continue;
                    }

                    // Method ensures that the actor is actually a jammer
                    device.AddActorToJammerList(villain);
                }
            }

            if (device.bCanUseForensicsDeviceDirectly)
            {
                rpc.bInvestigateMode = _isInvestigateMode;
            }
        }
    }
}

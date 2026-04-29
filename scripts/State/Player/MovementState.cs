using System.Numerics;
using BmSDK;

namespace Samuil1337.CharacterSwapping.State.Player
{
    sealed class MovementState : IStateComponent
    {
        // Camera position
        Vector3 _rpcLoc;
        Rotator _rpcRot;

        // Pawn position
        Vector3 _rppLoc;
        Rotator _rppRot;

        public void CaptureState(SwitchContext ctx)
        {
            _rpcLoc = ctx.Rpc.Location;
            _rpcRot = ctx.Rpc.Rotation;
            _rppLoc = ctx.Rpp.Location;
            _rppRot = ctx.Rpp.Rotation;
        }

        public void ApplyState(SwitchContext ctx)
        {
            ctx.Rpc.SetLocation(_rpcLoc);
            ctx.Rpc.SetRotation(_rpcRot);
            ctx.Rpp.SetLocation(_rppLoc);
            ctx.Rpp.SetRotation(_rppRot);
        }
    }
}

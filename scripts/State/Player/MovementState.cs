using System.Numerics;
using BmSDK;

namespace Samuil1337.CharacterSwapping.State.Player
{
    sealed class MovementState : IStateComponent
    {
        Vector3 _rpcLoc;
        Vector3 _rppLoc;
        Rotator _rpcRot;
        Rotator _rppRot;

        public void CaptureState(SwitchContext ctx)
        {
            _rpcLoc = ctx.Rpc.Location;
            _rppLoc = ctx.Rpp.Location;
            _rpcRot = ctx.Rpc.Rotation;
            _rppRot = ctx.Rpp.Rotation;
        }

        public void ApplyState(SwitchContext ctx)
        {
            ctx.Rpc.SetLocation(_rpcLoc);
            ctx.Rpp.SetLocation(_rppLoc);
            ctx.Rpc.SetRotation(_rpcRot);
            ctx.Rpp.SetRotation(_rppRot);
        }
    }
}

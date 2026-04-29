using static BmSDK.BmGame.RPawnPlayer;

namespace Samuil1337.CharacterSwapping.State.Player
{
    sealed class HealthState : IStateComponent
    {
        int _health;
        int _meleeArmor;
        int _ballisticArmor;
        int _cwMeleeArmor;
        int _cwBallisticArmor;

        public void CaptureState(SwitchContext ctx)
        {
            // Sync data from RPP to PData
            var rpp = ctx.Rpp;
            rpp.HealthUpdated();
            rpp.SetPersistentMeleeArmour(rpp.CurrentArmourLevels[(int)EArmourType.EA_ArmourMelee]);
            rpp.SetPersistentBallisticArmour(
                rpp.CurrentArmourLevels[(int)EArmourType.EA_ArmourBallistic]
            );

            var pData = ctx.PData;
            _health = pData.PlayerHealth;
            _meleeArmor = pData.MeleeArmour;
            _ballisticArmor = pData.BallisticArmour;
            _cwMeleeArmor = pData.CWMeleeArmour;
            _cwBallisticArmor = pData.CWBallisticArmour;
        }

        public void ApplyState(SwitchContext ctx)
        {
            // Transfer health in save data
            var pData = ctx.PData;
            pData.PlayerHealth = _health;
            pData.MeleeArmour = _meleeArmor;
            pData.BallisticArmour = _ballisticArmor;
            pData.CWMeleeArmour = _cwMeleeArmor;
            pData.CWBallisticArmour = _cwBallisticArmor;

            // Apply to pawn
            var rpp = ctx.Rpp;
            rpp.Health = _health;
            rpp.HealthUpdated();
            rpp.SetArmourCurrent(EArmourType.EA_ArmourMelee, rpp.GetPersistentMeleeArmour());
            rpp.SetArmourCurrent(
                EArmourType.EA_ArmourBallistic,
                rpp.GetPersistentBallisticArmour()
            );

            // Refresh HUD
            ctx.Rpc.ShowHealthBar(ctx.Rpc.HudMovieSide);
        }
    }
}

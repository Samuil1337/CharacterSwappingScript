using BmSDK.BmGame;

namespace Samuil1337.CharacterSwapping.State
{
    sealed class PersistentSmokeBomb(PersistentInventory inventory) : IPersistentGadget
    {
        readonly PersistentInventory _inventory = inventory;
        float _currentRechargeTime;
        bool _isRecharging;

        RSmokeBomb? GetSmokeBomb() => _inventory.Owner?.CombatPawn?.GadgetList_10 as RSmokeBomb;

        public bool IsActive => GetSmokeBomb() is not null;

        public void CaptureState()
        {
            var bomb = GetSmokeBomb()!;
            _currentRechargeTime = bomb.CurrentRechargeTime;
            _isRecharging = bomb.bRecharging;
        }

        public void OverworldTick(float dt)
        {
            if (!_isRecharging || _currentRechargeTime <= 0f)
            {
                return;
            }

            _currentRechargeTime -= dt;
        }

        public void OnRoomChange() => RestockAmmo();

        public void RestockAmmo() => _currentRechargeTime = 0f;

        public void ApplyState(bool isOverworld)
        {
            if (_currentRechargeTime <= 0f)
            {
                return;
            }

            var bomb = GetSmokeBomb()!;
            bomb.UpdateAllowRecharge();
            bomb.CurrentRechargeTime = Math.Max(0f, _currentRechargeTime);
            bomb.UpdateGadgetHUDParams();
        }
    }
}

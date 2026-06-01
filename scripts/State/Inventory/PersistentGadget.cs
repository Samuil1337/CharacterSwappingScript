using BmSDK.BmGame;

namespace Samuil1337.CharacterSwapping.State
{
    /// <summary>
    /// Declares the contract for gadgets with consumable ammo which should persist switches.
    /// </summary>
    interface IPersistentGadget
    {
        /// <summary>
        /// Whether the game's representation of the gadget is currently alive. Should usually
        /// correlate to whether the <see cref="RPawnPlayer.GadgetList"/> index is null.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Called right before a player switch occurs, so the tracked gadget
        /// should still be alive. Saves the ammo, max ammo and cooldown period.
        /// </summary>
        void CaptureState();

        /// <summary>
        /// Called each game tick while the actual gadget is not alive anymore and
        /// the player is in the overworld. Updates ammo and cooldowns.
        /// </summary>
        /// <param name="dt">Delta between the last tick and this one</param>
        void OverworldTick(float dt);

        /// <summary>
        /// Called every time a new area inside of interiors is entered while the
        /// actual gadget is not alive anymore. Updates ammo and cooldowns.
        /// </summary>
        void OnRoomChange();

        /// <summary>
        /// Max out ammo and reset cooldowns while the actual gadget is not alive anymore.
        /// </summary>
        void RestockAmmo();

        /// <summary>
        /// Called after a player switch occurred, so the tracked gadget should be alive again.
        /// Applies the new ammo and cooldowns to the game's gadget representation.
        /// </summary>
        void ApplyState(bool isOverworld);
    }

    sealed class PersistentJammer(PersistentInventory inventory) : IPersistentGadget
    {
        public bool IsActive => GetJammer() is not null;
        readonly PersistentInventory _inventory = inventory;
        int _maxAmmo;
        int _ammo;
        float _replenishTime;
        float _currentReplenishTime;

        public void CaptureState()
        {
            var jammer = GetJammer()!;
            _maxAmmo = jammer.MaxAmmo;
            _ammo = jammer.Ammo;
            _replenishTime = jammer.ReplenishTime;
            _currentReplenishTime = jammer.CurrentRechargeTime;
        }

        public void OverworldTick(float dt)
        {
            if (_ammo >= _maxAmmo)
            {
                return;
            }

            _currentReplenishTime -= dt;

            if (_currentReplenishTime <= 0)
            {
                _ammo++;
                _currentReplenishTime = _replenishTime;
            }
        }

        public void OnRoomChange() => RestockAmmo();

        public void RestockAmmo() => _ammo = _maxAmmo;

        public void ApplyState(bool isOverworld)
        {
            // Gadgets are instantiated with max ammo; nothing to do
            if (_ammo >= _maxAmmo)
            {
                return;
            }

            var jammer = GetJammer()!;
            jammer.Ammo = _ammo;
            jammer.UpdateGadgetHUDParams();

            if (isOverworld)
            {
                jammer.InterThrowRechargeTime = _replenishTime;
                jammer.CurrentRechargeTime = _currentReplenishTime;
                jammer.SetTickIsDisabled(false);
                jammer.SetTimer(_currentReplenishTime, false, "ReplenishAmmo");
            }
            else
            {
                jammer.InterThrowRechargeTime = -1;
                jammer.CurrentRechargeTime = 0;
            }
        }

        RJammerGadget? GetJammer() => _inventory.Owner?.CombatPawn?.JammerGadget;
    }

    abstract class PersistentGadget(PersistentInventory inventory) : IPersistentGadget
    {
        public bool IsActive => GetGadget() is not null;
        protected readonly PersistentInventory Inventory = inventory;
        protected int MaxAmmo { get; set; }
        protected int Ammo { get; set; }
        protected float ReplenishTime { get; set; }
        protected float CurrentReplenishTime { get; set; }

        public abstract void CaptureState();
        public abstract void ApplyState(bool isOverworld);
        protected abstract RInventoryGadget? GetGadget();

        public void OverworldTick(float dt)
        {
            if (Ammo >= MaxAmmo)
            {
                return;
            }

            CurrentReplenishTime -= dt;

            if (CurrentReplenishTime <= 0)
            {
                Ammo++;
                CurrentReplenishTime = ReplenishTime;
            }
        }

        public void OnRoomChange() => RestockAmmo();

        public void RestockAmmo() => Ammo = MaxAmmo;
    }
}

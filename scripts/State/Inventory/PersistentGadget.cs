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

    abstract class PersistentGadget<TGadget>(PersistentInventory inventory) : IPersistentGadget
        where TGadget : RInventoryGadget
    {
        protected readonly PersistentInventory Inventory = inventory;
        protected int MaxAmmo { get; set; }
        protected int Ammo { get; set; }
        protected float ReplenishTime { get; set; }
        protected float CurrentReplenishTime { get; set; }

        public bool IsActive => GetGadget() is not null;

        /// <summary>
        /// Gets the game's representation of the gadget.
        /// May be null if the instance is not alive anymore.
        /// </summary>
        protected abstract TGadget? GetGadget();

        public abstract void CaptureState();
        public abstract void ApplyState(bool isOverworld);

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

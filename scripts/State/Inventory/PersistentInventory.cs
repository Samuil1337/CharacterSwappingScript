using BmSDK.BmGame;

namespace Samuil1337.CharacterSwapping.State
{
    [ScriptComponent(AutoAttach = true)]
    sealed class PersistentInventory : ScriptComponent<RPlayerControllerCombat>
    {
        readonly IPersistentGadget[] _gadgets;

        public PersistentInventory()
        {
            _gadgets = [new PersistentJammer(this)];
        }

        public void CaptureState()
        {
            foreach (var gadget in _gadgets.Where(g => g.IsActive))
            {
                gadget.CaptureState();
            }
        }

        public override void OnTick()
        {
            if (!Game.GetGameRI().IsOverworldGameplay())
            {
                return;
            }

            var dt = Game.GetDeltaTime();
            foreach (var gadget in _gadgets.Where(g => !g.IsActive))
            {
                gadget.OverworldTick(dt);
            }
        }

        [ComponentRedirect(nameof(RPlayerControllerCombat.OnRoomChange))]
        void OnRoomChange()
        {
            Owner.OnRoomChange();
            if (Game.GetGameRI().IsOverworldGameplay())
            {
                return;
            }

            foreach (var gadget in _gadgets.Where(g => !g.IsActive))
            {
                gadget.OnRoomChange();
            }
        }

        [Redirect(typeof(RInventoryManager), nameof(RInventoryManager.RestockAmmo))]
        static void RInventoryManagerRestockAmmo(RInventoryManager self)
        {
            self.RestockAmmo();
            if (self.Owner is not RPawnPlayer rpp)
            {
                return;
            }

            var inventory = rpp.PlayerController?.GetScriptComponent<PersistentInventory>();
            inventory?.RestockAmmo();
        }

        public void RestockAmmo()
        {
            foreach (var gadget in _gadgets.Where(g => !g.IsActive))
            {
                gadget.RestockAmmo();
            }
        }

        public void ApplyState()
        {
            var isOverworld = Game.GetGameRI().IsOverworldGameplay();
            foreach (var gadget in _gadgets.Where(g => g.IsActive))
            {
                gadget.ApplyState(isOverworld);
            }
        }
    }
}

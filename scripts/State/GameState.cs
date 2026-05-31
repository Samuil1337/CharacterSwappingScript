namespace Samuil1337.CharacterSwapping.State
{
    /// <summary>
    /// Contract for DTO's copying over game state between switches.
    /// </summary>
    interface IStateComponent
    {
        void CaptureState(SwitchContext ctx);
        void ApplyState(SwitchContext ctx);
    }

    /// <summary>
    /// Represents a Data Transfer Object of the entire game state, including player position,
    /// health and animations. Use this record to snapshot and restore a player's state
    /// when switching characters.
    /// </summary>
    sealed class GameState : IStateComponent
    {
        readonly IStateComponent[] _components =
        [
            // Player state
            new MovementState(),
            new HealthState(),
            new ForensicDeviceState(),
            // World state
            new ProjectilesState(),
        ];

        internal GameState(SwitchContext ctx) => CaptureState(ctx);

        public void CaptureState(SwitchContext ctx)
        {
            ctx.Rpc.GetScriptComponent<PersistentInventory>()?.CaptureState();
            foreach (var component in _components)
            {
                component.CaptureState(ctx);
            }
        }

        public void ApplyState(SwitchContext ctx)
        {
            ctx.Rpc.GetScriptComponent<PersistentInventory>()?.ApplyState();
            foreach (var component in _components)
            {
                component.ApplyState(ctx);
            }
        }
    }
}

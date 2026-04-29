using Samuil1337.CharacterSwapping.State.Player;

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
        readonly List<IStateComponent> _stateComponents =
        [
            new MovementState(),
            new HealthState(),
            new ForensicDeviceState(),
        ];

        internal GameState(SwitchContext ctx) => CaptureState(ctx);

        public void CaptureState(SwitchContext ctx)
        {
            foreach (var component in _stateComponents)
            {
                component.CaptureState(ctx);
            }
        }

        public void ApplyState(SwitchContext ctx)
        {
            foreach (var component in _stateComponents)
            {
                component.ApplyState(ctx);
            }
        }
    }
}

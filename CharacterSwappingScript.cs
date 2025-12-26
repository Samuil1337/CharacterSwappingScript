using BmSDK;
using BmSDK.BmGame;
using BmSDK.BmScript;
using BmSDK.Engine;

[Script]
public class CharacterSwappingScript : Script
{
    /// <summary>
    /// Specifies the available playable characters in the game.
    /// </summary>
    /// <remarks>The underlying integral values correspond to the game's internal BaseIds.
    /// They are useless when you want to manually load in characters
    /// but the game uses them to register DLC skins.</remarks>
    private enum PlayableCharacter
    {
        BruceWayne = -1,
        Batman = 0,
        Catwoman = 1,
        Robin = 2,
        Nightwing = 3,
    }

    /// <summary>
    /// Represents information about a playable character that's necessary for loading them in.
    /// </summary>
    /// <param name="BaseId">The unique identifier of the character used by this script.</param>
    /// <param name="CharacterName">The name of the character used internally (i.e. the value of <see cref="RPawnPlayerCombat.CharacterName"/>).</param>
    /// <param name="Base">The prefix that every relevant package starts with (e.g. Playable_Batman)</param>
    /// <param name="Skin">The specific variant of the character (e.g. Std, Animated)</param>
    private sealed record CharacterInfo(
        PlayableCharacter BaseId,
        string CharacterName,
        string Base,
        string Skin
    )
    {
        /// <summary>
        /// Gets the name of the package which defines the characters animations, gadgets and moves.
        /// </summary>
        public string BasePackage => Base + "_SF";
        /// <summary>
        /// Gets the unique identifier for the skin, used by <see cref="RGameInfo.LoadPC(FName, int)"/>
        /// to create the desired character with the proper skin.
        /// </summary>
        public string SkinIdentifier => Base + "_" + Skin;
        /// <summary>
        /// Gets the name of the package which contains the corresponding skin.
        /// </summary>
        public string SkinPackage => SkinIdentifier + "_SF";
    }

    /// <summary>
    /// Provides a read-only mapping of each playable character to its associated character information.
    /// </summary>
    /// <remarks>The dictionary contains predefined entries for all supported playable characters. The
    /// collection is immutable and cannot be modified at runtime.</remarks>
    private static readonly IReadOnlyDictionary<PlayableCharacter, CharacterInfo> Characters =
        new Dictionary<PlayableCharacter, CharacterInfo>
        {
            [PlayableCharacter.BruceWayne] = new CharacterInfo(
                BaseId: PlayableCharacter.BruceWayne,
                CharacterName: "Bruce_Wayne",
                Base: "Playable_BruceWayne",
                Skin: "Std"
            ),
            [PlayableCharacter.Batman] = new CharacterInfo(
                BaseId: PlayableCharacter.Batman,
                CharacterName: "Batman",
                Base: "Playable_Batman",
                Skin: "Std"
            ),
            [PlayableCharacter.Catwoman] = new CharacterInfo(
                BaseId: PlayableCharacter.Catwoman,
                CharacterName: "Catwoman",
                Base: "Playable_Catwoman",
                Skin: "Std"
            ),
            [PlayableCharacter.Robin] = new CharacterInfo(
                BaseId: PlayableCharacter.Robin,
                CharacterName: "Robin",
                Base: "Playable_Robin",
                Skin: "Std"
            ),
            [PlayableCharacter.Nightwing] = new CharacterInfo(
                BaseId: PlayableCharacter.Nightwing,
                CharacterName: "Nightwing",
                Base: "Playable_Nightwing",
                Skin: "Std"
            ),
        };

    private void SwapCharacter(PlayableCharacter character)
    {
        // Make sure swapping is safe
        var rpc = Game.GetPlayerController();
        var rpp = rpc.CombatPawn;
        var wi = Game.GetWorldInfo();
        if (!rpc.IsValid() || rpp == null || !rpp.IsValid()) return;
        if (!wi.IsValid()) return;
        // Make sure swapping is necessary
        var charInfo = Characters[character];
        if (rpp.CharacterName == charInfo.CharacterName) return;

        // Load assets
        Game.LoadPackage(charInfo.BasePackage);
        Game.LoadPackage(charInfo.SkinPackage);
        gi.LoadPC(charInfo.SkinIdentifier);  // TODO(Samuil1337): Update DamageLevel properly

        // Switch character
        var act = new RSeqAct_SwitchPlayerCharacter(wi)
        {
            CharacterName = charInfo.CharacterName,
            PlayerStartPoint = Game.SpawnActor<PlayerStart>(rpp.Location, rpp.Rotation),
        };
        rpc.PrepareForPlayerSwitch();   // Resets HUD
        act.RestartPlayer(rpc); // Performs switch of Pawn
        rpp.Destroy();  // Removes old RPawnPlayer
    }

    public override void OnKeyDown(Keys key)
    {
        switch (key)
        {
            case Keys.F1:
                SwapCharacter(PlayableCharacter.Batman);
                break;
            case Keys.F2:
                SwapCharacter(PlayableCharacter.Catwoman);
                break;
            case Keys.F3:
                SwapCharacter(PlayableCharacter.Robin);
                break;
            case Keys.F4:
                SwapCharacter(PlayableCharacter.Nightwing);
                break;
            case Keys.F5:
                SwapCharacter(PlayableCharacter.BruceWayne);
                break;
        }
    }
}

using BmSDK.BmGame;

namespace Samuil1337.CharacterSwapping.Data
{
    /// <summary>
    /// Specifies the available playable characters in the game.
    /// </summary>
    /// <remarks>The underlying integral values correspond to the game's internal BaseIds.
    /// They are useless when you want to manually load in characters
    /// but the game uses them to register DLC skins.</remarks>
    enum PlayableCharacter
    {
        BruceWayne = -1,
        Batman = 0,
        Catwoman = 1,
        Robin = 2,
        Nightwing = 3,
    }

    static class CharacterRegistry
    {
        /// <summary>
        /// Provides a read-only mapping of each playable character to its associated character information.
        /// This is useful for getting data necessary for switching characters.
        /// </summary>
        static readonly Dictionary<PlayableCharacter, CharacterInfo> s_characters = new()
        {
            [PlayableCharacter.BruceWayne] = new(
                BaseId: PlayableCharacter.BruceWayne,
                CharacterName: "Bruce_Wayne",
                Base: "Playable_BruceWayne"
            ),
            [PlayableCharacter.Batman] = new(
                BaseId: PlayableCharacter.Batman,
                CharacterName: "Batman",
                Base: "Playable_Batman"
            ),
            [PlayableCharacter.Catwoman] = new(
                BaseId: PlayableCharacter.Catwoman,
                CharacterName: "Catwoman",
                Base: "Playable_Catwoman"
            ),
            [PlayableCharacter.Robin] = new(
                BaseId: PlayableCharacter.Robin,
                CharacterName: "Robin",
                Base: "Playable_Robin",
                DlcBase: "Playable_RobinStoryDLC"
            ),
            [PlayableCharacter.Nightwing] = new(
                BaseId: PlayableCharacter.Nightwing,
                CharacterName: "Nightwing",
                Base: "Playable_Nightwing"
            ),
        };

        static readonly Dictionary<string, CharacterInfo> s_charactersByName =
            s_characters.ToDictionary(kvp => kvp.Value.CharacterName, kvp => kvp.Value);

        internal static CharacterInfo ByEnum(PlayableCharacter character) =>
            s_characters[character];

        internal static CharacterInfo? ByName(string? name)
        {
            if (name is not null)
            {
                if (s_charactersByName.TryGetValue(name, out var result))
                {
                    return result;
                }
            }

            return null;
        }

        internal static CharacterInfo? ByPawn(RPawnPlayer? rpp) => ByName(rpp?.CharacterName);
    }
}

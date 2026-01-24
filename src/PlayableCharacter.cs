namespace CharacterSwapping;

/// <summary>
/// Specifies the available playable characters in the game.
/// </summary>
/// <remarks>The underlying integral values correspond to the game's internal BaseIds.
/// They are useless when you want to manually load in characters
/// but the game uses them to register DLC skins.</remarks>
public enum PlayableCharacter
{
    BruceWayne = -1,
    Batman = 0,
    Catwoman = 1,
    Robin = 2,
    Nightwing = 3,
}

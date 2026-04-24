namespace Samuil1337.CharacterSwapping;

/// <summary>
/// Represents information about a playable character that's necessary for loading them in.
/// </summary>
/// <param name="BaseId">The unique identifier of the character used by this script.</param>
/// <param name="CharacterName">The name of the character used internally
/// (i.e. the value of <see cref="RPawnPlayerCombat.CharacterName"/>).</param>
/// <param name="Base">The prefix that every relevant package starts with (e.g. Playable_Batman)</param>
/// <param name="DlcBase">The prefix that packages for the Harley Quinn's Revenge use (i.e. Playable_RobinStoryDLC).
/// Only necessary to set for Robin as the game crashes if the wrong character is loaded in HQR.
/// Keep it null for the others.</param>
sealed record CharacterInfo(
    PlayableCharacter BaseId,
    string CharacterName,
    string Base,
    string? DlcBase = null
)
{
    const string PkgSuffix = "_SF";
    const string StdSkinSuffix = "_Std";
    const string AnimatedSkinSuffix = "_Animated";

    static string BuildPkg(string name) => name + PkgSuffix;

    /// <summary>
    /// Gets the name of the package which defines the characters animations, gadgets and moves.
    /// </summary>
    public string BasePkg => BuildPkg(Base);

    /// <summary>
    /// Gets the name of the base package specific to the StoryDLC (PDLC)
    /// and returns the standard base package if no DLC base is defined/necessary.
    /// </summary>
    public string DlcBasePkg => DlcBase is not null ? BuildPkg(DlcBase) : BasePkg;

    /// <summary>
    /// Gets the unique identifier for the skin, used by <see cref="RGameInfo.LoadPC(FName, int)"/>
    /// to create the desired character with the proper skin.
    /// </summary>
    public string SkinId
    {
        get
        {
            // If base and skin are the same, the default's chosen
            var selection = GameFunctions.PlayerChosenSkinName;
            if (selection == Base)
            {
                return Base + StdSkinSuffix;
            }

            // If skin is specific to character, return it
            if (selection.StartsWith(Base))
            {
                return selection;
            }

            // Animated skins apply to other characters
            if (BaseId is not PlayableCharacter.BruceWayne)
            {
                if (selection.EndsWith(AnimatedSkinSuffix))
                {
                    return Base + AnimatedSkinSuffix;
                }
            }

            // Default to standard skin
            return Base + StdSkinSuffix;
        }
    }

    /// <summary>
    /// Gets the name of the package which contains the corresponding skin.
    /// </summary>
    public string GetSkinPkg(int damageLevel)
    {
        if (damageLevel != 0)
        {
            return BuildPkg(SkinId + $"_{damageLevel}");
        }

        return BuildPkg(SkinId);
    }
}

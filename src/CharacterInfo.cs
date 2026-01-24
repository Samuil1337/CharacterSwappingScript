namespace CharacterSwapping;

/// <summary>
/// Represents information about a playable character that's necessary for loading them in.
/// </summary>
/// <param name="BaseId">The unique identifier of the character used by this script.</param>
/// <param name="CharacterName">The name of the character used internally (i.e. the value of <see cref="RPawnPlayerCombat.CharacterName"/>).</param>
/// <param name="Base">The prefix that every relevant package starts with (e.g. Playable_Batman)</param>
/// <param name="Skin">The specific variant of the character (e.g. Std, Animated)</param>
/// <param name="DlcBase">The prefix that packages for the StoryDLC/PDCL (Harley Quinn's Revenge) use (i.e. Playable_RobinStoryDLC).
/// This is only necessary to set for Robin as the game crashes if the wrong character is loaded in the StoryDLC.
/// Keep it null for the others.</param>
public sealed record CharacterInfo(
    PlayableCharacter BaseId,
    string CharacterName,
    string Base,
    string Skin,
    string? DlcBase = null
)
{
    private const string PkgSuffix = "_SF";
    public const string StdSkin = "Std";
    public const string AnimatedSkin = "Animated";

    private static string BuildPkg(string name) => name + PkgSuffix;
    private static string BuildId(string prefix, string name) => prefix + "_" + name;

    /// <summary>
    /// Gets the name of the package which defines the characters animations, gadgets and moves.
    /// </summary>
    public string BasePkg => BuildPkg(Base);
    /// <summary>
    /// Gets the unique identifier for the skin, used by <see cref="RGameInfo.LoadPC(FName, int)"/>
    /// to create the desired character with the proper skin.
    /// </summary>
    public string SkinId => BuildId(Base, Skin);
    /// <summary>
    /// Gets the name of the package which contains the corresponding skin.
    /// </summary>
    public string SkinPkg => BuildPkg(SkinId);
    /// <summary>
    /// Returns a boolean indicating whether the current skin is the default skin.
    /// This is useful when applying Damage States as only the standard skins have them.
    /// </summary>
    public bool IsDefaultSkin => Skin == StdSkin;
    /// <summary>
    /// Gets the name of the base package specific to the StoryDLC (PDLC)
    /// and returns the standard base package if no DLC base is defined/necessary.
    /// </summary>
    public string DlcBasePkg => (DlcBase != null)
        ? BuildPkg(DlcBase)
        : BasePkg;
    /// <summary>
    /// Gets the identifier of the PDLC skin associated with the instance--similar use to <see cref="SkinId"/>.
    /// </summary>
    /// <remarks>When in the StoryDLC, the Playable_RobinStoryDLC_SF character has to be used.
    /// He has his own corresponding Std Skin "Playable_RobinStoryDLC_Std_SF" which must be loaded.
    /// However, if one wishes to use a different skin, the "StoryDLC" suffix must be dropped.
    /// This property facilitates that logic.</remarks>
    public string DlcSkinId => (DlcBase != null && IsDefaultSkin)
        ? BuildId(DlcBase, Skin)
        : SkinId;
    /// <summary>
    /// Gets the package name which contains the corresponding skin for the StoryDLC.
    /// </summary>
    public string DlcSkinPkg => BuildPkg(DlcSkinId);
}

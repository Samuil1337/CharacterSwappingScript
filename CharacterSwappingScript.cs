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
    }

    /// <summary>
    /// Represents an immutable Data Transfer Object of a player's state, including position, health and animations.
    /// </summary>
    /// <remarks>Use this record to snapshot and restore a player's state when switching characters.
    /// All values are intended to be consistent with the game world at the time of capture.</remarks>
    private sealed record PlayerState(
        GameObject.FVector RpcLoc, GameObject.FRotator RpcRot,
        GameObject.FVector RppLoc, GameObject.FRotator RppRot,
        int Health,
        int BallisticArmour, int MeleeArmour,
        int CWBallisticArmour, int CWMeleeArmour,
        bool DetectiveVision
    )
    {
        /// <summary>
        /// Creates a new DTO based on the specified RPC and persistent data.
        /// </summary>
        /// <remarks>Should be used before <see cref="DoSwitch(WorldInfo, CharacterInfo, RPawnPlayer, RPlayerController)">
        /// to capture the RPC state right before switching characters.</remarks>
        /// <param name="rpc">The RPC to snapshot before the character switch.</param>
        /// <param name="pData">The persistent data object containing player health and armor values.</param>
        /// <returns>A PlayerState snapshot of the player</returns>
        public static PlayerState FromRpc(RPlayerController rpc, RPersistentData pData)
        {
            var rpp = rpc.CombatPawn;
            return new PlayerState(
                rpc.Location, rpc.Rotation,
                rpp.Location, rpp.Rotation,
                pData.PlayerHealth,
                pData.BallisticArmour, pData.MeleeArmour,
                pData.CWBallisticArmour, pData.CWMeleeArmour,
                rpc.bInvestigateMode
            );
        }

        /// <summary>
        /// Applies the values of the DTO (before the character switch) to the RPC (after the switch),
        /// therefore, allowing for smoother transitions.
        /// </summary>
        /// <param name="rpc">The RPC to restore the state of</param>
        /// <param name="pData">The persistent data object containing player health and armor values.</param>
        public void ApplyToRpc(RPlayerController rpc, RPersistentData pData)
        {
            var rpp = rpc.CombatPawn;
            // RSeqAct_SwitchPlayerCharacter isn't reliable in Challenge Maps,
            // therefore, we must manually override the position
            rpp.SetLocation(RppLoc);
            rpp.SetRotation(RppRot);
            // Makes sure the camera doesn't change with the character swap
            rpc.SetLocation(RpcLoc);
            rpc.SetRotation(RpcRot);
            // Transfer health
            pData.PlayerHealth = Health;
            pData.BallisticArmour = BallisticArmour;
            pData.MeleeArmour = MeleeArmour;
            pData.CWBallisticArmour = CWBallisticArmour;
            pData.CWMeleeArmour = CWMeleeArmour;
            //pData.Armo
            rpp.LoadHealth();
            // Transfer Detective Mode state
            if (rpc.CurrentForensicsDevice?.bCanUseForensicsDeviceDirectly ?? false)
            {
                rpc.bInvestigateMode = DetectiveVision;
            }
            // Transfer movement state
        }
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
                Skin: CharacterInfo.StdSkin
            ),
            [PlayableCharacter.Batman] = new CharacterInfo(
                BaseId: PlayableCharacter.Batman,
                CharacterName: "Batman",
                Base: "Playable_Batman",
                Skin: CharacterInfo.StdSkin
            ),
            [PlayableCharacter.Catwoman] = new CharacterInfo(
                BaseId: PlayableCharacter.Catwoman,
                CharacterName: "Catwoman",
                Base: "Playable_Catwoman",
                Skin: CharacterInfo.StdSkin
            ),
            [PlayableCharacter.Robin] = new CharacterInfo(
                BaseId: PlayableCharacter.Robin,
                CharacterName: "Robin",
                Base: "Playable_Robin",
                Skin: CharacterInfo.StdSkin
            ),
            [PlayableCharacter.Nightwing] = new CharacterInfo(
                BaseId: PlayableCharacter.Nightwing,
                CharacterName: "Nightwing",
                Base: "Playable_Nightwing",
                Skin: CharacterInfo.StdSkin
            ),
        };

    private const bool SpawnEffectEnabled = false;
    private const float SpawnEffectScale = 1.0f;
    private ParticleSystem spawnEffectTemplate;
    // The timer is scaled by seconds
    private const float SwapCooldown = 0f;  // TODO(Samuil1337): Reenable cooldown when done testing
    private float swapCooldownTimer = SwapCooldown;

    public override void Main()
    {
        // TODO(Samuil1337): Find smaller package with the deps or create SF package
        if (!SpawnEffectEnabled) return;
        Game.LoadPackage("Under_C2_Ch5").AddToRoot();
        spawnEffectTemplate = Game.FindObject<ParticleSystem>("FFX_Combat.Particles.NinjaSmokeBomb");
    }

    public override void OnTick()
    {
        // Counts down timer each tick (which only occurs during gameplay)
        swapCooldownTimer -= Game.GetDeltaTime();
    }

    private static bool IsValid(params GameObject[] objects)
    {
        foreach (var obj in objects)
            if (obj == null || !obj.IsValid())
                return false;

        return true;
    }

    private static int GetDamageState(CharacterInfo charInfo, RGameRI gri)
    {
        if (charInfo.BaseId != PlayableCharacter.Batman
            || charInfo.BaseId != PlayableCharacter.Catwoman) return 0;

        var flagMan = gri.FlagManager;
        for (int i = 9; i >= 0; i--)
        {
            if (flagMan.GetGlobalFlag("BatmanDamageLevel" + i))
            {
                return i;
            }
        }
        return 0;
    }

    private static void LoadPackages(CharacterInfo charInfo, RGameInfo gi, RGameRI gri)
    {
        Game.LoadPackage(charInfo.BasePkg);
        Game.LoadPackage(charInfo.SkinPkg);
        gi.LoadPC(charInfo.SkinId, GetDamageState(charInfo, gri));  // TODO(Samuil1337): Update DamageLevel properly
    }

    private static void DoSwitch(WorldInfo wi, CharacterInfo charInfo, RPawnPlayer rpp, RPlayerController rpc)
    {
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

    private void PlayTransitionEffects(WorldInfo wi, RPlayerController rpc)
    {
        var spawnEffect = new ParticleSystemComponent(wi);
        spawnEffect.SetTemplate(spawnEffectTemplate);
        spawnEffect.SetScale(SpawnEffectScale);
        rpc.CombatPawn.AttachComponent(spawnEffect);
        spawnEffect.ActivateSystem();
    }

    private void SwapCharacter(PlayableCharacter character)
    {
        // Make sure swapping is allowed
        if (swapCooldownTimer > 0) return;
        // Make sure swapping is safe
        var rpc = Game.GetPlayerController();
        var rpp = rpc.CombatPawn;
        var gi = Game.GetGameInfo();
        var wi = Game.GetWorldInfo();
        var gri = Game.GetGameRI();
        var pData = Game.GetPersistentData();
        if (!IsValid(rpc, rpp, gi, wi, gri, pData)) return;
        // Make sure swapping is necessary
        var charInfo = Characters[character];
        if (rpp.CharacterName == charInfo.CharacterName) return;

        // Save data that should survive player reinstantiation
        var dto = PlayerState.FromRpc(rpc, pData);

        // Load assets
        LoadPackages(charInfo, gi, gri);

        DoSwitch(wi, charInfo, rpp, rpc);

        // Fix inconsistencies after player switch
        dto.ApplyToRpc(rpc, pData);

        if (SpawnEffectEnabled) PlayTransitionEffects(wi, rpc);

        // Apply swapping cooldown
        swapCooldownTimer = SwapCooldown;
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

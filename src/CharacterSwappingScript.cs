using System.Numerics;
using System.Runtime.InteropServices;
using BmSDK;
using BmSDK.BmGame;
using BmSDK.Engine;

namespace Samuil1337.CharacterSwapping;

[Script(name: "CharacterSwappingScript")]
sealed class CharacterSwappingScript : Script
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate int GetSavedDamageLevelForSkinNameDelegate(FString* skinName);

    public static readonly GetSavedDamageLevelForSkinNameDelegate GetSavedDamageLevelForSkinName =
        Marshal.GetDelegateForFunctionPointer<GetSavedDamageLevelForSkinNameDelegate>(
            MemUtil.GetBaseAddress() + 0x821550
        );

    /// <summary>
    /// Provides a read-only mapping of each playable character to its associated character information.
    /// This is useful for getting data necessary for switching characters.
    /// </summary>
    public static readonly IReadOnlyDictionary<PlayableCharacter, CharacterInfo> Characters =
        new Dictionary<PlayableCharacter, CharacterInfo>
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

    // Smoke effect on character switch
    const string SpawnEffectPkg = "Under_C2_Ch5"; // TODO: Create SF package or load together with Robin
    const string SpawnEffectPath = "FFX_Combat.Particles.NinjaSmokeBomb";
    static readonly bool s_spawnEffectEnabled = false; // TODO: Reenable spawn effect when done testing
    static readonly float s_spawnEffectScale = 1.0f;
    ParticleSystem? _spawnEffectTemplate;

    // Cooldown for character switch
    static readonly bool s_swapCooldownEnabled = false; // TODO: Reenable cooldown when done testing
    static readonly float s_swapCooldown = 5.0f; // The timer is scaled by seconds
    float _swapCooldownTimer = s_swapCooldown;

    public override void Main()
    {
        // Load in spawn effect assets if enabled
        if (s_spawnEffectEnabled)
        {
            Game.LoadPackage(SpawnEffectPkg);
            _spawnEffectTemplate = Game.FindObject<ParticleSystem>(SpawnEffectPath)!;
            _spawnEffectTemplate.AddToRoot();
        }
    }

    public override void OnLoad() => Main();

    public override void OnTick()
    {
        // Counts down timer each tick (which only occurs during gameplay)
        if (s_swapCooldownEnabled)
        {
            _swapCooldownTimer -= Game.GetDeltaTime();
        }
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
            /*case Keys.F5:
                SwapCharacter(PlayableCharacter.BruceWayne);
                break;*/
        }
    }

    void SwapCharacter(PlayableCharacter character)
    {
        // Make sure swapping is allowed
        if (s_swapCooldownEnabled && _swapCooldownTimer > 0)
            return;

        // Acquire important managers
        var wi = Game.GetWorldInfo();
        var gri = Game.GetGameRI();
        var rgi = Game.GetGameInfo();
        var pData = Game.GetPersistentData();
        var rpc = Game.GetPlayerController();
        var rpp = rpc.CombatPawn;
        if (!IsValid(wi, gri, rgi, pData, rpc, rpp))
            return;

        // Make sure swapping is necessary
        var charInfo = Characters[character];
        if (rpp.CharacterName == charInfo.CharacterName)
            return;

        // Make sure swapping is safe
        if (!IsSafeToSwitch(rpc))
            return;

        // Save data that should survive player reinstantiation
        var dto = PlayerState.FromRpc(rpc, pData);

        // Perform the actual switch
        LoadPackages(charInfo, rgi);
        rpp = DoSwitch(charInfo.CharacterName, wi, rpc, rpp);

        // Fix inconsistencies after player switch
        dto.ApplyToRpc(rpc, pData);

        if (s_spawnEffectEnabled)
        {
            PlayTransitionEffects(rpp.Location);
        }

        // Apply swapping cooldown
        if (s_swapCooldownEnabled)
        {
            _swapCooldownTimer = s_swapCooldown;
        }
    }

    static bool IsValid(params GameObject?[] objects) =>
        objects.All(obj => obj != null && obj.IsValid);

    static bool IsSafeToSwitch(RPlayerController rpc)
    {
        if (rpc.bCinematicMode || rpc.bForceCinematicMode)
            return false;
        if (rpc.ActiveCinematicMode != null)
            return false;
        if (rpc.BatmanCutscene != null)
            return false;
        if (rpc.IsPlayingFullScreenMovie())
            return false;
        if (rpc.IsLookInputIgnored())
            return false;
        if (rpc.IsMoveInputIgnored())
            return false;

        return true;
    }

    static void LoadPackages(CharacterInfo charInfo, RGameInfo rgi)
    {
        var basePkg = rgi.bStoryDLC ? charInfo.DlcBasePkg : charInfo.BasePkg;
        Game.LoadPackage(basePkg);

        var damageLevel = GetDamageState(charInfo);
        var skinPkg = charInfo.GetSkinPkg(damageLevel);
        Game.LoadPackage(skinPkg);

        rgi.LoadPC(charInfo.SkinId, damageLevel);
    }

    static unsafe int GetDamageState(CharacterInfo charInfo)
    {
        var skinName = new FString(charInfo.SkinId);
        return GetSavedDamageLevelForSkinName(&skinName);
    }

    static RPawnPlayer DoSwitch(
        string charName,
        WorldInfo wi,
        RPlayerController rpc,
        RPawnPlayer rpp
    )
    {
        // Switch character
        var act = new RSeqAct_SwitchPlayerCharacter(wi)
        {
            CharacterName = charName,
            PlayerStartPoint = Game.SpawnActor<PlayerStart>(rpp.Location, rpp.Rotation),
        };
        rpc.PrepareForPlayerSwitch(); // Resets HUD
        act.RestartPlayer(rpc); // Performs switch of Pawn
        rpp.Destroy(); // Removes old RPawnPlayer

        return rpc.CombatPawn;
    }

    void PlayTransitionEffects(Vector3 location)
    {
        var emitter = Game.SpawnActor<Emitter>(location)!;
        emitter.SetTemplate(_spawnEffectTemplate, bDestroyOnFinish: true);
        emitter.ParticleSystemComponent.SetScale(s_spawnEffectScale);
    }
}

using BmSDK;
using BmSDK.BmGame;
using BmSDK.BmScript;
using BmSDK.Engine;

namespace CharacterSwapping;

[Script]
public class CharacterSwappingScript : Script
{
    /// <summary>
    /// Provides a read-only mapping of each playable character to its associated character information.
    /// </summary>
    /// <remarks>The dictionary contains predefined entries for all supported playable characters. The
    /// collection is immutable and cannot be modified at runtime.</remarks>
    public static readonly IReadOnlyDictionary<PlayableCharacter, CharacterInfo> Characters =
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
                DlcBase: "Playable_RobinStoryDLC",
                Skin: CharacterInfo.StdSkin
            ),
            [PlayableCharacter.Nightwing] = new CharacterInfo(
                BaseId: PlayableCharacter.Nightwing,
                CharacterName: "Nightwing",
                Base: "Playable_Nightwing",
                Skin: CharacterInfo.StdSkin
            ),
        };

    private static readonly bool SpawnEffectEnabled = false;    // TODO(Samuil1337): Reenable spawn effect when done testing
    private const string SpawnEffectPkg = "Under_C2_Ch5";   // TODO(Samuil1337): Create SF package or load together with Robin
    private const string SpawnEffectPath = "FFX_Combat.Particles.NinjaSmokeBomb";
    private const float SpawnEffectScale = 1.0f;
    private ParticleSystem? spawnEffectTemplate;
    // The timer is scaled by seconds
    private const float SwapCooldown = 0f;  // TODO(Samuil1337): Reenable cooldown when done testing
    private float swapCooldownTimer = SwapCooldown;

    public override void Main()
    {
        if (!SpawnEffectEnabled) return;
        Game.LoadPackage(SpawnEffectPkg);
        spawnEffectTemplate = Game.FindObject<ParticleSystem>(SpawnEffectPath)!;
        spawnEffectTemplate.AddToRoot();
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

    private static bool IsSafeToSwitch(RPlayerController rpc)
    {
        if (rpc.bCinematicMode || rpc.bForceCinematicMode) return false;
        if (rpc.ActiveCinematicMode != null) return false;
        if (rpc.BatmanCutscene != null) return false;
        if (rpc.IsPlayingFullScreenMovie()) return false;
        if (rpc.IsLookInputIgnored()) return false;
        if (rpc.IsMoveInputIgnored()) return false;
        
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

    private static void LoadPackages(CharacterInfo charInfo, RGameInfo rgi, RGameRI gri)
    {
        bool isDlc = rgi.bStoryDLC;
        var basePkg = isDlc ? charInfo.DlcBasePkg : charInfo.BasePkg;
        var skinPkg = isDlc ? charInfo.DlcSkinPkg : charInfo.SkinPkg;
        var skinId = isDlc ? charInfo.DlcSkinId : charInfo.SkinId;
        Game.LoadPackage(basePkg);
        Game.LoadPackage(skinPkg);
        rgi.LoadPC(skinId, GetDamageState(charInfo, gri));  // TODO(Samuil1337): Update DamageLevel properly
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
        var rgi = Game.GetGameInfo();
        var wi = Game.GetWorldInfo();
        var gri = Game.GetGameRI();
        var pData = Game.GetPersistentData();
        if (!IsValid(rpc, rpp, rgi, wi, gri, pData)) return;
        if (!IsSafeToSwitch(rpc)) return;
        // Make sure swapping is necessary
        var charInfo = Characters[character];
        if (rpp.CharacterName == charInfo.CharacterName) return;

        // Save data that should survive player reinstantiation
        var dto = PlayerState.FromRpc(rpc, pData);

        // Load assets
        LoadPackages(charInfo, rgi, gri);

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

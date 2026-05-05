namespace Samuil1337.CharacterSwapping.Data
{
    record SwitchConfig(
        bool SpawnEffectEnabled,
        float SpawnEffectScale,
        bool SwapCooldownEnabled,
        float SwapCooldownValue
    )
    {
        internal static SwitchConfig FromToml(TomlTable modToml)
        {
            var config = (TomlTable)modToml["config"];
            return new(
                Convert.ToBoolean(config["spawn_effect_enabled"]),
                Convert.ToSingle(config["spawn_effect_scale"]),
                Convert.ToBoolean(config["swap_cooldown_enabled"]),
                Convert.ToSingle(config["swap_cooldown_value"])
            );
        }
    }
}

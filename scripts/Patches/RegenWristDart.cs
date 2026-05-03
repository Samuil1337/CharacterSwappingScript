using System.Numerics;
using BmSDK;
using BmSDK.BmGame;
using BmSDK.BmScript;

namespace Samuil1337.CharacterSwapping.Patches
{
    [ScriptComponent(AutoAttach = true)]
    sealed class RestockWristDartComponent : ScriptComponent<RNightwingWristDart>
    {
        static readonly float s_rechargeTime = RProjectileGadgetBase.DefaultObject.ReplenishTime;
        bool _replenishing;
        float _cooldown;

        [ComponentRedirect(nameof(RNightwingWristDart.FireDart))]
        void FireDart(Rotator rotation, Vector3 position)
        {
            if (Owner.Ammo > 0)
            {
                Owner.FireDart(rotation, position);
                OnDecrementAmmo();
            }
        }

        [ComponentRedirect(nameof(RNightwingWristDart.QuickFireDart))]
        void QuickFireDart()
        {
            if (Owner.QuickFireTarget is not null)
            {
                Owner.QuickFireDart();
                OnDecrementAmmo();
            }
        }

        void OnDecrementAmmo()
        {
            if (Game.GetGameRI().IsOverworldGameplay())
            {
                ScheduleIncrementAmmo();
            }
        }

        void ScheduleIncrementAmmo()
        {
            if (_replenishing)
            {
                return;
            }

            _cooldown = s_rechargeTime;
            _replenishing = true;
        }

        public override void OnTick()
        {
            if (_replenishing)
            {
                _cooldown -= Game.GetDeltaTime();
                if (_cooldown <= 0)
                {
                    _replenishing = false;
                    IncrementAmmo();
                }
            }
        }

        void IncrementAmmo()
        {
            if (!Game.GetGameRI().IsOverworldGameplay())
            {
                return;
            }

            if (Owner.Ammo < Owner.MaxAmmo)
            {
                Owner.Ammo++;
                Owner.UpdateGadgetHUDParams();
                if (Owner.Ammo < Owner.MaxAmmo)
                {
                    ScheduleIncrementAmmo();
                }
            }
        }

        [ComponentRedirect(nameof(RNightwingWristDart.OnRoomChange))]
        void OnRoomChange()
        {
            if (!Game.GetGameRI().IsOverworldGameplay())
            {
                Owner.RestockAmmo();
            }
        }

        [ComponentRedirect(nameof(RNightwingWristDart.RestockAmmo))]
        void RestockAmmo()
        {
            Owner.Ammo = Owner.MaxAmmo;
            Owner.NumHeadShotsInRound = 0;
            Owner.UpdateGadgetHUDParams();
        }

        [ComponentRedirect(nameof(RNightwingWristDart.OnLevelChange))]
        void OnLevelChange()
        {
            if (Game.GetGameRI().IsOverworldGameplay())
            {
                if (Owner.Ammo < Owner.MaxAmmo)
                {
                    ScheduleIncrementAmmo();
                }
            }
        }
    }
}

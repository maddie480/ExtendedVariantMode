using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using ExtendedVariants.Module;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    class MuteSeekerSounds : AbstractExtendedVariant {

        public MuteSeekerSounds() : base(typeof(bool), false) {
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Seeker.SpottedBegin  += onSeekerSpottedBegin;
            On.Celeste.Seeker.AttackBegin   += onSeekerAttackBegin;
            On.Celeste.Seeker.SkiddingBegin += onSeekerSkiddingBegin;
        }

        public override void Unload() {
            On.Celeste.Seeker.SpottedBegin  -= onSeekerSpottedBegin;
            On.Celeste.Seeker.AttackBegin   -= onSeekerAttackBegin;
            On.Celeste.Seeker.SkiddingBegin -= onSeekerSkiddingBegin;
        }

        private static void onSeekerSpottedBegin(
            On.Celeste.Seeker.orig_SpottedBegin orig,
            Seeker self
        ) {
            if (!GetVariantValue<bool>(Variant.MuteSeekerSounds)) {
                orig(self);
                return;
            }

            Logger.Log(LogLevel.Debug, "ExtendedVariantMode/MuteSeekerSounds", "Seeker attack sound muted");

            // Copy of vanilla SpottedBegin minus aggroSfx
            Player player = self.Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                self.TurnFacing(player.X - self.X, "spot");
            }

            self.spottedLosePlayerTimer = 0.6f;
            self.spottedTurnDelay = 1f;
        }

        private static void onSeekerAttackBegin(
            On.Celeste.Seeker.orig_AttackBegin orig,
            Seeker self
        ) {
            if (!GetVariantValue<bool>(Variant.MuteSeekerSounds)) {
                orig(self);
                return;
            }

            Logger.Log(LogLevel.Debug, "ExtendedVariantMode/MuteSeekerSounds", "Seeker dash sound muted");

            // Copy of vanilla AttackBegin minus Audio.Play
            self.attackWindUp = true;
            self.attackSpeed = -60f;
            self.Speed = (self.FollowTarget - self.Center).SafeNormalize(-60f);
        }

        private static void onSeekerSkiddingBegin(
            On.Celeste.Seeker.orig_SkiddingBegin orig,
            Seeker self
        ) {
            if (!GetVariantValue<bool>(Variant.MuteSeekerSounds)) {
                orig(self);
                return;
            }

            Logger.Log(LogLevel.Debug, "ExtendedVariantMode/MuteSeekerSounds", "Seeker skid sound muted");

            // Copy of vanilla SkiddingBegin minus Audio.Play
            self.strongSkid = false;
            self.TurnFacing((float)(-(float)self.facing), null);
        }
    }
}
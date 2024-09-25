using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/JumpCountTrigger")]
    public class JumpCountTrigger : IntegerExtendedVariantTrigger {
        private readonly bool capOnChange;

        public JumpCountTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            capOnChange = data.Bool("capOnChange");
        }

        public override void OnEnter(Player player) {
            checkCapOnChange(() => base.OnEnter(player));
        }

        public override void OnLeave(Player player) {
            checkCapOnChange(() => base.OnLeave(player));
        }

        private void checkCapOnChange(Action baseAction) {
            if (!capOnChange) {
                baseAction();
                return;
            }

            int oldJumpCount = (int) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(Variant.JumpCount);
            baseAction();
            int newJumpCount = (int) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(Variant.JumpCount);

            if (oldJumpCount != newJumpCount) {
                Logger.Log(LogLevel.Debug, "ExtendedVariantMode/JumpCountTrigger", $"Detected change in jump count ({oldJumpCount} => {newJumpCount}): capping jump count!");
                JumpCount.SetJumpCount(Math.Max(0, newJumpCount - 1), cap: true);
            }
        }

        protected override int getNewValue(EntityData data) {
            if (data.Bool("infinite")) return int.MaxValue;
            return base.getNewValue(data);
        }
    }
}

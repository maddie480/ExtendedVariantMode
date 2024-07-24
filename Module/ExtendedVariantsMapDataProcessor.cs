using Celeste;
using Celeste.Mod;
using ExtendedVariants.Variants;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsMapDataProcessor : EverestMapDataProcessor {
        public override Dictionary<string, Action<BinaryPacker.Element>> Init() {
            return new Dictionary<string, Action<BinaryPacker.Element>> {
                {
                    "triggers", triggerList => {
                        foreach (BinaryPacker.Element trigger in triggerList.Children.ToList()) {
                            // backwards compatibility for the Disable Climbing Up or Down extended variant trigger, that is now enum-based
                            if (trigger.Name == "ExtendedVariantMode/BooleanExtendedVariantTrigger" && trigger.Attr("variantChange") == "DisableClimbingUpOrDown") {
                                Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/ExtendedVariantsMapDataProcessor", $"Found a legacy DisableClimbingUpOrDown extended variant trigger with value {trigger.AttrBool("newValue")}!");
                                trigger.Name = "ExtendedVariantMode/DisableClimbingUpOrDownTrigger";
                                trigger.SetAttr("newValue", trigger.AttrBool("newValue") ? "Both" : "Disabled");
                            }
                            // backwards compatibility for the SwimmingSpeed extended variant trigger, that was split into 4 triggers
                            if (trigger.Name == "ExtendedVariantMode/FloatExtendedVariantTrigger" && trigger.Attr("variantChange") == "SwimmingSpeed") {
                                Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/ExtendedVariantsMapDataProcessor", $"Found a legacy SwimmingSpeed extended variant trigger with value {trigger.AttrBool("newValue")}!");
                                triggerList.Children.Remove(trigger);
                                triggerList.Children.Add(generateSwimmingSpeedTrigger(trigger, Variant.UnderwaterSpeedX, UnderwaterSpeedX.VanillaSpeed));
                                triggerList.Children.Add(generateSwimmingSpeedTrigger(trigger, Variant.UnderwaterSpeedY, UnderwaterSpeedY.VanillaSpeed));
                                triggerList.Children.Add(generateSwimmingSpeedTrigger(trigger, Variant.WaterSurfaceSpeedX, WaterSurfaceSpeedX.VanillaSpeed));
                                triggerList.Children.Add(generateSwimmingSpeedTrigger(trigger, Variant.WaterSurfaceSpeedY, WaterSurfaceSpeedY.VanillaSpeed));
                            }
                        }
                    }
                }
            };
        }

        private BinaryPacker.Element generateSwimmingSpeedTrigger(BinaryPacker.Element trigger, Variant variant, float normalValue) {
            BinaryPacker.Element clone = new BinaryPacker.Element() {
                Package = trigger.Package,
                Name = trigger.Name,
                Attributes = new Dictionary<string, object>(trigger.Attributes),
                Children = trigger.Children
            };

            clone.SetAttr("variantChange", variant.ToString());
            clone.SetAttr("newValue", normalValue * trigger.AttrFloat("newValue"));
            return clone;
        }

        public override void End() {
            // nothing to do
        }

        public override void Reset() {
            // nothing to do
        }
    }
}

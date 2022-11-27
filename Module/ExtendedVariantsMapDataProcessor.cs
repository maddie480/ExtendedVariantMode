using Celeste;
using Celeste.Mod;
using System;
using System.Collections.Generic;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsMapDataProcessor : EverestMapDataProcessor {
        public override Dictionary<string, Action<BinaryPacker.Element>> Init() {
            return new Dictionary<string, Action<BinaryPacker.Element>> {
                {
                    "triggers", triggerList => {
                        foreach (BinaryPacker.Element trigger in triggerList.Children) {
                            // backwards compatibility for the Disable Climbing Up or Down extended variant trigger, that is now enum-based
                            if (trigger.Name == "ExtendedVariantMode/BooleanExtendedVariantTrigger" && trigger.Attr("variantChange") == "DisableClimbingUpOrDown") {
                                Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/ExtendedVariantsMapDataProcessor", $"Found a legacy DisableClimbingUpOrDown extended variant trigger with value {trigger.AttrBool("newValue")}!");
                                trigger.Name = "ExtendedVariantMode/DisableClimbingUpOrDownTrigger";
                                trigger.SetAttr("newValue", trigger.AttrBool("newValue") ? "Both" : "Disabled");
                            }
                        }
                    }
                }
            };
        }

        public override void End() {
            // nothing to do
        }

        public override void Reset() {
            // nothing to do
        }
    }
}

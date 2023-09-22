using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using ExtendedVariants.Module;
using Celeste;
using ExtendedVariants.Variants;

namespace ExtendedVariants.Entities.ForMappers {
    // Initially done by lordseanington for Strawberry Jam.
    // Original at: https://github.com/StrawberryJam2021/StrawberryJam2021/blob/main/Entities/VariantToggleController.cs
    [CustomEntity("ExtendedVariantMode/VariantToggleController")]
    public class VariantToggleController : Entity {
        private string flag; //the flag that controls the variants
        private bool isFlagged; //last flag state
        private bool defaultValue;
        private Dictionary<ExtendedVariantsModule.Variant, object> variantValues;

        public VariantToggleController(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Attr("flag"), data.Attr("variantList"), data.Bool("defaultValue", true)) {
        }

        public VariantToggleController(Vector2 position, string flagName, string variantList, bool defValue)
            : base(position) {
            flag = flagName;
            defaultValue = defValue;
            variantValues = ParseParameterList(variantList);
        }

        public override void Update() {
            base.Update();
            UpdateFlag();
        }

        public override void Awake(Scene scene) {
            isFlagged = false;
            if (!string.IsNullOrEmpty(flag)) {
                SceneAs<Level>().Session.SetFlag(flag, defaultValue);
            }
            UpdateVariants();
        }

        private void UpdateVariants() {
            if (isFlagged) {
                foreach (KeyValuePair<ExtendedVariantsModule.Variant, object> variant in variantValues) {
                    ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(variant.Key, variant.Value, false, false, false, false);
                }
            } else {
                foreach (KeyValuePair<ExtendedVariantsModule.Variant, object> variant in variantValues) {
                    object defaultValue = ExtendedVariantTriggerManager.GetDefaultValueForVariant(variant.Key);
                    ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(variant.Key, defaultValue, false, false, false, false);
                }
            }
        }

        private void UpdateFlag() {
            if (string.IsNullOrEmpty(flag) || (isFlagged == SceneAs<Level>().Session.GetFlag(flag))) {
                return;
            }

            isFlagged = SceneAs<Level>().Session.GetFlag(flag);
            UpdateVariants();
        }

        private static Dictionary<ExtendedVariantsModule.Variant, object> ParseParameterList(string list) {
            Dictionary<ExtendedVariantsModule.Variant, object> variantList = new Dictionary<ExtendedVariantsModule.Variant, object>();
            if (string.IsNullOrEmpty(list)) {
                return variantList;
            }

            string[] keyValueList = list.Split(',');
            // comma separated list of Variant:Value pairs
            foreach (string keyValue in keyValueList) {
                string[] variantKeyValue = keyValue.Split(':');
                if (variantKeyValue.Length >= 2) {
                    ExtendedVariantsModule.Variant variant = (ExtendedVariantsModule.Variant) Enum.Parse(typeof(ExtendedVariantsModule.Variant), variantKeyValue[0]);
                    variantList[variant] = ParseParameterValue(variant, variantKeyValue[1]);
                }
            }

            return variantList;
        }

        private static object ParseParameterValue(ExtendedVariantsModule.Variant variant, string value) {
            Type type = ExtendedVariantsModule.Instance.VariantHandlers[variant].GetVariantType();

            if (type == typeof(string)) {
                return value;

            } else if (type == typeof(int)) {
                return int.Parse(value);

            } else if (type == typeof(float)) {
                return float.Parse(value);

            } else if (type == typeof(bool)) {
                return bool.Parse(value);

            } else if (type == typeof(bool[][])) {
                string[] split = value.Split(',');
                return new bool[][] {
                    new bool[] { bool.Parse(split[0]),bool.Parse(split[1]),bool.Parse(split[2]) },
                    new bool[] { bool.Parse(split[3]),bool.Parse(split[4]),bool.Parse(split[5]) },
                    new bool[] { bool.Parse(split[6]),bool.Parse(split[7]),bool.Parse(split[8]) }
                };

            } else if (type == typeof(DisplaySpeedometer.SpeedometerConfiguration)) {
                return Enum.Parse(typeof(DisplaySpeedometer.SpeedometerConfiguration), value);

            } else if (type == typeof(DontRefillDashOnGround.DashRefillOnGroundConfiguration)) {
                return Enum.Parse(typeof(DontRefillDashOnGround.DashRefillOnGroundConfiguration), value);

            } else if (type == typeof(MadelineBackpackMode.MadelineBackpackModes)) {
                return Enum.Parse(typeof(MadelineBackpackMode.MadelineBackpackModes), value);

            } else if (type == typeof(WindEverywhere.WindPattern)) {
                return Enum.Parse(typeof(WindEverywhere.WindPattern), value);

            } else if (type == typeof(JungleSpidersEverywhere.SpiderType)) {
                return Enum.Parse(typeof(JungleSpidersEverywhere.SpiderType), value);

            } else if (type == typeof(Assists.DashModes)) {
                return Enum.Parse(typeof(Assists.DashModes), value);

            } else if (type == typeof(DisableClimbingUpOrDown.ClimbUpOrDownOptions)) {
                return Enum.Parse(typeof(DisableClimbingUpOrDown.ClimbUpOrDownOptions), value);

            } else if (type == typeof(SpinnerColor.Color)) {
                return Enum.Parse(typeof(SpinnerColor.Color), value);

            } else if (type == typeof(DashRestriction.DashRestrictionType)) {
                return Enum.Parse(typeof(DashRestriction.DashRestrictionType), value);

            } else {
                throw new NotImplementedException("Cannot deserialize value of type " + type.FullName + "!");
            }
        }
    }
}

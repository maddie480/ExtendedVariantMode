using Celeste;
using Celeste.Mod;
using ExtendedVariants.Variants;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Module {
    /// <summary>
    /// Utility methods to turn a dictionary of variants from/to a savable format where every variant is a string.
    /// This is necessary to save the variant values and keep their types when loading them back.
    /// </summary>
    public static class ExtendedVariantSerializationUtils {
        public static Dictionary<Variant, object> FromSavableFormat(Dictionary<Variant, object> variantMap) {
            Dictionary<Variant, object> convertedVariants = new Dictionary<Variant, object>();

            try {
                foreach (Variant v in variantMap.Keys.ToList()) {
                    string value = variantMap[v].ToString();

                    string type = value.Substring(0, value.IndexOf(":"));
                    value = value.Substring(type.Length + 1);

                    switch (type) {
                        case "string":
                            convertedVariants[v] = value;
                            break;
                        case "int":
                            convertedVariants[v] = int.Parse(value);
                            break;
                        case "float":
                            convertedVariants[v] = float.Parse(value);
                            break;
                        case "bool":
                            convertedVariants[v] = bool.Parse(value);
                            break;
                        case "DashDirection":
                            string[] split = value.Split(',');
                            convertedVariants[v] = new bool[][] {
                                new bool[] { bool.Parse(split[0]),bool.Parse(split[1]),bool.Parse(split[2]) },
                                new bool[] { bool.Parse(split[3]),bool.Parse(split[4]),bool.Parse(split[5]) },
                                new bool[] { bool.Parse(split[6]),bool.Parse(split[7]),bool.Parse(split[8]) }
                            };
                            break;
                        case "DisplaySpeedometer":
                            convertedVariants[v] = Enum.Parse(typeof(DisplaySpeedometer.SpeedometerConfiguration), value);
                            break;
                        case "DontRefillDashOnGround":
                            convertedVariants[v] = Enum.Parse(typeof(DontRefillDashOnGround.DashRefillOnGroundConfiguration), value);
                            break;
                        case "MadelineBackpackMode":
                            convertedVariants[v] = Enum.Parse(typeof(MadelineBackpackMode.MadelineBackpackModes), value);
                            break;
                        case "WindEverywhere":
                            convertedVariants[v] = Enum.Parse(typeof(WindEverywhere.WindPattern), value);
                            break;
                        case "JungleSpidersEverywhere":
                            convertedVariants[v] = Enum.Parse(typeof(JungleSpidersEverywhere.SpiderType), value);
                            break;
                        case "DashModes":
                            convertedVariants[v] = Enum.Parse(typeof(Assists.DashModes), value);
                            break;
                        case "DisableClimbingUpOrDown":
                            convertedVariants[v] = Enum.Parse(typeof(DisableClimbingUpOrDown.ClimbUpOrDownOptions), value);
                            break;
                        default:
                            throw new NotImplementedException("Cannot deserialize value of type " + type + "!");
                    }
                }
            } catch (Exception e) {
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantModule", $"Failed to parse a value in session!");
                Logger.LogDetailed(e);
            }

            return convertedVariants;
        }

        public static Dictionary<Variant, object> ToSavableFormat(Dictionary<Variant, object> variantMap) {
            Dictionary<Variant, object> convertedVariants = new Dictionary<Variant, object>();

            foreach (Variant v in variantMap.Keys.ToList()) {
                object value = variantMap[v];

                switch (value) {
                    case string castValue:
                        convertedVariants[v] = "string:" + castValue;
                        break;
                    case int castValue:
                        convertedVariants[v] = "int:" + castValue;
                        break;
                    case float castValue:
                        convertedVariants[v] = "float:" + castValue;
                        break;
                    case bool castValue:
                        convertedVariants[v] = "bool:" + castValue;
                        break;
                    case bool[][] castValue:
                        convertedVariants[v] = "DashDirection:" +
                            castValue[0][0] + "," + castValue[0][1] + "," + castValue[0][2] + "," +
                            castValue[1][0] + "," + castValue[1][1] + "," + castValue[1][2] + "," +
                            castValue[2][0] + "," + castValue[2][1] + "," + castValue[2][2];
                        break;
                    case DisplaySpeedometer.SpeedometerConfiguration castValue:
                        convertedVariants[v] = "DisplaySpeedometer:" + castValue;
                        break;
                    case DontRefillDashOnGround.DashRefillOnGroundConfiguration castValue:
                        convertedVariants[v] = "DontRefillDashOnGround:" + castValue;
                        break;
                    case MadelineBackpackMode.MadelineBackpackModes castValue:
                        convertedVariants[v] = "MadelineBackpackMode:" + castValue;
                        break;
                    case WindEverywhere.WindPattern castValue:
                        convertedVariants[v] = "WindEverywhere:" + castValue;
                        break;
                    case JungleSpidersEverywhere.SpiderType castValue:
                        convertedVariants[v] = "JungleSpidersEverywhere:" + castValue;
                        break;
                    case Assists.DashModes castValue:
                        convertedVariants[v] = "DashModes:" + castValue;
                        break;
                    case DisableClimbingUpOrDown.ClimbUpOrDownOptions castValue:
                        convertedVariants[v] = "DisableClimbingUpOrDown:" + castValue;
                        break;
                    default:
                        Logger.Log(LogLevel.Error, "ExtendedVariantMode/ExtendedVariantModule", "Cannot serialize value of type " + value.GetType() + "!");
                        break;
                }
            }

            return convertedVariants;
        }
    }
}

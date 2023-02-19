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
        public static void FromSavableFormat(Dictionary<Variant, object> variantMap) {
            try {
                foreach (Variant v in variantMap.Keys.ToList()) {
                    string value = variantMap[v].ToString();

                    string type = value.Substring(0, value.IndexOf(":"));
                    value = value.Substring(type.Length + 1);

                    switch (type) {
                        case "string":
                            variantMap[v] = value;
                            break;
                        case "int":
                            variantMap[v] = int.Parse(value);
                            break;
                        case "float":
                            variantMap[v] = float.Parse(value);
                            break;
                        case "bool":
                            variantMap[v] = bool.Parse(value);
                            break;
                        case "DashDirection":
                            string[] split = value.Split(',');
                            variantMap[v] = new bool[][] {
                                new bool[] { bool.Parse(split[0]),bool.Parse(split[1]),bool.Parse(split[2]) },
                                new bool[] { bool.Parse(split[3]),bool.Parse(split[4]),bool.Parse(split[5]) },
                                new bool[] { bool.Parse(split[6]),bool.Parse(split[7]),bool.Parse(split[8]) }
                            };
                            break;
                        case "DisplaySpeedometer":
                            variantMap[v] = Enum.Parse(typeof(DisplaySpeedometer.SpeedometerConfiguration), value);
                            break;
                        case "DontRefillDashOnGround":
                            variantMap[v] = Enum.Parse(typeof(DontRefillDashOnGround.DashRefillOnGroundConfiguration), value);
                            break;
                        case "MadelineBackpackMode":
                            variantMap[v] = Enum.Parse(typeof(MadelineBackpackMode.MadelineBackpackModes), value);
                            break;
                        case "WindEverywhere":
                            variantMap[v] = Enum.Parse(typeof(WindEverywhere.WindPattern), value);
                            break;
                        case "JungleSpidersEverywhere":
                            variantMap[v] = Enum.Parse(typeof(JungleSpidersEverywhere.SpiderType), value);
                            break;
                        case "DashModes":
                            variantMap[v] = Enum.Parse(typeof(Assists.DashModes), value);
                            break;
                        case "DisableClimbingUpOrDown":
                            variantMap[v] = Enum.Parse(typeof(DisableClimbingUpOrDown.ClimbUpOrDownOptions), value);
                            break;
                        default:
                            throw new NotImplementedException("Cannot deserialize value of type " + type + "!");
                    }
                }
            } catch (Exception e) {
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantModule", $"Failed to parse a value in session!");
                Logger.LogDetailed(e);
            }
        }

        public static void ToSavableFormat(Dictionary<Variant, object> variantMap) {
            foreach (Variant v in variantMap.Keys.ToList()) {
                object value = variantMap[v];

                switch (value) {
                    case string castValue:
                        variantMap[v] = "string:" + castValue;
                        break;
                    case int castValue:
                        variantMap[v] = "int:" + castValue;
                        break;
                    case float castValue:
                        variantMap[v] = "float:" + castValue;
                        break;
                    case bool castValue:
                        variantMap[v] = "bool:" + castValue;
                        break;
                    case bool[][] castValue:
                        variantMap[v] = "DashDirection:" +
                            castValue[0][0] + "," + castValue[0][1] + "," + castValue[0][2] + "," +
                            castValue[1][0] + "," + castValue[1][1] + "," + castValue[1][2] + "," +
                            castValue[2][0] + "," + castValue[2][1] + "," + castValue[2][2];
                        break;
                    case DisplaySpeedometer.SpeedometerConfiguration castValue:
                        variantMap[v] = "DisplaySpeedometer:" + castValue;
                        break;
                    case DontRefillDashOnGround.DashRefillOnGroundConfiguration castValue:
                        variantMap[v] = "DontRefillDashOnGround:" + castValue;
                        break;
                    case MadelineBackpackMode.MadelineBackpackModes castValue:
                        variantMap[v] = "MadelineBackpackMode:" + castValue;
                        break;
                    case WindEverywhere.WindPattern castValue:
                        variantMap[v] = "WindEverywhere:" + castValue;
                        break;
                    case JungleSpidersEverywhere.SpiderType castValue:
                        variantMap[v] = "JungleSpidersEverywhere:" + castValue;
                        break;
                    case Assists.DashModes castValue:
                        variantMap[v] = "DashModes:" + castValue;
                        break;
                    case DisableClimbingUpOrDown.ClimbUpOrDownOptions castValue:
                        variantMap[v] = "DisableClimbingUpOrDown:" + castValue;
                        break;
                    default:
                        Logger.Log(LogLevel.Error, "ExtendedVariantMode/ExtendedVariantModule", "Cannot serialize value of type " + value.GetType() + "!");
                        break;
                }
            }
        }
    }
}

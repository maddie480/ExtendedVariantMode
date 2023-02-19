using Celeste;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedVariants.Entities {
    public class Speedometer : DashCountIndicator {
        private LinkedList<double> lastSpeeds = new LinkedList<double>();

        protected override bool shouldShowCounter() {
            return (DisplaySpeedometer.SpeedometerConfiguration) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.DisplaySpeedometer)
                != DisplaySpeedometer.SpeedometerConfiguration.DISABLED;
        }

        protected override float getExtraOffset() {
            return (bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.DisplayDashCount) ? 8f : 0f;
        }

        protected override string getNumberToDisplay(Player player) {
            double mostRecentNumber;
            switch ((DisplaySpeedometer.SpeedometerConfiguration) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.DisplaySpeedometer)) {
                case DisplaySpeedometer.SpeedometerConfiguration.HORIZONTAL:
                    mostRecentNumber = Math.Abs(Math.Round(player.Speed.X));
                    break;
                case DisplaySpeedometer.SpeedometerConfiguration.VERTICAL:
                    mostRecentNumber = Math.Abs(Math.Round(player.Speed.Y));
                    break;
                case DisplaySpeedometer.SpeedometerConfiguration.BOTH:
                    mostRecentNumber = Math.Round(player.Speed.Length());
                    break;
                default:
                    mostRecentNumber = 0;
                    break;
            }

            // we're displaying the top speed from the last 10 frames.
            lastSpeeds.AddLast(mostRecentNumber);
            if (lastSpeeds.Count > 10) {
                lastSpeeds.RemoveFirst();
            }
            return string.Format("{0:F0}", lastSpeeds.Max());
        }
    }
}

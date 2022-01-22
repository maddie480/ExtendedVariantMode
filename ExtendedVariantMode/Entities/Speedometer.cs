using Celeste;
using ExtendedVariants.Variants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedVariants.Entities {
    public class Speedometer : DashCountIndicator {
        private LinkedList<int> lastSpeeds = new LinkedList<int>();

        protected override bool shouldShowCounter() {
            return settings.DisplaySpeedometer != DisplaySpeedometer.SpeedometerConfiguration.DISABLED;
        }

        protected override float getExtraOffset() {
            return settings.DisplayDashCount ? 8f : 0f;
        }

        protected override string getNumberToDisplay(Player player) {
            int mostRecentNumber;
            switch (settings.DisplaySpeedometer) {
                case DisplaySpeedometer.SpeedometerConfiguration.HORIZONTAL:
                    mostRecentNumber = (int) Math.Abs(Math.Round(player.Speed.X));
                    break;
                case DisplaySpeedometer.SpeedometerConfiguration.VERTICAL:
                    mostRecentNumber = (int) Math.Abs(Math.Round(player.Speed.Y));
                    break;
                case DisplaySpeedometer.SpeedometerConfiguration.BOTH:
                    mostRecentNumber = (int) Math.Round(player.Speed.Length());
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
            return lastSpeeds.Max().ToString();
        }
    }
}

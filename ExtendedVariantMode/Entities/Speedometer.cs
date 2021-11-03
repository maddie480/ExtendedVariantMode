using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedVariants.Entities {
    public class Speedometer : DashCountIndicator {
        private LinkedList<int> lastSpeeds = new LinkedList<int>();

        protected override bool shouldShowCounter() {
            return settings.DisplaySpeedometer != 0;
        }

        protected override float getExtraOffset() {
            return settings.DisplayDashCount ? 8f : 0f;
        }

        protected override string getNumberToDisplay(Player player) {
            int mostRecentNumber;
            switch (settings.DisplaySpeedometer) {
                case 1:
                    mostRecentNumber = (int) Math.Abs(Math.Round(player.Speed.X));
                    break;
                case 2:
                    mostRecentNumber = (int) Math.Abs(Math.Round(player.Speed.Y));
                    break;
                case 3:
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

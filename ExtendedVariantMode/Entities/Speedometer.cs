using Celeste;
using System;

namespace ExtendedVariants.Entities {
    class Speedometer : DashCountIndicator {
        protected override bool shouldShowCounter() {
            return settings.DisplaySpeedometer != 0;
        }

        protected override float getExtraOffset() {
            return settings.DisplayDashCount ? 8f : 0f;
        }

        protected override string getNumberToDisplay(Player player) {
            switch (settings.DisplaySpeedometer) {
                case 1:
                    return ((int) Math.Abs(Math.Round(player.Speed.X))).ToString();
                case 2:
                    return ((int) Math.Abs(Math.Round(player.Speed.Y))).ToString();
                case 3:
                    return ((int) Math.Round(player.Speed.Length())).ToString();
                default:
                    return "";
            }
        }
    }
}

using Celeste;
using System;

namespace ExtendedVariants.UI {
    /// <summary>
    /// This class recreates some of the options in vanilla Celeste, making them extend TextMenuOptionExt instead of TextMenu.Option.
    /// </summary>
    public class TextMenuExt {
        public class Slider : TextMenuOptionExt<int> {
            public Slider(string label, Func<int, string> values, int min, int max, int value, int defaultIndex) : base(label, defaultIndex) {
                for (int i = min; i <= max; i++) {
                    Add(values(i), i, value == i);
                }
            }
        }
        public class OnOff : TextMenuOptionExt<bool> {
            public OnOff(string label, bool on, bool defaultValue) : base(label, defaultValue ? 1 : 0) {
                Add(Dialog.Clean("options_off", null), false, !on);
                Add(Dialog.Clean("options_on", null), true, on);
            }
        }
    }
}

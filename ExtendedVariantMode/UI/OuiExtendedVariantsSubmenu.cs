using Celeste;
using Celeste.Mod.UI;
using ExtendedVariants.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedVariants.UI {
    /// <summary>
    /// A dedicated submenu containing all extended variants options. Parameters =
    /// 0 = (bool) true if invoked from in-game, false otherwise
    /// </summary>
    class OuiExtendedVariantsSubmenu : AbstractSubmenu {
        public OuiExtendedVariantsSubmenu() : base("MODOPTIONS_EXTENDEDVARIANTS_PAUSEMENU_BUTTON", null) { }

        internal override void addOptionsToMenu(TextMenu menu, bool inGame, object[] parameters) {
            if (ExtendedVariantsModule.Settings.SubmenusForEachCategory) {
                // variants submenus + randomizer options
                new ModOptionsEntries().CreateAllOptions(ModOptionsEntries.VariantCategory.None, false, true, true, false,
                    () => OuiModOptions.Instance.Overworld.Goto<OuiExtendedVariantsSubmenu>(),
                    menu, inGame, false /* we don't care since there is no master switch */);
            } else {
                // all variants options + randomizer options
                new ModOptionsEntries().CreateAllOptions(ModOptionsEntries.VariantCategory.All, false, false, true, false,
                    () => OuiModOptions.Instance.Overworld.Goto<OuiExtendedVariantsSubmenu>(),
                    menu, inGame, false /* we don't care since there is no master switch */);
            }
        }

        internal override void gotoMenu(Overworld overworld) {
            overworld.Goto<OuiExtendedVariantsSubmenu>();
        }

        internal override string getMenuName(object[] parameters) {
            return base.getMenuName(parameters).ToUpperInvariant();
        }

        internal override string getButtonName(object[] parameters) {
            return Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_{(((bool) parameters[0]) ? "PAUSEMENU" : "MODOPTIONS")}_BUTTON");
        }
    }
}

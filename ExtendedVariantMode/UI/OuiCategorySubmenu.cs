using Celeste;

namespace ExtendedVariants.UI {
    /// <summary>
    /// A submenu for a specific category of variants. Parameters =
    /// 0 = (ModOptionsEntries.VariantCategory) The category to display
    /// </summary>
    public class OuiCategorySubmenu : AbstractSubmenu {

        public OuiCategorySubmenu() : base(null, null) { }

        protected override void addOptionsToMenu(TextMenu menu, bool inGame, object[] parameters) {
            ModOptionsEntries.VariantCategory category = (ModOptionsEntries.VariantCategory)parameters[0];

            // only put the category we're in
            new ModOptionsEntries().CreateAllOptions(category, false, false, false, false, null /* we don't care because there is no submenu */,
                menu, inGame, false /* we don't care because there is no master switch */);
        }

        protected override void gotoMenu(Overworld overworld) {
            Overworld.Goto<OuiCategorySubmenu>();
        }

        protected override string getButtonName(object[] parameters) {
            return Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_SUBMENU_{(ModOptionsEntries.VariantCategory)parameters[0]}");
        }

        protected override string getMenuName(object[] parameters) {
            return Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_SUBMENU_{(ModOptionsEntries.VariantCategory)parameters[0]}").ToUpperInvariant();
        }
    }
}

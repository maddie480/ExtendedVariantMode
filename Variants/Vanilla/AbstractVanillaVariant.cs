using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedVariants.Variants.Vanilla {
    public abstract class AbstractVanillaVariant : AbstractExtendedVariant {
        public override void Load() {
            // nothing
        }

        public override void Unload() {
            // nothing
        }

        public override bool IsVanilla() {
            return true;
        }
    }
}

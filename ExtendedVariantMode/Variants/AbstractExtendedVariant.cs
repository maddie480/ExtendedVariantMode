using ExtendedVariants.Module;
using System;

namespace ExtendedVariants.Variants {
    /// <summary>
    /// The base class for all extended variants.
    /// </summary>
    public abstract class AbstractExtendedVariant {
        protected ExtendedVariantsSettings Settings => ExtendedVariantsModule.Settings;

        /// <summary>
        /// Loads the extended variant (as in, hooks all the necessary methods to make it work).
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Unloads the extended variant (unhooking all methods).
        /// </summary>
        public abstract void Unload();

        /// <summary>
        /// Returns the parameter type of the variant.
        /// </summary>
        public abstract Type GetVariantType();

        /// <summary>
        /// Returns the default value for the variant.
        /// </summary>
        public abstract object GetDefaultVariantValue();

        /// <summary>
        /// Returns the current value for the variant.
        /// </summary>
        public abstract object GetVariantValue();

        /// <summary>
        /// Sets a new value for the variant.
        /// </summary>
        public void SetVariantValue(object value) {
            if (value.GetType() != GetVariantType()) {
                throw new Exception("Variant should be of type " + GetVariantType() + ", passed value of type " + value.GetType() + " for variant " + GetType() + "! Please report this to max480.");
            }

            DoSetVariantValue(value);
        }

        protected abstract void DoSetVariantValue(object value);

        /// <summary>
        /// Sets the variant according to the old "everything is an integer" rule.
        /// </summary>
        public abstract void SetLegacyVariantValue(int value);

        /// <summary>
        /// Called whenever a new level starts with the randomizer enabled and a set seed, to seed the variant's randomness as well.
        /// </summary>
        public virtual void SetRandomSeed(int seed) { }
    }
}

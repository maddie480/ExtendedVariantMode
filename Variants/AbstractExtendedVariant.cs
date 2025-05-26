using ExtendedVariants.Module;
using System;

namespace ExtendedVariants.Variants {
    /// <summary>
    /// The base class for all extended variants.
    /// </summary>
    public abstract class AbstractExtendedVariant {
        protected T GetVariantValue<T>(ExtendedVariantsModule.Variant variant) {
            return (T) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(variant);
        }

        private readonly Type variantType;
        private readonly object defaultVariantValue;

        protected AbstractExtendedVariant(Type variantType, object defaultVariantValue) {
            this.variantType = variantType;
            this.defaultVariantValue = defaultVariantValue;
        }

        /// <summary>
        /// Loads the extended variant (as in, hooks all the necessary methods to make it work).
        /// </summary>
        public virtual void Load() { }

        /// <summary>
        /// Unloads the extended variant (unhooking all methods).
        /// </summary>
        public virtual void Unload() { }

        /// <summary>
        /// Returns the parameter type of the variant.
        /// </summary>
        public Type GetVariantType() => variantType;

        /// <summary>
        /// Returns the default value for the variant.
        /// </summary>
        public object GetDefaultVariantValue() => defaultVariantValue;

        /// <summary>
        /// Called when a variant value is changed.
        /// </summary>
        public virtual void VariantValueChanged() { }

        /// <summary>
        /// Converts a variant value according to the old "everything is an integer" rule to its new value.
        /// </summary>
        public abstract object ConvertLegacyVariantValue(int value);

        /// <summary>
        /// Called whenever a new level starts with the randomizer enabled and a set seed, to seed the variant's randomness as well.
        /// </summary>
        public virtual void SetRandomSeed(int seed) { }
    }
}

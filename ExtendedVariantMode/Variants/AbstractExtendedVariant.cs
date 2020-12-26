using ExtendedVariants.Module;

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
        /// Returns the default value for the variant.
        /// </summary>
        public abstract int GetDefaultValue();

        /// <summary>
        /// Returns the current value for the variant.
        /// </summary>
        public abstract int GetValue();

        /// <summary>
        /// Sets a new value for the variant.
        /// </summary>
        public abstract void SetValue(int value);

        /// <summary>
        /// Called whenever a new level starts with the randomizer enabled and a set seed, to seed the variant's randomness as well.
        /// </summary>
        public virtual void SetRandomSeed(int seed) { }
    }
}

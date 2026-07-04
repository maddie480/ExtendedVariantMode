using System;
using Monocle;
using MonoMod.ModInterop;

namespace ExtendedVariants.Module {
    [ModImportName("MotionSmoothing")]
    public static class MotionSmoothingImports {
        public static Func<VirtualRenderTarget, VirtualRenderTarget> GetResizableBuffer;

        // [MotionSmoothing 1.5.6+] Ties a standalone entity's rendering to Madeline's smoothed
        // position so it stays glued to her under Motion Smoothing's subpixel rendering. Idempotent
        // and offset-based; null when Motion Smoothing isn't loaded or predates this export.
        public static Action<Entity> TieToPlayer;
    }
}
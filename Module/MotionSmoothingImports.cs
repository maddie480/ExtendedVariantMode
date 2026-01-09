using System;
using Monocle;
using MonoMod.ModInterop;

namespace ExtendedVariants.Module{    
    [ModImportName("MotionSmoothing")]
    public static class MotionSmoothingImports {
        public static Func<VirtualRenderTarget, VirtualRenderTarget> GetResizableBuffer;
    }
}
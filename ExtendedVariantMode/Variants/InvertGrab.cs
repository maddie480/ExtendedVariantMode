using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace ExtendedVariants.Variants {
    public class InvertGrab : AbstractExtendedVariant {
        private bool stopHookCheckKey = true;

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.InvertGrab ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.InvertGrab = value != 0;
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += modInputGrabCheck;
            IL.Celeste.Player.ClimbUpdate += modInputGrabCheck;
            IL.Celeste.Player.DashUpdate += modInputGrabCheck;
            IL.Celeste.Player.DashCoroutine += modInputGrabCheck;
            IL.Celeste.Player.SwimUpdate += modInputGrabCheck;
            IL.Celeste.Player.RedDashUpdate += modInputGrabCheck;
            IL.Celeste.Player.HitSquashUpdate += modInputGrabCheck;
            IL.Celeste.Player.LaunchUpdate += modInputGrabCheck;
            IL.Celeste.Player.StarFlyUpdate += modInputGrabCheck;
            On.Monocle.MInput.KeyboardData.Check_Keys += modCheckKeys;
            On.Monocle.MInput.GamePadData.Check_Buttons += modCheckButtons;
            On.Celeste.Player.DashCoroutine += modDashCoroutine;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= modInputGrabCheck;
            IL.Celeste.Player.ClimbUpdate -= modInputGrabCheck;
            IL.Celeste.Player.DashUpdate -= modInputGrabCheck;
            IL.Celeste.Player.DashCoroutine -= modInputGrabCheck;
            IL.Celeste.Player.SwimUpdate -= modInputGrabCheck;
            IL.Celeste.Player.RedDashUpdate -= modInputGrabCheck;
            IL.Celeste.Player.HitSquashUpdate -= modInputGrabCheck;
            IL.Celeste.Player.LaunchUpdate -= modInputGrabCheck;
            IL.Celeste.Player.StarFlyUpdate -= modInputGrabCheck;
            On.Monocle.MInput.KeyboardData.Check_Keys -= modCheckKeys;
            On.Monocle.MInput.GamePadData.Check_Buttons -= modCheckButtons;
            On.Celeste.Player.DashCoroutine -= modDashCoroutine;
        }

        private void modInputGrabCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // mod all Input.Grab.Check
            while (cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdsfld(typeof(Input), "Grab"),
                instr => instr.MatchCallvirt<VirtualButton>("get_Check")
            )) {
                Logger.Log("ExtendedVariantMode/InvertGrab", $"Adding code to apply Invert Grab at index {cursor.Index} in CIL code for Player.{cursor.Method.Name}");
                cursor.GotoNext().Remove().EmitDelegate<Func<VirtualButton, bool>>(invertButtonCheck);
            }
        }

        private bool invertButtonCheck(VirtualButton button) {
            return Settings.InvertGrab ? !button.Check : button.Check;
        }

        // Because we could not mod Input.Grab.Check in DashCoroutine with IL
        // So I finally chose hook these low level method to achieve the purpose.
        private bool modCheckKeys(On.Monocle.MInput.KeyboardData.orig_Check_Keys orig, MInput.KeyboardData self,
            Keys key) {
            bool result = orig(self, key);
            if (stopHookCheckKey || !Settings.InvertGrab) {
                return result;
            }

            if (!(Engine.Scene is Level level) || !level.CanPause || level.InCutscene) {
                return result;
            }

            if (Input.Grab.Nodes.Where(node => node is VirtualButton.KeyboardKey).Cast<VirtualButton.KeyboardKey>()
                .Select(keyboardKey => keyboardKey.Key).Contains(key)) {
                stopHookCheckKey = true;
                result = !Input.Grab.Check;
                stopHookCheckKey = false;
            }

            return result;
        }

        private bool modCheckButtons(On.Monocle.MInput.GamePadData.orig_Check_Buttons orig, MInput.GamePadData self,
            Buttons button) {
            bool result = orig(self, button);
            if (stopHookCheckKey || !Settings.InvertGrab) {
                return result;
            }

            if (!(Engine.Scene is Level level) || !level.CanPause || level.InCutscene) {
                return result;
            }

            if (Input.Grab.Nodes.Where(node => node is VirtualButton.PadButton).Cast<VirtualButton.PadButton>()
                .Select(padButton => padButton.Button).Contains(button)) {
                stopHookCheckKey = true;
                result = !Input.Grab.Check;
                stopHookCheckKey = false;
            }

            return result;
        }

        // only hook checkKey method include Input.Grab.Check part.
        private IEnumerator modDashCoroutine(On.Celeste.Player.orig_DashCoroutine orig, Player self) {
            IEnumerator coroutine = orig.Invoke(self);

            while (coroutine.MoveNext()) {
                object o = coroutine.Current;
   
                if (o is float && Settings.InvertGrab) {
                    stopHookCheckKey = true;
                }

                yield return o;

                if (o == null && Settings.InvertGrab) {
                    stopHookCheckKey = false;
                }
            }
            stopHookCheckKey = true;
        }
    }
}
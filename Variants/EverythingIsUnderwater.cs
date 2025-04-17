using Celeste;
using ExtendedVariants.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Celeste.Mod;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class EverythingIsUnderwater : AbstractExtendedVariant {

        public EverythingIsUnderwater() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
            IL.Celeste.Player.NormalUpdate += addNullChecksToWaterTopSurface;
            IL.Celeste.WaterFall.Update += addNullChecksToWaterTopSurface;

            // if already in a map, add the underwater switch controller right away.
            if (Engine.Scene is Level level) {
                level.Add(new UnderwaterSwitchController(() => GetVariantValue<bool>(Variant.EverythingIsUnderwater)));
                level.Entities.UpdateLists();
            }
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
            IL.Celeste.Player.NormalUpdate -= addNullChecksToWaterTopSurface;
            IL.Celeste.WaterFall.Update -= addNullChecksToWaterTopSurface;
        }

        private void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (!self.Session?.LevelData?.Underwater ?? false) {
                // inject a controller that will spawn/despawn water depending on the extended variant setting.
                self.Add(new UnderwaterSwitchController(() => GetVariantValue<bool>(Variant.EverythingIsUnderwater)));

                // when transitioning, don't update lists right away, but on the end of the frame.
                if (playerIntro != Player.IntroTypes.Transition) {
                    self.Entities.UpdateLists();
                } else {
                    self.OnEndOfFrame += () => self.Entities.UpdateLists();
                }
            }
        }

        private void addNullChecksToWaterTopSurface(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            VariableDefinition positionStore = new VariableDefinition(il.Import(typeof(Vector2)));
            il.Body.Variables.Add(positionStore);
            VariableDefinition multiplierStore = new VariableDefinition(il.Import(typeof(float)));
            il.Body.Variables.Add(multiplierStore);

            while (cursor.TryGotoNext(instr => instr.OpCode == OpCodes.Callvirt && (instr.Operand as MethodReference).Name == "DoRipple")) {
                Logger.Log("ExtendedVariantMode/EverythingIsUnderwater", $"Patching in null check at {cursor.Index} in IL for {cursor.Method.FullName}");

                // the goal here is not to call surface.DoRipple if surface is null.

                // store position and multiplier, duplicate WaterSurface
                cursor.Emit(OpCodes.Stloc, multiplierStore);
                cursor.Emit(OpCodes.Stloc, positionStore);
                cursor.Emit(OpCodes.Dup);

                // after DoRipple, insert a pop (to get rid of the duplicated WaterSurface) and place a br to skip over it if we come from DoRipple
                cursor.Index++;
                cursor.Emit(OpCodes.Br, cursor.Next);
                cursor.Emit(OpCodes.Pop);
                cursor.Index -= 3;

                // add a null check that jumps to the pop if WaterSurface is null
                cursor.Emit(OpCodes.Ldnull);
                cursor.Emit(OpCodes.Beq, cursor.Next.Next.Next);

                // reload position and multiplier, so that DoRipple can be called
                cursor.Emit(OpCodes.Ldloc, positionStore);
                cursor.Emit(OpCodes.Ldloc, multiplierStore);

                cursor.Index++;
            }
        }
    }
}

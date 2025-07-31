using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.EV {
    /**
     * Stolen from https://github.com/EverestAPI/Everest/pull/953
     * TODO: remove this once Everest hit stable
     */
    public static class SwapImmediatelyExtension {
        public static IEnumerator SafeEnumerate(this IEnumerator self) {
            Stack<IEnumerator> enums = new();
            enums.Push(self);

            while (enums.Count > 0) {
                IEnumerator cur = enums.Peek();

                if (cur.MoveNext()) {
                    object obj = cur.Current;

                    if (obj is SwapImmediately swap) {
                        enums.Push(swap.Inner);
                    } else {
                        yield return obj;
                    }
                } else {
                    enums.Pop();
                }
            }
        }
    }
}
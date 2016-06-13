using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xscreenshot.Cecil {
    public static class StringHelper {

        public static void FindString(string old, AssemblyDefinition asm, Func<MethodDefinition, Instruction, int, bool> callback) {
            foreach (ModuleDefinition mod in asm.Modules) {
                foreach (TypeDefinition td in mod.Types) {
                    IterateType(td, old, callback);
                }
            }
        }

        public static void ReplaceString(string old, string replacement, AssemblyDefinition asm) {
            FindString(old, asm, (m, inst, index) => {
                inst.Operand = replacement;
                return false; //Find and replace ALL
            });
        }
        public static void IterateType(TypeDefinition td, string old, Func<MethodDefinition, Instruction, int, bool> callback) {
            foreach (TypeDefinition ntd in td.NestedTypes) {
                IterateType(ntd, old, callback);
            }


            foreach (MethodDefinition md in td.Methods) {
                if (md.HasBody) {
                    for (int i = 0; i < md.Body.Instructions.Count - 1; i++) {
                        Instruction inst = md.Body.Instructions[i];

                        if (inst.OpCode == OpCodes.Ldstr) {
                            if (inst.Operand.ToString().Equals(old)) {

                                if (callback != null)
                                    if (callback(md, inst, i))
                                        return; //if callback returns true, exit search
                            }
                        }
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using Exiled.API.Features;

using HarmonyLib;

using UnityEngine;

namespace Labrunning.Patches
{
	[HarmonyPatch(typeof(Stamina), nameof(Stamina.CheckIfSprinting))]
	class CheckIfSprintingPatch
	{
		private static readonly MethodInfo vec3Equals = typeof(Vector3).GetMethod("op_Equality",
																	   BindingFlags.Public |
																	   BindingFlags.Static );
		private static readonly FieldInfo _prevPosition = typeof(Stamina).GetField("_prevPosition",
																					BindingFlags.NonPublic |
																					BindingFlags.Instance |
																					BindingFlags.Public);
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) 
		{
			List<CodeInstruction> inst = instructions.ToList();
			for(int i = 0; i < inst.Count; i++) 
			{
				var curInst = inst[i];
				if (curInst.Branches(out var where) && inst[Math.Max(i - 1, 0)].Calls(vec3Equals)) 
				{
					// We're inside a Vector3 comparison.
					// Find the else in the next few instructions (if it wasn't
					// patched already)
					for (int j = i; j < inst.Count; j++) 
					{
						if (j + 3 > inst.Count - 1)
						{
							// This should break if the jump target doesn't have enough
							// relevant instructions
							break;
						}
						// Check if this is the jump target
						if (inst[j].labels.Contains(where.Value)) 
						{
							// find an assignment and
							// check if it assigns _prevPosition
							for (int k = j + 1; k < inst.Count; k++) 
							{
								// This assigns _prevPosition, so our inst[i] should jump
								// to the next label (a.k.a. skip this instruction and
								// the instructions before)
								if (inst[k].StoresField(_prevPosition)) {
									// This shouldn't break unless .NET changes,
									// every instruction should have a label
									// Replace the jump
									curInst.operand = inst[k + 1].labels[0];
									return inst;
								}
							}
						}
					}
				}
			}
			return inst;
		}
	}
}

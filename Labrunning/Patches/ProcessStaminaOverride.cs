using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using Exiled.API.Features;

using HarmonyLib;

namespace Labrunning.Patches
{
	class ProcessStaminaOverride
	{
		// Get the method info. Scan for Public and NonPublic, since they might change it.
		private static readonly MethodInfo _processStamina = typeof(Stamina).GetMethod("ProcessStamina",
																	   BindingFlags.Public |
																	   BindingFlags.NonPublic |
																	   BindingFlags.Instance |
																	   BindingFlags.IgnoreCase);
		private static readonly MethodInfo _customStamina = typeof(ProcessStaminaOverride).GetMethod("CustomProcessStamina",
																					BindingFlags.Public |
																					BindingFlags.NonPublic |
																					BindingFlags.Static |
																					BindingFlags.IgnoreCase);

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var found = false;
			if (_processStamina == null)
			{
				Log.Error("Can't find ProcessStamina in the Stamina class. Don't worry, your server is fine, Labrunning is just outdated.");
				foreach (var instruction in instructions) yield return instruction;
				yield break;
			}

			foreach (var instruction in instructions)
			{

				if (instruction.Calls(_processStamina))
				{
					// These two yields do the following:
					// 1. Load the current instance into the stack
					// 2. Call the CustomProcessStamina function
					//
					// In a nutshell, it just calls CustomProcessStamina 
					// with the instance as a parameter.

					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, _customStamina);
					found = true;
				}

				yield return instruction;

			}
			if (!found) Log.Error($"Can't find instruction ProcessStamina in method ServersideUpdate. Labrunning is outdated, feel free to remove it.");
		}

		public static void CustomProcessStamina(Stamina _this)
		{

		}
	}
}

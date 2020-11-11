using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Exiled.API.Features;
using Exiled.API.Interfaces;

using HarmonyLib;

using Labrunning.Patches;

namespace Labrunning
{
	public class LabRunPlugin : Plugin<LabRunPlugin.Configs>
	{
		public class Configs : IConfig
		{
			public bool IsEnabled { get; set; } = true;
		
		}
		private int staticIncrementer = -1;
		public Harmony HarmonyInstance { get; set; } = null;

		
		private MethodInfo svsideUpdate = null;

		public override void OnEnabled()
		{
			HarmonyInstance = new Harmony($"LabRunning-{staticIncrementer++}");
			try
			{
				// Get the ServesideUpdate method. Hopefully it's not Ambiguous
				svsideUpdate = typeof(Stamina).GetMethod("ServesideUpdate",
						BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

				if (svsideUpdate == null)
				{
					// They fixed the name if it's null, get the method again lol
					svsideUpdate = typeof(Stamina).GetMethod("ServersideUpdate",
						BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

					if (svsideUpdate == null)
					{
						Log.Error("Can't find ServersideUpdate in class Stamina. This plugin is outdated, feel free to delete it.");
						return;
					}
				}

				// Patch the method
				HarmonyInstance.Patch(svsideUpdate, null, null, new HarmonyMethod(typeof(ProcessStaminaOverride), nameof(ProcessStaminaOverride.Transpiler)));
				// Patch everything else
				HarmonyInstance.PatchAll();
				base.OnEnabled();

			} catch (AmbiguousMatchException) {
				Log.Error("AmbiguousMatchException! Labrunning will not work until it gets updated, feel free to delete the plugin.");
			} catch (Exception ex) {
				Log.Error(ex.ToString());
				Log.Info("Don't worry: if you're seeing this message, your server is fine but the plugin is outdated. Feel free to delete the plugin.");
			}
		}

		public override void OnDisabled()
		{
			if (HarmonyInstance != null && svsideUpdate != null)
			{
				HarmonyInstance.Unpatch(svsideUpdate, HarmonyPatchType.Transpiler, HarmonyInstance.Id);
				base.OnDisabled();
			}
		}
	}

}

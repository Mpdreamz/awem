using System;
using System.Collections.Generic;
using System.Linq;
using Awem.PInvoke.Enums;

namespace Awem.KeyboardHandling
{
	public class KeyboardCombination
	{
		private readonly string _errorMessage;
		public HashSet<VirtualKeys> Shortcut { get; }
		public Action Action { get; }
		public string Command { get; }
		public HashSet<VirtualKeys>[] ShortcutAlternatives { get; }
		public bool IsPressed(KeyboardCombination combination) => this.ShortcutAlternatives.Any(p => p.SetEquals(combination.Shortcut));

		protected internal KeyboardCombination(string errorMessage = null) => this._errorMessage = errorMessage;

		protected internal KeyboardCombination(HashSet<VirtualKeys> shortcut, Action action = null, string commandName = null)
		{
			this.Command = commandName;
			this.Shortcut = shortcut ?? new HashSet<VirtualKeys>();
			this.ShortcutAlternatives = CreateShortCutAlternatives(shortcut).ToArray();
			this.Action = action;
		}

		private static IEnumerable<HashSet<VirtualKeys>> CreateShortCutAlternatives(HashSet<VirtualKeys> shortcut)
		{
			yield return shortcut;
			if (shortcut.Contains(VirtualKeys.Shift))
			{
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Shift, VirtualKeys.RightShift)) yield return p;
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Shift, VirtualKeys.LeftShift)) yield return p;
			}
			else if (shortcut.Contains(VirtualKeys.Control))
			{
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Control, VirtualKeys.LeftControl)) yield return p;
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Control, VirtualKeys.RightControl)) yield return p;
			}
			else if (shortcut.Contains(VirtualKeys.Menu))
			{
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Menu, VirtualKeys.LeftMenu)) yield return p;
				foreach (var p in LocalAlternatives(shortcut, VirtualKeys.Menu, VirtualKeys.RightMenu)) yield return p;
			}
		}

		private static IEnumerable<HashSet<VirtualKeys>> LocalAlternatives(HashSet<VirtualKeys> shortcut, VirtualKeys oldKey, VirtualKeys newKey)
		{
			var a = CreateAlternative(shortcut, oldKey, newKey);
			foreach (var al in CreateShortCutAlternatives(a)) yield return al;
		}

		private static HashSet<VirtualKeys> CreateAlternative(HashSet<VirtualKeys> shortcut, VirtualKeys oldKey, VirtualKeys newKey)
		{
			var temp = new VirtualKeys[shortcut.Count];
			shortcut.CopyTo(temp);
			var alternative = new HashSet<VirtualKeys>(temp);
			alternative.Remove(oldKey);
			alternative.Add(newKey);
			return alternative;
		}

		public override string ToString()
		{
			var keys = this.Shortcut.Select(k => Enum.GetName(typeof(VirtualKeys), k).Replace("Menu", "Alt"));
			return $"{string.Join("+", keys)}";
		}

	}
}

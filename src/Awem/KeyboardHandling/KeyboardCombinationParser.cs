using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Awem.PInvoke.Enums;

namespace Awem.KeyboardHandling
{
	public class KeyboardCombinationParser
	{
		private readonly WindowManagerActions _windowManagerActions;

		public KeyboardCombinationParser(VirtualKeys leaderKey, WindowManagerActions windowManagerActions, IEnumerable<string> keyboardConfig)
		{
			this.LeaderKey = leaderKey;
			_windowManagerActions = windowManagerActions;

			var parsedCombinations = this.ToKeyboardCombinations(keyboardConfig);
			this.KeyboardCombinations = new ReadOnlyCollection<KeyboardCombination>(parsedCombinations);
		}

		public VirtualKeys LeaderKey { get; }

		public IReadOnlyList<KeyboardCombination> KeyboardCombinations { get; }

		private readonly string[] _defaultShortcuts =
		{
			$"{nameof(WindowManagerActions.GotoDesktop)}0=N1",
			$"{nameof(WindowManagerActions.GotoDesktop)}1=2",
			$"{nameof(WindowManagerActions.GotoDesktop)}2=N3",
			$"{nameof(WindowManagerActions.GotoDesktop)}3=4",
			$"{nameof(WindowManagerActions.GotoDesktop)}4=N5",
			$"{nameof(WindowManagerActions.GotoDesktop)}5=N6",
			$"{nameof(WindowManagerActions.GotoDesktop)}6=N7",
			$"{nameof(WindowManagerActions.GotoDesktop)}7=N8",
			$"{nameof(WindowManagerActions.GotoDesktop)}8=N9",
			$"{nameof(WindowManagerActions.GotoDesktop)}9=N10",
			$"{nameof(WindowManagerActions.GotoDesktop)}0=N11",
			$"{nameof(WindowManagerActions.MoveToDesktop)}0=Shift+N1",
			$"{nameof(WindowManagerActions.MoveToDesktop)}1=Shift+2",
			$"{nameof(WindowManagerActions.MoveToDesktop)}2=Shift+N3",
			$"{nameof(WindowManagerActions.MoveToDesktop)}3=Shift+4",
			$"{nameof(WindowManagerActions.MoveToDesktop)}4=Shift+N5",
			$"{nameof(WindowManagerActions.MoveToDesktop)}5=Shift+6",
			$"{nameof(WindowManagerActions.MoveToDesktop)}6=Shift+7",
			$"{nameof(WindowManagerActions.MoveToDesktop)}7=Shift+8",
			$"{nameof(WindowManagerActions.MoveToDesktop)}8=Shift+9",
			$"{nameof(WindowManagerActions.MoveToDesktop)}9=Shift+10",
			$"{nameof(WindowManagerActions.MoveToDesktop)}0=Shift+11",
			$"{nameof(WindowManagerActions.Die)}=X",
			$"{nameof(WindowManagerActions.ToggleLauncher)}=D",
			$"{nameof(WindowManagerActions.GotoPreviousDesktop)}=Back",
		};

		private KeyboardCombination[] ToKeyboardCombinations(IEnumerable<string> config = null)
		{
			var c = _defaultShortcuts.Union(config ?? Enumerable.Empty<string>());
			return c.Select(this.Parse).Where(p => p.Shortcut != null).OrderByDescending(p => p.Shortcut.Count)
				.ToArray();
		}

		public KeyboardCombination Parse(string combination)
		{
			var tokens = combination.Split(new[] {'='}, 2, StringSplitOptions.RemoveEmptyEntries);
			if (tokens.Length != 2)
				return new KeyboardCombination($"Could not parse a key and value from {combination}");

			var shortcut = tokens[1].Trim();
			var command = tokens[0].Trim();
			Action action;
			if (Regex.IsMatch(command, @"\d+$"))
			{
				var subTokens = Regex.Split(command, @"(?=\d)");
				command = subTokens[0];
				var numeric = int.Parse(subTokens[1]);
				if (this._windowManagerActions.NumericCommands.TryGetValue(command, out var r) && r != null)
					action = () => r(numeric);
				else
					return new KeyboardCombination($"{command}{numeric} is not a valid action");
			}
			else
			{
				if (this._windowManagerActions.Commands.TryGetValue(command, out var r) && r != null)
					action = r;
				else
					return new KeyboardCombination($"{command} is not a valid action");
			}
			return this.ParseShortcut(shortcut, action, command);

		}

		private KeyboardCombination ParseShortcut(string shortcut, Action action, string commandName)
		{
			var keys = shortcut.Split(new[] {"+"}, StringSplitOptions.RemoveEmptyEntries)
				.Select(key => key.Trim())
				.ToArray();

			var virtualKeys = keys.Select(k => new
			{
				parsed = TryParseSingleKey(k, out var key),
				key
			}).ToArray();
			if (virtualKeys.All(v => v.parsed))
			{
				var set = new[] {this.LeaderKey}.Concat(virtualKeys.Select(v => v.key));
				var shortCut = new HashSet<VirtualKeys>(set.ToArray());
				return new KeyboardCombination(shortCut, action, commandName);
			}

			var invalidKeys = string.Join(", ", virtualKeys.Where(v => !v.parsed).Select(v => v.key));
			var errorMessage = $"The following keys could not be parsed:{invalidKeys}";
			return new KeyboardCombination(errorMessage);
		}

		private static bool TryParseSingleKey(string key, out VirtualKeys virtualKey)
		{
			if (int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out var i))
				key = $"N{i}";
			return Enum.TryParse(key, ignoreCase: true, out virtualKey);
		}
	}
}

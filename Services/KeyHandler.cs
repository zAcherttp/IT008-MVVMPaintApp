using System.Diagnostics;
using System.Windows.Input;

namespace MVVMPaintApp.Services
{
    public class KeyHandler
    {
        private readonly List<KeyCommand> keyCommands = [];

        public void RegisterCommand(Key mainKey, Action action, string description, params Key[] keySet)
        {
            keyCommands.Add(new KeyCommand(mainKey, action, description, keySet));
        }

        public void HandleKeyPress(Key pressedKey, IEnumerable<Key> currentlyPressedKeys)
        {
            foreach (var command in keyCommands)
            {
                if (command.Matches(pressedKey, currentlyPressedKeys))
                {
                    command.Action();
                    Debug.WriteLine($"Key command executed: {command.Description}");
                    break;
                }
            }
        }

        public List<string> GetKeyBindings()
        {
            return [.. keyCommands.Select(cmd =>
            {
                var keySet = string.Join("+", cmd.ModifierKeys);
                var keyBinding = $"{keySet}";
                return $"{keyBinding}: {cmd.Description}";
            })];
        }
    }

    public class KeyCommand(Key mainKey, Action action, string description, params Key[] keySet)
    {
        public Key MainKey { get; } = mainKey;
        public HashSet<Key> ModifierKeys { get; } = [.. keySet];
        public Action Action { get; } = action;
        public string Description { get; } = description;

        public bool Matches(Key pressedKey, IEnumerable<Key> currentlyPressedKeys)
        {
            if (pressedKey != MainKey) return false;
            var currentModifiers = new HashSet<Key>(currentlyPressedKeys);
            return ModifierKeys.SetEquals(currentModifiers);
        }
    }
}

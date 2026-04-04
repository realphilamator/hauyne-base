using System;
using UnityEngine;
using System.Collections.Generic;

// -------------------------------------------------------------------------
// InputAction Enum
// -------------------------------------------------------------------------

/// <summary>
/// All bindable player actions. Each action maps to an InputBinding (primary + secondary key).
/// Add new actions here and add their default bindings in InputManager.GetDefaultBinding().
/// The integer values are used for array indexing — do not reorder or remove entries
/// without also clearing saved PlayerPrefs bindings, or old saves will mismap.
/// Count must always be the last entry.
/// </summary>
public enum InputAction
{
    MoveLeft = 0,
    MoveRight = 1,
    MoveForward = 2,
    MoveBackward = 3,
    Interact = 4,
    UseItem = 5,
    Slot0 = 6,
    Slot1 = 7,
    Slot2 = 8,
    Slot3 = 9,
    Slot4 = 10,
    Run = 11,
    LookBehind = 12,
    Jump = 13,
    PauseOrCancel = 14,
    Count               // Sentinel — always keep this last
}

// -------------------------------------------------------------------------
// Supporting Structs
// -------------------------------------------------------------------------

/// <summary>
/// Stores a primary and optional secondary key for a single action.
/// Either key being held will trigger the action.
/// </summary>
[Serializable]
public struct InputBinding
{
    public KeyCode primaryKey;
    public KeyCode secondaryKey;

    public InputBinding(KeyCode primary, KeyCode secondary = KeyCode.None)
    {
        primaryKey = primary;
        secondaryKey = secondary;
    }
}

/// <summary>
/// Tracks whether a simulated key is active this frame and last frame.
/// Used by SimulateKey() to feed virtual input into the action state system.
/// </summary>
[Serializable]
public struct SimulatedKeyState
{
    public bool currentFrame;
    public bool previousFrame;
}

// -------------------------------------------------------------------------
// InputManager
// -------------------------------------------------------------------------

/// <summary>
/// Singleton that manages all player input. Features:
///   - Rebindable primary and secondary keys per action
///   - Save/load bindings via PlayerPrefs
///   - GetActionKey / GetActionKeyDown / GetActionKeyUp (frame-accurate)
///   - SimulateKey() for programmatic input (e.g. cutscenes, tutorials)
///
/// Setup: Place a GameObject in your scene with this component. It will
/// persist across scenes automatically via DontDestroyOnLoad.
/// The ControlsMenu script reads and writes this manager to support
/// in-game rebinding UI.
/// </summary>
public class InputManager : Singleton<InputManager>
{
    // -------------------------------------------------------------------------
    // Public Data
    // -------------------------------------------------------------------------

    /// <summary>
    /// The active key bindings for all actions. Populated from PlayerPrefs on Awake,
    /// or from defaults if no saved data exists.
    /// </summary>
    public Dictionary<InputAction, InputBinding> KeyboardMapping = new Dictionary<InputAction, InputBinding>();

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    /// <summary>Tracks keys being held via SimulateKey() this and last frame.</summary>
    private Dictionary<KeyCode, SimulatedKeyState> simulatedKeys = new Dictionary<KeyCode, SimulatedKeyState>();

    /// <summary>Action held states for the current and previous frame, used for Down/Up detection.</summary>
    private bool[] currentKeyStates = new bool[(int)InputAction.Count];
    private bool[] previousKeyStates = new bool[(int)InputAction.Count];

    /// <summary>PlayerPrefs key prefix for saved bindings.</summary>
    private const string SavePrefix = "InputBinding_";

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns KeyboardMapping, loading from PlayerPrefs first if empty.
    /// Useful when accessing mappings before Awake has run.
    /// </summary>
    public Dictionary<InputAction, InputBinding> Mappings
    {
        get
        {
            if (KeyboardMapping == null || KeyboardMapping.Count == 0)
                Load();
            return KeyboardMapping;
        }
    }

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    protected override void Awake()
    {
        // Enforce singleton — destroy duplicate instances
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Update()
    {
        // Swap buffers: last frame's current becomes this frame's previous
        bool[] temp = previousKeyStates;
        previousKeyStates = currentKeyStates;
        currentKeyStates = temp;

        UpdateSimulatedKeys();

        // Evaluate each action: held if either bound key is currently down
        for (int i = 0; i < (int)InputAction.Count; i++)
        {
            if (!KeyboardMapping.TryGetValue((InputAction)i, out InputBinding binding))
                continue;

            currentKeyStates[i] =
                (binding.primaryKey != KeyCode.None && GetKeyState(binding.primaryKey)) ||
                (binding.secondaryKey != KeyCode.None && GetKeyState(binding.secondaryKey));
        }
    }

    // -------------------------------------------------------------------------
    // Simulated Input
    // -------------------------------------------------------------------------

    /// <summary>
    /// Advances the simulated key states by one frame and removes expired entries.
    /// SimulateKey() only marks a key active for a single frame, so it must be
    /// called every frame to stay active.
    /// </summary>
    private void UpdateSimulatedKeys()
    {
        if (simulatedKeys.Count == 0) return;

        var keys = new List<KeyCode>(simulatedKeys.Keys);
        foreach (var key in keys)
        {
            var state = simulatedKeys[key];
            state.previousFrame = state.currentFrame;
            state.currentFrame = false;            // Reset — must be re-triggered each frame
            simulatedKeys[key] = state;

            // Clean up entries that are no longer active
            if (!state.currentFrame && !state.previousFrame)
                simulatedKeys.Remove(key);
        }
    }

    /// <summary>
    /// Programmatically marks a key as held for the current frame.
    /// Call every frame to keep the key "held". Useful for tutorials or cutscenes.
    /// </summary>
    public void SimulateKey(KeyCode key)
    {
        if (!simulatedKeys.TryGetValue(key, out var state))
            state = new SimulatedKeyState();

        state.currentFrame = true;
        simulatedKeys[key] = state;
    }

    /// <summary>
    /// Returns true if the key is physically held OR simulated this frame.
    /// </summary>
    private bool GetKeyState(KeyCode key)
    {
        if (simulatedKeys.TryGetValue(key, out var state) && state.currentFrame)
            return true;

        return Input.GetKey(key);
    }

    // -------------------------------------------------------------------------
    // Public Query API
    // -------------------------------------------------------------------------

    /// <summary>Returns true while the action's bound key is held.</summary>
    public bool GetActionKey(InputAction action) => currentKeyStates[(int)action];

    /// <summary>Returns true only on the first frame the action's bound key is pressed.</summary>
    public bool GetActionKeyDown(InputAction action) => currentKeyStates[(int)action] && !previousKeyStates[(int)action];

    /// <summary>Returns true only on the frame the action's bound key is released.</summary>
    public bool GetActionKeyUp(InputAction action) => !currentKeyStates[(int)action] && previousKeyStates[(int)action];

    // -------------------------------------------------------------------------
    // Binding Modification
    // -------------------------------------------------------------------------

    /// <summary>Clears the primary key for an action (sets it to KeyCode.None).</summary>
    public void ClearPrimaryKey(InputAction action)
    {
        if (KeyboardMapping.TryGetValue(action, out var binding))
        {
            binding.primaryKey = KeyCode.None;
            KeyboardMapping[action] = binding;
        }
    }

    /// <summary>Clears the secondary key for an action (sets it to KeyCode.None).</summary>
    public void ClearSecondaryKey(InputAction action)
    {
        if (KeyboardMapping.TryGetValue(action, out var binding))
        {
            binding.secondaryKey = KeyCode.None;
            KeyboardMapping[action] = binding;
        }
    }

    /// <summary>
    /// Updates the binding for an action. Pass KeyCode.None to leave a slot unchanged.
    /// Used by ControlsMenu during the rebind flow.
    /// </summary>
    public void Modify(InputAction action, KeyCode newKey, KeyCode secondaryKey = KeyCode.None)
    {
        if (!KeyboardMapping.TryGetValue(action, out var binding))
            binding = new InputBinding();

        if (newKey != KeyCode.None) binding.primaryKey = newKey;
        if (secondaryKey != KeyCode.None) binding.secondaryKey = secondaryKey;

        KeyboardMapping[action] = binding;
    }

    // -------------------------------------------------------------------------
    // Save / Load
    // -------------------------------------------------------------------------

    /// <summary>
    /// Saves all current bindings to PlayerPrefs.
    /// Called automatically by ControlsMenu when the player exits the controls screen.
    /// </summary>
    public void Save()
    {
        foreach (var pair in KeyboardMapping)
        {
            string key = SavePrefix + pair.Key;
            string value = $"{(int)pair.Value.primaryKey},{(int)pair.Value.secondaryKey}";
            PlayerPrefs.SetString(key, value);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads bindings from PlayerPrefs. Falls back to defaults for any action
    /// with missing or corrupt save data. Called automatically on Awake.
    /// </summary>
    public void Load()
    {
        KeyboardMapping.Clear();

        for (int i = 0; i < (int)InputAction.Count; i++)
        {
            InputAction action = (InputAction)i;
            string key = SavePrefix + action;

            if (PlayerPrefs.HasKey(key))
            {
                string[] parts = PlayerPrefs.GetString(key).Split(',');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int primary) &&
                    int.TryParse(parts[1], out int secondary))
                {
                    KeyboardMapping[action] = new InputBinding((KeyCode)primary, (KeyCode)secondary);
                    continue;
                }
            }

            // No valid save found — use the hardcoded default
            KeyboardMapping[action] = GetDefaultBinding(action);
        }
    }

    /// <summary>
    /// Resets all bindings to their hardcoded defaults and saves immediately.
    /// Called by ControlsMenu when the player hits "Reset to Defaults".
    /// </summary>
    public void SetDefaults()
    {
        KeyboardMapping.Clear();

        for (int i = 0; i < (int)InputAction.Count; i++)
        {
            var action = (InputAction)i;
            KeyboardMapping[action] = GetDefaultBinding(action);
        }

        Save();
    }

    // -------------------------------------------------------------------------
    // Display Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a human-readable string for an action's currently bound key.
    /// Prefers the primary key; falls back to secondary; falls back to the action name.
    /// Used to display hints like "Press [E] to interact" in UI.
    /// </summary>
    public string ConvertInputActionToString(InputAction action)
    {
        if (!KeyboardMapping.TryGetValue(action, out var binding))
            return action.ToString();

        if (binding.primaryKey != KeyCode.None) return KeyCodeToDisplayString(binding.primaryKey);
        if (binding.secondaryKey != KeyCode.None) return KeyCodeToDisplayString(binding.secondaryKey);

        return action.ToString();
    }

    /// <summary>
    /// Converts a KeyCode to a readable display string for UI labels.
    /// Handles special cases like Alpha keys (shows "1" instead of "Alpha1")
    /// and mouse buttons.
    /// </summary>
    public string KeyCodeToDisplayString(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Escape: return "ESC";
            case KeyCode.Mouse0: return "Left Mouse Button";
            case KeyCode.Mouse1: return "Right Mouse Button";
            case KeyCode.Mouse2: return "Middle Mouse Button";
            case KeyCode.Alpha0: return "0";
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            case KeyCode.Alpha7: return "7";
            case KeyCode.Alpha8: return "8";
            case KeyCode.Alpha9: return "9";
            case KeyCode.None: return "";
            default: return key.ToString();
        }
    }

    // -------------------------------------------------------------------------
    // Default Bindings
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the default InputBinding for each action.
    /// Edit this method to change the default controls for your game.
    /// </summary>
    private InputBinding GetDefaultBinding(InputAction action)
    {
        switch (action)
        {
            case InputAction.MoveLeft: return new InputBinding(KeyCode.A);
            case InputAction.MoveRight: return new InputBinding(KeyCode.D);
            case InputAction.MoveForward: return new InputBinding(KeyCode.W);
            case InputAction.MoveBackward: return new InputBinding(KeyCode.S);
            case InputAction.Interact: return new InputBinding(KeyCode.None, KeyCode.Mouse0);
            case InputAction.UseItem: return new InputBinding(KeyCode.None, KeyCode.Mouse1);
            case InputAction.Slot0: return new InputBinding(KeyCode.Alpha1);
            case InputAction.Slot1: return new InputBinding(KeyCode.Alpha2);
            case InputAction.Slot2: return new InputBinding(KeyCode.Alpha3);
            case InputAction.Slot3: return new InputBinding(KeyCode.Alpha4);
            case InputAction.Slot4: return new InputBinding(KeyCode.Alpha5);
            case InputAction.Run: return new InputBinding(KeyCode.LeftShift);
            case InputAction.LookBehind: return new InputBinding(KeyCode.C);
            case InputAction.Jump: return new InputBinding(KeyCode.Space);
            case InputAction.PauseOrCancel: return new InputBinding(KeyCode.Escape);
            default: return new InputBinding(KeyCode.None);
        }
    }
}
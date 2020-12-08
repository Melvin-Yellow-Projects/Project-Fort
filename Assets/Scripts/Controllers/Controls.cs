// GENERATED AUTOMATICALLY FROM 'Assets/Input/Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Controls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""General"",
            ""id"": ""18f87afc-c593-47e2-9e01-b0be5e43e884"",
            ""actions"": [
                {
                    ""name"": ""Affirmation"",
                    ""type"": ""Button"",
                    ""id"": ""794d2499-7cbf-427c-9f92-22418f3c54b8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""0c015171-f84c-4a5e-8432-eebe08c539ac"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard & Mouse"",
                    ""action"": ""Affirmation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Player"",
            ""id"": ""4799735f-4a80-44bc-8262-ffedc7d78836"",
            ""actions"": [
                {
                    ""name"": ""Selection"",
                    ""type"": ""Button"",
                    ""id"": ""ed3ad872-cb71-49ce-b89e-95490c2cc117"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Command"",
                    ""type"": ""Button"",
                    ""id"": ""18560551-d8af-4985-82eb-33f215fdb266"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""7be68def-20d5-4840-a67e-1db3288f3910"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard & Mouse"",
                    ""action"": ""Selection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a28f7896-5caa-418f-995b-df8a4d015777"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard & Mouse"",
                    ""action"": ""Command"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard & Mouse"",
            ""bindingGroup"": ""Keyboard & Mouse"",
            ""devices"": []
        }
    ]
}");
        // General
        m_General = asset.FindActionMap("General", throwIfNotFound: true);
        m_General_Affirmation = m_General.FindAction("Affirmation", throwIfNotFound: true);
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Selection = m_Player.FindAction("Selection", throwIfNotFound: true);
        m_Player_Command = m_Player.FindAction("Command", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // General
    private readonly InputActionMap m_General;
    private IGeneralActions m_GeneralActionsCallbackInterface;
    private readonly InputAction m_General_Affirmation;
    public struct GeneralActions
    {
        private @Controls m_Wrapper;
        public GeneralActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Affirmation => m_Wrapper.m_General_Affirmation;
        public InputActionMap Get() { return m_Wrapper.m_General; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GeneralActions set) { return set.Get(); }
        public void SetCallbacks(IGeneralActions instance)
        {
            if (m_Wrapper.m_GeneralActionsCallbackInterface != null)
            {
                @Affirmation.started -= m_Wrapper.m_GeneralActionsCallbackInterface.OnAffirmation;
                @Affirmation.performed -= m_Wrapper.m_GeneralActionsCallbackInterface.OnAffirmation;
                @Affirmation.canceled -= m_Wrapper.m_GeneralActionsCallbackInterface.OnAffirmation;
            }
            m_Wrapper.m_GeneralActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Affirmation.started += instance.OnAffirmation;
                @Affirmation.performed += instance.OnAffirmation;
                @Affirmation.canceled += instance.OnAffirmation;
            }
        }
    }
    public GeneralActions @General => new GeneralActions(this);

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Selection;
    private readonly InputAction m_Player_Command;
    public struct PlayerActions
    {
        private @Controls m_Wrapper;
        public PlayerActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Selection => m_Wrapper.m_Player_Selection;
        public InputAction @Command => m_Wrapper.m_Player_Command;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Selection.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSelection;
                @Selection.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSelection;
                @Selection.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSelection;
                @Command.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCommand;
                @Command.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCommand;
                @Command.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCommand;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Selection.started += instance.OnSelection;
                @Selection.performed += instance.OnSelection;
                @Selection.canceled += instance.OnSelection;
                @Command.started += instance.OnCommand;
                @Command.performed += instance.OnCommand;
                @Command.canceled += instance.OnCommand;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard & Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface IGeneralActions
    {
        void OnAffirmation(InputAction.CallbackContext context);
    }
    public interface IPlayerActions
    {
        void OnSelection(InputAction.CallbackContext context);
        void OnCommand(InputAction.CallbackContext context);
    }
}

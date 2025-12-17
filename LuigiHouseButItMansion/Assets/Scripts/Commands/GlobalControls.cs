
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalControls : SingletonBehaviour<GlobalControls>
{
    [SerializeField]
    private InputActionAsset inputActionAsset;
    private InputAction exitGameAction;
    
    private void OnEnable()
    {
        inputActionAsset.FindActionMap("GlobalControls").Enable();
    }
    
    private void OnDisable()
    {
        inputActionAsset.FindActionMap("GlobalControls").Disable();
    }

    private void Start()
    {
        exitGameAction = InputSystem.actions.FindAction("Exit");
        exitGameAction.started += context =>
        {
            new ExitCommand().Execute();
        };
    }
}
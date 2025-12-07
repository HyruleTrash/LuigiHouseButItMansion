using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActionAsset;
    private InputAction shootAction;
    private bool isTryingToShoot;

    private void OnEnable()
    {
        inputActionAsset.FindActionMap("Player").Enable();
    }

    private void Start()
    {
        shootAction = InputSystem.actions.FindAction("Attack");
        shootAction.started += context => { isTryingToShoot = true;};
        shootAction.canceled += context => { isTryingToShoot = false;};
    }

    private void Update()
    {
        if (isTryingToShoot)
            TryShoot();
    }

    private void TryShoot()
    {
        Debug.Log("Shooting!");
    }
}
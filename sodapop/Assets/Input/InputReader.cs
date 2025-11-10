using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    private Controls controls;

    #region Events
    public event Action<bool> OnFireEvent;
    public event Action OnJumpEvent;
    public event Action<Vector3> OnMoveEvent;
    public event Action<bool> OnSprintEvent;
    #endregion

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
        }
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    public void OnFirePrimary(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnFireEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            OnFireEvent?.Invoke(false);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        OnMoveEvent?.Invoke(context.ReadValue<Vector3>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJumpEvent?.Invoke();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnSprintEvent?.Invoke(true);
        else if (context.canceled)
            OnSprintEvent?.Invoke(false);
    }

}
using System;
using System.Collections;
using Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    
    private Animator _animator;
    private InputSystem_Actions _input;

    private bool _canMove = false;
    private readonly float _delayMove = 1f;
    
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _input = new InputSystem_Actions();

        GameController.Instance.OnGameStart += StartGame;
    }

    public void StartGame()
    {
        _input.Player.Enable();
        _input.Player.Attack.performed += OnAttackPerformed;

        _canMove = true;
    }

    private void OnDestroy()
    {
        GameController.Instance.OnGameStart -= StartGame;
        _input.Player.Attack.performed -= OnAttackPerformed;
    }

    private void Update()
    {
        if (_canMove)
        {
            UpdateMovement(Time.deltaTime);
        }
    }

    private void UpdateMovement(float deltaTime)
    {
        Vector2 inputMove = _input.Player.Move.ReadValue<Vector2>();
        
        Vector2 movement = new Vector2();
        movement.x += inputMove.x * deltaTime * moveSpeed;
        movement.y += inputMove.y * deltaTime * moveSpeed;
        
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.Clamp(currentPosition.x + movement.x, -6f, 6f);
        currentPosition.y = transform.position.y;
        currentPosition.z = Mathf.Clamp(currentPosition.z + movement.y, -7f, 5f);
        this.transform.position = currentPosition;
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (!_canMove) return;
        
        _animator.SetTrigger("Hit");
        _canMove = false;

        StartCoroutine(StartMoveAgain(_delayMove));
    }

    private IEnumerator StartMoveAgain(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        _canMove = true;
    }

    #region animation_event
    
    private void OnHitEffect()
    {
        
    }
    
    #endregion

    public void PlayerScoreHit()
    {
        //throw new NotImplementedException();
    }
}
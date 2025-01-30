using System;
using System.Collections;
using System.Threading.Tasks;
using Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    
    private Animator _animator;
    private InputSystem_Actions _input;

    public bool isPlayer = false;
    private bool _canMove = false;
    private readonly float _delayMove = 1f;
    public int position = 0;
    public int score = 0;

    private AudioSource _audioSource;
    [SerializeField] AudioClip _swingSound;
    [SerializeField] AudioClip _hitSound;
    [SerializeField] ParticleSystem _hitEffect;
    [SerializeField] TextMesh _youSign;
    
    public static readonly float sendingPeriod = 0.03f;
    private float timeLastSending = 0.0f;
    public bool send = false;
    
    private PlayerTransform lastState;
    private PlayerTransform newState;

    private SyncManager _syncManager;
    
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _input = new InputSystem_Actions();
        _audioSource = GetComponent<AudioSource>();
        _syncManager = GetComponent<SyncManager>();
        
        _input.Player.Enable();
        _input.Player.Attack.performed += OnAttackPerformed;

        Debug.Log("Start");
    }

    public void StartGame()
    {
        _canMove = true;
        
        Debug.Log("Start Game"+this.name);
    }

    private void OnDestroy()
    {
        Debug.Log("on destroy");
        
        GameController.Instance.OnGameStart -= StartGame;
        _input.Player.Attack.performed -= OnAttackPerformed;
        _input.Player.Disable();
    }

    private void Update()
    {
        if (isPlayer)
        {
            if (_canMove)
            {
                UpdateMovement(Time.deltaTime);
            }
            SendTransform();
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
        if (!isPlayer) return;
        if (!_canMove) return;
        
        _animator.SetTrigger("Hit");
        NetworkController.Instance.SendAnimationState("hitting");
        
        _canMove = false;

        _audioSource.PlayOneShot(_swingSound);
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

    public void PlayerScoreHit(int moleIndex)
    {
        _audioSource.PlayOneShot(_hitSound);
        _hitEffect.Play();
        //throw new NotImplementedException();

        NetworkController.Instance.SendPlayerScore(moleIndex);
    }
    
    public void AnimationSync(string value)
    {
        if (_animator)
        {
            if (value == "hitting")
            {
                this._animator.SetTrigger("Hit");
                _audioSource.PlayOneShot(_swingSound);
            }
        }
    }
    
    /**
    * If this Script is on the Local Players (isPlayer) SendTransform is called.
    * If this is a remote character, checkPosition is called every frame.
    * The ReceiveTransform method is called to update the position
    * and depends which Network Sync mode is used.
    */

    #region  Transform Synchronisation
    public void SendTransform()
    {
        if (isPlayer)
        {
            if (timeLastSending >= sendingPeriod)
            {
                lastState = PlayerTransform.FromTransform(this.transform);
                NetworkController.Instance.SendTransform(lastState);
                timeLastSending = 0;
                return;
            }
            timeLastSending += Time.deltaTime;
        }
    }

    public void ReceiveTransform(PlayerTransform chtransform)
    {
        if (!isPlayer)
        {
            if (_syncManager)
                _syncManager.ReceivedTransform(chtransform);
        }
    }

    #endregion
    
}
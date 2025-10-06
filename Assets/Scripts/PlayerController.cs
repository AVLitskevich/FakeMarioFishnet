using FishNet;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using FishNet.Utility.Template;
using GameKit.Dependencies.Utilities;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class PlayerController : TickNetworkBehaviour
    {
        public struct Input : IReplicateData
        {
            private uint _tick;

            public float Movement;
            public bool Jump;

            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
            public void Dispose() { }
        }
        
        public struct State : IReconcileData
        {
            private uint _tick;

            public float KnockbackTimer;
            public float Health;
            public PredictionRigidbody2D Rigidbody;
            
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;

            public void Dispose() { }
        }

        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private Image _hpBarImage;
        [SerializeField] private Rigidbody2D _rigidbody;

        [Header("Movements")] 
        [SerializeField] private PlayerMovementConfig _playerMovementConfig;
        
        [Header("Debug")]
        [SerializeField] private bool _canMove;
        [SerializeField] private GameState _state;
        
        private int _jumpsUsed;
        private bool _wasGrounded;

        private bool IsGrounded => Physics2D.Raycast(transform.position, Vector2.down,
            _playerMovementConfig._groundCheckDistance, _groundMask);

        private bool CanMove()
        {
            return RaceManager.Instance.CurrentState == GameState.Race || RaceManager.Instance.CurrentState == GameState.Waiting;
        }

        private PredictionRigidbody2D _predictionRigidbody;
        private PlayerControls _playerControls;

        private int _defaultLayer;
        private int? _layer;

        private float _movement;
        private bool _jump;
        private float _health;
        private float _knockbackTimer;

        private void Awake()
        {
            _predictionRigidbody = ObjectCaches<PredictionRigidbody2D>.Retrieve();
            _predictionRigidbody.Initialize(_rigidbody);

            _knockbackTimer = 0f;
            _health = _playerMovementConfig._maxHealth;
            _defaultLayer = gameObject.layer;
        }

        private void OnCollisionStateChanged(bool prev, bool next, bool asServer)
        {
            if (next)
            {
                ReturnLayer();
            }
            else
            {
                ReturnLayer();
                GetLayer();
            }
        }

        private void GetLayer()
        {
            if (LayerProvider.Instance.TryGetLayer(out int layer))
            {
                gameObject.SetLayerWithChildren(layer);
                _layer = layer;
            }
        }

        private void ReturnLayer()
        {
            if (_layer.HasValue)
            {
                LayerProvider.Instance.ReturnLayer(_layer.Value);
                _layer = null;
                gameObject.SetLayerWithChildren(_defaultLayer);
            }
        }

        public override void OnStartNetwork()
        {
            LayerProvider.Instance.CollisionsState.OnChange += OnCollisionStateChanged;
            if (!LayerProvider.Instance.CollisionsState.Value)
                GetLayer();

            if (!Owner.IsLocalClient)
            {
                TryApplyNonLocalVisuals();
                return;
            }

            _playerControls = new PlayerControls();
            _playerControls.Enable();
            _playerControls.Player.Move.performed += OnMovePerformed;
            _playerControls.Player.Move.canceled += OnMovePerformed;
            _playerControls.Player.Jump.performed += OnJumpPerformed;
            
            var cinemachineCamera = FindAnyObjectByType<CinemachineCamera>(FindObjectsInactive.Include);
            if (cinemachineCamera != null)
                cinemachineCamera.Target.TrackingTarget = NetworkObject.GetGraphicalObject().transform;
        }

        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            _movement = ctx.ReadValue<Vector2>().x;
        }

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            _jump = true;
        }

        private void TryApplyNonLocalVisuals()
        {
            var sprite = GetComponentInChildren<SpriteRenderer>();
            if (sprite == null)
            {
                return;
            }
            var c = sprite.color;
            c *= 0.7f;
            c.a = 0.9f;
            sprite.color = c;
        }

        private void OnDestroy()
        {
            LayerProvider.Instance.CollisionsState.OnChange -= OnCollisionStateChanged;
            
            if (_playerControls != null)
            {
                _playerControls.Player.Move.performed -= OnMovePerformed;
                _playerControls.Player.Move.canceled -= OnMovePerformed;
                _playerControls.Player.Jump.performed -= OnJumpPerformed;
                _playerControls.Disable();
                _playerControls = null;
            }
            
            ReturnLayer();
            ObjectCaches<PredictionRigidbody2D>.StoreAndDefault(ref _predictionRigidbody);
        }

        protected override void TimeManager_OnTick()
        {
            SimulateInputs(GetInput());
        }

        private Input GetInput()
        {
            if (!IsOwner)
                return default;

            var input = new Input
            {
                Movement = _movement,
                Jump = _jump
            };
            _jump = false;
            return input;
        }

        [Replicate]
        private void SimulateInputs(Input input, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            float dt = (float)TimeManager.TickDelta;
            Vector2 currentVelocity = _rigidbody.linearVelocity;
            _hpBarImage.fillAmount = _health / _playerMovementConfig._maxHealth;
            if (_knockbackTimer > 0f)
            {
                _knockbackTimer -= dt;
                if (_knockbackTimer <= 0f && _health <= 0f)
                {
                    _predictionRigidbody.Velocity(Vector2.zero);
                    _rigidbody.position = Vector2.zero;
                    _health = _playerMovementConfig._maxHealth;
                }
            }
            else if (CanMove())
            {
                float newX = currentVelocity.x;
                if (Mathf.Abs(input.Movement) > 0.01f)
                {
                    float targetSpeed = input.Movement * _playerMovementConfig._speed;
                    var acceleration = IsGrounded ? _playerMovementConfig._groundAcceleration : _playerMovementConfig._airAcceleration;
                    newX = Mathf.MoveTowards(currentVelocity.x, targetSpeed, acceleration * dt);
                    _predictionRigidbody.Velocity(new Vector2(newX, _rigidbody.linearVelocity.y));
                    if (input.Movement > 0)
                    {
                        _spriteRenderer.flipX = false;
                    }
                    else if (input.Movement < 0)
                    {
                        _spriteRenderer.flipX = true;
                    }
                }
                else
                {
                    var deceleration = IsGrounded ? _playerMovementConfig._groundDeceleration : _playerMovementConfig._airDeceleration;
                    newX = Mathf.MoveTowards(currentVelocity.x, 0f, deceleration * dt);
                    _predictionRigidbody.Velocity(new Vector2(newX, _rigidbody.linearVelocity.y));
                }

                if (input.Jump && IsGrounded)
                {
                    Jump(state);
                }
            }

            _canMove = CanMove();
            _state = RaceManager.Instance.CurrentState;
            _predictionRigidbody.AddForce(Physics2D.gravity);
            var velocity = _rigidbody.linearVelocity;
            if (velocity.y < -_playerMovementConfig._maxFallSpeed)
            {
                velocity.y = -_playerMovementConfig._maxFallSpeed;
                _predictionRigidbody.Velocity(velocity);
            }

            bool groundedNow = IsGrounded;
            if (state.ContainsCreated() && !state.IsFuture())
            {
                _animator.SetFloat("SpeedX", Mathf.Abs(_rigidbody.linearVelocity.x));
                _animator.SetFloat("VelY", Mathf.Abs(_rigidbody.linearVelocity.y));
                _animator.SetBool("Grounded", groundedNow);
                if (state.ContainsTicked() && !state.ContainsReplayed())
                {
                    if (!_wasGrounded && groundedNow)
                    {
                        _animator.ResetTrigger("JumpTrigger");
                    }
                }
            }

            _wasGrounded = groundedNow;
            
            _predictionRigidbody.Simulate();
        }

        protected override void TimeManager_OnPostTick()
        {
            CreateReconcile();
        }

        public override void CreateReconcile()
        {
            var state = new State
            {
                Rigidbody = _predictionRigidbody,
                KnockbackTimer = _knockbackTimer,
                Health = _health
            };
            ReconcileState(state);
        }

        [Reconcile]
        private void ReconcileState(State state, Channel channel = Channel.Unreliable)
        {
            _knockbackTimer = state.KnockbackTimer;
            _health = state.Health;
            _predictionRigidbody.Reconcile(state.Rigidbody);
        }
        
        private void Jump(ReplicateState state)
        {
            float jumpForce = Mathf.Sqrt(Mathf.Abs(-2.0f * Physics.gravity.y * _playerMovementConfig._jumpHeight * _rigidbody.gravityScale));
            if (_rigidbody.linearVelocity.y < 0f)
                jumpForce -= _rigidbody.linearVelocity.y;

            if (state.ContainsTicked() && !state.ContainsReplayed())
            {
                _animator.SetTrigger("JumpTrigger");
            }
            _predictionRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        public void TakeDamage(ObstacleController obstacleController)
        {
            _health -= obstacleController.Damage;
            Knockback(obstacleController.transform.position);
        }
        
        private void Knockback(Vector3 attackerPosition)
        {
            Vector2 direction = (transform.position - attackerPosition).normalized;

            if (Mathf.Abs(direction.x) < 0.1f) {
                direction.x = transform.position.x >= attackerPosition.x ? 1f : -1f;
            }

            direction.y = Mathf.Abs(direction.y);
            direction.Normalize();

            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.AddForce(direction * _playerMovementConfig._knockbackForce, ForceMode2D.Impulse);
            
            _knockbackTimer = .5f;
        }
    }
}
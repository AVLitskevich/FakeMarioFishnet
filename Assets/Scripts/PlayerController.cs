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

            public void Dispose()
            {
            }
        }

        public struct State : IReconcileData
        {
            private uint _tick;

            public float KnockbackTimer;
            public float CoyoteTimer;
            public float Health;
            public float PrevHorizontalSpeed;
            public float HorizontalSpeed;
            public bool WasGrounded;
            public PredictionRigidbody2D Rigidbody;

            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;

            public void Dispose()
            {
            }
        }

        [SerializeField] private bool _predictInputs;
        [SerializeField] private float _speed;
        [SerializeField] private float _jumpHeight;
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _knockbackForce;
        [SerializeField] private float _groundCheckDistance;
        [SerializeField] private float _coyoteTime;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private Image _hpBarImage;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private float _groundAcceleration;
        [SerializeField] private float _groundDeceleration;
        [SerializeField] private float _airAcceleration;
        [SerializeField] private float _airDeceleration;
        [SerializeField] private float _maxFallSpeed;

        private bool IsGrounded => Physics2D.Raycast(transform.position, Vector2.down,
            _groundCheckDistance, _groundMask);

        private bool CanMove()
        {
            return RaceManager.Instance.CurrentState == GameState.Race ||
                   RaceManager.Instance.CurrentState == GameState.Waiting;
        }

        private PredictionRigidbody2D _predictionRigidbody;
        private PlayerControls _playerControls;

        private int _defaultLayer;
        private int? _layer;

        private float _movement;
        private bool _jump;
        private float _health;
        private float _knockbackTimer;
        private float _coyoteTimer;
        
        private float _prevHorizontalSpeed;
        private float _horizontalSpeed;
        private bool _wasGrounded;

        private Input _lastCreatedInput;

        private void Awake()
        {
            _predictionRigidbody = ObjectCaches<PredictionRigidbody2D>.Retrieve();
            _predictionRigidbody.Initialize(_rigidbody);

            _knockbackTimer = 0f;
            _coyoteTimer = _coyoteTime;
            _health = _maxHealth;
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
                return;

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
        private void SimulateInputs(Input input, ReplicateState state = ReplicateState.Invalid,
            Channel channel = Channel.Unreliable)
        {
            float dt = (float)TimeManager.TickDelta;
            Vector2 currentVelocity = _rigidbody.linearVelocity;
            _prevHorizontalSpeed = _horizontalSpeed;
            _horizontalSpeed = currentVelocity.x;
            _hpBarImage.fillAmount = _health / _maxHealth;
            
            bool isGrounded = IsGrounded;
            if (state.IsFuture())
            {
                if (_predictInputs)
                    input = _lastCreatedInput;
            }
            else if (state.IsReplayedCreated())
            {
                _lastCreatedInput = input;
            }

            if (_knockbackTimer > 0f)
            {
                _knockbackTimer -= dt;
                if (_knockbackTimer <= 0f && _health <= 0f)
                {
                    _predictionRigidbody.Velocity(Vector2.zero);
                    _rigidbody.position = Vector2.zero;
                    _health = _maxHealth;
                }
            }
            else if (CanMove())
            {
                if (isGrounded && !_wasGrounded)
                {
                    if (!Mathf.Approximately(_horizontalSpeed, 0f))
                    {
                        float landAcceleration = _prevHorizontalSpeed / _horizontalSpeed;
                        _predictionRigidbody.AddForce(currentVelocity * landAcceleration);
                        currentVelocity.x += landAcceleration;
                    }
                }
                
                if (Mathf.Abs(input.Movement) > 0.01f)
                {
                    float targetSpeed = input.Movement * _speed;
                    var acceleration = IsGrounded ? _groundAcceleration : _airAcceleration;
                    float newX = Mathf.MoveTowards(currentVelocity.x, targetSpeed, acceleration * dt);
                    _predictionRigidbody.Velocity(new Vector2(newX, currentVelocity.y));
                }
                else
                {
                    var deceleration = IsGrounded ? _groundDeceleration : _airDeceleration;
                    float newX = Mathf.MoveTowards(currentVelocity.x, 0f, deceleration * dt);
                    _predictionRigidbody.Velocity(new Vector2(newX, currentVelocity.y));
                }

                if (input.Jump && (isGrounded || _coyoteTimer > 0f))
                {
                    Jump();
                }
            }

            if (isGrounded)
                _coyoteTimer = _coyoteTime;
            else
                _coyoteTimer -= (float)TimeManager.TickDelta;

            _wasGrounded = isGrounded;
            _predictionRigidbody.AddForce(Physics2D.gravity);
            
            var velocity = _rigidbody.linearVelocity;
            if (velocity.y < -_maxFallSpeed)
            {
                velocity.y = -_maxFallSpeed;
                _predictionRigidbody.Velocity(velocity);
            }

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
                Health = _health,
                PrevHorizontalSpeed = _prevHorizontalSpeed,
                HorizontalSpeed = _horizontalSpeed,
                WasGrounded = _wasGrounded,
            };
            ReconcileState(state);
        }

        [Reconcile]
        private void ReconcileState(State state, Channel channel = Channel.Unreliable)
        {
            _knockbackTimer = state.KnockbackTimer;
            _health = state.Health;
            _coyoteTimer = state.CoyoteTimer;
            _prevHorizontalSpeed = state.PrevHorizontalSpeed;
            _horizontalSpeed = state.HorizontalSpeed;
            _wasGrounded = state.WasGrounded;
            _predictionRigidbody.Reconcile(state.Rigidbody);
        }

        private void Jump()
        {
            float jumpForce = Mathf.Sqrt(Mathf.Abs(-2.0f * Physics.gravity.y * _jumpHeight * _rigidbody.gravityScale));
            if (_rigidbody.linearVelocity.y < 0f)
                jumpForce -= _rigidbody.linearVelocity.y;

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

            if (Mathf.Abs(direction.x) < 0.1f)
            {
                direction.x = transform.position.x >= attackerPosition.x ? 1f : -1f;
            }

            direction.y = Mathf.Abs(direction.y);
            direction.Normalize();

            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.AddForce(direction * _knockbackForce, ForceMode2D.Impulse);

            _knockbackTimer = .5f;
        }
    }
}
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using FishNet.Utility.Template;
using GameKit.Dependencies.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
    public class PlayerMovement : TickNetworkBehaviour

    {
        public struct Input : IReplicateData
        {
            private uint _tick;
            public float Movement;
            public bool Jump;
            public uint GetTick()
            {
                return _tick;
            }

            public void SetTick(uint value)
            {
                _tick = value;
            }

            public void Dispose()
            {
            }
        }

        public struct State : IReconcileData
        {
            private uint _tick;

            public float KnockbackTimer;
            public float Health;
            public float CoyoteTimer;
            public PredictionRigidbody2D Rigidbody;

            public float BuffTimer;
            public float SlowTimer;
            public float SlowChargeTimer;
            public bool HasSlowCharge;

            public uint GetTick()
            {
                return _tick;
            }

            public void SetTick(uint value)
            {
                _tick = value;
            }

            public void Dispose()
            {
            }
        }

        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private Rigidbody2D _rigidbody;
        
        [SerializeField] private PlayerMovementConfig _playerMovementConfig;

        public Vector2 Velocity
        {
            get { return _rigidbody.linearVelocity; }
        }

        public bool Grounded
        {
            get
            {
                return Physics2D.Raycast(transform.position, Vector2.down, _playerMovementConfig._groundCheckDistance, _groundMask);
            }
        }

        public float Health01 => Mathf.Clamp01(_health / _playerMovementConfig._maxHealth);
        public bool HasSpeedBuff
        {
            get { return _buffTimer > 0f; }
        }

        public bool HasSlowDebuff
        {
            get { return _slowTimer > 0f; }
        }

        public bool HasSlowCharge
        {
            get { return _hasSlowCharge; }
        }

        public event System.Action JumpFx;

        private bool CanMove()
        {
            return _knockbackTimer <= 0 && _health > 0 && RaceManager.Instance.CurrentState is GameState.Race or GameState.Waiting;
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
        private bool _wasGrounded;

        private float _buffTimer;
        private float _slowTimer;
        private float _slowChargeTimer;
        private bool _hasSlowCharge;

        private Input _lastCreatedInput;

        private float SpeedMultiplier
        {
            get
            {
                float multiplier = 1f;
                if (_buffTimer > 0f)
                {
                    multiplier *= _playerMovementConfig._speedBuffMultiplier;
                }

                if (_slowTimer > 0f)
                {
                    multiplier *= _playerMovementConfig._slowDebuffMultiplier;
                }
                return multiplier;
            }
        }

        private void Awake()
        {
            _predictionRigidbody = ObjectCaches<PredictionRigidbody2D>.Retrieve();
            _predictionRigidbody.Initialize(_rigidbody);

            _knockbackTimer = 0f;
            _health = _playerMovementConfig._maxHealth;
            _defaultLayer = gameObject.layer;

            _buffTimer = 0f;
            _slowTimer = 0f;
            _slowChargeTimer = 0f;
            _hasSlowCharge = false;
        }

        public override void OnStartNetwork()
        {
            LayerProvider.Instance.CollisionsState.OnChange += OnCollisionStateChanged;
            if (!LayerProvider.Instance.CollisionsState.Value)
            {
                GetLayer();
            }

            if (!Owner.IsLocalClient)
            {
                return;
            }
            
            _playerControls = new PlayerControls();
            _playerControls.Enable();
            _playerControls.Player.Move.performed += OnMovePerformed;
            _playerControls.Player.Move.canceled += OnMovePerformed;
            _playerControls.Player.Jump.performed += OnJumpPerformed;
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
        
        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            _movement = ctx.ReadValue<Vector2>().x;
        }

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            _jump = true;
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
        
        protected override void TimeManager_OnTick() => SimulateInputs(GetInput());

        private Input GetInput()
        {
            if (!IsOwner)
            {
                return default;
            }
            var input = new Input { Movement = _movement, Jump = _jump };
            _jump = false;
            return input;
        }
        
        [Replicate]
        private void SimulateInputs(Input input, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            float dt = (float)TimeManager.TickDelta;
            Vector2 currentVelocity = _rigidbody.linearVelocity;
            bool isGrounded = Grounded;

            if (state.IsFuture())
            {
                if (_playerMovementConfig._predictInputs)
                    input = _lastCreatedInput;
            }
            else if (state.IsReplayedCreated())
            {
                _lastCreatedInput = input;
            }

            if (_knockbackTimer > 0f)
            {
                _knockbackTimer -= dt;
            }
            else if (_knockbackTimer <= 0f && _health <= 0f)
            {
                _predictionRigidbody.Velocity(Vector2.zero);
                _rigidbody.position = Vector2.zero;
                _health = _playerMovementConfig._maxHealth;
            }

            if (_buffTimer > 0f)
            {
                _buffTimer -= dt;
            }

            if (_slowTimer > 0f)
            {
                _slowTimer -= dt;
            }
            if (_hasSlowCharge)
            {
                _slowChargeTimer -= dt;
                if (_slowChargeTimer <= 0f)
                {
                    _hasSlowCharge = false;
                    _slowChargeTimer = 0f;
                }
            }

            if (CanMove())
            {
                if (Mathf.Abs(input.Movement) > 0.01f)
                {
                    float baseSpeed = _playerMovementConfig._speed * SpeedMultiplier;
                    float targetSpeed = input.Movement * baseSpeed;

                    float acceleration = Grounded ? _playerMovementConfig._groundAcceleration : _playerMovementConfig._airAcceleration;
                    float newX = Mathf.MoveTowards(currentVelocity.x, targetSpeed, acceleration * dt);
                    _predictionRigidbody.Velocity(new Vector2(newX, currentVelocity.y));
                }
                else
                {
                    float deceleration = Grounded ? _playerMovementConfig._groundDeceleration : _playerMovementConfig._airDeceleration;
                    float newX = Mathf.MoveTowards(currentVelocity.x, 0f, deceleration * dt);
                    _predictionRigidbody.Velocity(new Vector2(newX, currentVelocity.y));
                }

                if (input.Jump && (isGrounded || _coyoteTimer > 0f))
                {
                    Jump(state);
                }
            }

            if (isGrounded)
            {
                _coyoteTimer = _playerMovementConfig._coyoteTime;
            }
            else
            {
                _coyoteTimer -= (float)TimeManager.TickDelta;
            }

            _wasGrounded = isGrounded;

            Vector2 velocity = _rigidbody.linearVelocity;
            if (velocity.y < -_playerMovementConfig._maxFallSpeed)
            {
                velocity.y = -_playerMovementConfig._maxFallSpeed;
                _predictionRigidbody.Velocity(velocity);
            }

            _predictionRigidbody.AddForce(Physics2D.gravity);
            _predictionRigidbody.Simulate();
        }
        
        protected override void TimeManager_OnPostTick()
        {
            CreateReconcile();
        }

        public override void CreateReconcile()
        {
            State state = new State
            {
                Rigidbody = _predictionRigidbody,
                KnockbackTimer = _knockbackTimer,
                Health = _health,
                CoyoteTimer = _coyoteTimer,

                BuffTimer = _buffTimer,
                SlowTimer = _slowTimer,
                SlowChargeTimer = _slowChargeTimer,
                HasSlowCharge = _hasSlowCharge
            };
            ReconcileState(state);
        }
        
        [Reconcile]
        private void ReconcileState(State state, Channel channel = Channel.Unreliable)
        {
            _knockbackTimer = state.KnockbackTimer;
            _health = state.Health;
            _coyoteTimer = state.CoyoteTimer;
            _predictionRigidbody.Reconcile(state.Rigidbody);

            _buffTimer = state.BuffTimer;
            _slowTimer = state.SlowTimer;
            _slowChargeTimer = state.SlowChargeTimer;
            _hasSlowCharge = state.HasSlowCharge;
        }
        
        private void Jump(ReplicateState state)
        {
            float jumpForce = Mathf.Sqrt(Mathf.Abs(-2.0f * Physics.gravity.y * _playerMovementConfig._jumpHeight * _rigidbody.gravityScale));
            if (_rigidbody.linearVelocity.y < 0f)
            {
                jumpForce -= _rigidbody.linearVelocity.y;
            }

            if (state.ContainsTicked() && !state.ContainsReplayed())
            {
                JumpFx?.Invoke();
            }

            _predictionRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        
        public void TakeDamage(ObstacleController obstacleController)
        {
            _health -= obstacleController.Damage;
            if (_health <= 0)
            {
                return;
            }
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
            _rigidbody.AddForce(direction * _playerMovementConfig._knockbackForce, ForceMode2D.Impulse);
            _knockbackTimer = .5f;
        }
        
        [Server] public void ApplySpeedBuff(float duration)
        {
            _buffTimer = Mathf.Max(_buffTimer, duration);
        }

        [Server] public void ApplySlowDebuff(float duration)
        {
            _slowTimer = Mathf.Max(_slowTimer, duration);
        }

        [Server]
        public void GrantSlowCharge(float window)
        {
            _hasSlowCharge = true;
            _slowChargeTimer = Mathf.Max(_slowChargeTimer, window);
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!IsServerInitialized || !_hasSlowCharge)
            {
                return;
            }
            PlayerMovement target = other.collider.GetComponentInParent<PlayerMovement>();
            if (target == null || target == this)
            {
                return;
            }

            target.ApplySlowDebuff(_playerMovementConfig._slowDebuffDuration);
            _hasSlowCharge = false;
            _slowChargeTimer = 0f;
        }
    }
}
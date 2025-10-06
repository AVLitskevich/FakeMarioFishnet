using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerMovementConfig
    {
        [SerializeField] public int _maxAirJumps = 1;
        [SerializeField] public float _groundAcceleration;
        [SerializeField] public float _groundDeceleration;
        [SerializeField] public float _airAcceleration;
        [SerializeField] public float _airDeceleration;
        [SerializeField] public float _maxFallSpeed;
        [SerializeField] public float _speed;
        [SerializeField] public float _jumpHeight;
        [SerializeField] public float _maxHealth;
        [SerializeField] public float _knockbackForce;
        [SerializeField] public float _groundCheckDistance;
        [SerializeField] public float _coyoteTime;
    }
}
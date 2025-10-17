using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Player
{
    public class PlayerVisuals : MonoBehaviour
    {
        private static readonly int JumpTrigger = Animator.StringToHash("JumpTrigger");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int VelY = Animator.StringToHash("VelY");
        private static readonly int SpeedX = Animator.StringToHash("SpeedX");
        
        [Header("References")]
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Image _hpBarImage;
        
        [Header("Config")]
        [SerializeField] private float _speedThreshold;
        [SerializeField] private float _remoteDarkenMul = 0.7f;
        [SerializeField] private float _remoteAlpha = 0.9f;

        private void OnEnable()
        {
            _movement.JumpFx += OnJumpFx;
        }

        private void OnDisable()
        {
            _movement.JumpFx -= OnJumpFx;
        }

        private void Start()
        {
            if (_movement.NetworkObject.Owner.IsLocalClient)
            {
                CinemachineCamera targetCamera = FindAnyObjectByType<CinemachineCamera>(FindObjectsInactive.Include);
                if (targetCamera != null)
                {
                    targetCamera.Target.TrackingTarget = transform;
                }
            }
            else
            {
                TryApplyNonLocalVisuals();
            }
        }

        private void Update()
        {
            Vector2 vel = _movement.Velocity;
            _animator.SetFloat(SpeedX, GetAbsWithThreshold(vel.x));
            _animator.SetFloat(VelY, GetAbsWithThreshold(vel.y));
            _animator.SetBool(Grounded, _movement.IsGrounded);

            if (vel.x > 0.01f)
            {
                _spriteRenderer.flipX = false;
            }
            else if (vel.x < -0.01f)
            {
                _spriteRenderer.flipX = true;
            }
            
            _hpBarImage.fillAmount = _movement.Health01;
        }

        private float GetAbsWithThreshold(float value)
        {
            var speed = Mathf.Abs(value);
            if (speed < _speedThreshold)
                speed = 0;

            return speed;
        }
        
        private void OnJumpFx()
        {
            _animator.SetTrigger(JumpTrigger);
        }
        
        private void TryApplyNonLocalVisuals()
        {
            if (_spriteRenderer == null)
            {
                return;
            }
            
            Color c = _spriteRenderer.color;
            c *= _remoteDarkenMul;
            c.a = _remoteAlpha;
            _spriteRenderer.color = c;
        }
    }
}
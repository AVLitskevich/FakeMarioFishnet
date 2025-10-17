using System;
using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine;
using Unity.UI;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class PlayerVisuals : MonoBehaviour
    {
        private static readonly int JumpTrigger = Animator.StringToHash("JumpTrigger");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int VelY = Animator.StringToHash("VelY");
        private static readonly int SpeedX = Animator.StringToHash("SpeedX");
        
        [SerializeField] private PlayerMovement _movement;
        
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Image _hpBarImage;
        
        [SerializeField] private float _remoteDarkenMul = 0.7f;
        [SerializeField] private float _remoteAlpha = 0.9f;
      

        private void OnEnable()
        {
            if (_movement != null)
            {
                _movement.JumpFx += OnJumpFx;
            }
        }

        private void OnDisable()
        {
            if (_movement != null)
            {
                _movement.JumpFx -= OnJumpFx;
            }
        }

        private void Start()
        {
            NetworkObject networkObject = _movement.NetworkObject;
            if (networkObject != null && networkObject.Owner.IsLocalClient)
            {
                CinemachineCamera cine = FindAnyObjectByType<CinemachineCamera>(FindObjectsInactive.Include);
                if (cine != null)
                {
                    cine.Target.TrackingTarget = networkObject.GetGraphicalObject().transform;
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
            _animator.SetFloat(SpeedX, Mathf.Abs(vel.x));
            _animator.SetFloat(VelY, Mathf.Abs(vel.y));
            _animator.SetBool(Grounded, _movement.Grounded);

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
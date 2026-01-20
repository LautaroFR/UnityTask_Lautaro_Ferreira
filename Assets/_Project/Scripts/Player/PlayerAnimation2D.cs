using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class PlayerAnimator2D : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private PlayerMovement2D movement;

        [Header("Tuning")]
        [SerializeField, Min(0f)] private float idleFreezeThreshold = 0.01f;

        private Animator _animator;
        private SpriteRenderer _spriteRenderer;

        // Direction used by the blend tree (discrete to avoid ambiguity)
        private Vector2 _animDir = Vector2.down;

        private float _baseAnimatorSpeed = 1f;
        private bool _lastFlipX;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _baseAnimatorSpeed = _animator.speed;

            // Initialize flip cache
            _lastFlipX = _spriteRenderer.flipX;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-wire in editor to reduce setup mistakes.
            if (movement == null)
                movement = GetComponentInParent<PlayerMovement2D>();
        }
#endif

        private void Update()
        {
            if (movement == null)
                return;

            Vector2 move = movement.MoveDir;
            float speed01 = movement.Speed01;

            // Update animation direction only when the player is actually moving.
            if (move.sqrMagnitude > 0.0001f)
                _animDir = GetDiscreteDirection(move);

            // Drive blend tree params.
            _animator.SetFloat(SpeedHash, speed01);
            _animator.SetFloat(MoveXHash, _animDir.x);
            _animator.SetFloat(MoveYHash, _animDir.y);

            // Flip only when relevant and only if it changed.
            if (Mathf.Abs(_animDir.x) > 0.1f)
            {
                bool flipX = _animDir.x > 0f;
                if (flipX != _lastFlipX)
                {
                    _spriteRenderer.flipX = flipX;
                    _lastFlipX = flipX;
                }
            }

            // Freeze animation when idle (keeps last pose).
            _animator.speed = (speed01 > idleFreezeThreshold) ? _baseAnimatorSpeed : 0f;
        }

        private static Vector2 GetDiscreteDirection(Vector2 move)
        {
            // Dominant axis selection for stable direction (prevents jitter on diagonals).
            if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
                return new Vector2(Mathf.Sign(move.x), 0f);
            return new Vector2(0f, Mathf.Sign(move.y));  
        }
    }
}

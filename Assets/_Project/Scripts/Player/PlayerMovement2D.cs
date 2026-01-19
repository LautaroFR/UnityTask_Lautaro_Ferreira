using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement2D : MonoBehaviour
    {
        [Header("Tuning")]
        [SerializeField] private float moveSpeed = 4.5f;

        private Rigidbody2D _rb;
        private Vector2 _input;
        private Vector2 _move;

        public Vector2 LastMoveDir { get; private set; } = Vector2.down;
        public float Speed01 { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            _move = _input.normalized;

            if (_move.sqrMagnitude > 0.0001f)
                LastMoveDir = _move;

            Speed01 = Mathf.Clamp01(_move.magnitude);
        }

        private void FixedUpdate()
        {
            Vector2 newPos = _rb.position + _move * (moveSpeed * UnityEngine.Time.fixedDeltaTime);
            _rb.MovePosition(newPos);
        }
    }
}

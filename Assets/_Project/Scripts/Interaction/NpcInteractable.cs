using UnityEngine;
using Project.UI;

namespace Project.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class NpcInteractable : MonoBehaviour, IInteractable
    {
        [System.Serializable]
        public struct DialogueLine
        {
            public Speakers speaker;
            [TextArea] public string text;
        }

        [Header("Dialogue")]
        [SerializeField] private DialogueLine[] lines;

        [Header("Wiring")]
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private Transform tooltip;

        private bool _playerInRange;
        private int _lineIndex = 0;
        private bool _isDialogueOpen = false;

        private PlayerInteractor _interactor;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Awake()
        {
            if (tooltip != null)
                tooltip.gameObject.SetActive(false);
        }

        // 🔹 Este método reemplaza el Input.GetKeyDown(KeyCode.E)
        public void Interact()
        {
            if (!_playerInRange) return;
            AdvanceDialogue();
        }

        private void AdvanceDialogue()
        {
            if (dialogueUI == null) return;
            if (lines == null || lines.Length == 0) return;

            if (!_isDialogueOpen)
            {
                _isDialogueOpen = true;
                _lineIndex = 0;
                ShowCurrentLine();
                return;
            }

            _lineIndex++;

            if (_lineIndex >= lines.Length)
            {
                CloseDialogue();
                return;
            }

            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            var line = lines[_lineIndex];
            dialogueUI.Show(line.speaker.ToString(), line.text);
        }

        private void CloseDialogue()
        {
            _isDialogueOpen = false;
            _lineIndex = 0;

            if (dialogueUI != null)
                dialogueUI.Hide();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInRange = true;

            _interactor = other.GetComponent<PlayerInteractor>();
            if (_interactor != null)
                _interactor.SetInteractable(this);

            if (tooltip != null)
                tooltip.gameObject.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInRange = false;

            if (_interactor != null)
                _interactor.ClearInteractable(this);

            _interactor = null;

            if (tooltip != null)
                tooltip.gameObject.SetActive(false);

            CloseDialogue();
        }
    }

    public enum Speakers
    {
        Sarah,
        Player
    }
}

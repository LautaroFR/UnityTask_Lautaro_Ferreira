using TMPro;
using UnityEngine;

namespace Project.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text dialogueText;

        private void Awake()
        {
            Hide();
        }

        public void Show(string speaker, string text)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (speakerText != null) speakerText.text = speaker;
            if (dialogueText != null) dialogueText.text = text;
        }

        public void Hide()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }
    }
}

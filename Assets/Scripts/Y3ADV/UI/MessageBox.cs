using TMPro;
using UnityEngine;

namespace Y3ADV
{
    public class MessageBox : MonoBehaviour
    {
        public RectTransform rectTransform;
        public TextMeshProUGUI nameTag;
        public TextMeshProUGUI messagePanel;
        public Breathe nextMessageIndicator;

        private RichText _currentText;

        private RichText currentText
        {
            get => _currentText ??= new RichText("");
            set => _currentText = value;
        }

        public string displayText => messagePanel.text;

        private float timeStarted = 1f;
        private float displayTime;
        private bool finished;
        private int lastCharacterIndex = -1;

        public void SetText(string text)
        {
            currentText = new RichText(text.Replace("\\n", "\n"));
            displayTime =
                currentText.Length *
                0.05f; //SingletonMonoBehaviour<ScenarioConfig>.Instance.intervalForCharacterDisplay;
            timeStarted = Time.time;
            lastCharacterIndex = -1;
            finished = false;
            messagePanel.text = string.Empty;
            messagePanel.lineSpacing = 1f - 38f; //SingletonMonoBehaviour<ScenarioConfig>.Instance.messageLineSpacing;

            nextMessageIndicator.gameObject.SetActive(false);
        }

        public void EnterAutoMode()
        {
            nextMessageIndicator.gameObject.SetActive(false);
        }

        public void ExitAutoMode()
        {
            if (IsCompleteDisplayText)
            {
                nextMessageIndicator.gameObject.SetActive(true);
            }
        }

        public void SkipDisplay()
        {
            displayTime = 0;
        }

        private void Update()
        {
            if (finished)
            {
                return;
            }

            int characterIndex = (int) (Mathf.Clamp01((Time.time - timeStarted) / displayTime) * currentText.Length);
            if (characterIndex != lastCharacterIndex)
            {
                messagePanel.text = currentText.Length == 0 ? string.Empty : currentText.Substring(0, characterIndex);
                lastCharacterIndex = characterIndex;
            }

            if (IsCompleteDisplayText)
            {
                finished = true;
                
                if (!Y3ScriptModule.InstanceInScene.autoMode && !string.IsNullOrEmpty(messagePanel.text))
                    nextMessageIndicator.gameObject.SetActive(true);
            }
        }


        public bool IsCompleteDisplayText => Time.time > timeStarted + displayTime;
    }
}
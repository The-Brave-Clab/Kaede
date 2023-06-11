using TMPro;
using UnityEngine;

namespace Y3ADV
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class MessageBox : MonoBehaviour
    {
        private RichText _currentText;

        private RichText currentText
        {
            get => _currentText ??= new RichText("");
            set => _currentText = value;
        }

        private TextMeshProUGUI _uiText;

        private TextMeshProUGUI uiText
        {
            get
            {
                if (_uiText == null)
                    _uiText = GetComponent<TextMeshProUGUI>();
                return _uiText;
            }
        }

        public string displayText => uiText.text;

        private float timeStarted = 1f;
        private float displayTime;
        private bool finished;
        private int lastCharacterIndex = -1;
        
        public Breathe NextMessageIndicator;

        public void SetText(string text)
        {
            currentText = new RichText(text.Replace("\\n", "\n"));
            displayTime =
                currentText.Length *
                0.05f; //SingletonMonoBehaviour<ScenarioConfig>.Instance.intervalForCharacterDisplay;
            timeStarted = Time.time;
            lastCharacterIndex = -1;
            finished = false;
            uiText.text = string.Empty;
            uiText.lineSpacing = 1f - 38f; //SingletonMonoBehaviour<ScenarioConfig>.Instance.messageLineSpacing;

            NextMessageIndicator.gameObject.SetActive(false);
        }

        public void EnterAutoMode()
        {
            NextMessageIndicator.gameObject.SetActive(false);
        }

        public void ExitAutoMode()
        {
            if (IsCompleteDisplayText)
            {
                NextMessageIndicator.gameObject.SetActive(true);
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
                uiText.text = currentText.Length == 0 ? string.Empty : currentText.Substring(0, characterIndex);
                lastCharacterIndex = characterIndex;
            }

            if (IsCompleteDisplayText)
            {
                finished = true;
                
                if (!Y3ScriptModule.InstanceInScene.autoMode && !string.IsNullOrEmpty(uiText.text))
                    NextMessageIndicator.gameObject.SetActive(true);
            }
        }


        public bool IsCompleteDisplayText => Time.time > timeStarted + displayTime;
    }
}
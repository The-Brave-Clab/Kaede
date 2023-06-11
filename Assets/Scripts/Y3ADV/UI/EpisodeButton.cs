using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class EpisodeButton : MonoBehaviour
    {
        public Image image;
        public Button button;
        public TextMeshProUGUI text;
        public string episodeID;
        public int subCategoryID;

        public Sprite buttonIcon;
        public string buttonText;

        public void RefreshImage()
        {
            if (buttonIcon == null)
            {
                if (text != null)
                    text.text = buttonText;
            }
            else
            {
                if (image != null)
                    image.sprite = buttonIcon;
                if (text != null)
                    text.text = "";
            }
        }

        public virtual void RefreshFinishedStatus()
        {
            text.color = GameManager.IsScenarioFinished(episodeID) ? new Color(0, 0.5f, 0) : Color.black;
        }

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(episodeID))
                RefreshFinishedStatus();
        }
    }
}
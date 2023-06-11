using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Y3ADV
{
    public class EpisodePanel : MonoBehaviour
    {
        public GameObject buttonPrefab;

        public Transform contentHolder;

        public class ButtonInfo
        {
            public string episodeName;
            public string episodeFileId;
            public UnityAction buttonEvent;
        }

        public void AddButton(ButtonInfo buttonInfo)
        {
            GameObject newButton = Instantiate(buttonPrefab, contentHolder);
            EpisodeButton button = newButton.GetComponent<EpisodeButton>();
            button.text.text = buttonInfo.episodeName;
            button.button.onClick.AddListener(buttonInfo.buttonEvent);
            button.episodeID = buttonInfo.episodeFileId;
            button.RefreshFinishedStatus();
        }
        
        public void ClearButton()
        {
            foreach (Transform child in contentHolder) 
            {
                Destroy(child.gameObject);
            }
        }
    }
}

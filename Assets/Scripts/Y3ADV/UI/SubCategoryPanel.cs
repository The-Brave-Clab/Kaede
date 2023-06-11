using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Y3ADV
{
    public class SubCategoryPanel : MonoBehaviour
    {
        public GameObject buttonPrefab;

        public Transform contentHolder;
        public class ButtonInfo
        {
            public Sprite image = null;
            public string chapterName = "";
            public string episodeId = null;
            public int subCategoryId = -1;
            public UnityAction buttonEvent = null;
        }

        public SubCategoryButton AddButton(ButtonInfo buttonInfo)
        {
            GameObject newButton = Instantiate(buttonPrefab, contentHolder);
            SubCategoryButton button = newButton.GetComponent<SubCategoryButton>();
            button.buttonIcon = buttonInfo.image;
            button.buttonText = buttonInfo.chapterName;
            button.RefreshImage();

            button.episodeID = buttonInfo.episodeId;
            button.subCategoryID = buttonInfo.subCategoryId;
            if (!string.IsNullOrEmpty(button.episodeID))
                button.RefreshFinishedStatus();
            button.button.onClick.AddListener(buttonInfo.buttonEvent);

            return button;
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

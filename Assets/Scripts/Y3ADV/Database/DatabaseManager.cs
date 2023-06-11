using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SQLite4Unity3d;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Y3ADV.Database
{
    public class DatabaseManager : MonoBehaviour
    {
        private List<adventure_books> table = null;

        public CategoryPanel categoryPanel;
        public SubCategoryPanel subCategoryPanel;
        public EpisodePanel episodePanel;
        public GameObject backButton;
        public GameObject sceneRoot;

        public static DatabaseManager Instance;

        private int currentCategory = -1;
        private int currentSubCategory = -1;

        public static int CurrentCategory => Instance.currentCategory;
        public static int CurrentSubCategory => Instance.currentSubCategory;
        public static List<adventure_books> Table => Instance.table;

        private enum CurrentStage
        {
            Category,
            SubCategory,
            Episode
        }

        private CurrentStage currentStage = CurrentStage.Category;

        private const string VERSION_URL = 
            "https://tqdc60uiqc.execute-api.ap-northeast-1.amazonaws.com/test/master_data";
        private const string LOCAL_DATA_VERSION_FILE = "master_data.version.json";
        
        private IEnumerator DownloadDatabase(LocalResourceManager.Callback cb = null)
        {
            yield return LocalResourceManager.DownloadLocalResource(
                $"{VERSION_URL}/jp", // Language option here
                LOCAL_DATA_VERSION_FILE,
                data =>
                    data.key.Contains("adventure_books.db.compress") ||
                    data.key.Contains("event_stories.db.compress"));
            
            cb?.Invoke();
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(DownloadDatabase(LoadDatabase));
        }

        private void LoadDatabase()
        {
            var connection = new SQLiteConnection(
                Path.Combine(Application.persistentDataPath, "adventure_books.db.compress"),
                SQLiteOpenFlags.ReadOnly);
            table = connection.Table<adventure_books>().ToList();
            connection.Close();


            var categoryIDs = table.Select(x => x.category_id).Distinct().ToList();
            categoryPanel.PopulateButtons(categoryIDs.Count);

            for (int i = 0; i < categoryIDs.Count; ++i)
            {
                int categoryID = categoryIDs[i];
                string category = table.First(x => x.category_id == categoryID).category;
                SubCategoryButton button = categoryPanel.Buttons[i];
                button.text.text = category;
                button.button.onClick.AddListener(() => { StartCoroutine(SelectCategory(categoryID)); });
            }

            categoryPanel.gameObject.SetActive(true);
        }

        private IEnumerator SelectCategory(int categoryID)
        {
            currentCategory = categoryID;

            List<adventure_books> categoryAdvs = table.Where(x => x.category_id == categoryID).OrderBy(x => x.id).ToList();

            if (categoryID == 4)
            {
                foreach (var categoryAdv in categoryAdvs)
                {
                    SubCategoryPanel.ButtonInfo buttonInfo = new SubCategoryPanel.ButtonInfo();
                    buttonInfo.image = null;
                    buttonInfo.chapterName = categoryAdv.display_name;
                    buttonInfo.episodeId = "0";
                    buttonInfo.buttonEvent += () => { SelectScenario(categoryAdv.file_id); };
                    buttonInfo.episodeId = categoryAdv.file_id;

                    subCategoryPanel.AddButton(buttonInfo);
                }
            }
            else
            {
                List<int> subCategoryIDs = categoryAdvs.OrderBy(x => x.id).Select(x => x.sub_category_id)
                    .Distinct().ToList();
                foreach (var subCategoryID in subCategoryIDs)
                {
                    string bannerFilePath = "";
                    string bannerSubDirectory = "";
                    string bannerFileName = "";
                    if (categoryID == 3)
                    {
                        bannerSubDirectory = "eventbanner";
                        bannerFileName = $"event_{subCategoryID:D8}.png";
                    }
                    else
                    {
                        bannerSubDirectory = "storybanner";
                        int subCategoryIDFixed = subCategoryID;
                        if (subCategoryIDFixed == 9) subCategoryIDFixed = 8;
                        else if (subCategoryIDFixed == 8) subCategoryIDFixed = 9;
                        bannerFileName = $"story_{subCategoryIDFixed:D8}.png";
                    }

                    bannerFilePath = $"banner/{bannerSubDirectory}/{bannerFileName}";

                    SubCategoryPanel.ButtonInfo buttonInfo = new SubCategoryPanel.ButtonInfo
                    {
                        image = null,
                        chapterName = categoryAdvs.First(x => x.sub_category_id == subCategoryID).sub_category,
                        episodeId = subCategoryID.ToString(),
                        subCategoryId = subCategoryID
                    };
                    buttonInfo.buttonEvent += () => { SelectSubCategory(subCategoryID); };
                    var button = subCategoryPanel.AddButton(buttonInfo);

                    GameManager.AddCoroutine(LocalResourceManager.LoadTexture2DFromFile(
                        new[] { bannerFilePath },
                        obj =>
                        {
                            if (obj == null) return;
                            
                            Texture2D loadedTexture = (Texture2D)obj;
                            Rect rect = new Rect(0, 0, loadedTexture.width, loadedTexture.height);
                            Vector2 pivot = new Vector2(0.5f, 0.5f);
                            button.buttonIcon = Sprite.Create(loadedTexture, rect, pivot, 100.0f,  0, SpriteMeshType.FullRect);
                            button.RefreshImage();
                        }));
                }
            }


            backButton.SetActive(true);
            currentStage = CurrentStage.SubCategory;
            categoryPanel.gameObject.SetActive(false);
            subCategoryPanel.gameObject.SetActive(true);
            episodePanel.gameObject.SetActive(false);

            yield return null;
        }

        public void SelectSubCategory(int subCategoryID)
        {
            currentSubCategory = subCategoryID;

            List<adventure_books> categoryAdvs = table
                .Where(x => x.category_id == currentCategory && x.sub_category_id == subCategoryID).OrderBy(x => x.id).ToList();

            foreach (var categoryAdv in categoryAdvs)
            {
                EpisodePanel.ButtonInfo buttonInfo = new EpisodePanel.ButtonInfo();
                buttonInfo.episodeName = $"{categoryAdv.episode} {categoryAdv.display_name}";
                buttonInfo.episodeFileId = categoryAdv.file_id;
                buttonInfo.buttonEvent += () => { SelectScenario(categoryAdv.file_id); };

                episodePanel.AddButton(buttonInfo);
            }

            backButton.SetActive(true);
            currentStage = CurrentStage.Episode;
            categoryPanel.gameObject.SetActive(false);
            subCategoryPanel.gameObject.SetActive(false);
            episodePanel.gameObject.SetActive(true);
        }

        public void SelectScenario(string scenarioFileID)
        {
            sceneRoot.SetActive(false);
            GameManager.StartScenario(scenarioFileID);
        }

        public void GoBack()
        {
            switch (currentStage)
            {
                case CurrentStage.Category:
                    break;
                case CurrentStage.SubCategory:
                    currentCategory = -1;
                    backButton.SetActive(false);
                    currentStage = CurrentStage.Category;
                    categoryPanel.gameObject.SetActive(true);
                    subCategoryPanel.ClearButton();
                    subCategoryPanel.gameObject.SetActive(false);
                    episodePanel.ClearButton();
                    episodePanel.gameObject.SetActive(false);
                    break;
                case CurrentStage.Episode:
                    currentSubCategory = -1;
                    backButton.SetActive(true);
                    currentStage = CurrentStage.SubCategory;
                    categoryPanel.gameObject.SetActive(false);
                    subCategoryPanel.gameObject.SetActive(true);
                    episodePanel.ClearButton();
                    episodePanel.gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
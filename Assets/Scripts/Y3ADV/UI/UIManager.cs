using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Y3ADV.Scenario;

namespace Y3ADV
{
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        [Header("Message Box")] 
        public RectTransform messageBox;
        public TextMeshProUGUI nameTag;
        public TextMeshProUGUI messagePanel;
        public MessageBox message;

        [Header("Caption")] 
        public Image captionBox;
        public TextMeshProUGUI caption;

        [Header("Sub-Canvas")] 
        public RectTransform contentCanvas;
        public GameObject loadingCanvas;

        [Header("Actor Canvas")] 
        public RectTransform actorCanvas;

        [Header("Background")] 
        public RectTransform backgroundCanvas;
        public GameObject backgroundPrefab;

        [Header("Still")] 
        public Transform stillCanvas;
        public GameObject stillPrefab;

        [Header("Fade")] 
        public FadeTransition fade;

        [Header("Sprite")] 
        public Transform spriteWrapper;
        public GameObject spritePrefab;

        [Header("UI")] 
        public Transform uiCanvas;
        public GameObject showMenuButton;
        public GameObject menuHolder;
        public GameObject fullscreenHideHolder;
        public Button exitFullscreenButton;
        public Button autoButton;
        public Button dramaButton;
        public GameObject skipButton;
        public GameObject bugReportButton;
        public Button disableAutoButton;
        public Button disableDramaButton;

        [Header("Prefab")] 
        public PrefabRenderer prefabRenderer;
        public GameObject eventSystemPrefab;

        [Header("Play Control")] 
        public GameObject PlayCanvas;
        public GameObject ReplayCanvas;

        [NonSerialized]
        public Color CaptionDefaultColor = new Color(1, 1, 1, 1f);

        protected override void Awake()
        {
            base.Awake();
            
            EventSystem es = EventSystem.current;
            if (es == null)
                Instantiate(eventSystemPrefab);
        }

        private void Start()
        {
            PlayCanvas.SetActive(!GameManager.CanPlay);
            
#if WEBGL_BUILD
            WebGLInterops.RegisterUIManagerGameObject(gameObject.name);
            fullscreenHideHolder.SetActive(false);
            exitFullscreenButton.gameObject.SetActive(true);
            exitFullscreenButton.onClick.AddListener(() => { FullscreenManager.Instance.ChangeFullscreen(0); });
            exitFullscreenButton.onClick.AddListener(() => { WebGLInterops.OnExitFullscreen(); });
            
            skipButton.SetActive(false);
            bugReportButton.SetActive(false);
            
            autoButton.onClick.AddListener(() => { WebGLInterops.OnToggleAutoMode(1); });
            dramaButton.onClick.AddListener(() => { WebGLInterops.OnToggleDramaMode(1); });
            disableAutoButton.onClick.AddListener(() => { WebGLInterops.OnToggleAutoMode(0); });
            disableDramaButton.onClick.AddListener(() => { WebGLInterops.OnToggleDramaMode(0); });
#else
            fullscreenHideHolder.SetActive(true);
            exitFullscreenButton.gameObject.SetActive(false);
#endif
        }

#if WEBGL_BUILD
        private void Update()
        {
            fullscreenHideHolder.SetActive(FullscreenManager.Instance.Fullscreen);
        }

        public void ToggleAutoMode(int on)
        {
            if (on > 0)
                autoButton.onClick.Invoke();
            else
                disableAutoButton.onClick.Invoke();
        }

        public void ToggleDramaMode(int on)
        {
            if (on > 0)
                dramaButton.onClick.Invoke();
            else
                disableDramaButton.onClick.Invoke();
        }
#endif

        public GameObject Background(Texture2D texture, string name, string resourceName)
        {
            GameObject findResult = GameObject.Find(name);
            if (findResult != null) return findResult;
            GameObject result = Instantiate(backgroundPrefab, backgroundCanvas);
            result.name = name;
            BackgroundImage image = result.GetComponent<BackgroundImage>();
            image.image.texture = texture;
            image.resourceName = resourceName;
            return result;
        }

        public GameObject Still(Texture2D texture, string name)
        {
            GameObject result = Instantiate(stillPrefab, stillCanvas);
            result.name = name;
            BackgroundImage image = result.GetComponent<BackgroundImage>();
            image.image.texture = texture;
            return result;
        }

        public GameObject Sprite(Texture2D texture, string name, string resourceName)
        {
            GameObject result = Instantiate(spritePrefab, spriteWrapper);
            result.name = name;
            Image sprite = result.GetComponent<Image>();

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            sprite.sprite = UnityEngine.Sprite.Create(texture, rect, pivot, 100.0f,  0, SpriteMeshType.FullRect);

            RectTransform rectTransform = result.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(texture.width, texture.height);

            SpriteImage spriteImage = result.GetComponent<SpriteImage>();
            spriteImage.resourceName = resourceName;

            return result;
        }

        public void ToggleMsgBoxShowHide()
        {
            if (!string.IsNullOrEmpty(message.displayText))
                messageBox.gameObject.SetActive(!messageBox.gameObject.activeSelf);
        }

        public void HideMenu()
        {
            showMenuButton.SetActive(true);
            menuHolder.SetActive(false);
        }

        public static Vector2 CameraPos
        {
            get => Instance.actorCanvas.anchoredPosition * -1;
            set
            {
                Instance.actorCanvas.anchoredPosition = value * -1.0f;
                Instance.backgroundCanvas.anchoredPosition = value * -1.0f;
            }
        }

        public static Vector2 CameraPosDefault => Vector2.zero;

        public static Vector2 CameraScale
        {
            get => Instance.actorCanvas.localScale;
            set
            {
                Instance.actorCanvas.localScale = new Vector3(value.x, value.y, Instance.actorCanvas.localScale.z);
                Instance.backgroundCanvas.localScale = new Vector3(value.x, value.y, Instance.backgroundCanvas.localScale.z);
            }
        }

        public static Vector2 CameraScaleDefault => Vector2.one;

        public static Vector2 MessageBoxPos
        {
            get => Instance.messageBox.anchoredPosition * -1;
            set => Instance.messageBox.anchoredPosition = value * -1.0f;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

namespace ColorBath
{
    public class ChatField : MonoBehaviour
    {
        public int DiscoveryNum { get; private set; } = 0;
        [SerializeField] private Transform _chatFieldContent;
        [SerializeField] private GameObject _discoveryHukidashi;
        [SerializeField] private GameObject _aizuchiHukidashi;
        [SerializeField] private Button _camButton;
        public static ChatField Instance { get; private set; }

        void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            _camButton.onClick.AddListener(UIDirector.Instance.DeviceCamOn);
            StartCoroutine(ScrollToBottomNextFrame());
        }

        private IEnumerator ScrollToBottomNextFrame()
        {
            yield return null;

            gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        }

        //できたらいいな。後から追加
        public void DeleteDiscovery()
        {
            //<TODO>発見を消す処理

            StartCoroutine(ScrollToBottomNextFrame());
        }

        public void PrintDiscovery(string message, string imagePath ="")
        {
            GameObject newDiscoveryHukidashi = Instantiate(_discoveryHukidashi, _chatFieldContent);
            if (imagePath != "")
            {
                SetHukidashiImage(imagePath, newDiscoveryHukidashi);
            }
            SetHukidashiText(message, newDiscoveryHukidashi);
            StartCoroutine(ScrollToBottomNextFrame());
        }

        public void PrintDiscovery(string message, Texture2D texture)
        {
            GameObject newDiscoveryHukidashi = Instantiate(_discoveryHukidashi, _chatFieldContent);
            SetHukidashiImage(texture, newDiscoveryHukidashi);

            SetHukidashiText(message, newDiscoveryHukidashi);
            StartCoroutine(ScrollToBottomNextFrame());
        }

        public void PrintAizuchi(string message)
        {
            GameObject newAizhchiHukidashi = Instantiate(_aizuchiHukidashi, _chatFieldContent);
            SetHukidashiText(message, newAizhchiHukidashi);
            StartCoroutine(ScrollToBottomNextFrame());
        }

        private void SetHukidashiText(string message, GameObject hukidashiInstance)
        {
            TextMeshProUGUI text = hukidashiInstance.transform.Find("Hukidashi/Text").GetComponent<TextMeshProUGUI>();
            if (message != null)
            {
                text.text = message;
            }
        }
        
        private void SetHukidashiImage(string imagePath, GameObject hukidashiInstance)
        {
            if (!File.Exists(imagePath))
            {
                Debug.LogWarning("画像が見つからない" + imagePath);
                return;
            }

            byte[] imageBytes = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(2, 2);
            Image targetImage = hukidashiInstance.transform.Find("Hukidashi/Image").GetComponent<Image>();

            if (texture.LoadImage(imageBytes))
            {
                float MaxSize = 100.0f;
                float width;
                float height;
                if (texture.width < texture.height)
                {
                    width = texture.width * MaxSize / texture.height;
                    height = MaxSize;
                }
                else
                {
                    width = MaxSize;
                    height = texture.height * MaxSize / texture.width;
                }
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                targetImage.sprite = sprite;

                RectTransform rt = targetImage.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(width, height);
            }
            else
            {
                Debug.LogWarning("画像の読み込みに失敗しました");
            }
        }

        private void SetHukidashiImage(Texture2D texture, GameObject hukidashiInstance)
        {
            Image targetImage = hukidashiInstance.transform.Find("Hukidashi/Image").GetComponent<Image>();

            float aspect = (float)texture.width / texture.height;

            // スプライト設定
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            targetImage.sprite = sprite;

            // AspectRatioFitterを設定
            AspectRatioFitter fitter = targetImage.GetComponent<AspectRatioFitter>();
            if (fitter == null)
                fitter = targetImage.gameObject.AddComponent<AspectRatioFitter>();

            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = aspect;

            // LayoutElementで高さ指定（重要！）
            LayoutElement layout = targetImage.GetComponent<LayoutElement>();
            if (layout == null)
                layout = targetImage.gameObject.AddComponent<LayoutElement>();

            layout.preferredHeight = 100f; // ここで高さを明示
            layout.flexibleHeight = 0;
            layout.minHeight = 0;

            // AspectRatioFitterに合わせるので幅は自動になる（HeightControlsWidthの効果が出る）
        }

    }
}

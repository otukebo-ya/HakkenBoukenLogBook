using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Linq;
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
            StartCoroutine(ScrollToBottomNextFrame());
        }

        // 最新のやり取りが表示されるように、チャット画面の一番下へスクロール
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

        // ユーザの発見メモを表示(オーバロードあり)
        // 画像がイメージパスで与えられる場合
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

        // ユーザの発見メモを表示（オーバロードあり）
        // 画像がテクスチャで与えられる場合
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

        // 画像を吹き出しにセット（オーバロードあり）
        // 画像のパスが与えられた場合
        private void SetHukidashiImage(string imagePath, GameObject hukidashiInstance)
        {
            if (!File.Exists(imagePath))
            {
                Debug.LogWarning("画像が見つからない" + imagePath);
                return;
            }

            // 
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

        // 画像を吹き出しにセット（オーバロードあり）
        // 画像のテクスチャが与えられた場合
        private void SetHukidashiImage(Texture2D texture, GameObject hukidashiInstance)
        {
            Image targetImage = hukidashiInstance.transform.Find("Hukidashi/Image").GetComponent<Image>();

            float aspect = (float)texture.width / texture.height;

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            targetImage.sprite = sprite;

            AspectRatioFitter fitter = targetImage.GetComponent<AspectRatioFitter>();
            if (fitter == null)
                fitter = targetImage.gameObject.AddComponent<AspectRatioFitter>();

            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = aspect;

            LayoutElement layout = targetImage.GetComponent<LayoutElement>();
            if (layout == null)
                layout = targetImage.gameObject.AddComponent<LayoutElement>();

            layout.preferredHeight = 100f;
            layout.flexibleHeight = 0;
            layout.minHeight = 0;
        }

        // その日に、すでに行われている発見を一気に並べる
        public void LineUpTodayDiscoveries(Discovery[] discoveries)
        {
            for (int i = 0; i < discoveries.Length; i++)
            {
                PrintDiscovery(discoveries[i].Memo, imagePath: discoveries[0].ImagePath);
                PrintAizuchi(discoveries[i].Aizuchi);
            }
        }

    }
}

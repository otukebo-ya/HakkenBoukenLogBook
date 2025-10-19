using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColorBath{
    // <TODO> ChatFieldと被る機能があるので切り分けを行いたい
    public class KakoLogCanvas : MonoBehaviour
    {
        [SerializeField] private RectTransform  _chatFieldContent;
        [SerializeField] private GameObject _discoveryHukidashi;
        [SerializeField] private GameObject _aizuchiHukidashi;
        [SerializeField] private Button _retrnButton;
        [SerializeField] private TextMeshProUGUI _theme;
        [SerializeField] private TextMeshProUGUI _date;

        void Start(){
            _retrnButton.onClick.AddListener(Close);
        }

        public void Open(DateTime date)
        {
            History? history = UsageHistoryManager.Instance.GetHistory(date);
            DeleteChatLogs();

            _theme.text = history.Theme;
            _date.text = date.ToString("yyyy_MM_dd");
            // Discoveries が null の可能性に備える
            if (history.Discoveries != null)
            {
                for (int i = 0; i < history.Discoveries.Length; i++)
                {
                    var d = history.Discoveries[i];

                    // 発見の吹き出し
                    GameObject newDiscovery = Instantiate(_discoveryHukidashi, _chatFieldContent);
                    // 画像があればセット（Image コンポーネントを探して設定）
                    try
                    {
                        string imagePath = (d?.ImagePath) ?? "";
                        if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                        {
                            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                            Texture2D texture = new Texture2D(2, 2);
                            Image targetImage = newDiscovery.transform.Find("Hukidashi/Image").GetComponent<Image>();

                            if (texture.LoadImage(imageBytes))
                            {
                                Sprite sprite = Sprite.Create(
                                    texture,
                                    new Rect(0, 0, texture.width, texture.height),
                                    new Vector2(0.5f, 0.5f)
                                );
                                targetImage.sprite = sprite;

                                AspectRatioFitter fitter = targetImage.GetComponent<AspectRatioFitter>();
                                if (fitter == null) fitter = targetImage.gameObject.AddComponent<AspectRatioFitter>();
                                fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                                fitter.aspectRatio = (float)texture.width / texture.height;

                                LayoutElement layout = targetImage.GetComponent<LayoutElement>();
                                if (layout == null) layout = targetImage.gameObject.AddComponent<LayoutElement>();
                                layout.preferredHeight = 100f;
                                layout.flexibleHeight = 0f;
                                layout.minHeight = 0f;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"Failed to set discovery image: {ex.Message}");
                    }

                    // メモテキストをセット
                    var textComp = newDiscovery.transform.Find("Hukidashi/Text")?.GetComponent<TMPro.TextMeshProUGUI>();
                    if (textComp != null)
                    {
                        textComp.text = d?.Memo ?? "";
                    }

                    // 相槌の吹き出し
                    GameObject newAizuchi = Instantiate(_aizuchiHukidashi, _chatFieldContent);
                    var aizText = newAizuchi.transform.Find("Hukidashi/Text")?.GetComponent<TMPro.TextMeshProUGUI>();
                    if (aizText != null)
                    {
                        aizText.text = d?.Aizuchi ?? "";
                    }
                }
            }

            // 最後に表示
            gameObject.SetActive(true);
        }

        public void Close()
        {
            _theme.text = "theme";
            _date.text = "yyyy_MM_dd";
            gameObject.SetActive(false);
        }

        // ScrollRect の content 配下の子を全削除してスクロール内容をリセット
        public void DeleteChatLogs()
        {   
            for (int i = _chatFieldContent.childCount - 1; i >= 0; i--)
            {
                var child = _chatFieldContent.GetChild(i)?.gameObject;
                if (child != null) Destroy(child);
            }
        }
    }
}
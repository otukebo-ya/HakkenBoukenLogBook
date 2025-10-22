using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace ColorBath
{
    public class DiscoveryInputForm : MonoBehaviour
    {
        private InputField _inputField;
        public string InputText
        {
            get { return _inputField.text; }
        }
        public static DiscoveryInputForm Instance { get; private set; }

        public Image InputImage;
        private Texture2D _captuedTexture;
        [SerializeField] private RawImage _capturedImage;
        [SerializeField] private string _capturedImagePath;
        [SerializeField] public Button CameraButton;
        [SerializeField] private Button _sendButton;

        void Awake()
        {
            Instance = this;
            _inputField = GetComponent<InputField>();

            // 送信ボタンの設定
            _sendButton.onClick.AddListener(() =>
            {
                _ = SendButtonClicked();
            });

            // 撮影した画像を表示する場所を非表示にしておく
            _capturedImage.gameObject.SetActive(false);
        }

        // 送信ボタンの処理
        private async Task SendButtonClicked()
        {
            string inputText = GetTextFormInput();
            if(inputText == "") return ;
            string aizuchi = "";
            if (_captuedTexture is null) // 画像がない場合
            {
                // ユーザの発見メモを表示
                ChatField.Instance.PrintDiscovery(inputText);

                // 相槌作成
                aizuchi = await GeminiClient.Instance.SendAizuchiPrompt(inputText);
            }
            else　// 画像が撮影されていた場合
            {
                ChatField.Instance.PrintDiscovery(inputText, _captuedTexture);
                aizuchi = await GeminiClient.Instance.SendAizuchiPrompt(inputText, _captuedTexture);
            }

            // 相槌の表示
            ChatField.Instance.PrintAizuchi(aizuchi);

            // Discovery型のオブジェクトに格納し、ファイルに保存
            Discovery discovery = MakeNewDiscovery(inputText, aizuchi);
            UsageHistoryManager.Instance.SaveDiscovery(discovery);

            // 初期化
            ClearInputForm();
        }

        // ユーザの入力テキスト（発見メモ）を取得
        public string GetTextFormInput()
        {
            return _inputField.text;
        }

        // 入力場所の初期化関数
        public void ClearInputForm()
        {
            _captuedTexture = null;
            _inputField.text = "";

            RemoveCapturedImage();
            _capturedImage.gameObject.SetActive(false);
        }

        // デバイスのカメラで撮影した画像を小さく表示
        public void SetCapturedImage(Texture2D texture, string path)
        {
            _captuedTexture = texture;
            _capturedImagePath = path;
            _capturedImage.texture = texture;
            _capturedImage.gameObject.SetActive(true);
        }

        public void RemoveCapturedImage()
        {
            _capturedImage.texture = null ;
            _capturedImagePath = "";
        }

        // 発見メモ、画像、それに対する相槌をまとめてオブジェクト化
        private Discovery MakeNewDiscovery(string inputText, string aizuchi)
        {
            Discovery discovery = new Discovery();
            discovery.Memo = inputText;
            discovery.Aizuchi = aizuchi;
            if (_captuedTexture is not null) { discovery.ImagePath = _capturedImagePath; }

            return discovery;
        }
    }
}

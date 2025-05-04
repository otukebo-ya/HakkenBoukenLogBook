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
            _sendButton.onClick.AddListener(() =>
            {
                _ = SendButtonClicked();
            });
            _capturedImage.gameObject.SetActive(false);
        }
        private async Task SendButtonClicked()
        {
            string inputText = GetTextFormInput();

            string aizuchi = "";
            if (_captuedTexture is null)
            {
                ChatField.Instance.PrintDiscovery(inputText);
                aizuchi = await GeminiClient.Instance.SendAizuchiPrompt(inputText);
            }
            else
            {
                ChatField.Instance.PrintDiscovery(inputText, _captuedTexture);
                aizuchi = await GeminiClient.Instance.SendAizuchiPrompt(inputText, _captuedTexture);
            }
            ChatField.Instance.PrintAizuchi(aizuchi);

            Discovery discovery = new Discovery();
            discovery.Memo = inputText;
            discovery.Aizuchi = aizuchi;
            if(_captuedTexture is not null) { discovery.ImagePath = _capturedImagePath; }

            UsageHistoryManager.Instance.SaveDiscovery(discovery);
            ClearInputForm();
        }

        public string GetTextFormInput()
        {
            return _inputField.text;
        }

        public void SaveHistory()
        {

        }

        public Discovery GetInput()
        {
            Discovery discovery = new Discovery();
            return discovery;
        }

        public void ClearInputForm()
        {
            _captuedTexture = null;
            _capturedImagePath = "";
            _capturedImage.texture = null;
            _inputField.text = "";
            _capturedImage.gameObject.SetActive(false);
        }

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
        }

        public string GenerateAizuchi()
        {
            return "";
        }
    }
}

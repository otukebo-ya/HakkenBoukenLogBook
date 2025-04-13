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
            Debug.Log("sendButtonClicked!!");
            string inputText = GetTextFormInput();

            string aizuchi = "";
            if (_captuedTexture is null)
            {
                Debug.Log("âÊëúÇ»ÇµÇÃî≠å©");
                ChatField.Instance.PrintDiscovery(inputText);
                aizuchi = await GeminiClient.Instance.SendAizuchiPrompt(inputText);
            }
            else
            {
                Debug.Log("âÊëúÇ†ÇËÇÃî≠å©");
                ChatField.Instance.PrintDiscovery(inputText, _captuedTexture);
                aizuchi = await GeminiClient.Instance.SendAizuchiPrompt(inputText, _captuedTexture);
            }
            ChatField.Instance.PrintAizuchi(aizuchi);

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
            Debug.Log("InputFormÇÉNÉäÉAÅ[");
            _captuedTexture = null;
            _capturedImage.texture = null;
            _inputField.text = "";
            _capturedImage.gameObject.SetActive(false);
        }

        public void SetCapturedImage(Texture2D texture)
        {
            _captuedTexture = texture;
            Debug.Log(_captuedTexture);
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

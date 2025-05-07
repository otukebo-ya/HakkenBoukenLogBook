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

            // ���M�{�^���̐ݒ�
            _sendButton.onClick.AddListener(() =>
            {
                _ = SendButtonClicked();
            });

            // �B�e�����摜��\������ꏊ���\���ɂ��Ă���
            _capturedImage.gameObject.SetActive(false);
        }

        // ���M�{�^���̏���
        private async Task SendButtonClicked()
        {
            string inputText = GetTextFormInput();

            string aizuchi = "";
            if (_captuedTexture is null) // �摜���Ȃ��ꍇ
            {
                // ���[�U�̔���������\��
                ChatField.Instance.PrintDiscovery(inputText);

                // ���ƍ쐬
                aizuchi = await GeminiClient.Instance.SendAizuchiPrompt(inputText);
            }
            else�@// �摜���B�e����Ă����ꍇ
            {
                ChatField.Instance.PrintDiscovery(inputText, _captuedTexture);
                aizuchi = await GeminiClient.Instance.SendAizuchiPrompt(inputText, _captuedTexture);
            }

            // ���Ƃ̕\��
            ChatField.Instance.PrintAizuchi(aizuchi);

            // Discovery�^�̃I�u�W�F�N�g�Ɋi�[���A�t�@�C���ɕۑ�
            Discovery discovery = MakeNewDiscovery(inputText, aizuchi);
            UsageHistoryManager.Instance.SaveDiscovery(discovery);

            // ������
            ClearInputForm();
        }

        // ���[�U�̓��̓e�L�X�g�i���������j���擾
        public string GetTextFormInput()
        {
            return _inputField.text;
        }

        // ���͏ꏊ�̏������֐�
        public void ClearInputForm()
        {
            _captuedTexture = null;
            _inputField.text = "";

            RemoveCapturedImage();
            _capturedImage.gameObject.SetActive(false);
        }

        // �f�o�C�X�̃J�����ŎB�e�����摜���������\��
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

        // ���������A�摜�A����ɑ΂��鑊�Ƃ��܂Ƃ߂ăI�u�W�F�N�g��
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

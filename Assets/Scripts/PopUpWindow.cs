using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ColorBath
{
    public class PopUpWindow : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text _title;
        [SerializeField] private TMPro.TMP_Text _mainText;
        [SerializeField] private TMPro.TMP_Text _errorText;
        [SerializeField] private InputField _inputField;
        [SerializeField] private Button _okButton;

        private System.Action<string> _onOkCallback;

        void Start()
        {
            // OK�{�^���̏������Z�b�g
            _okButton.onClick.AddListener(OnOkButtonClicked);
        }

        public void SetTitleText(string title) 
        {
            _title.text = title; 
        }

        public void SetMainText(string text) 
        {
            _mainText.text = text; 
        }
        
        public void SetErrorText(string text) 
        {
            _errorText.text = text; 
        }

        public string GetTextFormInput()
        {
            return _inputField.text; 
        }

        // OK�{�^���������̏���
        private void OnOkButtonClicked()
        {
            if (_inputField.gameObject.activeSelf)// ���[�U���͂��󂯎��ꍇ
            {
                string inputValue = GetTextFormInput();
                Debug.Log("����"+inputValue);
                if (string.IsNullOrWhiteSpace(inputValue))
                {
                    // <TODO>�G���[�e�L�X�g�̓_��
                    SetErrorText("�l����͂��Ă��������I�I");
                    return;
                }
                _onOkCallback?.Invoke(inputValue);
            }
            PopUpWindowController.Instance.Close();
        }

        public void WithoutInputField()
        {
            _inputField.gameObject.SetActive(false);
        }

        public void WithInputField()
        {
            _inputField.gameObject.SetActive(true);
        }

        // �|�b�v�A�b�v�E�B���h�E�����ۂ̃R�[���o�b�N�֐���ݒ�
        public void SetOnOkCallback(System.Action<string> callback)
        {
            _onOkCallback = callback;
        }
    }
}
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

        private void OnOkButtonClicked()
        {
            if (_inputField.gameObject.activeSelf)
            {
                string inputValue = GetTextFormInput();
                Debug.Log("入力"+inputValue);
                if (string.IsNullOrWhiteSpace(inputValue))
                {
                    // <TODO>エラーテキストの点滅
                    SetErrorText("値を入力してください！！");
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

        public void SetOnOkCallback(System.Action<string> callback)
        {
            _onOkCallback = callback;
        }
    }
}
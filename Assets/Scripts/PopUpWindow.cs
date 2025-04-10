using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ColorBath
{
    public class PopUpWindow : MonoBehaviour
    {
        private TMPro.TMP_Text _title;
        private TMPro.TMP_Text _mainText;
        private TMPro.TMP_Text _errorText;
        private InputField _textForm;

        // Start is called before the first frame update
        void Start()
        {
            _title = transform.Find("Title").GetComponent<TMP_Text>();
            _mainText = transform.Find("MainText").GetComponent<TMP_Text>();
            _errorText = transform.Find("ErrorText").GetComponent<TMP_Text>();
            _textForm = transform.Find("InputFieldInWindow").gameObject.GetComponent<InputField>();
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
            return _textForm.text; 
        }
    }
}
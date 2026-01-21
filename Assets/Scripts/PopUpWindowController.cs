using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ColorBath{
    public class PopUpWindowController : MonoBehaviour
    {
        [SerializeField] private GameObject _window;
        [SerializeField] private GameObject _windowShadow;
        private PopUpWindow _windowScript;

        public static PopUpWindowController Instance;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            _windowScript = _window.GetComponent<PopUpWindow>();
            _window.SetActive(false);
            _windowShadow.SetActive(false);
        }

        // ポップアップウィンドウの表示
        public void PopUp(string title, string mainText, string errorText = "", bool withoutInputField = false, System.Action<string> onOk = null)
        {
            // <TODO>アニメーションつける

            _window.SetActive(true);
            _windowShadow.SetActive(true);
            _windowScript.SetTitleText(title);
            _windowScript.SetMainText(mainText);
            _windowScript.SetErrorText(errorText);
            
            
            if (withoutInputField)
            {
                _windowScript.WithoutInputField();
            }
            else
            {
                _windowScript.WithInputField();
            }
            _windowScript.SetOnOkCallback(onOk);
        }

        // ウィンドウの非表示
        public void Close()
        {
            // <TODO>アニメーションつける

            _window.SetActive(false);
            _windowShadow.SetActive(false);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBath{
    public class UIDirector : MonoBehaviour
    {
        [SerializeField] GameObject PopUpController;
        public static UIDirector Instance;

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
        }
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void ThemeDecideAnimation()
        {

        }

        public void DisplayReviewWindow()
        {

        }

        public IEnumerator RequestTokenInput(System.Action<string> onReceived)
        {
            bool isInputDone = false;
            string input = "";

            PopUpWindowController.Instance.PopUp(
                title: "GeminiAPIのトークンが見つかりません",
                mainText: "トークンを入力してください",
                errorText: "",
                withoutInputField: false,
                onOk: (value) =>
                {
                    input = value;
                    isInputDone = true;
                }
            );

            yield return new WaitUntil(() => isInputDone);

            onReceived?.Invoke(input);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;


namespace ColorBath
{
    public class UIDirector : MonoBehaviour
    {
        [SerializeField] GameObject PopUpController;
        [SerializeField] GameObject camImage;
        [SerializeField] GameObject TodaysThemePanel;

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

        
        public void ThemeDecideAnimation()
        {

        }

        public void DisplayReviewWindow()
        {

        }

        // APIトークンを入力させるウィンドウの表示
        public async Task<string> RequestTokenInput()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            string token = "";
            PopUpWindowController.Instance.PopUp(
                title: "GeminiAPIのトークンが見つかりません",
                mainText: "トークンを入力してください",
                errorText: "",
                withoutInputField: false,
                onOk: (value) =>
                {
                    token = value;
                    tcs.SetResult(value);
                }
            );

            await tcs.Task;
            return token;
        }

        public void SetCamImage(){
            camImage.SetActive(true);
        }

        // 今日のテーマを表示
        public void SetTodaysTheme(string theme)
        {
            TodaysThemePanel.GetComponent<TextMeshProUGUI>().text = theme;
        }
    }
}
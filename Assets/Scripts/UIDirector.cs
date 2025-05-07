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

        // API�g�[�N������͂�����E�B���h�E�̕\��
        public async Task<string> RequestTokenInput()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            string token = "";
            PopUpWindowController.Instance.PopUp(
                title: "GeminiAPI�̃g�[�N����������܂���",
                mainText: "�g�[�N������͂��Ă�������",
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

        // �J�����f���̕\��
        public void DeviceCamOn()
        {
            camImage.SetActive(true);
            DeviceCam camScript = camImage.GetComponent<DeviceCam>();
            camScript.CameraOn();
        }

        // �����̃e�[�}��\��
        public void SetTodaysTheme(string theme)
        {
            TodaysThemePanel.GetComponent<TextMeshProUGUI>().text = theme;
        }
    }
}
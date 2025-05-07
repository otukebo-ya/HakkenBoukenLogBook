using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
#nullable enable
namespace ColorBath
{
    public class AppManager : MonoBehaviour
    {
        public string Theme = "";
        public DateTime Today;
        public static AppManager? Instance;

        public AppManager()
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

        void Start()
        {
            Initialize().Forget();
        }

        async Task Initialize()
        {
            // gemini�p�g�[�N����ݒ�
            // ������ΐݒ肳����
            bool existUserData = CheckUserData();
            if (!existUserData)
            {
                UserData.Token = await UIDirector.Instance.RequestTokenInput();
            }

            // �����̃e�[�}���擾
            Theme = UsageHistoryManager.Instance.GetTodayTheme();

            // �{�������O�C���Ȃ�
            if (Theme == "")
            {
                //�@�e�[�}�̃|�b�v�A�b�v
                await DecideTheme();

                // ���߂̗������m�F���A���r���[����ĂȂ���΃��r���[����
                History? latestHistory = UsageHistoryManager.Instance.GetLatestHistory();
                bool isReviewed = (latestHistory?.Review ?? "") == "";

                if (!isReviewed)
                {
                    // ���r���[�̃|�b�v�A�b�v
                    await Review(latestHistory);
                }
            }

            // GeminiAPI�̏�����
            GeminiClient.Instance.SetBackBone(Theme);

            // ��ʏ�ɍ����̃e�[�}�ƁA����܂ł̔������L�q
            UIDirector.Instance.SetTodaysTheme(Theme);
            Discovery[]? todayDiscoveries = UsageHistoryManager.Instance.GetTodayDiscoveries();
            if(todayDiscoveries is not null)
            {
                ChatField.Instance.LineUpTodayDiscoveries(todayDiscoveries);
            }
        }

        // �g�[�N�������邩�H�m�F
        public bool CheckUserData()
        {
            if (UserData.Token is not null)
            {
                return true;
            }
            return false;
        }

        // �e�[�}��Gemini api�Ɍ��肳���A�|�b�v�A�b�v�\������
        async Task DecideTheme()
        {
            // �ߋ������̃e�[�}���擾
            string[] recentThemes = UsageHistoryManager.Instance.LoadRecentThemes();
            Theme = await GeminiClient.Instance.SendThemeDecidePrompt(recentThemes);


            // �|�b�v�A�b�v�\��
            PopUpWindowController.Instance.PopUp(
                title: "�����̔������n�߂��I",
                mainText: "�{���̃e�[�}��\n" +
                "�u" + Theme + "�v\n" +
                "�Ɍ��܂�����I",
                errorText: "",
                withoutInputField: true,
                onOk: null
            );

            // �����̃��O�ۑ��p�t�@�C���̍쐬
            UsageHistoryManager.Instance.MakeTodaysFile(Theme);
        }

        async Task Review(History latestHistory)
        {
            string review = await GeminiClient.Instance.SendReviewPrompt(latestHistory);

            PopUpWindowController.Instance.PopUp(
                title: "�O��̃��r���[!!",
                mainText: review,
                errorText: "",
                withoutInputField: true,
                onOk: null
            );
        }
    }

    // Start�̍ۂ�async���s�����߂̂���
    public static class TaskExtensions
    {
        public static async void Forget(this Task task)
        {
            try
            {
                await task;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Task exception: {ex}");
            }
        }
    }
}

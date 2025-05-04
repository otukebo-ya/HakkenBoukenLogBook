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
            bool existUserData = CheckUserData();
            if (!existUserData)
            {
                UserData.Token = await UIDirector.Instance.RequestTokenInput();
            }

            Theme = UsageHistoryManager.Instance.GetTodayTheme();
            if (Theme == "")
            {

                Debug.Log("�������߂Ẵ��O�C���ł��B");
                string[] recentThemes = UsageHistoryManager.Instance.LoadRecentThemes();
                Theme = await GeminiClient.Instance.SendThemeDecidePrompt(recentThemes);
                
                PopUpWindowController.Instance.PopUp(
                    title:"�����̔������n�߂��I", 
                    mainText:"�{���̃e�[�}��\n" +
                    "�u" + Theme + "�v\n" +
                    "�Ɍ��܂�����I", 
                    errorText:"", 
                    withoutInputField:true, 
                    onOk:null
                );

                UsageHistoryManager.Instance.MakeTodaysFile(Theme);
            }

            GeminiClient.Instance.SetBackBone(Theme);

            UIDirector.Instance.SetTodaysTheme(Theme);
            Discovery[]? todayDiscoveries = UsageHistoryManager.Instance.GetTodayDiscoveries();
            if(todayDiscoveries is not null)
            {
                ChatField.Instance.LineUpTodayDiscoveries(todayDiscoveries);
            }
        }

        void Update()
        {

        }

        public bool CheckUserData()
        {
            if (UserData.Token is not null)
            {
                return true;
            }
            return false;
        }

        public void DecideTheme()
        {

        }

        public void Review()
        {

        }
    }

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

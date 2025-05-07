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
            // gemini用トークンを設定
            // 無ければ設定させる
            bool existUserData = CheckUserData();
            if (!existUserData)
            {
                UserData.Token = await UIDirector.Instance.RequestTokenInput();
            }

            // 今日のテーマを取得
            Theme = UsageHistoryManager.Instance.GetTodayTheme();

            // 本日未ログインなら
            if (Theme == "")
            {
                //　テーマのポップアップ
                await DecideTheme();

                // 直近の履歴を確認し、レビューされてなければレビューする
                History? latestHistory = UsageHistoryManager.Instance.GetLatestHistory();
                bool isReviewed = (latestHistory?.Review ?? "") == "";

                if (!isReviewed)
                {
                    // レビューのポップアップ
                    await Review(latestHistory);
                }
            }

            // GeminiAPIの初期化
            GeminiClient.Instance.SetBackBone(Theme);

            // 画面上に今日のテーマと、これまでの発見を記述
            UIDirector.Instance.SetTodaysTheme(Theme);
            Discovery[]? todayDiscoveries = UsageHistoryManager.Instance.GetTodayDiscoveries();
            if(todayDiscoveries is not null)
            {
                ChatField.Instance.LineUpTodayDiscoveries(todayDiscoveries);
            }
        }

        // トークンがあるか？確認
        public bool CheckUserData()
        {
            if (UserData.Token is not null)
            {
                return true;
            }
            return false;
        }

        // テーマをGemini apiに決定させ、ポップアップ表示する
        async Task DecideTheme()
        {
            // 過去数日のテーマを取得
            string[] recentThemes = UsageHistoryManager.Instance.LoadRecentThemes();
            Theme = await GeminiClient.Instance.SendThemeDecidePrompt(recentThemes);


            // ポップアップ表示
            PopUpWindowController.Instance.PopUp(
                title: "今日の発見を始めるよ！",
                mainText: "本日のテーマは\n" +
                "「" + Theme + "」\n" +
                "に決まったよ！",
                errorText: "",
                withoutInputField: true,
                onOk: null
            );

            // 今日のログ保存用ファイルの作成
            UsageHistoryManager.Instance.MakeTodaysFile(Theme);
        }

        async Task Review(History latestHistory)
        {
            string review = await GeminiClient.Instance.SendReviewPrompt(latestHistory);

            PopUpWindowController.Instance.PopUp(
                title: "前回のレビュー!!",
                mainText: review,
                errorText: "",
                withoutInputField: true,
                onOk: null
            );
        }
    }

    // Startの際にasyncを行うためのもの
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

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
#nullable enable

namespace ColorBath
{
    public class GeminiClient
    {
        private static GeminiClient _instance = new GeminiClient();
        public static GeminiClient Instance
        {
            get
            {
                return _instance;
            }
        }

        private string _backbone;
        private string _todayTheme;
        private float _temperature = 2.0f;// GeminiAPIの回答のランダム性
        private int _top_k = 100;// ランダム性にかかわるパラメータ
        private float _top_p = 0.9f;// ランダム性にかかわるパラメータ
        private string _apiEndpoint = "";
        private string token = "";
        private GeminiClient()
        {
            token = UserData.Token;
            _apiEndpoint = "https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key=" + token;
        }

        // 今日のテーマをセット
        public void SetTodayTheme(string theme)
        {
            _todayTheme = theme;
        }

        // キャラ付けと、やっていることをGeminiAPIに教えるためのプロンプト
        public void SetBackBone() {
            _backbone = GeminiPrompts.GetBackBonePrompt(_todayTheme);
        }

        // プロンプトの送信（オーバロードあり）
        // 画像がない場合の処理
        private async Task<string> SendPrompt(string prompt, Texture2D image = null, int maxRetries = 3, float retryDelaySeconds = 10f)
        {
            List<object> parts = new List<object>();
            parts.Add(new { text = prompt });

            // 画像があれば送信可能な形式に
            if (image != null)
            {
                byte[] imageBytes = image.EncodeToJPG();
                string base64Image = Convert.ToBase64String(imageBytes);
                parts.Add(new
                {
                    inlineData = new
                    {
                        mimeType = "image/jpeg",
                        data = base64Image
                    }
                });
            }

            // リクエストデータをまとめる
            var requestData = new
            {
                contents = new[]
                {
                    new { parts = parts.ToArray() }
                },
                generationConfig = new
                {
                    temperature = _temperature,
                    top_k = _top_k,
                    top_p = _top_p
                }
            };

            // Json化
            string json = JsonConvert.SerializeObject(requestData);

            int attempt = 0;
            while (attempt < maxRetries)
            {
                if (attempt > 0)
                {
                    await Task.Delay((int)(retryDelaySeconds * 1000));
                }
                attempt++;
                
                using (UnityWebRequest www = new UnityWebRequest(_apiEndpoint, "POST"))
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                    www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    www.downloadHandler = new DownloadHandlerBuffer();
                    www.SetRequestHeader("Content-Type", "application/json");

                    await www.SendWebRequestAsync();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        return www.downloadHandler.text;
                    }
                }
            }

            return ""; // 最終的に失敗した場合
        }

        // 先日の発見に対するレビュー用のラッパー
        public async Task<string> SendReviewPrompt(History history)
        {
            string prompt = _backbone + GeminiPrompts.GetReviewPrompt(history);

            // 送信
            return await SendPrompt(prompt);
        }

        // 発見に対する相槌作成用のラッパー
        public async Task<string> SendAizuchiPrompt(string input, Texture2D image = null)
        {
            // 背景とタスクを合わせる
            string prompt = GeminiPrompts.GetAizuchiPrompt(_backbone, _todayTheme, input);

            string responce = await SendPrompt(prompt, image);

            // 応答から必要な部分を取り出す。
            if (!string.IsNullOrEmpty(responce))
            {
                try
                {
                    // JSONとしてパースを試みる
                    string responceText = GetResponceText(responce);
                    return responceText;
                }
                catch (JsonException e)
                {
                    Debug.LogError($"JSON パースエラー: {e.Message}\n応答: {responce}");
                    return "";
                }
            }
            else
            {
                Debug.Log("レスポンス無し");
                return "";
            }
        }

        // テーマ決定用のラッパー
        public async Task<string> SendThemeDecidePrompt(string[] recentThemes)
        {
            // 過去数日分のテーマを教えることで、テーマかぶりを避ける
            string themesString = "[" + string.Join(",", recentThemes.Select(t => $"\"{t}\"")) + "]";
            string prompt = GeminiPrompts.GetThemeDecidePrompt(themesString);

            string responce;
            responce = await SendPrompt(prompt);

            // 返答から必要な部分のみ取り出す
            if(!string.IsNullOrEmpty(responce))
            {
                try
                {
                    // JSONとしてパースを試みる
                    string responceText = GetResponceText(responce);
                    string cleanedJson = Regex.Replace(responceText, @"^```json\s*|```$", "", RegexOptions.Multiline).Trim();
                    JObject themeJson = JObject.Parse(cleanedJson);
                    string theme = themeJson["theme"]?.ToString();
                    Debug.Log(theme);
                    return theme;
                }
                catch (JsonException e)
                {
                    Debug.LogError($"JSON パースエラー: {e.Message}\n応答: {responce}");
                    return "";
                }
            }else
            {
                Debug.Log("レスポンス無し");
                return "";
            }
        }

        public string GetResponceText(string responce)
        {
            JObject json = JObject.Parse(responce);
            string rawText = json["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
            return rawText;
        }
    }

    public static class UnityWebRequestExtensions
    {
        public static Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();
            var operation = request.SendWebRequest();
            operation.completed += _ =>
            {
                tcs.SetResult(request);
            };
            return tcs.Task;
        }
    }
}
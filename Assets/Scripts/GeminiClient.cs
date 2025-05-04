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
        private float _temperature = 2.0f;
        private int _top_k = 40;
        private float _top_p = 1.0f;
        private string _apiEndpoint = "";
        private string token = "";
        private GeminiClient()
        {
            token = UserData.Token;
            _apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key=" + token;
        }

        public void SetBackBone(string theme) {
            _backbone = 
            $@"
            ＜あなたの設定＞
            あなたは高飛車なお嬢様です。
            私に対してフレンドリーな口調で接します。
            また、細かいところに気を配り人をほめることが得意です。
            顔文字は使わないようにしましょう。
            ＜私の設定＞
            私はカラーバスを行っています。今日のテーマは{theme}です。
            "; 
        }

        private async Task<string> SendPrompt(string prompt)
        {
            var requestData = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = _backbone + prompt } } }
                },
                generationConfig = new
                {
                    temperature = _temperature,
                    top_k = _top_k,
                    top_p = _top_p
                }
            };
            string json = JsonConvert.SerializeObject(requestData);

            using (UnityWebRequest www = new UnityWebRequest(_apiEndpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                await www.SendWebRequestAsync();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Gemini API Error: {www.error}");
                    return "";
                }
                else
                {
                    return www.downloadHandler.text;
                }
            }
        }

        private async Task<string> SendPrompt(string prompt, Texture2D image)
        {
            List<object> parts = new List<object>();
            parts.Add(new { text = _backbone + prompt });
            
            if (image != null)
            {
                byte[] imageBytes = image.EncodeToJPG(); // または image.EncodeToPNG();
                string base64Image = Convert.ToBase64String(imageBytes);
                parts.Add(new
                {
                    inlineData = new
                    {
                        mimeType = "image/jpeg", // または "image/png"
                        data = base64Image
                    }
                });
            }

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
            string json = JsonConvert.SerializeObject(requestData);

            using (UnityWebRequest www = new UnityWebRequest(_apiEndpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                await www.SendWebRequestAsync();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"API Error: {www.error}");
                    return "";
                }
                else
                {
                    return www.downloadHandler.text;
                }
            }
        }

        public async Task<string> SendReviewPrompt()
        {
            string prompt = _backbone + 
            $@"
            ＜タスク＞
　　　　　　本日のこれまでの私の発見について、簡単に総括を行ったうえで、
            レビューを行ってください。
            文字数は200字以内とします。
            ";
            return await SendPrompt(prompt);
        }

        public async Task<string> SendAizuchiPrompt(string input, Texture2D image = null)
        {
            Debug.Log(_backbone);
            string prompt = 
            $@"
            {_backbone}\n
            ＜タスク＞
            今から、私の発見したものをテキスト形式であなたに渡します。
            あなたはそれを確認し、私のその発見に対する相槌やレビューを行ってください。
            文字数は100文字以内とします。
            ＜入力＞
            {input}
            ";

            string responce = "";
            if (image != null)
            {
                responce = await SendPrompt(prompt, image);
                
            }
            else
            {
                responce = await SendPrompt(prompt);
            }

            if (!string.IsNullOrEmpty(responce))
            {
                try
                {
                    // JSONとしてパースを試みる
                    string responceText = GetResponceText(responce);
                    Debug.Log(responceText);
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

        public async Task<string> SendThemeDecidePrompt(string[] recentThemes)
        {
            string themesString = "[" + string.Join(",", recentThemes.Select(t => $"\"{t}\"")) + "]";
            Debug.Log("以前のテーマ"+themesString);
            string prompt =
            $@"私はカラーバスを行っています。
            これは、毎日抽象的で簡単なテーマを設定し、該当するモノをたくさん探すことで、身の回りのモノへの意識を高まり、発想力の強化につながります。
            ここ数日間のテーマは以下の通りです。
            {themesString}
            これらのテーマとはかぶらないように、本日探すべきテーマをランダムに一つ決めてしてください。
            テーマは色、形、質感、擬音、その他様々なバリエーションから決定してください。
            テーマは該当する範囲が狭くなりすぎないように、抽象的かつ、簡潔な表現にしてください。
            複数の表現を組み合わされると該当するモノが減り困ります。
            また、時間帯や土地名などは、その場にいないといけないので困るから選ばないように。
            必ず応答は以下の形式のJSON形式であることを守ってください。
            {{
                  ""theme"": ""提案するテーマ""
            }}

            JSON形式以外での応答を行わないでください。
            ";
            string responce;
            responce = await SendPrompt(prompt);
            Debug.Log(responce);
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

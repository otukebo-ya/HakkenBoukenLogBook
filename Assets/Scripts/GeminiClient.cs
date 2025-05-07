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
        private string _todayTheme;
        private float _temperature = 2.0f;// GeminiAPIの回答のランダム性
        private int _top_k = 40;// ランダム性にかかわるパラメータ
        private float _top_p = 1.0f;// ランダム性にかかわるパラメータ
        private string _apiEndpoint = "";
        private string token = "";
        private GeminiClient()
        {
            token = UserData.Token;
            _apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key=" + token;
        }

        // キャラ付けと、やっていることをGeminiAPIに教えるためのプロンプト
        public void SetBackBone(string theme) {
            _todayTheme = theme;
            _backbone = 
            $@"
            ＜あなたの設定＞
            あなたは高飛車なお嬢様です。
            私に対してフレンドリーな口調で接します。
            また、細かいところに気を配り人をほめることが得意です。
            顔文字は使わないようにしましょう。
            ＜私の設定＞
            私はカラーバスを行っています。
            毎日抽象的で簡単なテーマを設定し、該当するモノをたくさん探すことで、身の回りのモノへの意識を高まり、発想力の強化につなげます。
            "; 
        }

        // プロンプトの送信（オーバロードあり）
        // 画像がない場合の処理
        private async Task<string> SendPrompt(string prompt)
        {
            // リクエストデータをJsonに
            var requestData = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new
                {
                    temperature = _temperature,
                    top_k = _top_k,
                    top_p = _top_p
                }
            };
            string json = JsonConvert.SerializeObject(requestData);


            // APIにポストを行う
            using (UnityWebRequest www = new UnityWebRequest(_apiEndpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");// 回答をJson形式に指定
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

        // プロンプトの送信（オーバロードあり）
        // 画像がある場合の処理
        private async Task<string> SendPrompt(string prompt, Texture2D image)
        {
            List<object> parts = new List<object>();
            parts.Add(new { text = prompt });
            
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

        // 先日の発見に対するレビュー用のラッパー
        public async Task<string> SendReviewPrompt(History history)
        {
            // 先日の履歴から、発見と相槌を取り出し
            Discovery[] discoveries = history.Discoveries;
            string discoveriesText="";
            for (int d = 1; d <= discoveries.Length; d++)
            {
                discoveriesText += $@"<発見{d}個目>\n"
                                +$@"　・私の発見：{discoveries[d-1].Memo}\n"
                                +$@"　・あなたの相槌：{discoveries[d-1].Aizuchi}\n";
            }

            // 背景とタスクを合わせる
            string prompt = _backbone + 
            $@"
            ＜タスク＞
            今から、私がテーマに沿って発見したものとそれに対するあなたの相槌のログを教えます。
　　　　　　私の発見について、簡単に総括を行ったうえで、
            レビューを行ってください。
            内容はポジティブだといいな。
            文字数は200字以内とします。
            ＜ログ＞
            {discoveriesText}
            ";

            // 送信
            return await SendPrompt(prompt);
        }

        // 発見に対する相槌作成用のラッパー
        public async Task<string> SendAizuchiPrompt(string input, Texture2D image = null)
        {
            // 背景とタスクを合わせる
            string prompt = 
            $@"
            {_backbone}\n
            ＜タスク＞
            今から、私の発見したものをテキスト形式であなたに渡します。
            あなたはそれを確認し、私のその発見に対する相槌やレビューを行ってください。
            文字数は100文字以内とします。
            本日のカラーバスのテーマは{_todayTheme}です。
            ＜入力＞
            {input}
            ";

            // 画像があるかどうかで関数の使い分け
            string responce = "";
            if (image != null)
            {
                responce = await SendPrompt(prompt, image);
                
            }
            else
            {
                responce = await SendPrompt(prompt);
            }

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

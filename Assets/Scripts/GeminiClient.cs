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
        private int _top_k = 100;// ランダム性にかかわるパラメータ
        private float _top_p = 0.9f;// ランダム性にかかわるパラメータ
        private string _apiEndpoint = "";
        private string token = "";
        private GeminiClient()
        {
            token = UserData.Token;
            _apiEndpoint = "https://generativelanguage.googleapis.com/v1/models/gemini-2.0-flash:generateContent?key=" + token;
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
            日ごとにテーマを設定し、それに該当するモノをたくさん探すことで、身の回りのモノへの意識を高まり、発想力の強化につなげます。
            "; 
        }

        // プロンプトの送信（オーバロードあり）
        // 画像がない場合の処理
        private async Task<string> SendPrompt(string prompt, int maxRetries = 3, float retryDelaySeconds = 2f)
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

            int attempt = 0;

            while (attempt < maxRetries)
            {
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
                    else
                    {
                        Debug.LogWarning($"Gemini API Error (attempt {attempt}/{maxRetries}): {www.error}");

                        if (attempt < maxRetries)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
                            continue; // 再試行
                        }
                        else
                        {
                            Debug.LogError($"Gemini API failed after {maxRetries} attempts: {www.error}");
                            return "";
                        }
                    }
                }
            }

            return ""; // 最終的に失敗した場合
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
            あなたはそれを確認し、私のその発見に対する相槌を行ってください。
            文字数は100文字以内とします。
            本日のカラーバスのテーマは{_todayTheme}なので、そのことを踏まえた相槌を行ってください。
            カラーバスを行うことは当たり前なので、”カラーバス頑張っているのね”のような、カラーバスの取り組んでいることについての言及を行わないでください。
            文字数がもったいないので。
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
            // ランダム要素を入れて、固定化を防ぐ
            string randomizer = Guid.NewGuid().ToString();
            // 過去数日分のテーマを教えることで、テーマかぶりを避ける
            string themesString = "[" + string.Join(",", recentThemes.Select(t => $"\"{t}\"")) + "]";
            string prompt =
            $@"
            ## タスク
            私はカラーバスを行っているよ。
            これは、日によって異なるさまざまなテーマを設定し、該当するモノをたくさん探すことで、身の回りのモノへの意識を高まり、発想力の強化につながるんだ。
            ここ数日間のテーマはthemesに記述するかも。でも、ここ数日のテーマはない場合もあるよ！
            過去テーマと被らないほうがいいね。
            本日探すべきテーマを単語としてランダムに選んでほしいな。
            ここで、カラーバスは見た目の話だから、見た目から感じ取れるテーマである必要があるから、
            注意してほしいな。
            色、形、質感、擬音、感覚、そのほかいろんなジャンルがあるよね。
            あなたには自由に様々なジャンルからユニークなものを選んでほしいね。
            テーマは抽象的だと、あてはまるものが多くなってカラーバスが楽しくなるな～。
            
            ## 応答の形式
            {{
                [""theme"": ""提案するテーマ""]
            }}

            ## themas（過去数日間のテーマ）ないこともある
            {themesString}

            ## randomizer（発想にもちいる乱数）
            {randomizer}
            ";
            Debug.Log(themesString);

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
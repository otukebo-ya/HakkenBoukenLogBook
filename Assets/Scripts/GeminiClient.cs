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

        private string _apiEndpoint = "";
        private string token = "";
        private GeminiClient()
        {
            token = UserData.Token;
            _apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=" + token;
        }

        private Func<string, string> _backboneFactory = (theme) => $@"
            �����Ȃ��̐ݒ聄
            ���Ȃ��͍���ԂȂ���l�ł��B
            ���ɑ΂��ăt�����h���[�Ȍ����Őڂ��܂��B
            �܂��A�ׂ����Ƃ���ɋC��z��l���ق߂邱�Ƃ����ӂł��B
            �當���͎g��Ȃ��悤�ɂ��܂��傤�B
            �����̐ݒ聄
            ���̓J���[�o�X���s���Ă��܂��B�����̃e�[�}��{theme}�ł��B
        ";

        private async Task<string> SendPrompt(string prompt)
        {
            var requestData = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = _backbone + prompt } } }
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
                byte[] imageBytes = image.EncodeToJPG(); // �܂��� image.EncodeToPNG();
                string base64Image = Convert.ToBase64String(imageBytes);
                parts.Add(new
                {
                    inlineData = new
                    {
                        mimeType = "image/jpeg", // �܂��� "image/png"
                        data = base64Image
                    }
                });
            }

            var requestData = new
            {
                contents = new[]
                {
                    new { parts = parts.ToArray() }
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
            ���^�X�N��
�@�@�@�@�@�@�{���̂���܂ł̎��̔����ɂ��āA�ȒP�ɑ������s���������ŁA
            ���r���[���s���Ă��������B
            ��������300���ȓ��Ƃ��܂��B
            ";
            return await SendPrompt(prompt);
        }

        public async Task<string> SendAizuchiPrompt(string input, Texture2D image = null)
        {
            string prompt = _backbone + 
            $@"
            ���^�X�N��
            ������A���̔����������̂��e�L�X�g�`���ł��Ȃ��ɓn���܂��B
            ���Ȃ��͂�����m�F���A���̂��̔����ɑ΂��鑊�Ƃ⃌�r���[���s���Ă��������B
            ��������300�����ȓ��Ƃ��܂��B
            �����́�
            {input}
            ";

            if (image != null)
            {
                return await SendPrompt(prompt, image);
            }
            else
            {
                return await SendPrompt(prompt);
            }
        }

        public async Task<string> SendThemeDecidePrompt(string[] recentThemes)
        {
            string themesString = "[" + string.Join(",", recentThemes.Select(t => $"\"{t}\"")) + "]";
            Debug.Log("�ȑO�̃e�[�}"+themesString);
            string prompt =
            $@"���̓J���[�o�X���s���Ă��܂��B
            ���������Ԃ̃e�[�}�͈ȉ��̒ʂ�ł��B
            {themesString}
            �����̃e�[�}�Ƃ͂��Ԃ�Ȃ��悤�ɁA�{���T���ׂ��e�[�}������肵�Ă��������B
            �e�[�}�̃J�e�S���͊ȒP�Ȃ��̂ł�����F�A�`�A�����A�[���A���̑��l�X�ȃo���G�[�V�������猈�肵�Ă��������B
            �K�������͈ȉ��̌`����JSON�`���ł��邱�Ƃ�����Ă��������B
            {{
                  ""theme"": ""��Ă���e�[�}""
            }}

            JSON�`���ȊO�ł̉������s��Ȃ��ł��������B
            ";
            string responce;
            responce = await SendPrompt(prompt);
            Debug.Log(responce);
            if(!string.IsNullOrEmpty(responce))
            {
                try
                {
                    // JSON�Ƃ��ăp�[�X�����݂�
                    JObject json = JObject.Parse(responce);
                    string rawText = json["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                    string cleanedJson = Regex.Replace(rawText, @"^```json\s*|```$", "", RegexOptions.Multiline).Trim();
                    JObject themeJson = JObject.Parse(cleanedJson);
                    string theme = themeJson["theme"]?.ToString();
                    Debug.Log(theme);
                    _backbone = _backboneFactory(theme);
                    return theme;
                }
                catch (JsonException e)
                {
                    Debug.LogError($"JSON �p�[�X�G���[: {e.Message}\n����: {responce}");
                    return "";
                }
            }else
            {
                Debug.Log("���X�|���X����");
                return "";
            }
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

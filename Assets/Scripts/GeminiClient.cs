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
        private float _temperature = 2.0f;// GeminiAPI�̉񓚂̃����_����
        private int _top_k = 40;// �����_�����ɂ������p�����[�^
        private float _top_p = 1.0f;// �����_�����ɂ������p�����[�^
        private string _apiEndpoint = "";
        private string token = "";
        private GeminiClient()
        {
            token = UserData.Token;
            _apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key=" + token;
        }

        // �L�����t���ƁA����Ă��邱�Ƃ�GeminiAPI�ɋ����邽�߂̃v�����v�g
        public void SetBackBone(string theme) {
            _todayTheme = theme;
            _backbone = 
            $@"
            �����Ȃ��̐ݒ聄
            ���Ȃ��͍���ԂȂ���l�ł��B
            ���ɑ΂��ăt�����h���[�Ȍ����Őڂ��܂��B
            �܂��A�ׂ����Ƃ���ɋC��z��l���ق߂邱�Ƃ����ӂł��B
            �當���͎g��Ȃ��悤�ɂ��܂��傤�B
            �����̐ݒ聄
            ���̓J���[�o�X���s���Ă��܂��B
            �������ۓI�ŊȒP�ȃe�[�}��ݒ肵�A�Y�����郂�m����������T�����ƂŁA�g�̉��̃��m�ւ̈ӎ������܂�A���z�͂̋����ɂȂ��܂��B
            "; 
        }

        // �v�����v�g�̑��M�i�I�[�o���[�h����j
        // �摜���Ȃ��ꍇ�̏���
        private async Task<string> SendPrompt(string prompt)
        {
            // ���N�G�X�g�f�[�^��Json��
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


            // API�Ƀ|�X�g���s��
            using (UnityWebRequest www = new UnityWebRequest(_apiEndpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");// �񓚂�Json�`���Ɏw��
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

        // �v�����v�g�̑��M�i�I�[�o���[�h����j
        // �摜������ꍇ�̏���
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

        // ����̔����ɑ΂��郌�r���[�p�̃��b�p�[
        public async Task<string> SendReviewPrompt(History history)
        {
            // ����̗�������A�����Ƒ��Ƃ����o��
            Discovery[] discoveries = history.Discoveries;
            string discoveriesText="";
            for (int d = 1; d <= discoveries.Length; d++)
            {
                discoveriesText += $@"<����{d}��>\n"
                                +$@"�@�E���̔����F{discoveries[d-1].Memo}\n"
                                +$@"�@�E���Ȃ��̑��ƁF{discoveries[d-1].Aizuchi}\n";
            }

            // �w�i�ƃ^�X�N�����킹��
            string prompt = _backbone + 
            $@"
            ���^�X�N��
            ������A�����e�[�}�ɉ����Ĕ����������̂Ƃ���ɑ΂��邠�Ȃ��̑��Ƃ̃��O�������܂��B
�@�@�@�@�@�@���̔����ɂ��āA�ȒP�ɑ������s���������ŁA
            ���r���[���s���Ă��������B
            ���e�̓|�W�e�B�u���Ƃ����ȁB
            ��������200���ȓ��Ƃ��܂��B
            �����O��
            {discoveriesText}
            ";

            // ���M
            return await SendPrompt(prompt);
        }

        // �����ɑ΂��鑊�ƍ쐬�p�̃��b�p�[
        public async Task<string> SendAizuchiPrompt(string input, Texture2D image = null)
        {
            // �w�i�ƃ^�X�N�����킹��
            string prompt = 
            $@"
            {_backbone}\n
            ���^�X�N��
            ������A���̔����������̂��e�L�X�g�`���ł��Ȃ��ɓn���܂��B
            ���Ȃ��͂�����m�F���A���̂��̔����ɑ΂��鑊�Ƃ⃌�r���[���s���Ă��������B
            ��������100�����ȓ��Ƃ��܂��B
            �{���̃J���[�o�X�̃e�[�}��{_todayTheme}�ł��B
            �����́�
            {input}
            ";

            // �摜�����邩�ǂ����Ŋ֐��̎g������
            string responce = "";
            if (image != null)
            {
                responce = await SendPrompt(prompt, image);
                
            }
            else
            {
                responce = await SendPrompt(prompt);
            }

            // ��������K�v�ȕ��������o���B
            if (!string.IsNullOrEmpty(responce))
            {
                try
                {
                    // JSON�Ƃ��ăp�[�X�����݂�
                    string responceText = GetResponceText(responce);
                    return responceText;
                }
                catch (JsonException e)
                {
                    Debug.LogError($"JSON �p�[�X�G���[: {e.Message}\n����: {responce}");
                    return "";
                }
            }
            else
            {
                Debug.Log("���X�|���X����");
                return "";
            }
        }

        // �e�[�}����p�̃��b�p�[
        public async Task<string> SendThemeDecidePrompt(string[] recentThemes)
        {
            // �ߋ��������̃e�[�}�������邱�ƂŁA�e�[�}���Ԃ�������
            string themesString = "[" + string.Join(",", recentThemes.Select(t => $"\"{t}\"")) + "]";
            string prompt =
            $@"���̓J���[�o�X���s���Ă��܂��B
            ����́A�������ۓI�ŊȒP�ȃe�[�}��ݒ肵�A�Y�����郂�m����������T�����ƂŁA�g�̉��̃��m�ւ̈ӎ������܂�A���z�͂̋����ɂȂ���܂��B
            ���������Ԃ̃e�[�}�͈ȉ��̒ʂ�ł��B
            {themesString}
            �����̃e�[�}�Ƃ͂��Ԃ�Ȃ��悤�ɁA�{���T���ׂ��e�[�}�������_���Ɉ���߂Ă��Ă��������B
            �e�[�}�͐F�A�`�A�����A�[���A���̑��l�X�ȃo���G�[�V�������猈�肵�Ă��������B
            �e�[�}�͊Y������͈͂������Ȃ肷���Ȃ��悤�ɁA���ۓI���A�Ȍ��ȕ\���ɂ��Ă��������B
            �����̕\����g�ݍ��킳���ƊY�����郂�m�����荢��܂��B
            �܂��A���ԑт�y�n���Ȃǂ́A���̏�ɂ��Ȃ��Ƃ����Ȃ��̂ō��邩��I�΂Ȃ��悤�ɁB
            �K�������͈ȉ��̌`����JSON�`���ł��邱�Ƃ�����Ă��������B
            {{
                  ""theme"": ""��Ă���e�[�}""
            }}

            JSON�`���ȊO�ł̉������s��Ȃ��ł��������B
            ";

            string responce;
            responce = await SendPrompt(prompt);

            // �ԓ�����K�v�ȕ����̂ݎ��o��
            if(!string.IsNullOrEmpty(responce))
            {
                try
                {
                    // JSON�Ƃ��ăp�[�X�����݂�
                    string responceText = GetResponceText(responce);
                    string cleanedJson = Regex.Replace(responceText, @"^```json\s*|```$", "", RegexOptions.Multiline).Trim();
                    JObject themeJson = JObject.Parse(cleanedJson);
                    string theme = themeJson["theme"]?.ToString();
                    Debug.Log(theme);
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

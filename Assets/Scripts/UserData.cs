using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

namespace ColorBath
{
    public static class UserData
    {
        public static string Token
        {
            get { return GetGeminiToken(); }
            set { SetGeiminiToken(value); }
        }

        public static string GetGeminiToken()
        {
            string path = Path.Combine(Application.persistentDataPath, "GeminiToken.json");

            if (!File.Exists(path))
            {
                Debug.Log("ユーザデータが見つかりませんでした");
                return null;
            }

            string encryptedJson = File.ReadAllText(path);
            string json = CryptoHelper.Decrypt(encryptedJson);
            GeminiTokenJson data = JsonUtility.FromJson<GeminiTokenJson>(json);
            return data.Token;
        }

        public static void SetGeiminiToken(string token)
        {
            GeminiTokenJson geminiToken = new GeminiTokenJson();
            geminiToken.Token = token;
            string json = JsonUtility.ToJson(geminiToken);
            string encrypted = CryptoHelper.Encrypt(json);

            string path = Path.Combine(Application.persistentDataPath, "GeminiToken.json");
            File.WriteAllText(path, encrypted);
        }

        [System.Serializable]
        public class GeminiTokenJson
        {
            public string Token;
        }
    }
}

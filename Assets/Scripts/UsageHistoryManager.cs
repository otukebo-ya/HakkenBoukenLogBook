using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#nullable enable

namespace ColorBath
{
    public class UsageHistoryManager
    {
        private string _path;
        private int _recentlyViewedLogNum = 0;
        private static int NUMBER_OF_FILES = 50;

        private static UsageHistoryManager _instance = new UsageHistoryManager();

        public static UsageHistoryManager Instance
        {
            get
            {
                return _instance;
            }
        }

        private UsageHistoryManager()
        {
            _path = Path.Combine(Application.persistentDataPath, "Histories");
        }

        // 今日のテーマを取得
        public string GetTodayTheme()
        {

            DateTime today = DateTime.Today;
            string todaysFileName = Date2JsonFileName(today, "history");
            string[] files = Directory.GetFiles(Application.persistentDataPath, $"{todaysFileName}");
            return (files.Length > 0) ? GetTheme(files[0]) : "";
        }

        // 今日の発見を取得
        public Discovery[]? GetTodayDiscoveries() {
            DateTime today = DateTime.Today;
            string todaysFileName = Date2JsonFileName(today, "history");
            string[] files = Directory.GetFiles(Application.persistentDataPath, $"{todaysFileName}");
            return (files.Length > 0) ? GetDiscoveries(files[0]) : null;
        }

        // 直近の履歴を取得
        public History? GetLatestHistory()
        {
            string historyPath = Path.Combine(Application.persistentDataPath, "Histories");
            string[] files = Directory.GetFiles(historyPath, "*.json");
            if (files.Length == 0)
            {
                Debug.Log("HistoryフォルダにJSONファイルがありません");
                return null;
            }

            var file = files
            .Select(file => new
            {
                FileName = file,
                Date = JsonFileName2Date(Path.GetFileName(file))
            })
            .Where(file => file.Date.HasValue)
            .OrderByDescending(file => file.Date)
            .FirstOrDefault();


            if (file == null) { return null; };
            string path = file.FileName;

            return GetHistory(path);
        }

        // ここ数日のテーマを取得
        public string[] LoadRecentThemes()
        {
            string historyPath = Path.Combine(Application.persistentDataPath, "Histories");
            if (!Directory.Exists(historyPath))
            {
                Directory.CreateDirectory(historyPath);
            }
            string[] files = Directory.GetFiles(historyPath, "*.json");
            if (files.Length == 0)
            {
                Debug.Log("HistoryフォルダにJSONファイルがありません");
                return new string[0];
            }

            string[] recentThemes = files
            .Select(file => new
            {
                FileName = file,
                Date = JsonFileName2Date(Path.GetFileName(file))
            })
            .Where(file => file.Date.HasValue)
            .OrderByDescending(file => file.Date)
            .Take(NUMBER_OF_FILES)
            .Select(file => GetTheme(file.FileName))
            .ToArray();

            return recentThemes;
        }

        // 特定のファイルからテーマを取得
        private string GetTheme(string path)
        {
            History? history = GetHistory(path);
            return history?.Theme ?? "";
        }

        // 特定のファイルから発見を取得
        private Discovery[]? GetDiscoveries(string path)
        {
            History? history = GetHistory(path);
            return history?.Discoveries ?? null;
        }

        // 特定のファイルから履歴を取得
        private History? GetHistory(string path)
        {
            try
            {
                // 暗号化する場合
                //string encryptedJson = File.ReadAllText(path);
                //string json = CryptoHelper.Decrypt(encryptedJson);

                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<History>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading or parsing the file: {ex.Message}");
                return null;
            }
        }

        // その日の記録を行うファイルを作成
        public void MakeTodaysFile(string theme)
        {
            string todaysPath = MakeTodaysHistoryPath();
            string path = Path.Combine(Application.persistentDataPath, todaysPath);
            History history = new History();
            history.Theme = theme;
            history.Date = DateTime.Today;
            string json = JsonUtility.ToJson(history);
            
            //暗号化する場合
            //string encrypted = CryptoHelper.Encrypt(json);
            //File.WriteAllText(path, encrypted);

            File.WriteAllText(path, json);
            Debug.Log("今日のファイルが作成されました。" + path);
        }

        // 発見をその日のファイルに保存
        public void SaveDiscovery(Discovery discovery)
        {
            string todaysPath = MakeTodaysHistoryPath();
            History ? history = GetHistory(todaysPath);
            if (history != null)
            {
                List<Discovery> discoveryList = history.Discoveries?.ToList() ?? new List<Discovery>();
                discoveryList.Add(discovery);
                history.Discoveries = discoveryList.ToArray();
                Debug.Log("My discovery was saved to  " + todaysPath);
                Debug.Log(history.Discoveries[0].Memo);
                SaveHistory(todaysPath, history);
            }
            else
            {
                Debug.Log("Don't exist history!!");
            }
        }

        public void DeleteDiscovery(History target, int discoveryNum)
        {
            string fileName = Date2JsonFileName(target.Date, "history");
            string path = Path.Combine(Application.persistentDataPath, fileName);

            History? history = GetHistory(path);
            if (history != null)
            {
                List<Discovery> discoveryList = history.Discoveries?.ToList() ?? new List<Discovery>();
                discoveryList.RemoveAt(discoveryNum - 1);
                history.Discoveries = discoveryList.ToArray();

                SaveHistory(path, history);
            }
        }

        // 特定の日付のレビューを保存
        public void SaveReview(string review, DateTime dateTime)
        {
            string fileName = Date2JsonFileName(dateTime, "history");
            string path = Path.Combine(Application.persistentDataPath, fileName);

            History? history = GetHistory(path);
            if (history != null)
            {
                history.Review = review;
                SaveHistory(path, history);
            }
        }

        // 今日の履歴の場所を表すパスを作成
        public string MakeTodaysHistoryPath()
        {
            DateTime today = DateTime.Today;
            string todaysFileName = Date2JsonFileName(today, "history");
            string path = Path.Combine(Application.persistentDataPath, todaysFileName);
            return path;
        }

        // 特定のファイルに履歴を保存
        private static void SaveHistory(string filePath, History history)
        {

            string json = JsonConvert.SerializeObject(history);
            // 読めなくする場合
            //string encrypted = CryptoHelper.Encrypt(json);
            //File.WriteAllText(filePath, encrypted);
            Debug.Log("saved"+json);
            File.WriteAllText(filePath, json);
        }

        // Jsonファイル名　＝＞　日付
        private DateTime? JsonFileName2Date(string fileName)
        {
            var datePart = fileName.Substring(fileName.LastIndexOf('_') + 1).Replace(".json", "");

            if (DateTime.TryParse(datePart, out DateTime date))
            {
                return date;
            }
            return null;
        }

        // 日付　＝＞　Jsonファイル名
        private string Date2JsonFileName(DateTime date, string prefix)
        {
            string date_str= date.ToString("yyyy_MM_dd");
            return $"{prefix}_{date_str}.json";
        }
    }
}


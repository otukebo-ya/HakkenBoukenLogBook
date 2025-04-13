using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
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

        public string GetTodayTheme()
        {
            DateTime today = DateTime.Today;
            string todaysFileName = Date2JsonFileName(today, "history");
            string[] files = Directory.GetFiles(Application.persistentDataPath, $"{todaysFileName}");
            return (files.Length > 0) ? GetTheme(files[0]) : "";
        }

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

        private string GetTheme(string path)
        {
            History? history = GetHistory(path);
            return history?.Theme ?? "";
        }

        private History? GetHistory(string path)
        {
            try
            {
                string encryptedJson = File.ReadAllText(path);
                string json = CryptoHelper.Decrypt(encryptedJson);
                return JsonUtility.FromJson<History>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading or parsing the file: {ex.Message}");
                return null;
            }
        }

        public void MakeTodaysFile(string theme)
        {
            string todaysPath = MakeTodaysHistoryPath();
            string path = Path.Combine(Application.persistentDataPath, todaysPath);
            History history = new History();
            history.Theme = theme;
            string json = JsonUtility.ToJson(history);
            string encrypted = CryptoHelper.Encrypt(json);
            File.WriteAllText(path, encrypted);
            Debug.Log("今日のファイルが作成されました。" + path);
        }

        public void SaveDiscovery(Discovery discovery)
        {
            string todaysPath = MakeTodaysHistoryPath();
            History ? history = GetHistory(todaysPath);
            if (history != null)
            {
                List<Discovery> discoveryList = history.Discoveries?.ToList() ?? new List<Discovery>();
                discoveryList.Add(discovery);
                history.Discoveries = discoveryList.ToArray();

                SaveHistory(todaysPath, history);
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

        public string MakeTodaysHistoryPath()
        {
            DateTime today = DateTime.Today;
            string todaysFileName = Date2JsonFileName(today, "history");
            string path = Path.Combine(Application.persistentDataPath, todaysFileName);
            return path;
        }

        private static void SaveHistory(string filePath, History history)
        {
            string json = JsonUtility.ToJson(history, true);
            string encrypted = CryptoHelper.Encrypt(json);
            File.WriteAllText(filePath, encrypted);
        }

        private DateTime? JsonFileName2Date(string fileName)
        {
            var datePart = fileName.Substring(fileName.LastIndexOf('_') + 1).Replace(".json", "");

            if (DateTime.TryParse(datePart, out DateTime date))
            {
                return date;
            }
            return null;
        }

        private string Date2JsonFileName(DateTime date, string prefix)
        {
            string date_str= date.ToString("yyyy_MM_dd");
            return $"{prefix}_{date_str}.json";
        }
    }
}


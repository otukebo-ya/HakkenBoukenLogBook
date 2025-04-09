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
        public string[] RecentThemes { get; } = new string[0];
        //public History[] Histories { get; }
        private string _path;
        private int _recentlyViewedLogNum = 0;
        private static int NUMBER_OF_FILES = 50;

        public UsageHistoryManager()
        {
            _path = Path.Combine(Application.persistentDataPath, "Histories");
            if (isLoggedInToday())
            {
                RecentThemes = LoadRecentThemes(_path);
            }
        }

        private bool isLoggedInToday()
        {
            DateTime today = DateTime.Today;
            string todaysFileName = Date2FileName(today, "history", ".json");
            string[] files = Directory.GetFiles(Application.persistentDataPath, $"{todaysFileName}");
            return (files.Length > 0) ? true : false; 
        }

        public string[] LoadRecentThemes(string path)
        {
            string historyPath = Path.Combine(Application.persistentDataPath, "Histories");
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
                Date = FileName2Date(Path.GetFileName(file), ".json")
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
                using (StreamReader reader = new StreamReader(path))
                {
                    string datastr = reader.ReadToEnd();
                    return JsonUtility.FromJson<History>(datastr);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading or parsing the file: {ex.Message}");
                return null;
            }
        }

        public void SaveDiscovery(History history)
        {

        }

        public void DeleteDiscovery()
        {

        }

        public History GetLatestHistry()
        {
            History history = new History();
            return history;
        }

        public void SaveReview()
        {

        }

        private DateTime? FileName2Date(string fileName, string extension)
        {
            var datePart = fileName.Substring(fileName.LastIndexOf('_') + 1).Replace(extension, "");

            if (DateTime.TryParse(datePart, out DateTime date))
            {
                return date;
            }
            return null;
        }

        private string Date2FileName(DateTime date, string prefix, string extension)
        {
            string date_str= date.ToString("yyyy_MM_dd");
            return $"{prefix}_{date_str}{extension}";
        }
    }
}


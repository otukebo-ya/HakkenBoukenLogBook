using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
#nullable enable

namespace ColorBath
{
    public class UsageHistoryManager
    {
        private static readonly UsageHistoryManager _instance = new UsageHistoryManager();
        public static UsageHistoryManager Instance => _instance;

        private readonly string _historyDir;
        private const int NUMBER_OF_FILES = 50;

        private UsageHistoryManager()
        {
            _historyDir = Path.Combine(Application.persistentDataPath, "Histories");
            EnsureDirectoryExists();
        }

        // --- 共通ヘルパー ---

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_historyDir)) Directory.CreateDirectory(_historyDir);
        }

        private string GetHistoryPath(DateTime date) => Path.Combine(_historyDir, Date2JsonFileName(date));

        // 有効な履歴ファイルを日付降順で取得する共通ロジック
        private IEnumerable<(string Path, DateTime Date)> GetSortedHistoryFiles()
        {
            EnsureDirectoryExists();
            return Directory.GetFiles(_historyDir, "*.json")
                .Select(path => (Path: path, Date: JsonFileName2Date(Path.GetFileName(path))))
                .Where(x => x.Date.HasValue)
                .Select(x => (x.Path, Date: x.Date!.Value))
                .OrderByDescending(x => x.Date);
        }

        // --- 公開メソッド ---

        public string GetTodayTheme() => GetHistory(DateTime.Today)?.Theme ?? "";

        public Discovery[]? GetTodayDiscoveries() => GetHistory(DateTime.Today)?.Discoveries;

        public History? GetLatestHistory() => GetSortedHistoryFiles().Select(x => GetHistory(x.Path)).FirstOrDefault();

        public string[] LoadRecentThemes() => GetSortedHistoryFiles().Take(NUMBER_OF_FILES).Select(x => GetHistory(x.Path)?.Theme ?? "").ToArray();

        public History[] LoadRecentHistories() => GetSortedHistoryFiles().Take(NUMBER_OF_FILES).Select(x => GetHistory(x.Path)).Where(h => h != null).Cast<History>().ToArray();

        public History? GetHistory(DateTime date) => GetHistory(GetHistoryPath(date));

        private History? GetHistory(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                string json = File.ReadAllText(path);
                var history = JsonConvert.DeserializeObject<History>(json);
                if (history != null) history.Date = JsonFileName2Date(Path.GetFileName(path)) ?? history.Date;
                return history;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Load Error: {path} - {ex.Message}");
                return null;
            }
        }

        // --- 保存・削除系 ---

        public void MakeTodaysFile(string theme) => SaveHistory(GetHistoryPath(DateTime.Today), new History { Theme = theme, Date = DateTime.Today });

        public void SaveDiscovery(Discovery discovery)
        {
            var history = GetHistory(DateTime.Today);
            if (history == null) return;

            var list = history.Discoveries?.ToList() ?? new List<Discovery>();
            list.Add(discovery);
            history.Discoveries = list.ToArray();
            SaveHistory(GetHistoryPath(DateTime.Today), history);
        }

        public void SaveReview(string review, DateTime date)
        {
            var history = GetHistory(date);
            if (history == null) return;
            history.Review = review;
            SaveHistory(GetHistoryPath(date), history);
        }

        private void SaveHistory(string path, History history)
        {
            EnsureDirectoryExists();
            string json = JsonConvert.SerializeObject(history, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        // --- 日付・ファイル名変換 ---

        private DateTime? JsonFileName2Date(string fileName)
        {
            string datePart = fileName.Replace("history_", "").Replace(".json", "");
            return DateTime.TryParseExact(datePart, "yyyy_MM_dd", null, System.Globalization.DateTimeStyles.None, out var date) ? date : null;
        }

        private string Date2JsonFileName(DateTime date) => $"history_{date:yyyy_MM_dd}.json";
    }
}
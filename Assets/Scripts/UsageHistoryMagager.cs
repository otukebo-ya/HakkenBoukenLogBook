using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBath
{
    public class UsageHistoryManager
    {
        public string RecentTheme { get; }
        public History[] Histories { get; }

        public UsageHistoryManager()
        {
            this.Histories = LoadHistories();
        }

        public History[] LoadHistories()
        {
            History[] histories = Array.Empty<History>();
            return histories;
        }

        public DateTime CheckLastDate()
        {
            return DateTime.MinValue;
        }

        public string[] GetRecentTheme()
        {
            string[] theme = null;
            return theme;
        }

        public void SaveHistry()
        {

        }

        public void DeleteDiscovery()
        {

        }

        public History GetLatestHistry()
        {
            History history = null;
            return history;
        }

        public void SaveReview()
        {

        }
    }
}


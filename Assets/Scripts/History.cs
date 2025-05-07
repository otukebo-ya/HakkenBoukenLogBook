using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBath
{
    [System.Serializable]
    public class History
    {
        public DateTime Date;// 履歴の日付
        public string Theme;// その日のテーマ
        public Discovery[] Discoveries;// その日の発見たち
        public string Review;// その日の発見に対するレビュー
    }
}

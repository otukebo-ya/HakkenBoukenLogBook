using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBath
{
    [System.Serializable]
    public class History
    {
        public DateTime Date;
        public string Theme;
        public Discovery[] Discoveries;
        public string Review;
        public bool Reviewed;
    }
}

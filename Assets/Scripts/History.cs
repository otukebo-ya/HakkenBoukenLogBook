using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBath
{
    [System.Serializable]
    public class History
    {
        public DateTime Date;// �����̓��t
        public string Theme;// ���̓��̃e�[�}
        public Discovery[] Discoveries;// ���̓��̔�������
        public string Review;// ���̓��̔����ɑ΂��郌�r���[
    }
}

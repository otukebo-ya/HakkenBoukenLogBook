using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ColorBath{
    public class PopUpWindow : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text _title;
        [SerializeField] private TMPro.TMP_Text _mainText;
        [SerializeField] private TMPro.TMP_Text _errorText;
        private GameObject _textForm;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void PopUp(string title, string mainText, string errorText="")
        {

        }

        public void Close()
        {

        }
    }
}
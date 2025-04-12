using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBath
{
    public class DiscoveryInputForm : MonoBehaviour
    {
        private InputField _inputField;
        public string InputText
        {
            get { return _inputField.text; }
        }

        public Image InputImage;
        [SerializeField] public Button CameraButton;

        void Awake()
        {
            _inputField = GetComponent<InputField>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SaveHistory()
        {

        }
        public Discovery GetInput()
        {
            Discovery discovery = new Discovery();
            return discovery;
        }
        public string GenerateAizuchi()
        {
            return "";
        }
    }
}

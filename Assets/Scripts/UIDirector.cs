using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBath{
    public class UIDirector : MonoBehaviour
    {
        [SerializeField] GameObject PopUpController;
        public static UIDirector Instance;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void ThemeDecideAnimation()
        {

        }

        public void DisplayReviewWindow()
        {

        }
    }
}
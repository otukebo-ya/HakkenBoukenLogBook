using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBath
{
    public class AppManager : MonoBehaviour
    {
        public string theme;
        public DateTime today;
        public static AppManager Instance;

        public AppManager()
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
        IEnumerator Start()
        {
            bool existUserData = CheckUserData();
            if (!existUserData)
            {
                string token = "";
                yield return StartCoroutine(UIDirector.Instance.RequestTokenInput((input) => {
                    token = input;
                }));

                UserData.Token = token;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public bool CheckUserData()
        {
            if (UserData.Token is not null)
            {
                return true;
            }
            return false;
        }

        public void DecideTheme()
        {

        }

        public void Review()
        {

        }
    }
}

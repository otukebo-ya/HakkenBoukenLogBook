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
            bool isInputDone = false;
            string receivedToken = "";
            bool existUseData = CheckUserData();
            if(!existUseData)
            {
                PopUpWindowController.Instance.PopUp(
                    title: "GeminiAPI�̃g�[�N����������܂���",
                    mainText: "�g�[�N������͂��Ă�������",
                    errorText: "",
                    withoutInputField: false,
                    onOk: (inputValue) =>
                    {
                        receivedToken = inputValue;
                        isInputDone = true;
                    }
                );
                yield return new WaitUntil(() => isInputDone);
                UserData.Token = receivedToken;
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

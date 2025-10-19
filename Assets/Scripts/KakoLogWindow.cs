using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ColorBath;

public class KakoLogWindow : MonoBehaviour
{
    [SerializeField] private GameObject LogButton;
    [SerializeField] private GameObject KakoLog;
    [SerializeField] private Button Closebutton;
    [SerializeField] private GameObject KakoLogCanvas;

    void Start(){
        Closebutton.onClick.AddListener(Close);
    }

    public void Activate(){
        gameObject.SetActive(true);
        LineUpLogButtons();
    }

    public void Close(){
        gameObject.SetActive(false);
    }

    public void AllClear(){
        for (int i = KakoLog.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(KakoLog.transform.GetChild(i).gameObject);
        }
    }

    public void LineUpLogButtons(){
        AllClear();

        History[] recentHistories = UsageHistoryManager.Instance.LoadRecentHistories();

        for (int i = 0; i < recentHistories.Length; i++)
        {
            string theme = recentHistories[i].Theme;
            DateTime date = recentHistories[i].Date;
            GameObject btnObj = Instantiate(LogButton, KakoLog.transform);

            Text uiText = btnObj.GetComponentInChildren<Text>();
            if (uiText != null)
            {
                uiText.text = theme + "\n" + date.ToString("yyyy_MM_dd");
            }
            else
            {
                var tmp = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = theme + "\n" + date.ToString("yyyy_MM_dd");
                }
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => {
                    ShowLogCanv(date);
                });
            }
        }
    }

    public void ShowLogCanv(DateTime date){
        KakoLogCanvas.GetComponent<KakoLogCanvas>().Open(date);
    }
}

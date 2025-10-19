using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KakoLogButton : MonoBehaviour
{
    [SerializeField] private KakoLogWindow _kakoLogWindow;
    private Button _button;
    void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(OpenKakoLogWindow);
        }
    }

    void OpenKakoLogWindow()
    {
        if (_kakoLogWindow != null)
        {
            _kakoLogWindow.Activate();
        }
        else
        {
            Debug.LogError("_kakoLogWindow が設定されていません！");
        }
    }
}

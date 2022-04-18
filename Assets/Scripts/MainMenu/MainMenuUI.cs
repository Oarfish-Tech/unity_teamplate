using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    private VisualElement _rootEle;
    public Texture2D _playClickTexture;
    public Texture2D _playTexture;
    public Texture2D _quitClickTexture;
    public Texture2D _qiutTexture;
    public VisualElement RootElement
    {
        get
        {
            if (_rootEle == null)
            {
                _rootEle = GetComponent<UIDocument>().rootVisualElement;
            }
            return _rootEle;
        }
        set
        {
            _rootEle = value;
        }
    }

    private Button _startNewBtn;

    public Button StartNewBtn
    {
        get
        {
            if (_startNewBtn == null)
                _startNewBtn = RootElement.Q<Button>("StartNewBtn");
            return _startNewBtn;
        }
    }

    private Button _quitGameBtn;

    public Button QuitGameBtn
    {
        get
        {
            if (_quitGameBtn == null)
                _quitGameBtn = RootElement.Q<Button>("QuitGameBtn");
            return _quitGameBtn;
        }
    }

    public void QuitGame()
    {
        QuitGameBtn.style.backgroundImage = _quitClickTexture;
        Application.Quit();
    }

    // Start is called before the first frame update
    void Start()
    {
        QuitGameBtn.RegisterCallback<ClickEvent>(ev => QuitGame());
        QuitGameBtn.RegisterCallback<MouseEnterEvent>(ev => QuitGameBtn.style.backgroundImage = _quitClickTexture);
        QuitGameBtn.RegisterCallback<MouseLeaveEvent>(ev => QuitGameBtn.style.backgroundImage = _qiutTexture);
        StartNewBtn.RegisterCallback<ClickEvent>(ev => PlayGame());
        StartNewBtn.RegisterCallback<MouseEnterEvent>(ev => StartNewBtn.style.backgroundImage = _playClickTexture);
        StartNewBtn.RegisterCallback<MouseLeaveEvent>(ev => StartNewBtn.style.backgroundImage = _playTexture);
    }

    private void PlayGame()
    {
        StartNewBtn.style.backgroundImage= _playClickTexture;
        SceneManager.LoadScene("BattleScene");
    }
}

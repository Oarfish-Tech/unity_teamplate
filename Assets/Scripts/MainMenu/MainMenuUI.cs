using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    private VisualElement _rootEle;
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
        Application.Quit();
    }

    // Start is called before the first frame update
    void Start()
    {
        QuitGameBtn.RegisterCallback<ClickEvent>(ev => QuitGame());
        StartNewBtn.RegisterCallback<ClickEvent>(ev => SceneManager.LoadScene("BattleScene"));
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;

public enum BattleState { START, QUESTION, MESSAGE, WON, LOST }
public class BattleSystem : MonoBehaviour
{
    private int _questionIndex = 0;
    private bool _newQuestion = true;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    private GameObject _enemyGO;

    public GameObject[] enemiesArray;
    public GameObject[] background;

    public Transform _Dynamic;
    public Transform battleStation;

    Unit playerUnit;
    Unit enemyUnit;

    public UIDocument battleHudDoc;
    public BattleHud battleHud;
    public QuestionBuilder questionBuilder;
    VisualElement rootHudEle;
    VisualElement controlsContainer;

    public BattleState state;
    private int _currentEnemyIndex = 0;
    private string _currentMessage = "";

    private void AnswerEvent(ChangeEvent<int> ev)
    {
        OnAnswer(questionBuilder.GetQuestions().records[_questionIndex].answerIndex == ev.newValue);
    }
    public void SetupQuestion(QuestionBuilder.Record question)
    {
        if (_newQuestion)
        {
            battleHud.DisplayQuestion(question);
            _newQuestion = false;
        }
        var radioGroup = battleHud.BaseUI.RootElement.Q<RadioButtonGroup>("QuestionRadioGroup");
        radioGroup.RegisterValueChangedCallback(AnswerEvent);
    }

    // UIElement CallBack functions
    public void OnAnswer(bool isCorrect)
    {
        if (state != BattleState.QUESTION)
            return;

        if (isCorrect)
        {
            PlayerAttack();
        }
        else
            EnemyAttack();
    }

    private void OnResetBtn()
    {
        SceneManager.LoadScene("BattleScene");
    }

    private void OnQuitBtn()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    void Start()
    {
        //set inital state
        state = BattleState.START;

        //get the root UIElement
        rootHudEle = battleHudDoc.rootVisualElement;
        controlsContainer = rootHudEle.Q<VisualElement>("ControlsContainer");

        var resetBtn = controlsContainer.Q<Button>("ResetBtn");
        resetBtn.style.display = DisplayStyle.None;
        resetBtn.RegisterCallback<ClickEvent>(ev => OnResetBtn());

        var quitBtn = controlsContainer.Q<Button>("QuitBtn");
        quitBtn.style.display = DisplayStyle.None;
        quitBtn.RegisterCallback<ClickEvent>(ev => OnQuitBtn());

        var playerGO = Instantiate(playerPrefab, _Dynamic);
        playerUnit = playerGO.GetComponent<Unit>();

        BattleFsm();
    }

    void DismissMessage(ClickEvent ev)
    {
        state = BattleState.QUESTION;
        battleHud.BaseUI.RootElement.Q<VisualElement>("Background").UnregisterCallback<ClickEvent>(DismissMessage);
        LoadEnemy();
        BattleFsm();
    }

    void BattleFsm()
    {
        switch (state)
        {
            case BattleState.START:
                SetupBattle();
                break;

            case BattleState.QUESTION:
                PlayerTurn();
                break;

            case BattleState.MESSAGE:
                battleHud.SetDialogueText(_currentMessage);
                battleHud.BaseUI.RootElement.Q<VisualElement>("Background").RegisterCallback<ClickEvent>(DismissMessage);
                break;

            case BattleState.WON:
                WinBattle();
                return;

            case BattleState.LOST:
                LoseBattle();
                return;
            default:
                break;
        }
    }

    //Sets up the enemy and player UIs
    void SetupBattle()
    {
        if (_currentMessage == "")
            _currentMessage = $"Monsters have challenged you to learn math!\n\nAnswer questions correctly to attack.\nIncorrect answers will cause the enemey to attack.\n\n Press anywhere to begin...";
        
        battleHud.SetUnitHud(playerUnit);
        state = BattleState.MESSAGE;
        BattleFsm();
    }

    private void LoadEnemy()
    {
        enemyPrefab = enemiesArray[_currentEnemyIndex];
        _enemyGO = Instantiate(enemyPrefab, battleStation);
        enemyUnit = _enemyGO.GetComponent<Unit>();
        enemyUnit.unitLevel = _currentEnemyIndex + 1;
        rootHudEle = battleHudDoc.rootVisualElement;        
        battleHud.SetUnitHud(enemyUnit);
    }

    private void PlayerTurn()
    {
        SetupQuestion(questionBuilder.GetQuestions().records[_questionIndex]);
    }

    //Player and Enemy Attacks, each update the UI and call the BattleFSM
    private void PlayerAttack()
    {
        var isDead = enemyUnit.TakeDamage(playerUnit.damage);
        StartCoroutine(enemyUnit.DamageAnimation());
        battleHud.SetUnitHp(enemyUnit);
        _newQuestion = true;
        _questionIndex++;

        if (isDead)
            state = BattleState.WON;
        else
            state = BattleState.QUESTION;

        BattleFsm();
    }

    private void EnemyAttack()
    {
        var isDead = playerUnit.TakeDamage(enemyUnit.damage);
        StartCoroutine(enemyUnit.AttackAnimation());
        battleHud.SetUnitHp(playerUnit);

        if (isDead)
            state = BattleState.LOST;
        else
            state = BattleState.QUESTION;

        BattleFsm();
    }

    //Win lose methods
    private void LoseBattle()
    {
        battleHud.SetDialogueText($"{enemyUnit.unitName} has defeated you!\n Game Over");
    }

    private void WinBattle()
    {
        if (_currentEnemyIndex >= enemiesArray.Length - 1)
        {
            battleHud.SetDialogueText("Yay! You win!");
            battleHud.ShowResetBtn(OnResetBtn);
            battleHud.ShowQuitBtn(OnQuitBtn);
        }
        else
        {
            Destroy(_enemyGO);
            _currentMessage = $"You have defeated an enemy!\nGet ready for the next one!\n\nPress anywhere to begin...";
            _currentEnemyIndex++;
            if (_currentEnemyIndex > 2 && _currentEnemyIndex <= 5)
            {
                background[0].SetActive(false);
                background[1].SetActive(true);
                background[2].SetActive(false);
            } 
            if (_currentEnemyIndex > 5)
            {
                background[0].SetActive(false);
                background[1].SetActive(false);
                background[2].SetActive(true);
            }
            state = BattleState.MESSAGE;
            BattleFsm();
        }
    }
}

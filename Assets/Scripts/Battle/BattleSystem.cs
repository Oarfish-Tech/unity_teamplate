using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }
public class BattleSystem : MonoBehaviour
{
    private const float DIALOGUE_WAIT = 3f;
    private int _questionIndex = 0;

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
    VisualElement playerContainer;
    VisualElement enemyContainer;
    VisualElement controlsContainer;

    public BattleState state;
    private int _currentEnemyIndex = 0;

    private void AnswerEvent(ChangeEvent<int> ev)
    {
        OnAnswer(questionBuilder.GetQuestions().records[_questionIndex].answerIndex == ev.newValue);
    }
    public void SetupQuestion(QuestionBuilder.Record question)
    {
        var radioGroup = battleHud.BaseUI.RootElement.Q<RadioButtonGroup>("QuestionRadioGroup");
        battleHud.DisplayQuestion(question);
        radioGroup.RegisterValueChangedCallback(AnswerEvent);
    }

    // UIElement CallBack functions
    public void OnAnswer(bool isCorrect)
    {
        if (state != BattleState.PLAYERTURN)
            return;

        if (isCorrect)
        {
            StartCoroutine(PlayerAttack());
            _questionIndex++;
            SetupQuestion(questionBuilder.GetQuestions().records[_questionIndex]);
        }
        else
            StartCoroutine(EnemyAttack());
    }

    private void OnHealBtn()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerHeal());
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

        //set callbacks for the UI buttons
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

    void BattleFsm()
    {
        switch (state)
        {
            case BattleState.START:
                SetupBattle();
                break;

            case BattleState.PLAYERTURN:
                PlayerTurn();
                break;

            case BattleState.ENEMYTURN:
                EnemyTurn();
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

    private IEnumerator WriteDialog(string message)
    {
        battleHud.SetDialogueText(message);
        yield return new WaitForSeconds(DIALOGUE_WAIT);
    }

    //Sets up the enemy and player UIs
    void SetupBattle()
    {
        enemyPrefab = enemiesArray[_currentEnemyIndex];
        _enemyGO = Instantiate(enemyPrefab, battleStation);
        enemyUnit = _enemyGO.GetComponent<Unit>();
        enemyUnit.unitLevel = _currentEnemyIndex + 1;

        rootHudEle = battleHudDoc.rootVisualElement;
        battleHud.SetUnitHud(playerUnit);
        battleHud.SetUnitHud(enemyUnit);
        var questions = questionBuilder.GetQuestions().records;

        WriteDialog($"{enemyUnit.unitName} challenges you to learn math!\n\nAnswer questions correctly to attack.\nIncorrect answers will cause the enemey to attack.");

        SetupQuestion(questions[_questionIndex]);

        state = BattleState.PLAYERTURN;
        BattleFsm();
    }

    //Enemy and player turns
    private void EnemyTurn()
    {
        StartCoroutine(EnemyAttack());
    }

    private void PlayerTurn()
    {
        
    }

    //Player and Enemy Attacks, each update the UI and call the BattleFSM
    private IEnumerator PlayerAttack()
    {
        //battleHud.SetDialogueText($"You deal {playerUnit.damage} damage to {enemyUnit.unitName}");
        var previousHp = enemyUnit.currentHP;
        var isDead = enemyUnit.TakeDamage(playerUnit.damage);
        StartCoroutine(enemyUnit.DamageAnimation());
        battleHud.SetUnitHp(enemyUnit);

        if (isDead)
            state = BattleState.WON;
        else
            state = BattleState.PLAYERTURN;

        yield return new WaitForSeconds(DIALOGUE_WAIT);

        BattleFsm();
    }

    private IEnumerator PlayerHeal()
    {
        const int healAmount = 2; //hardcoded heal for right now, will change later.
        var previousHp = playerUnit.currentHP;
        playerUnit.Heal(healAmount);
        battleHud.SetUnitHp(playerUnit);
        //battleHud.SetDialogueText($"You heal for {healAmount} points");

        state = BattleState.ENEMYTURN;

        yield return new WaitForSeconds(DIALOGUE_WAIT);

        BattleFsm();
    }

    private IEnumerator EnemyAttack()
    {
        //battleHud.SetDialogueText($"{enemyUnit.unitName} uses Chomp!\n You take {enemyUnit.damage} damage.");
        var previousHp = playerUnit.currentHP;
        var isDead = playerUnit.TakeDamage(enemyUnit.damage);
        StartCoroutine(enemyUnit.AttackAnimation());
        battleHud.SetUnitHp(playerUnit);

        if (isDead)
            state = BattleState.LOST;
        else
            state = BattleState.PLAYERTURN;

        yield return new WaitForSeconds(DIALOGUE_WAIT);

        BattleFsm();
    }

    private IEnumerator OnEnemyDefeat()
    {
        battleHud.SetDialogueText($"You have defeated an enemy!\n\nGet ready for the next one!");
        yield return new WaitForSeconds(DIALOGUE_WAIT);
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
            StartCoroutine(OnEnemyDefeat());
            _currentEnemyIndex++;
            if (_currentEnemyIndex > 2)
            {
                background[0].SetActive(false);
                background[1].SetActive(true);
                background[2].SetActive(false);
            } else if (_currentEnemyIndex > 5)
            {
                background[0].SetActive(false);
                background[1].SetActive(false);
                background[2].SetActive(true);
            }
            state = BattleState.START;
            BattleFsm();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System.Linq;

public class BattleHud : MonoBehaviour
{
    private BaseUI _baseUI;
    private const float DIALOGUE_WAIT = 1f;
    private RadioButtonGroup _radioButtonGroup;
    
    public BaseUI BaseUI
    {
        get
        {
            if (_baseUI == null)
                _baseUI = GetComponent<BaseUI>(); ;

            return _baseUI;
        }
    }

    public RadioButtonGroup RadioButtonGroup
    {
        get
        {
            if (_radioButtonGroup == null)
                _radioButtonGroup = BaseUI.RootElement.Q<RadioButtonGroup>("QuestionRadioGroup");
            return _radioButtonGroup;
        }
    }

    public void SetUnitHud(Unit unit)
    {
        var unitContainer = BaseUI.RootElement.Q<VisualElement>(unit.unitUIContainer);
        unitContainer.Q<Label>("UnitName").text = unit.unitName;
        unitContainer.Q<Label>("UnitLevel").text = $"Lv. {unit.unitLevel}";

        SetUnitHp(unitContainer, unit);
    }
    private Length GetHpPercent(int currentHP, int maxHP)
    {
        float hpPercent = ((float)currentHP / (float)maxHP) * 100f;
        return Length.Percent(hpPercent);
    }

    private void SetUnitHp(VisualElement unitContainer, Unit unit)
    {
        // StartCoroutine(HpDamageAnimation(unit, previousHp));
        unitContainer.Q<VisualElement>("UnitHpFill").style.width = GetHpPercent(unit.currentHP, unit.maxHP);
    }

    public void SetUnitHp(Unit unit)
    {
        var unitContainer = BaseUI.RootElement.Q<VisualElement>(unit.unitUIContainer);
        SetUnitHp(unitContainer, unit);
    }

    public void SetDialogueText(string text)
    {
        RadioButtonGroup.parent.style.display = DisplayStyle.None;
        var dialogBox = BaseUI.ControlContainer.Q<Label>("DialogueText");
        dialogBox.text = text;
        dialogBox.style.display = DisplayStyle.Flex;
    }

    public void ShowResetBtn(Action onResetBtn)
    {
        var resetBtn = BaseUI.ControlContainer.Q<Button>("ResetBtn");
        resetBtn.style.display = DisplayStyle.Flex;

        resetBtn.RegisterCallback<ClickEvent>(ev => onResetBtn());
    }

    public void ShowQuitBtn(Action OnQuitBtn)
    {
        var quitBtn = BaseUI.ControlContainer.Q<Button>("QuitBtn");
        quitBtn.style.display = DisplayStyle.Flex;
        quitBtn.RegisterCallback<ClickEvent>(ev => OnQuitBtn());
    }

    public void DisplayQuestion(QuestionBuilder.Record record)
    {
        var radioParent = RadioButtonGroup.parent;
        BaseUI.ControlContainer.Q<Label>("DialogueText").style.display = DisplayStyle.None;
        RadioButtonGroup.label = $"{record.question}:";
        RadioButtonGroup.parent.Remove(RadioButtonGroup);
        RadioButtonGroup.value = -1;
        radioParent.Add(RadioButtonGroup);
        RadioButtonGroup.choices = record.answers;
        RadioButtonGroup.parent.style.display = DisplayStyle.Flex;
        RadioButtonGroup.SetEnabled(true);
    }
}

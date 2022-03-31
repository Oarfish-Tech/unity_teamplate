using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Unit : MonoBehaviour
{
	public string unitName;
	public int unitLevel;

	public int damage;

	public int maxHP;
	public int currentHP;

	public string unitUIContainer;

    public bool TakeDamage(int dmg)
	{
		currentHP -= dmg;

		if (currentHP <= 0)
			return true;
		else
			return false;
	}

	public void Heal(int amount)
	{
		currentHP += amount;
		if (currentHP > maxHP)
			currentHP = maxHP;
	}

	public IEnumerator DamageAnimation() {
		transform.localScale = Vector3.one;
		Tween damageAni = transform.DOPunchScale(new Vector3(-0.75f, -0.25f), 0.25f, 10, 1);
		yield return damageAni.WaitForCompletion();
	}

	public IEnumerator AttackAnimation() {
		transform.localScale = Vector3.one;
		var attackAni = transform.DOPunchScale(new Vector3(0.75f, 0.75f), 0.5f, 0, 0);
		yield return attackAni.WaitForCompletion();
	}

}

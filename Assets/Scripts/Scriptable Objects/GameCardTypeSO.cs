using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Wizard,
    Warrior,
    Villager
}

[CreateAssetMenu(fileName = "GameCardTypeSO", menuName = "ScriptableObjects/GameCardTypeSO", order = 1)]
public class GameCardTypeSO : ScriptableObject
{
    public Sprite cardFrontSprite;
    public CardType cardType;

}

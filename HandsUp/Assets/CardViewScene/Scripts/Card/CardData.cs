using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/cardData", order = 1)]
public class CardData : ScriptableObject
{
    public int user_id;
    public int card_id;
    public int category_id;
    public string name;
    public string img_path;
    public bool card_is_built_in;
    public bool is_new;
}

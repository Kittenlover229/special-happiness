using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Debug only
// Emplaces the cards into the player's hand
[RequireComponent(typeof(CardsController))]
public class DebugDeckFiller : MonoBehaviour
{
    CardsController target;
    public List<CardDescriptor> cards = new();

    void Start()
    {
        this.target = GetComponent<CardsController>();

        foreach (CardDescriptor card in cards)
        {
            target.AddCard(card);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class IdleState : PassiveState
{
    Card HoveredCard = null;

    public IdleState(CardsController controller) : base(controller) { }

    bool isHovered(Card card) => card == HoveredCard;

    protected override Vector2 CalculatePositionForCard(int index, int total, Card card)
    {
        return
            base.CalculatePositionForCard(index, total, card)
                + (isHovered(card)
                        ? Vector2.up * Controller.Parameters.HoverUp
                        : Vector2.zero);
    }

    protected override Quaternion CalculateRotationForCard(int index, int total, Card card)
    {
        return isHovered(card)
            ? Quaternion.identity
            : base.CalculateRotationForCard(index, total, card);
    }

    protected override ICardControllerState CardPreUpdate(Card card, int activeCardCount, int idx)
    {
        if (Input.GetMouseButtonDown(0) && HoveredCard != null)
            return new CardSelectedState(Controller, HoveredCard);
        else
            return this;
    }

    public override void NotifyEnterCard(Card card)
    {
        HoveredCard = card;
        card.transform.SetAsLastSibling();
    }

    public override void NotifyExitCard(Card card)
    {
        HoveredCard = null;
        for (int i = 0; i < this.Controller.Cards.Count; i++)
            this.Controller.Cards[i].transform.SetSiblingIndex(i);
    }
}

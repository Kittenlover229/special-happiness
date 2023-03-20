using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardControllerState
{
    abstract ICardControllerState Update();
    abstract void NotifyEnterCard(Card card);
    abstract void NotifyExitCard(Card card);
}

[System.Serializable]
public class CardAnimationParameters
{
    // So this is UI, so position of the cards on the screen is defined rather 
    // weirdly. The screen is separated into .y equally sized horizontal 
    // stripes and .x defines at the bottom of which stripe the cards are
    public Vector2Int CardOffsetFractionOfTheScreen;
    public float CardSpread;
    public float PerCardRotation;
    public AnimationCurve CardArcCurve;
    public float CardArcLift;
    public float CardMovementSmoothness;
    public float CardRotationSpeed;
    public float HoverUp;
    public float CardPrefferedDistance;
}

class IdleState : ICardControllerState
{
    CardsController Controller;
    Card HoveredCard = null;

    public IdleState(CardsController controller)
    {
        this.Controller = controller;
    }

    public ICardControllerState Update()
    {
        var parameters = Controller.Parameters;
        int inactiveCards = Controller.Cards.FindAll((card) => !card.gameObject.activeInHierarchy).Count;
        int inactiveCardsEncountered = 0;
        int activeCardCount = Controller.Cards.Count - 1 - inactiveCards;

        for (int i = 0; i < Controller.Cards.Count; i++)
        {
            Card card = Controller.Cards[i];
            if (!card.gameObject.activeInHierarchy)
            {
                inactiveCardsEncountered += 1;
                continue;
            }

            i -= inactiveCardsEncountered;
            int activeCardIdx = i - inactiveCardsEncountered;

            Rect canvasRect = Controller.Canvas.pixelRect;

            bool isCardHovered = HoveredCard == card;
            float horizontalSpread = Mathf.Min(parameters.CardSpread / activeCardCount, parameters.CardPrefferedDistance);
            float maxHorizontalSpread = horizontalSpread * activeCardCount;
            float cardHorizontalSpread = activeCardIdx * horizontalSpread;
            float horizontalCenter = maxHorizontalSpread / 2;
            float verticalArcNormalized = parameters.CardArcCurve.Evaluate(Mathf.Abs(1 - activeCardIdx / (float)(activeCardCount != 0 ? activeCardCount : 1) * 2));
            float verticalArc = verticalArcNormalized * parameters.CardArcLift;
            float vertialOffset = canvasRect.height / parameters.CardOffsetFractionOfTheScreen.y * parameters.CardOffsetFractionOfTheScreen.x;
            float cardHover = (isCardHovered ? parameters.HoverUp : 0);

            Vector3 newPosition = new Vector2(canvasRect.center.x, 0)
                + Vector2.up * vertialOffset
                + Vector2.right * cardHorizontalSpread
                - Vector2.right * horizontalCenter
                - Vector2.up * verticalArc
                + Vector2.up * cardHover;

            card.transform.position =
                Vector3.Lerp(
                    card.transform.position,
                    newPosition,
                    Time.deltaTime * parameters.CardMovementSmoothness);

            var rotation = isCardHovered ? Quaternion.identity :
                Quaternion.Euler(
                    0, 0,
                    (i - (float)activeCardCount / 2) * parameters.PerCardRotation);

            card.transform.rotation = Quaternion.Lerp(card.transform.rotation, rotation, Time.deltaTime * parameters.CardRotationSpeed);
        }

        return this;
    }

    public void NotifyEnterCard(Card card)
    {
        HoveredCard = card;
        card.transform.SetAsLastSibling();
    }

    public void NotifyExitCard(Card card)
    {
        HoveredCard = null;
        for (int i = 0; i < this.Controller.Cards.Count; i++)
        {
            this.Controller.Cards[i].transform.SetSiblingIndex(i);
        }
    }
}

[ExecuteInEditMode]
public class CardsController : MonoBehaviour
{
    public Canvas Canvas;
    public CardAnimationParameters Parameters;
    public List<Card> Cards = new List<Card>();
    public ICardControllerState State = null;

    void Awake()
    {
        State = new IdleState(this);
    }

    void Update()
    {
        if (State == null)
            State = new IdleState(this);
        State = State.Update();
    }
}

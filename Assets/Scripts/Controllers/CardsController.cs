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

    private Vector2 CalculatePositionForCard(int index, int total, Card card)
    {
        var parameters = Controller.Parameters;
        Rect canvasRect = Controller.Canvas.pixelRect;
        bool isCardHovered = HoveredCard == card;

        float horizontalSpread = Mathf.Min(parameters.CardSpread / total, parameters.CardPrefferedDistance);
        float maxHorizontalSpread = horizontalSpread * total;
        float cardHorizontalSpread = index * horizontalSpread;
        float horizontalSpreadCenter = maxHorizontalSpread / 2;
        float verticalArcNormalized = parameters.CardArcCurve.Evaluate(Mathf.Abs(1 - index / (float)(total != 0 ? total : 1) * 2));
        float verticalArc = verticalArcNormalized * parameters.CardArcLift;
        float vertialOffset = canvasRect.height / parameters.CardOffsetFractionOfTheScreen.y * parameters.CardOffsetFractionOfTheScreen.x;
        float cardHover = (isCardHovered ? parameters.HoverUp : 0);

        Vector3 newPosition = new Vector2(canvasRect.center.x, 0)
            + Vector2.up * vertialOffset
            + Vector2.right * cardHorizontalSpread
            - Vector2.right * horizontalSpreadCenter
            - Vector2.up * verticalArc
            + Vector2.up * cardHover
            ;

        return newPosition;
    }

    private Quaternion CalculateRotationForCard(int index, int total, Card card)
    {
        bool isCardHovered = HoveredCard == card;
        return isCardHovered ? Quaternion.identity :
            Quaternion.Euler(
                0, 0,
                (index - (float)total / 2) * Controller.Parameters.PerCardRotation);

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
            int activeCardIdx = i - inactiveCardsEncountered;

            var position = CalculatePositionForCard(activeCardIdx, activeCardCount, card);
            card.transform.position =
                Vector3.Lerp(
                    card.transform.position,
                    position,
                    Time.deltaTime * parameters.CardMovementSmoothness);

            var rotation = CalculateRotationForCard(activeCardIdx, activeCardCount, card);
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
            this.Controller.Cards[i].transform.SetSiblingIndex(i);
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

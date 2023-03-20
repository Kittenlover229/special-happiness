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
    public float MaxCardRotation;
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
        var cardCount = Controller.Cards.Count;
        var cards = Controller.Cards;

        int inactiveCards = cards.FindAll((card) => !card.gameObject.activeInHierarchy).Count;

        // TODO: center this bullshit on the screen
        int inactiveCardsEncountered = 0;
        int iters = cardCount - 1 - inactiveCards;
        for (int i = 0; i < cardCount; i++)
        {
            if (!cards[i].gameObject.activeInHierarchy) {
                inactiveCardsEncountered += 1;
                continue;
            }

            Card card = cards[i];
            bool isCardHovered = HoveredCard == card;
            i -= inactiveCardsEncountered;

            float horizontalSpread = Mathf.Min(parameters.CardSpread / iters, parameters.CardPrefferedDistance);
            float maxHorizontalSpread = horizontalSpread * iters;

            Rect canvasRect = Controller.Canvas.pixelRect;
            Vector3 newPosition = new Vector2(canvasRect.center.x, 0)
                + Vector2.up * canvasRect.height / parameters.CardOffsetFractionOfTheScreen.y * parameters.CardOffsetFractionOfTheScreen.x
                + Vector2.right * (i * horizontalSpread - maxHorizontalSpread / 2)
                - Vector2.up * (parameters.CardArcCurve.Evaluate(Mathf.Abs(1 - i / (float)(iters != 0 ? iters : 1) * 2)) - 1) * parameters.CardArcLift
                + (isCardHovered ? (Vector2.up * parameters.HoverUp) : Vector3.zero);


            card.transform.position =
                Vector3.Lerp(
                    card.transform.position,
                    newPosition,
                    Time.deltaTime * parameters.CardMovementSmoothness);

            var rotation = isCardHovered ? Quaternion.identity :
                Quaternion.Euler(
                    0, 0,
                    (i - (float)iters / 2) * parameters.MaxCardRotation);

            card.transform.rotation = Quaternion.Lerp(card.transform.rotation, rotation, Time.deltaTime * parameters.CardRotationSpeed);
            i += inactiveCardsEncountered;
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

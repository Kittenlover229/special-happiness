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
    public Vector2Int CardOffsetFractionOfTheScreen;
    public Vector2 CardSpread;
    public float maxCardRotation;
    public AnimationCurve cardArcCurve;
    public float cardArcLift;
    public float cardMovementSmoothness;
    public float CardRotationSpeed;
    public float HoverUp;
}

class IdleState : ICardControllerState
{
    CardsController controller;
    List<Card> hoveredCards = new();

    public IdleState(CardsController controller)
    {
        this.controller = controller;
    }

    public ICardControllerState Update()
    {
        var parameters = controller.Parameters;
        var cardCount = controller.Cards.Count;
        var cards = controller.Cards;

        // TODO: center this bullshit on the screen
        float iters = cardCount - 1;
        for (int i = 0; i < cardCount; i++)
        {
            bool isCardHovered = hoveredCards.Contains(cards[i]);

            Rect canvasRect = controller.Canvas.pixelRect;
            Vector3 newPosition = new Vector2(canvasRect.center.x, 0)
                + Vector2.up * canvasRect.height / parameters.CardOffsetFractionOfTheScreen.y * parameters.CardOffsetFractionOfTheScreen.x
                + i * (parameters.CardSpread / iters)
                - parameters.CardSpread / 2
                - Vector2.up * (parameters.cardArcCurve.Evaluate(Mathf.Abs(1 - i / iters * 2)) - 1) * parameters.cardArcLift
                + (isCardHovered ? (Vector2.up * parameters.HoverUp) : Vector3.zero);


            cards[i].transform.position =
                Vector3.Lerp(
                    cards[i].transform.position,
                    newPosition,
                    Time.deltaTime * parameters.cardMovementSmoothness);

            var rotation = hoveredCards.Contains(cards[i]) ? Quaternion.identity :
                Quaternion.Euler(
                    0, 0,
                    ((float)i - cardCount / 2) * parameters.maxCardRotation);

            cards[i].transform.rotation = Quaternion.Lerp(cards[i].transform.rotation, rotation, Time.deltaTime * parameters.CardRotationSpeed);
        }

        return this;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = controller.Canvas.transform.localToWorldMatrix;
        foreach (Card card in hoveredCards)
        {
            Gizmos.DrawSphere(card.rect.anchorMin, 0.5f);
        }
    }

    public void NotifyEnterCard(Card card)
    {
        hoveredCards.Add(card);
        card.transform.SetAsLastSibling();
    }

    public void NotifyExitCard(Card card)
    {
        hoveredCards.Remove(card);
        // Reorder the children of the component
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

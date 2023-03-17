using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ICardControllerState
{
    abstract ICardControllerState Update();
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
}

class IdleState : ICardControllerState
{
    CardsController controller;

    public IdleState(CardsController controller) {
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
            Rect canvasRect = controller.Canvas.pixelRect;
            Vector3 newPosition = new Vector2(canvasRect.center.x, 0) 
                + Vector2.up * canvasRect.height / parameters.CardOffsetFractionOfTheScreen.y * parameters.CardOffsetFractionOfTheScreen.x
                + i * (parameters.CardSpread / iters)
                - parameters.CardSpread / 2
                - Vector2.up * (parameters.cardArcCurve.Evaluate(Mathf.Abs(1 - i / iters * 2)) - 1) * parameters.cardArcLift;

            cards[i].transform.position =
                Vector3.Lerp(
                    cards[i].transform.position,
                    newPosition,
                    Time.deltaTime * parameters.cardMovementSmoothness);

            Quaternion newRotation =
                Quaternion.Euler(
                    0, 0,
                    ((float)i - cardCount / 2) * parameters.maxCardRotation);

            cards[i].transform.rotation = newRotation;
        }

        return this;
    }

    void OnDrawGizmos() {
        Gizmos.matrix = controller.Canvas.transform.localToWorldMatrix;
    }
}

class CardSelectedState : ICardControllerState
{
    public ICardControllerState Update()
    {
        return this;
    }
}

[ExecuteInEditMode]
public class CardsController : MonoBehaviour
{
    public Canvas Canvas;
    public CardAnimationParameters Parameters;
    public List<Card> Cards = new List<Card>();

    ICardControllerState State = null;

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

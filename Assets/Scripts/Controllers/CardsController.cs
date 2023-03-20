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
    public float VerticalDownOffsetWhenSelected;
}

class PassiveState : ICardControllerState
{
    public PassiveState(CardsController controller)
    {
        this.Controller = controller;
    }

    public CardsController Controller { get; }

    public virtual void NotifyEnterCard(Card card) { }
    public virtual void NotifyExitCard(Card card) { }

    protected virtual Vector2 CalculatePositionForCard(int index, int total, Card card)
    {
        var parameters = Controller.Parameters;
        Rect canvasRect = Controller.Canvas.pixelRect;

        float horizontalSpread = Mathf.Min(parameters.CardSpread / total, parameters.CardPrefferedDistance);
        float maxHorizontalSpread = horizontalSpread * total;
        float cardHorizontalSpread = index * horizontalSpread;
        float horizontalSpreadCenter = maxHorizontalSpread / 2;
        float verticalArcNormalized = parameters.CardArcCurve.Evaluate(Mathf.Abs(1 - index / (float)(total != 0 ? total : 1) * 2));
        float verticalArc = verticalArcNormalized * parameters.CardArcLift;
        float vertialOffset = canvasRect.height / parameters.CardOffsetFractionOfTheScreen.y * parameters.CardOffsetFractionOfTheScreen.x;

        Vector3 newPosition = new Vector2(canvasRect.center.x, 0)
            + Vector2.up * vertialOffset
            + Vector2.right * cardHorizontalSpread
            - Vector2.right * horizontalSpreadCenter
            - Vector2.up * verticalArc
            ;

        return newPosition;
    }

    protected virtual Quaternion CalculateRotationForCard(int index, int total, Card card)
    {
        return Quaternion.Euler(
                        0, 0,
                        (index - (float)total / 2) * Controller.Parameters.PerCardRotation);

    }

    protected virtual ICardControllerState CardPreUpdate(
        ref bool cancel, Card card, int activeCardCount, int idx) => this;

    public ICardControllerState Update()
    {
        var parameters = Controller.Parameters;
        int skippedUpdateCount = Controller.Cards.FindAll((card) => !card.gameObject.activeInHierarchy).Count;
        int updatesSkipped = 0;
        int activeCardCount = Controller.Cards.Count - 1 - skippedUpdateCount;

        for (int i = 0; i < Controller.Cards.Count; i++)
        {
            Card card = Controller.Cards[i];


            if (!card.gameObject.activeInHierarchy)
            {
                updatesSkipped++;
                continue;
            }
            int activeCardIdx = i - updatesSkipped;

            bool interrupted = false;
            var nextState = CardPreUpdate(ref interrupted, card, activeCardCount, activeCardIdx);
            if (nextState != this)
                return nextState;

            if (interrupted)
            {
                skippedUpdateCount++;
                continue;
            }

            var position = CalculatePositionForCard(activeCardIdx, activeCardCount, card);
            card.transform.position =
                Vector3.Lerp(
                    card.transform.position,
                    position,
                    Time.deltaTime * parameters.CardMovementSmoothness);

            var rotation = CalculateRotationForCard(activeCardIdx, activeCardCount, card);
            card.transform.rotation = Quaternion.Lerp(card.transform.rotation, rotation, Time.deltaTime * parameters.CardRotationSpeed);
        }

        // XXX: debug only
        if (Input.GetKeyDown(KeyCode.K))
            return new IdleState(Controller);

        return this;
    }
}

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

    protected override ICardControllerState CardPreUpdate(ref bool cancel, Card card, int activeCardCount, int idx)
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

class CardSelectedState : PassiveState
{
    public Card selected;

    public CardSelectedState(CardsController controller, Card selected) : base(controller)
    {
        this.selected = selected;
    }

    protected Vector2 CalculatePositionForSelectedCard()
    {
        var rect = Controller.Canvas.pixelRect;
        return new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
    }

    protected override ICardControllerState CardPreUpdate(ref bool cancel, Card card, int activeCardCount, int idx)
    {
        if (Input.GetMouseButtonDown(1))
            return new IdleState(Controller);
        if (Input.GetKeyDown(KeyCode.K) && card == selected) {
            Controller.Cards.Remove(card);
            UnityEngine.Object.Destroy(card.gameObject);
            return new IdleState(Controller);
        }
        else
            return this;
    }

    protected override Vector2 CalculatePositionForCard(int index, int total, Card card)
    {
        bool isSelected = card == selected;
        return isSelected
            ? CalculatePositionForSelectedCard()
            : base.CalculatePositionForCard(index, total, card)
                + (Vector2.down * Controller.Parameters.VerticalDownOffsetWhenSelected);
    }

    protected override Quaternion CalculateRotationForCard(int index, int total, Card card)
    {
        bool isSelected = card == selected;
        return isSelected
            ? Quaternion.identity 
            : base.CalculateRotationForCard(index, total, card);
    }
}

[ExecuteInEditMode]
public class CardsController : MonoBehaviour
{
    public Canvas Canvas;
    public CardAnimationParameters Parameters;
    public List<Card> Cards = new List<Card>();
    public ICardControllerState State = null;

    void Update()
    {
        if (State == null)
            State = new PassiveState(this);
        State = State.Update();
    }
}

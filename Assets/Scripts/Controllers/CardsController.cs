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
    [Header("Passive")]
    // So this is UI, so position of the cards on the screen is defined rather 
    // weirdly. The screen is separated into .y equally sized horizontal 
    // stripes and .x defines at the bottom of which stripe the cards are
    public Vector2Int CardOffsetFractionOfTheScreen;
    [Tooltip("Controls which distance apart the cards prefer to keep. If there are too many cards they are cramped according to Card Spread")]
    public float CardPrefferedDistance;
    [Tooltip("Maximum horizontal amount in pixels that the cards can span")]
    public float CardSpread;
    [Tooltip("The card is rotated dy this menu degrees the further it is from the center")]
    public float PerCardRotation;
    [Tooltip("Curve which controls how the cards are held in the hand, reflected at y = 1")]
    public AnimationCurve CardArcCurve;
    [Tooltip("Reflects the vertical offset of each card according to the curve, 0 at the edges and 1 * this at center")]
    public float CardArcLift;

    [Tooltip("Controls how fast the cards translates around")]
    public float CardMovementSmoothness;
    [Tooltip("Controls how fast the cards rotate")]
    public float CardRotationSpeed;
    [Tooltip("The amount by which the cards are shifted down when inactive")]
    public float VerticalDownOffsetWhenInactive;

    [Header("Idle")]
    public float HoverUp;

    [Header("Selected")]
    [Tooltip("Offset of the selected card from center. The topleft is (-0.5, -0.5) and the top right is (0.5, 0.5)")]
    public Vector2 CardSelectedOffset;
}

class PassiveState : ICardControllerState
{
    protected bool pullCardsUpOnHover = true;

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
        var isInLowerThird = Input.mousePosition.y <= canvasRect.height / 3;

        float horizontalSpread = Mathf.Min(parameters.CardSpread / total, parameters.CardPrefferedDistance);
        float maxHorizontalSpread = horizontalSpread * total;
        float cardHorizontalSpread = index * horizontalSpread;
        float horizontalSpreadCenter = maxHorizontalSpread / 2;
        float verticalArcNormalized = parameters.CardArcCurve.Evaluate(Mathf.Abs(1 - index / (float)(total != 0 ? total : 1) * 2));
        float verticalArc = verticalArcNormalized * parameters.CardArcLift;
        float vertialOffset = canvasRect.height / parameters.CardOffsetFractionOfTheScreen.y * parameters.CardOffsetFractionOfTheScreen.x;
        float inactiveOffset = pullCardsUpOnHover && isInLowerThird ? 0 : parameters.VerticalDownOffsetWhenInactive;

        Vector3 newPosition = new Vector2(canvasRect.center.x, 0)
            + Vector2.up * vertialOffset
            + Vector2.right * cardHorizontalSpread
            - Vector2.up * verticalArc
            - Vector2.right * horizontalSpreadCenter
            - Vector2.up * inactiveOffset
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
        Card card, int activeCardCount, int idx) => this;

    public ICardControllerState Update()
    {
        var parameters = Controller.Parameters;
        int skippedUpdateCount = Controller.Cards.FindAll((card) => card != null && !card.gameObject.activeInHierarchy).Count;
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

            var nextState = CardPreUpdate(card, activeCardCount, activeCardIdx);
            if (nextState != this)
                return nextState;

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

public class CardsController : MonoBehaviour
{
    public Canvas Canvas;
    public CardAnimationParameters Parameters;
    public List<Card> Cards = new();
    public GameObject CardPrefab;
    public GameObject TileHightlight;
    public Camera Camera;
    public ICardControllerState State = null;

    public void AddCard(CardDescriptor desc) {
        var newCard = Instantiate(CardPrefab, Vector3.zero, Quaternion.identity);
        newCard.transform.SetParent(transform);
        var card = newCard.GetComponent<Card>();
        card.master = this;
        card.EmplaceDescriptor(desc);
        this.Cards.Add(card);
    }

    void Update()
    {
        if (State == null)
            State = new PassiveState(this);
        State = State.Update();
    }
}

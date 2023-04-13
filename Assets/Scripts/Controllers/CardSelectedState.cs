using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class CardSelectedState : PassiveState
{
    public Card selected;

    ICardControllerState IdleState() => new IdleState(Controller);

    public CardSelectedState(CardsController controller, Card selected) : base(controller)
    {
        this.selected = selected;
        this.pullCardsUpOnHover = false;
    }

    protected Vector2 CalculatePositionForSelectedCard()
    {
        var rect = Controller.Canvas.pixelRect;
        var cardSelectedOffset = Controller.Parameters.CardSelectedOffset;
        var offset = new Vector2(cardSelectedOffset.x * rect.width, cardSelectedOffset.y * rect.height);
        return (Vector2)Input.mousePosition + offset;
    }

    bool TryPlayCard(Card card, Tile tile) {
        if(card.TryPlay(tile)) {
            this.Controller.Cards.Remove(card);
            card.gameObject.transform.SetParent(null);
            Controller.TileHightlight.SetActive(false);
            return true;
        }
        return false;
    }

    protected override ICardControllerState CardPreUpdate(Card card, int activeCardCount, int idx)
    {
        RaycastHit hit;
        if (Physics.Raycast(Controller.Camera.ScreenPointToRay(Input.mousePosition), out hit))
        {
            Tile tile = null;
            if (hit.transform.TryGetComponent(out tile))
            {
                Controller.TileHightlight.transform.position = tile.GetWorldPivot();
                Controller.TileHightlight.SetActive(true);

                if (Input.GetMouseButtonDown(0)) {
                    if(TryPlayCard(card, tile)) {
                        return IdleState();
                    }
                }
            }
        }
        else
            Controller.TileHightlight.SetActive(false);

        if (Input.GetMouseButtonDown(1))
        {
            Controller.TileHightlight.SetActive(false);
            return IdleState();
        }

        if (card != selected || !Input.GetKeyDown(KeyCode.K))
            return this;

        Controller.Cards.Remove(card);
        Object.Destroy(card.gameObject);
        return IdleState();
    }

    protected override Vector2 CalculatePositionForCard(int index, int total, Card card)
    {
        bool isSelected = card == selected;
        return isSelected
            ? CalculatePositionForSelectedCard()
            : base.CalculatePositionForCard(index, total, card);
    }

    protected override Quaternion CalculateRotationForCard(int index, int total, Card card)
    {
        bool isSelected = card == selected;
        return isSelected
            ? Quaternion.identity
            : base.CalculateRotationForCard(index, total, card);
    }
}

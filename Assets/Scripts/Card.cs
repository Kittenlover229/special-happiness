using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CardsController master;
    public RectTransform rect;
    public bool MouseOver;


    public void OnPointerEnter(PointerEventData eventData) {
        this.master.State.NotifyEnterCard(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        this.master.State.NotifyExitCard(this);
    }

    void Start()
    {
        this.rect = GetComponent<RectTransform>();
    }
}

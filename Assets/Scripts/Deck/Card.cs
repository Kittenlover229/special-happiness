using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CardsController master;
    public RectTransform rect;
    public bool MouseOver;
    
    public CardDescriptor desc;
    public Text nameText;
    public Text descriptionText;
    public Image artworkRenderer;

    public bool TryPlay(Tile tile) {
        if(desc.TryPlay(tile)) {
            Destroy(gameObject, 0.1f);
            return true;
        } else {
            return false;
        }
    }
    
    public void EmplaceDescriptor(CardDescriptor desc) {
        this.desc = desc;
        nameText.text = desc.Name;
        descriptionText.text = desc.Description;
        artworkRenderer.sprite = desc.Artwork;
    }

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

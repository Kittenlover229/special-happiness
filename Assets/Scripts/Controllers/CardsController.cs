using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsController : MonoBehaviour
{
    [SerializeField]
    Canvas canvas;
    [SerializeField]
    Vector2 cardOffset;
    [SerializeField]
    Vector2 cardSpread;
    [SerializeField]
    float maxCardRotation;
    [SerializeField]
    List<Card> cards = new List<Card>();
    [SerializeField]
    AnimationCurve cardArcCurve;
    [SerializeField]
    float cardArcLift;
    [SerializeField]
    float cardMovementSmoothness;

    void Update()
    {
        // TODO: center this bullshit on the screen
        float iters = cards.Count - 1;
        for (int i = 0; i < cards.Count; i++)
        {
            float d = i / iters;
            Vector3 newPosition = cardOffset - i * (cardSpread / iters) + cardSpread / 2 + Vector2.up * cardArcCurve.Evaluate(Mathf.Abs(1 - d * 2)) * cardArcLift;
            cards[i].transform.position = Vector3.Lerp(cards[i].transform.position, newPosition, Time.deltaTime * cardMovementSmoothness);
            Quaternion newRotation = Quaternion.Euler(0, 0, ((float)i - cards.Count / 2) * maxCardRotation);
            cards[i].transform.rotation = newRotation;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = canvas.transform.localToWorldMatrix;
    }
}

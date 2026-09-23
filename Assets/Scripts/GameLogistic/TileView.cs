using System;
using UnityEngine;

public class TileView : MonoBehaviour, ITileTarget, IInteractable {
    [SerializeField] private Transform envelopeContainer;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color rightColor = new(0.4f, 0.85f, 0.4f);
    [SerializeField] private Color wrongColor = new(0.9f, 0.35f, 0.35f);

    public Tile Tile { get; private set; }
    public QuestionEnvelopeView Envelope { get; private set; }

    public void Initialize(Tile tile) => Tile = tile;

    public void AttachEnvelope(QuestionEnvelopeView envelope) {
        Envelope = envelope;
        envelope.Bind(Tile);
        envelope.transform.SetParent(envelopeContainer, false);
        envelope.transform.localPosition = Vector3.zero;
    }

    public void RemoveEnvelope() {
        if (Envelope == null) return;
        Destroy(Envelope.gameObject);
        Envelope = null;
    }

    public void ShowState(TileState state) {
        if (meshRenderer == null) return;
        meshRenderer.material.color = state switch {
            TileState.RightAnswer => rightColor,
            TileState.WrongAnswer => wrongColor,
            _ => idleColor
        };
    }

    // Сам тайл ничего не анимирует. Ховер/выбор делегируются открытке, если она есть.
    public void OnSelect(bool isSelected) => Envelope?.OnSelect(isSelected);
    public void OnHoverEnter() => Envelope?.OnHoverEnter();
    public void OnHoverExit() => Envelope?.OnHoverExit();
}

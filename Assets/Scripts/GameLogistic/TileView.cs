using System;
using UnityEngine;

public class TileView : MonoBehaviour, IInteractable {
    [SerializeField] Transform questionEnvelopeContainer;

    public int Row { get; private set; }
    public int Col { get; private set; }

    private QuestionEnvelopeView _envelope;

    public event Action<TileView> Clicked;

    public void Initialize(int row, int col) {
        Row = row;
        Col = col;
    }

    public void AttachEnvelope(QuestionEnvelopeView questionEnvelopeView) {
        questionEnvelopeView.transform.SetParent(questionEnvelopeContainer, false);
        questionEnvelopeView.transform.localPosition = Vector3.zero;
        _envelope = questionEnvelopeView;
        _envelope.Clicked += HandleEnvelopeClick;
    }

    public QuestionEnvelopeView DettachEnvelope() {
        _envelope.Clicked -= HandleEnvelopeClick;
        var detached = _envelope;
        _envelope = null;
        return detached;
    }

    private void HandleEnvelopeClick(QuestionEnvelopeView questionEnvelopeView) {
        Clicked?.Invoke(this);
    }

    public void HideEnvelope() {
        if (_envelope != null) _envelope.gameObject.SetActive(false);
    }

    public void OnSelect() {
        Debug.Log("Tile selected");
        Clicked?.Invoke(this);
    }

    public void OnHoverEnter() {
        Debug.Log("Tile Unhovered");
    }

    public void OnHoverExit() {
        Debug.Log("Tile Unhovered");
    }
}

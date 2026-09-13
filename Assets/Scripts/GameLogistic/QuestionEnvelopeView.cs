using System;
using UnityEngine;

public class QuestionEnvelopeView : MonoBehaviour, IInteractable {
    [SerializeField] MeshRenderer meshRenderer;

    public Action<QuestionEnvelopeView> Clicked { get; internal set; }

    private void Awake() {
        if (meshRenderer == null) {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }
    }
    public void SetMainColor(Color mainColor) {
        meshRenderer.material.color = mainColor;
    }

    private void OnMouseDown() => Clicked?.Invoke(this);

    public void OnSelect() {
        Debug.Log("Envelope Selected");
    }

    public void OnHoverEnter() {
        Debug.Log("Envelope Hovered");
    }

    public void OnHoverExit() {
        Debug.Log("Envelope Unhovered");
    }
}
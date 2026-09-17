using DG.Tweening;
using System;
using UnityEngine;

public class QuestionEnvelopeView : MonoBehaviour, IInteractable {
    [SerializeField] MeshRenderer meshRenderer;

    public Action<QuestionEnvelopeView> Clicked { get; internal set; }

    private Material _material;

    [Header("Animation")]
    private Tween _scaleTween;
    [SerializeField] private float hoverScale = 1.08f;
    private Vector3 _baseScale;
    [SerializeField] private float animationDuration = 0.15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    private bool _isSelected = false;

    private void Awake() {
        if (meshRenderer == null) {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }
        _material = meshRenderer.material;
        _baseScale = transform.localScale;
    }
    public void SetMainColor(Color mainColor) {
        meshRenderer.material.color = mainColor;
    }

    private void OnMouseDown() => Clicked?.Invoke(this);

    public void OnSelect(bool isSelected) {
        Debug.Log("Envelope Selected");
        _isSelected = isSelected;

        transform.DOKill();
        transform.localScale = _baseScale * (hoverScale * 0.9f);
        transform.DOScale(_baseScale * hoverScale, animationDuration)
                 .SetEase(Ease.OutBack);

        Debug.Log("Envelope Selected");
    }

    public void OnHoverEnter() {
        Debug.Log("Envelope Hovered");
        if (!_isSelected) {
            AnimateScale(hoverScale);
        }
    }

    public void OnHoverExit() {
        Debug.Log("Envelope Unhovered");

        if (!_isSelected) {
            AnimateScale(1f);
        }
    }

    private void AnimateScale(float multiplier) {
        _scaleTween?.Kill();
        _scaleTween = transform
            .DOScale(_baseScale * multiplier, animationDuration)
            .SetEase(scaleEase)
            .SetUpdate(UpdateType.Normal, isIndependentUpdate: true);
    }
}
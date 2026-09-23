using DG.Tweening;
using UnityEngine;

public class QuestionEnvelopeView : MonoBehaviour, ITileTarget, IInteractable {
    [SerializeField] private MeshRenderer meshRenderer;
    [Header("Animation")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float animationDuration = 0.15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    public Tile Tile { get; private set; }

    private Vector3 _baseScale;
    private bool _isSelected;

    private void Awake() {
        if (meshRenderer == null) meshRenderer = GetComponentInChildren<MeshRenderer>();
        _baseScale = transform.localScale;
    }

    public void Bind(Tile tile) => Tile = tile;

    public void SetMainColor(Color color) => meshRenderer.material.color = color;

    public void OnSelect(bool isSelected) {
        _isSelected = isSelected;
        if (isSelected) {
            transform.DOKill();
            transform.localScale = _baseScale * (hoverScale * 0.9f);
            AnimateScale(hoverScale);
        } else {
            AnimateScale(1f);
        }
    }

    public void OnHoverEnter() { if (!_isSelected) AnimateScale(hoverScale); }
    public void OnHoverExit() { if (!_isSelected) AnimateScale(1f); }

    private void AnimateScale(float multiplier) {
        transform.DOKill();
        transform.DOScale(_baseScale * multiplier, animationDuration)
                 .SetEase(scaleEase)
                 .SetUpdate(UpdateType.Normal, true);
    }

    private void OnDestroy() => transform.DOKill();
}
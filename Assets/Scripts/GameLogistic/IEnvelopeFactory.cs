using UnityEngine;

public interface IEnvelopeFactory {
    QuestionEnvelopeView CreateEnvelope(QuestionData question);
    void ReleaseEnvelope(QuestionEnvelopeView view);
}


public class EnvelopeFactory : IEnvelopeFactory {
    private QuestionEnvelopeView _prefab;
    private ICategoryColorProvider _categoryColorProvider;
    public EnvelopeFactory(QuestionEnvelopeView prefab, ICategoryColorProvider categoryColorProvider) {
        _prefab = prefab;
        _categoryColorProvider = categoryColorProvider;
    }

    public QuestionEnvelopeView CreateEnvelope(QuestionData question) {
        QuestionEnvelopeView view = UnityEngine.Object.Instantiate(_prefab);
        Color categoryColor = _categoryColorProvider.GetColorForCategory(question.Category);
        view.SetMainColor(categoryColor);
        return view;
    }

    public void ReleaseEnvelope(QuestionEnvelopeView view) {
        UnityEngine.Object.Destroy(view);
    }
}

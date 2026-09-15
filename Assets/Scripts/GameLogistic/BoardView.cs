using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardViewConfig {
    public float tileSpacing = 0.2f;
    public float tileWidth = 1.0f;
    public float tileHeight = 1.0f;
}


public class BoardView : MonoBehaviour {
    [SerializeField] private TileView tileViewPrefab;
    [SerializeField] private BoardViewConfig config;

    private readonly List<TileView> _tiles = new();
    public IReadOnlyList<TileView> Tiles => _tiles;


    public IReadOnlyList<TileView> GenerateBoard(int size) {
        Clear();

        float stepX = config.tileWidth + config.tileSpacing;
        float stepZ = config.tileHeight + config.tileSpacing;

        for (int row = 0; row < size; row++) {
            for (int col = 0; col < size; col++) {
                Vector3 position = new Vector3(col * stepX, 0f, row * stepZ);

                var tileView = Instantiate(tileViewPrefab, position, Quaternion.identity, transform);
                tileView.gameObject.name = $"Tile [Row : {row}, Column :{col}]";
                tileView.Initialize(row, col);
                _tiles.Add(tileView);
            }
        }
        return _tiles;
    }

    public void RemoveTile(TileView view) {
        if (!_tiles.Remove(view)) return;
        Destroy(view.gameObject);
    }

    public void Clear() {
        foreach (var t in _tiles) {
            if (t != null && t.gameObject != null) {
                Destroy(t.gameObject);
            }
            
        }
        _tiles.Clear();
    }
}

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


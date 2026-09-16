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
    private readonly List<Transform> _rowParents = new();
    public IReadOnlyList<TileView> Tiles => _tiles;

    public IReadOnlyList<TileView> GenerateBoard(int size) {
        Clear();

        float stepX = config.tileWidth + config.tileSpacing;
        float stepZ = config.tileHeight + config.tileSpacing;

        for (int row = 0; row < size; row++) {
            // родитель ряда смещён по Z на row * stepZ
            var rowParent = new GameObject($"Row [{row}]").transform;
            rowParent.SetParent(transform, worldPositionStays: false);
            rowParent.localPosition = new Vector3(0f, 0f, row * stepZ);
            _rowParents.Add(rowParent);

            for (int col = 0; col < size; col++) {
                // локальная позиция внутри ряда: только по X
                Vector3 localPos = new Vector3(col * stepX, 0f, 0f);

                var tileView = Instantiate(tileViewPrefab, rowParent);
                tileView.transform.localPosition = localPos;
                tileView.transform.localRotation = Quaternion.identity;
                tileView.gameObject.name = $"Tile [{row},{col}]";
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
        // Проще снести всех детей transform'а — это разом уберёт и тайлы, и родителей
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Destroy(transform.GetChild(i).gameObject);
        }
        _tiles.Clear();
        _rowParents.Clear();
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


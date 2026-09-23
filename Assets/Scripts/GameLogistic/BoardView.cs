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

    public IReadOnlyList<TileView> GenerateBoard(Grid grid) {
        Clear();
        float stepX = config.tileWidth + config.tileSpacing;
        float stepZ = config.tileHeight + config.tileSpacing;

        for (int row = 0; row < grid.Size; row++) {
            var rowParent = new GameObject($"Row [{row}]").transform;
            rowParent.SetParent(transform, false);
            rowParent.localPosition = new Vector3(0f, 0f, row * stepZ);

            for (int col = 0; col < grid.Size; col++) {
                var tile = grid.GetTile(row, col);
                var view = Instantiate(tileViewPrefab, rowParent);
                view.transform.localPosition = new Vector3(col * stepX, 0f, 0f);
                view.transform.localRotation = Quaternion.identity;
                view.name = $"Tile [{row},{col}]";
                view.Initialize(tile);
                _tiles.Add(view);
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


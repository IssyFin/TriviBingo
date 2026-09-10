using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Board оркестрирует логику Bingo и содержит маппинг "Клетка -> Игровые данные".
/// </summary>
public class BingoBoard {
    public Grid Grid { get; }

    // Связываем структуру сетки с игровой логикой
    private readonly Dictionary<Tile, BingoTileData> _tileDataMap;

    public BingoBoard(int size, IQuestionProvider questionProvider) {
        Grid = new Grid(size);
        _tileDataMap = new Dictionary<Tile, BingoTileData>();

        var questions = questionProvider.GetQuestions(size * size);
        int index = 0;

        foreach (var tile in Grid.AllTiles()) {
            _tileDataMap[tile] = new BingoTileData(questions[index], tile);
            index++;
        }
    }

    // --- Доступ к игровым данным ---

    public BingoTileData GetData(Tile tile) {
        if (!_tileDataMap.TryGetValue(tile, out var data))
            throw new InvalidOperationException($"Tile {tile} has no game data.");
        return data;
    }

    // --- Игровая логика ---

    public void MarkTileFilled(Tile tile) {
        var data = GetData(tile);
        if (data.State != TileState.Unrevealed)
            throw new InvalidOperationException($"Tile {tile} already resolved as {data.State}.");
        data.State = TileState.Filled;
    }

    public void MarkTileDead(Tile tile) {
        var data = GetData(tile);
        if (data.State != TileState.Unrevealed)
            throw new InvalidOperationException($"Tile {tile} already resolved as {data.State}.");
        data.State = TileState.Dead;
    }

    // --- Логика проверки линий Bingo ---

    public bool IsLineComplete(Line line) {
        return line.Tiles.All(t => GetData(t).State == TileState.Filled);
    }

    public bool IsLineBlocked(Line line) {
        return line.Tiles.Any(t => GetData(t).State == TileState.Dead);
    }

    public bool IsLineStillPossible(Line line) {
        return !IsLineBlocked(line) && !IsLineComplete(line);
    }
}

public enum TileState {
    Unrevealed,
    Filled,
    Dead
}

/// <summary>
/// Контейнер для игровых данных, привязанных к конкретной клетке.
/// </summary>
public class BingoTileData {
    public Tile tile;
    public QuestionData Question { get; }
    public TileState State { get; set; } = TileState.Unrevealed;

    public BingoTileData(QuestionData question, Tile tile) {
        Question = question ?? throw new ArgumentNullException(nameof(question));
        this.tile = tile ?? throw new ArgumentNullException(nameof(tile));
    }
}

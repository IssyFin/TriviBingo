using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Board оркестрирует логику Bingo и содержит маппинг "Клетка -> Игровые данные".
/// </summary>
public class QuizBoard {
    public Grid Grid { get; private set; }

    // Связываем структуру сетки с игровой логикой
    private Dictionary<Tile, TileData> _tileDataMap;

    public void Generate(int size, IQuestionProvider questionProvider) {
        int tileCount = size * size;

        Grid = new Grid(size);
        _tileDataMap = new Dictionary<Tile, TileData>(tileCount);

        var questions = questionProvider.GetRandomQuestions(Grid.Count);
        if (questions.Count < tileCount)
            throw new ArgumentException($"Need {tileCount} questions, got {questions.Count}");

        int index = 0;
        foreach (var tile in Grid.AllTiles()) {
            _tileDataMap[tile] = new TileData(questions[index++], tile);
        }
    }

    // --- Доступ к игровым данным ---

    public TileData GetData(Tile tile) {
        if (!_tileDataMap.TryGetValue(tile, out var data))
            throw new InvalidOperationException($"Tile {tile} has no game data.");
        return data;
    }

    // --- Игровая логика ---

    public void MarkTileState(Tile tile, TileState state) {
        var data = GetData(tile);
        if (data.State != TileState.Idle)
            throw new InvalidOperationException($"Tile {tile} already resolved as {data.State}.");
        data.State = state;
    }

    // --- Логика проверки линий Bingo ---

    public bool IsLineComplete(Line line) {
        return line.Tiles.All(t => GetData(t).State == TileState.RightAnswer);
    }

    public bool IsLineBlocked(Line line) {
        return line.Tiles.Any(t => GetData(t).State == TileState.WrongAnswer);
    }

    public bool IsLineStillPossible(Line line) {
        return !IsLineBlocked(line) && !IsLineComplete(line);
    }
}

public enum TileState {
    Idle,
    RightAnswer,
    WrongAnswer
}

/// <summary>
/// Контейнер для игровых данных, привязанных к конкретной клетке.
/// </summary>
public class TileData {
    public Tile tile;
    public QuestionData Question { get; }
    public TileState State { get; set; } = TileState.Idle;

    public TileData(QuestionData question, Tile tile) {
        Question = question ?? throw new ArgumentNullException(nameof(question));
        this.tile = tile ?? throw new ArgumentNullException(nameof(tile));
    }
}

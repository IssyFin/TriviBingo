using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Описывает структурную линию на сетке (ряд, колонка, диагональ).
/// </summary>
public class Line {
    public string Name { get; }
    public IReadOnlyList<Tile> Tiles { get; }

    public Line(string name, IReadOnlyList<Tile> tiles) {
        Name = name;
        Tiles = tiles;
    }
}

public enum TileState { Idle, RightAnswer, WrongAnswer }
public enum GameState { Playing, Won, Lost }

public class TileData {
    public Tile Tile { get; }
    public QuestionData Question { get; }
    public TileState State { get; internal set; } = TileState.Idle;

    public TileData(QuestionData question, Tile tile) {
        Question = question ?? throw new ArgumentNullException(nameof(question));
        Tile = tile ?? throw new ArgumentNullException(nameof(tile));
    }
}

public class QuizBoard {
    public Grid Grid { get; private set; }
    public List<Line> AllLines => _lines;
    private List<Line> _lines = new();
    public GameState State { get; private set; } = GameState.Playing;

    /// Вызывается после того, как ответ на клетку учтён.
    public event Action<TileData> TileResolved;
    public event Action<GameState> GameFinished;

    private Dictionary<Tile, TileData> _tileData;

    public IEnumerable<TileData> AllData => _tileData.Values;

    public void Generate(int size, IQuestionProvider questionProvider) {
        Grid = new Grid(size);
        State = GameState.Playing;

        int tileCount = size * size;
        var questions = questionProvider.GetRandomQuestions(tileCount);
        if (questions.Count < tileCount)
            throw new ArgumentException($"Need {tileCount} questions, got {questions.Count}");

        _tileData = new Dictionary<Tile, TileData>(tileCount);
        int i = 0;
        foreach (var tile in Grid.AllTiles())
            _tileData[tile] = new TileData(questions[i++], tile);

        _lines = BuildLines(Grid);   // ← строим линии здесь
    }

    private static List<Line> BuildLines(Grid grid) {
        int size = grid.Size;
        var lines = new List<Line>();

        // Ряды
        for (int row = 0; row < size; row++) {
            var tiles = new List<Tile>(size);
            for (int col = 0; col < size; col++) tiles.Add(grid.GetTile(row, col));
            lines.Add(new Line($"Row {row + 1}", tiles));
        }

        // Колонки
        for (int col = 0; col < size; col++) {
            var tiles = new List<Tile>(size);
            for (int row = 0; row < size; row++) tiles.Add(grid.GetTile(row, col));
            lines.Add(new Line($"Column {(char)('A' + col)}", tiles));
        }

        // Диагонали
        var diagMain = new List<Tile>(size);
        var diagAnti = new List<Tile>(size);
        for (int i = 0; i < size; i++) {
            diagMain.Add(grid.GetTile(i, i));
            diagAnti.Add(grid.GetTile(i, size - 1 - i));
        }
        lines.Add(new Line("Diagonal \u2199\u2197 (main)", diagMain));
        lines.Add(new Line("Diagonal \u2198\u2196 (anti)", diagAnti));

        return lines;
    }

    public TileData GetData(Tile tile) =>
        _tileData.TryGetValue(tile, out var d)
            ? d
            : throw new InvalidOperationException($"Tile {tile} has no game data.");

    public bool CanAnswer(Tile tile) =>
        State == GameState.Playing && GetData(tile).State == TileState.Idle;

    /// Единственная точка изменения состояния. Вся Bingo-логика здесь.
    public void ResolveTile(Tile tile, bool isCorrect) {
        if (!CanAnswer(tile)) return;

        var data = GetData(tile);
        data.State = isCorrect ? TileState.RightAnswer : TileState.WrongAnswer;
        TileResolved?.Invoke(data);

        EvaluateGameState();
    }

    private void EvaluateGameState() {
        if (_lines.Any(IsLineComplete)) {
            Finish(GameState.Won);
        } else if (!_lines.Any(IsLineStillPossible)) {
            Finish(GameState.Lost);
        }
    }

    public bool IsLineComplete(Line line) =>
        line.Tiles.All(t => GetData(t).State == TileState.RightAnswer);

    public bool IsLineBlocked(Line line) =>
        line.Tiles.Any(t => GetData(t).State == TileState.WrongAnswer);

    public bool IsLineStillPossible(Line line) =>
        !IsLineBlocked(line) && !IsLineComplete(line);

    private void Finish(GameState state) {
        State = state;
        GameFinished?.Invoke(state);
    }
}
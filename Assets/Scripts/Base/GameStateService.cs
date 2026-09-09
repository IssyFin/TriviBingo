using System;
using System.Linq;

public enum GameStatus {
    InProgress,
    Won,
    Lost
}

public interface IGameStateService {
    bool CheckWinCondition();
    bool CheckLossCondition();
    bool IsTileAvailable(Tile tile);
    GameStatus GetStatus();
    int GetAvailableTilesCount();
    int GetTotalTilesCount();
}

public class GameStateService : IGameStateService {
    private readonly BingoBoard _board;

    public GameStateService(BingoBoard board) {
        _board = board ?? throw new ArgumentNullException(nameof(board));
    }

    public bool CheckWinCondition() {
        return _board.Grid.AllLines().Any(line => _board.IsLineComplete(line));
    }

    public bool CheckLossCondition() {
        return _board.Grid.AllLines().All(line => _board.IsLineBlocked(line));
    }

    public bool IsTileAvailable(Tile tile) {
        return _board.GetData(tile).State == TileState.Unrevealed;
    }

    public GameStatus GetStatus() {
        if (CheckWinCondition())
            return GameStatus.Won;
        if (CheckLossCondition())
            return GameStatus.Lost;
        return GameStatus.InProgress;
    }

    public int GetAvailableTilesCount() {
        return _board.Grid.AllTiles()
                         .Count(tile => _board.GetData(tile).State == TileState.Unrevealed);
    }

    public int GetTotalTilesCount() {
        return _board.Grid.Size * _board.Grid.Size;
    }
}

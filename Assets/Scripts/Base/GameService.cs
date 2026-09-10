using System;
using System.Linq;

public interface IGameService {
    bool TrySelectTile(string address, out BingoTileData tileData);
    bool TryAnswerQuestion(BingoTileData tileData, string userAnswer, out bool isCorrect);
    GameState GetGameState();
    void ResetGame();
}



public class GameState {
    public GameStatus Status { get; set; }
    public int AvailableTiles { get; set; }
    public int TotalTiles { get; set; }
    public int BoardSize { get; set; }
}
public class GameService : IGameService {
    private readonly BingoBoard _board;
    private readonly IQuestionService _questionService;
    private readonly IGameStateService _gameStateService;
    private readonly IUIService _uiService;

    public GameService(
        BingoBoard board,
        IQuestionService questionService,
        IGameStateService gameStateService,
        IUIService uiService) {
        _board = board ?? throw new ArgumentNullException(nameof(board));
        _questionService = questionService ?? throw new ArgumentNullException(nameof(questionService));
        _gameStateService = gameStateService ?? throw new ArgumentNullException(nameof(gameStateService));
        _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
    }

    public bool TrySelectTile(string address, out BingoTileData tileData) {
        tileData = null;

        try {
            var tile = _board.Grid.GetTileByAddress(address);
            tileData = _board.GetData(tile);

            if (!_gameStateService.IsTileAvailable(tile)) {
                _uiService.DisplayMessage(
                    $"Tile {address} is already {tileData.State}. Choose another one.",
                    MessageType.Warning);
                return false;
            }

            return true;
        } catch (Exception ex) {
            _uiService.DisplayMessage(ex.Message, MessageType.Error);
            return false;
        }
    }

    public bool TryAnswerQuestion(BingoTileData tileData, string userAnswer, out bool isCorrect) {
        var tile = tileData.tile;
        var question = _questionService.GetQuestionForTile(tile);
        isCorrect = _questionService.ValidateAnswer(question, userAnswer);

        if (isCorrect) {
            _board.MarkTileFilled(tile);
            _uiService.DisplayMessage("Correct! You get a ball on this tile.", MessageType.Success);
        } else {
            _board.MarkTileDead(tile);
            string correctAnswer = question.Answers.Where(a => a.IsCorrect).First().Text;
            _uiService.DisplayMessage(
                $"Wrong! The correct answer was: {correctAnswer}. Tile is burned.",
                MessageType.Error);
        }

        return true;
    }

    public GameState GetGameState() {
        return new GameState {
            Status = _gameStateService.GetStatus(),
            AvailableTiles = _gameStateService.GetAvailableTilesCount(),
            TotalTiles = _gameStateService.GetTotalTilesCount(),
            BoardSize = _board.Grid.Size
        };
    }

    public void ResetGame() {
        // Implementation for resetting the game
        throw new NotImplementedException();
    }
}

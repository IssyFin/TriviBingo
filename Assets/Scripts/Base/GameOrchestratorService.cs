using System;


public class GameOrchestratorService {
    private readonly IGameService _gameService;
    private readonly IUIService _uiService;
    private readonly BoardRendererService _boardRenderer;

    public GameOrchestratorService(
        IGameService gameService,
        IUIService uiService,
        BoardRendererService boardRenderer) {
        _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
        _boardRenderer = boardRenderer ?? throw new ArgumentNullException(nameof(boardRenderer));
    }

    public void RunGame() {
        _uiService.DisplayMessage("=== BINGO TRIVIA ===");
        _uiService.DisplayMessage("Win: complete a line (O).");
        _uiService.DisplayMessage("Lose: all possible lines are blocked (X).");

        while (true) {
            _uiService.DisplayBoard(_boardRenderer.RenderBoard());

            var gameState = _gameService.GetGameState();

            if (gameState.Status == GameStatus.Won) {
                _uiService.DisplayMessage("\n🎉 BINGO! You completed a line and won!", MessageType.Success);
                break;
            }

            if (gameState.Status == GameStatus.Lost) {
                _uiService.DisplayMessage("\n💀 DEFEAT! All lines are blocked. No available combinations.", MessageType.Error);
                break;
            }

            string address = _uiService.GetUserInput("\nChoose a cell (e.g., A1, B2): ");

            if (_gameService.TrySelectTile(address, out var tileData)) {
                _uiService.DisplayQuestion(tileData.Question, GetAnswerOptions(tileData.Question));
                string answer = _uiService.GetUserInput("Your answer: ");

                if (_gameService.TryAnswerQuestion(tileData, answer, out _)) {
                    //_uiService.GetUserInput("\nPress Enter to continue...");
                    //Console.Clear();
                }
            }
        }
    }

    private string GetAnswerOptions(Question question) {
        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < question.Answers.Count; i++) {
            builder.AppendLine($"{i + 1}. {question.Answers[i]}");
        }
        return builder.ToString();
    }
}
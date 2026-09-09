
class Program {
    static void Main(string[] args) {
        // Setup
        int boardSize = 4;
        var questionProvider = new StubQuestionProvider();
        var board = new BingoBoard(boardSize, questionProvider);

        // Services
        var questionService = new QuestionService(board);
        var gameStateService = new GameStateService(board);
        var uiService = new ConsoleUIService();
        var boardRenderer = new BoardRendererService(board);
        var gameService = new GameService(board, questionService, gameStateService, uiService);
        var orchestrator = new GameOrchestratorService(gameService, uiService, boardRenderer);

        // Run
        orchestrator.RunGame();
    }
}

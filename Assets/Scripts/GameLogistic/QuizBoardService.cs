using System;
using UnityEngine;
using Zenject;

public class QuizBoardController : IInitializable, IDisposable {
    private readonly BoardView _boardViewPrefab;
    private readonly IEnvelopeFactory _envelopeFactory;
    private readonly DiContainer _diContainer;
    private readonly IQuestionProvider _questionProvider;
    private readonly QuizInteractionHandler _interactionHandler;
    private readonly IQuestionUIService _questionService;
    private BoardView _activeBoardView;
    private QuizBoard _boardData;

    private bool _isInteractionBlocked = false;

    public QuizBoardController(
        BoardView boardViewPrefab,
        IEnvelopeFactory envelopeFactory,
        DiContainer diContainer,
        IQuestionProvider questionProvider,
        QuizInteractionHandler interactionHandler,
        IQuestionUIService uiService) {
        _boardViewPrefab = boardViewPrefab ?? throw new ArgumentNullException(nameof(boardViewPrefab));
        _envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
        _diContainer = diContainer;
        _questionProvider = questionProvider;
        _interactionHandler = interactionHandler;
        _questionService = uiService;

        _boardData = new QuizBoard();
    }

    public void ShowBoard(int size) {
        ClearBoard();
        
        _boardData.Generate(size, _questionProvider);

        // Исправлено: сохраняем в поле класса, а не в локальную переменную
        _activeBoardView = _diContainer.InstantiatePrefabForComponent<BoardView>(_boardViewPrefab);
        _activeBoardView.transform.SetParent(null);

        var tiles = _activeBoardView.GenerateBoard(_boardData.Grid.Size);

        foreach (var tileView in tiles) {
            AttachEnvelopeToTile(tileView);
        }
    }

    private void AttachEnvelopeToTile(TileView tileView) {
        var tile = _boardData.Grid.GetTile(tileView.Row, tileView.Col);
        var data = _boardData.GetData(tile);

        if (data.Question == null) return;

        QuestionEnvelopeView envelope = _envelopeFactory.CreateEnvelope(data.Question);
        envelope.gameObject.name = $"Envelope for {tileView.name}";
        tileView.AttachEnvelope(envelope);
    }

    public void ClearBoard() {
        if (_activeBoardView != null) {
            _activeBoardView.Clear();
            UnityEngine.Object.Destroy(_activeBoardView.gameObject);
            _activeBoardView = null;
        }
    }

    public void Dispose() {
        ClearBoard();
        if (_interactionHandler != null)
        _interactionHandler.OnTileSelected -= HandleTileSelected;

        if (_questionService != null) {
            _questionService.onAnswerCompleted += OnQuestionSessionCompleted;
        }
    }

    public void Initialize() {
        if (_interactionHandler != null)
        _interactionHandler.OnTileSelected += HandleTileSelected;

        if (_questionService != null) {
            _questionService.onAnswerCompleted += OnQuestionSessionCompleted;
        }
    }

    private void HandleTileSelected(int row, int col) {
        if (_isInteractionBlocked || _boardData == null) return;

        var tile = _boardData.Grid.GetTile(row, col);
        var tileData = _boardData.GetData(tile);
        if (tileData?.Question == null) return;

        //_isInteractionBlocked = true;
        _questionService.OpenQuestionWindow(tileData.Question);
    }

    private void OnQuestionSessionCompleted(RespondStatus respondStatus) {
        if (respondStatus.IsCorrect) {
            // + points
        } else {
            // - points
        }

        //Remove envelope

        // Снимаем блокировку
        _isInteractionBlocked = false;
    }
}


public class QuizInteractionHandler : IInitializable, IDisposable {
    private readonly IInteractionService _interactionService;

    // Событие передает только логические координаты
    public event Action<int, int> OnTileSelected;

    public QuizInteractionHandler([InjectOptional] IInteractionService interactionService) {
        _interactionService = interactionService;
    }

    public void Initialize() {
        if (_interactionService != null) {
            _interactionService.Selected += HandleSelection;
        }
    }

    public void Dispose() {
        if (_interactionService != null) {
            _interactionService.Selected -= HandleSelection;
        }
    }

    private void HandleSelection(GameObject clickedObject) {
        IInteractable interactable = clickedObject.GetComponentInParent<IInteractable>();

        TileView clickedTile = null;

        // Определяем, к какому тайлу относится клик
        switch (interactable) {
            case QuestionEnvelopeView envelopeView:
                Debug.Log($"Envelope : {envelopeView}");
                // Если кликнули по открытке, получаем тайл, на котором она лежит
                clickedTile = envelopeView.GetComponentInParent<TileView>();
                break;
            case TileView tileView:
                Debug.Log($"Tile : {tileView}");
                clickedTile = tileView;
                break;
        }

        if (clickedTile != null) {
            OnTileSelected?.Invoke(clickedTile.Row, clickedTile.Col);
        }
    }
}




public struct RespondStatus {
    public bool IsCorrect { get;  }

    public RespondStatus(bool isCorrect) {
        IsCorrect = isCorrect;
    }
}
public interface IQuestionUIService {
    public event Action<RespondStatus> onAnswerCompleted;
    // Вызываем окно, передаем данные вопроса и коллбек, который сработает при закрытии/ответе
    void OpenQuestionWindow(QuestionData question);
}

public class QuestionUIService : IQuestionUIService {
    public event Action<RespondStatus> onAnswerCompleted;

    public void OpenQuestionWindow(QuestionData question) {
        Debug.Log($"Question : {question.Text}");
    }
}


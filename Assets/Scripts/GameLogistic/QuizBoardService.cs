using System;
using UnityEngine;
using Zenject;

public class QuizBoardController : IInitializable, IDisposable {
    private readonly BoardView _boardViewPrefab;
    private readonly DiContainer _diContainer;
    private readonly QuizInteractionHandler _interactionHandler;

    private readonly IEnvelopeFactory _envelopeFactory;
    private readonly IQuestionProvider _questionProvider;
    private readonly IQuizService _questionService;

    private BoardView _activeBoardView;
    private QuizBoard _boardData;

    public QuizBoardController(
        BoardView boardViewPrefab,
        IEnvelopeFactory envelopeFactory,
        DiContainer diContainer,
        IQuestionProvider questionProvider,
        QuizInteractionHandler interactionHandler,
        IQuizService uiService) {
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

        _interactionHandler.ToggleInteraction(true);
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
            _questionService.onAnswerReceived += OnQuestionSessionCompleted;
        }
    }

    public void Initialize() {
        if (_interactionHandler != null)
        _interactionHandler.OnTileSelected += HandleTileSelected;

        if (_questionService != null) {
            _questionService.onAnswerReceived += OnQuestionSessionCompleted;
        }
    }

    private void HandleTileSelected(int row, int col) {
        var tile = _boardData.Grid.GetTile(row, col);
        var tileData = _boardData.GetData(tile);
        if (tileData?.Question == null) return;

        _interactionHandler.ToggleInteraction(false);
        _questionService.BeginQuestion(tileData.Question);
    }

    private void OnQuestionSessionCompleted(RespondStatus respondStatus) {
        Debug.Log($"Answer Received {respondStatus.IsCorrect}");
        if (respondStatus.IsCorrect) {
            // + points
        } else {
            // - points
        }


        //Remove envelope


        // Снимаем блокировку
        _interactionHandler.ToggleInteraction(true);
    }
}


public class QuizInteractionHandler : IInitializable, IDisposable {
    private readonly IInteractionService _interactionService;

    // Событие передает только логические координаты
    public event Action<int, int> OnTileSelected;

    private IInteractable lastSelected;
    private bool _isInteractionBlocked = false;

    public QuizInteractionHandler([InjectOptional] IInteractionService interactionService) {
        _interactionService = interactionService;
    }

    public void Initialize() {
        if (_interactionService != null) {
            _interactionService.Selected += HandleSelection;
            _interactionService.HoverChanged += HandleHover;
        }
    }

    public void Dispose() {
        if (_interactionService != null) {
            _interactionService.Selected -= HandleSelection;
            _interactionService.HoverChanged -= HandleHover;
        }
    }

    public void ToggleInteraction(bool isEnabled) {
        _isInteractionBlocked = isEnabled;

        if (!_isInteractionBlocked && lastSelected != null) {
            lastSelected.OnSelect(false);
            lastSelected.OnHoverExit();
            lastSelected = null;
        }
    }

    private void HandleSelection(GameObject clickedObject) {
        if (!_isInteractionBlocked) return;

        lastSelected?.OnSelect(false);

        lastSelected = clickedObject.GetComponentInParent<IInteractable>();

        TileView clickedTile = null;

        // Определяем, к какому тайлу относится клик
        switch (lastSelected) {
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

        lastSelected.OnSelect(true);

        if (clickedTile != null) {
            OnTileSelected?.Invoke(clickedTile.Row, clickedTile.Col);
        }
    }

    private void HandleHover(GameObject currentObject, bool isHovered) {
        if (!_isInteractionBlocked) return;

        IInteractable interactable = currentObject.GetComponentInParent<IInteractable>();
        if (interactable == null) return;

        if (isHovered) {
            interactable.OnHoverEnter();
        } else {
            interactable.OnHoverExit();
        }
    }
}




public struct RespondStatus {
    public bool IsCorrect { get;  }

    public RespondStatus(bool isCorrect) {
        IsCorrect = isCorrect;
    }
}
public interface IQuizService {
    public event Action<RespondStatus> onAnswerReceived;
    // Вызываем окно, передаем данные вопроса и коллбек, который сработает при закрытии/ответе
    void BeginQuestion(QuestionData question);
}

public class QuizService : IQuizService {
    public event Action<RespondStatus> onAnswerReceived;

    [Inject] private IWindowService uIService;

    private QuestionWindow currentWindow;

    public void BeginQuestion(QuestionData question) {
        Debug.Log($"Question : {question.Text}");

        currentWindow = uIService.OpenWindow<QuestionWindow>();
        currentWindow.OnAnswerReceived += HandleAnswerReceived;
        currentWindow.SetQuestion(question);
    }

    private void HandleAnswerReceived(Answer answer) {
        // Здесь определяете статус: правильный/неправильный ответ
        RespondStatus status = new RespondStatus(answer.IsCorrect);

        onAnswerReceived?.Invoke(status);

        Cursor.visible = true;
        if (currentWindow != null) {
            currentWindow.OnAnswerReceived -= HandleAnswerReceived;
            uIService.Hide<QuestionWindow>();
            currentWindow = null;
        }
    }
}

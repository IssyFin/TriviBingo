using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class QuizGameController : IInitializable, IDisposable {
    private readonly QuizBoard _board;
    private readonly BoardPresenter _presenter;
    private readonly QuizInputRouter _input;
    private readonly IQuizService _quiz;
    private readonly IQuestionProvider _questionProvider;

    public event Action<GameState> GameFinished;

    public QuizGameController(
        BoardPresenter presenter,
        QuizInputRouter input,
        IQuizService quiz,
        IQuestionProvider questionProvider) {
        _board = new();
        _presenter = presenter;
        _input = input;
        _quiz = quiz;
        _questionProvider = questionProvider;
    }

    public void Initialize() {
        _input.TilePicked += OnTilePicked;
        _quiz.onAnswerReceived += OnAnswerReceived;
        _board.GameFinished += OnGameFinished;
    }

    public void Dispose() {
        _input.TilePicked -= OnTilePicked;
        _quiz.onAnswerReceived -= OnAnswerReceived;
        _board.GameFinished -= OnGameFinished;
    }

    public void StartGame(int size) {
        _board.Generate(size, _questionProvider);
        _presenter.Show(_board);
        _input.SetEnabled(true);
    }

    private Tile _pendingTile;

    private void OnTilePicked(Tile tile) {
        if (!_board.CanAnswer(tile)) return;

        _pendingTile = tile;
        _input.SetEnabled(false);
        _quiz.BeginQuestion(_board.GetData(tile).Question);
    }

    private void OnAnswerReceived(RespondStatus status) {
        if (_pendingTile == null) return;

        var tile = _pendingTile;
        _pendingTile = null;

        // Меняем модель. Презентер сам уберёт открытку и покрасит тайл.
        _board.ResolveTile(tile, status.IsCorrect);

        // Если игра закончилась, ввод остаётся заблокированным навсегда.
        if (_board.State == GameState.Playing)
            _input.SetEnabled(true);
    }

    private void OnGameFinished(GameState state) {
        _input.SetEnabled(false);
        Debug.Log(state == GameState.Won ? "Победа!" : "Поражение");
        GameFinished?.Invoke(state);
    }
}

public class BoardPresenter : IDisposable {
    private readonly BoardView _boardViewPrefab;
    private readonly DiContainer _container;
    private readonly IEnvelopeFactory _envelopeFactory;

    private BoardView _view;
    private QuizBoard _board;
    private readonly Dictionary<Tile, TileView> _tileViews = new();

    public BoardPresenter(BoardView boardViewPrefab, DiContainer container, IEnvelopeFactory envelopeFactory) {
        _boardViewPrefab = boardViewPrefab ?? throw new ArgumentNullException(nameof(boardViewPrefab));
        _container = container;
        _envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
    }

    public void Show(QuizBoard board) {
        Clear();
        _board = board;
        _board.TileResolved += OnTileResolved;

        _view = _container.InstantiatePrefabForComponent<BoardView>(_boardViewPrefab);
        _view.transform.SetParent(null);

        foreach (var tileView in _view.GenerateBoard(board.Grid)) {
            _tileViews[tileView.Tile] = tileView;

            var data = board.GetData(tileView.Tile);
            var envelope = _envelopeFactory.CreateEnvelope(data.Question);
            envelope.name = $"Envelope for {tileView.name}";
            tileView.AttachEnvelope(envelope);
        }
    }

    private void OnTileResolved(TileData data) {
        if (!_tileViews.TryGetValue(data.Tile, out var tileView)) return;
        tileView.RemoveEnvelope();
        tileView.ShowState(data.State);
    }

    public void Clear() {
        if (_board != null) _board.TileResolved -= OnTileResolved;
        _board = null;
        _tileViews.Clear();

        if (_view != null) {
            _view.Clear();
            UnityEngine.Object.Destroy(_view.gameObject);
            _view = null;
        }
    }

    public void Dispose() => Clear();
}


public class QuizInputRouter : IInitializable, IDisposable {
    private readonly IInteractionService _interaction;

    public event Action<Tile> TilePicked;

    private IInteractable _selected;
    private bool _enabled = true;

    public QuizInputRouter([InjectOptional] IInteractionService interaction) {
        _interaction = interaction;
    }

    public void Initialize() {
        if (_interaction == null) return;
        _interaction.Selected += OnSelected;
        _interaction.HoverChanged += OnHover;
    }

    public void Dispose() {
        if (_interaction == null) return;
        _interaction.Selected -= OnSelected;
        _interaction.HoverChanged -= OnHover;
    }

    public void SetEnabled(bool enabled) {
        _enabled = enabled;
        if (!enabled) ClearSelection();
    }

    private void OnSelected(GameObject go) {
        if (!_enabled) return;
        if (!go.TryGetComponentInParent(out ITileTarget target)) return;

        ClearSelection();
        if (target is IInteractable interactable) {
            _selected = interactable;
            interactable.OnSelect(true);
        }
        TilePicked?.Invoke(target.Tile);
    }

    private void OnHover(GameObject go, bool isHovered) {
        if (!_enabled) return;
        var interactable = go.GetComponentInParent<IInteractable>();
        if (interactable == null) return;

        if (isHovered) interactable.OnHoverEnter();
        else interactable.OnHoverExit();
    }

    private void ClearSelection() {
        if (_selected == null) return;
        // После удаления открытки объект может быть уже уничтожен (Unity-null)
        if (_selected is UnityEngine.Object o && o == null) { _selected = null; return; }
        _selected.OnSelect(false);
        _selected.OnHoverExit();
        _selected = null;
    }
}

public static class GameObjectExtensions {
    public static bool TryGetComponentInParent<T>(this GameObject go, out T component) where T : class {
        component = go.GetComponentInParent<T>();
        return component != null;
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

        if (currentWindow != null) {
            currentWindow.OnAnswerReceived -= HandleAnswerReceived;
            uIService.Hide<QuestionWindow>();
            currentWindow = null;
        }
    }
}

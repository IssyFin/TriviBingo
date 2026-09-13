using System;
using UnityEngine;
using Zenject;

public class QuizBoardService : IDisposable, IInitializable {
    private BoardView boardView;
    private QuizBoard _board;
    private IEnvelopeFactory _envelopeFactory;
    IInteractionService interactionService;

    public QuizBoardService(BoardView boardView, IEnvelopeFactory envelopeFactory, [InjectOptional] IInteractionService interactionService) {
        this.boardView = boardView ?? throw new ArgumentNullException(nameof(boardView));
        this._envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
        this.interactionService = interactionService;
    }

    public void Initialize() {
        if (interactionService == null) return;

        interactionService.Selected += HandleSelection;
    }

    public void Dispose() {
        if (interactionService == null) return;

        interactionService.Selected -= HandleSelection;
    }

    private void HandleSelection(GameObject clickedObject) {
        IInteractable interactable = clickedObject.GetComponentInParent<IInteractable>();

        switch (interactable) {
            case QuestionEnvelopeView view:
                Debug.Log($"Envelope : {view}");
                break;
            case TileView view:
                Debug.Log($"Tile : {view}");
                break;
            default:
                break;
        }
    }

    public void ShowBoard(QuizBoard board) {
        _board = board;
        var tiles = boardView.GenerateBoard(board.Grid.Size);

        foreach (var tileView in tiles) {
            HandleEnvelope(tileView);
        }
    }

    private void HandleEnvelope(TileView tileView) {
        var data = DataFor(tileView);
        QuestionEnvelopeView questionEnvelopeView = _envelopeFactory.CreateEnvelope(data.Question);
        questionEnvelopeView.gameObject.name = $"Envelope for {tileView.name}";
        tileView.AttachEnvelope(questionEnvelopeView);
    }

    private void OnTileClicked(TileView view) {
        var data = DataFor(view);
        Debug.Log($"Choosen : {view.Row} {view.Col}");
        // логика выбора
    }

    private TileData DataFor(TileView view) {
        var tile = _board.Grid.GetTile(view.Row, view.Col);
        return _board.GetData(tile);
    }

    
}


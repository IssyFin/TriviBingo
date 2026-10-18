using Zenject;

public interface IBoardViewFactory {
    BoardView Create();
}

public class BoardViewFactory : IBoardViewFactory {
    private readonly BoardView _prefab;
    private readonly DiContainer _container;

    public BoardViewFactory(BoardView prefab, DiContainer container) {
        _prefab = prefab;
        _container = container;
    }

    public BoardView Create() =>
        _container.InstantiatePrefabForComponent<BoardView>(_prefab);
}
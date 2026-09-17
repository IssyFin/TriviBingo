using BingoTrivia.Validation;
using Cysharp.Threading.Tasks;
using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;

public interface IQuestionService {
    QuestionData GetQuestionForTile(Tile tile);
    IReadOnlyList<QuestionData> GetAllBoardQuestions();
    bool ValidateAnswer(QuestionData question, string userAnswer);
}

public class QuestionService : IQuestionService {
    private readonly QuizBoard _board;
    private readonly IAnswerValidator _defaultValidator;

    public QuestionService(QuizBoard board) {
        _board = board ?? throw new ArgumentNullException(nameof(board));
        _defaultValidator = new CompositeAnswerValidator(
                new IndexBasedAnswerValidator(),
                new TextMatchAnswerValidator()
            );
    }

    public QuestionData GetQuestionForTile(Tile tile) {
        return _board.GetData(tile).Question;
    }

    public bool ValidateAnswer(QuestionData question, string userAnswer) {
        var validator = _defaultValidator;
        return validator.IsCorrect(question, userAnswer);
    }

    public IReadOnlyList<QuestionData> GetAllBoardQuestions() {
        return _board.Grid.AllTiles()
                         .Select(tile => _board.GetData(tile).Question)
                         .ToList();
    }
}

public interface IQuestionRepository {
    QuestionData? GetQuestionById(string id);
    IReadOnlyList<QuestionData> GetAllQuestions();
    IReadOnlyList<QuestionData> GetQuestionsByCategory(QuestionCategory category);
}

public class QuestionRepository : IQuestionRepository {
    private readonly IDataSource<QuestionDatabase> _source;
    public QuestionRepository(IDataSource<QuestionDatabase> source) {
        _source = source;
    }
    public QuestionData? GetQuestionById(string id) {
        var data = _source.Get();
        return data?.Questions?.FirstOrDefault(q => q.Id == id);
    }

    public IReadOnlyList<QuestionData> GetAllQuestions() {
        var data = _source.Get();
        return data?.Questions ?? (IReadOnlyList<QuestionData>)Array.Empty<QuestionData>();
    }

    public IReadOnlyList<QuestionData> GetQuestionsByCategory(QuestionCategory category) {
        var data = _source.Get();
        return data?.Questions?.Where(q => q.Category == category).ToList() ?? (IReadOnlyList<QuestionData>)Array.Empty<QuestionData>();
    }
}


/// <summary>
/// Абстракция источника вопросов. Игровая логика (Board/BingoGame)
/// никогда не знает, откуда вопросы приходят — JSON, ScriptableObject,
/// сервер, генератор для тестов и т.д.
/// </summary>
public interface IQuestionProvider {
    List<QuestionData> GetRandomQuestions(int count);
}


public class StubQuestionProvider : IQuestionProvider {
    public List<QuestionData> GetRandomQuestions(int count) {
        var result = new List<QuestionData>(count);
        for (int i = 0; i < count; i++) {
            var q = new QuestionData {
                Id = $"stub_{i}",
                Text = $"Is 2 + 2 = 4?",
                Answers = new List<Answer> {
                    new Answer { Text = "no",  IsCorrect = false },
                    new Answer { Text = "yes", IsCorrect = true  }
                }
            };
            result.Add(q);
        }
        return result;
    }
}

public sealed class QuestionProvider : IQuestionProvider {
    private readonly IQuestionRepository _repository;
    private readonly Random _random;

    // Очередь "оставшихся" вопросов текущего цикла.
    private readonly Queue<QuestionData> _bag = new();

    // Кэш последнего снапшота репозитория, чтобы не дёргать его на каждый refill.
    // (Опционально — см. раздел про инвалидацию ниже.)
    private int _cachedRepositoryCount = -1;

    public QuestionProvider(IQuestionRepository repository, Random random = null) {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _random = random ?? new Random();
    }

    public List<QuestionData> GetRandomQuestions(int count) {
        if (count <= 0)
            return new List<QuestionData>();

        var result = new List<QuestionData>(count);
        var all = _repository.GetAllQuestions();

        if (all == null || all.Count == 0)
            return result;

        // Если репозиторий изменился (например, добавили вопросы) — сбрасываем мешок,
        // чтобы не отдавать устаревшие ссылки.
        if (all.Count != _cachedRepositoryCount) {
            _bag.Clear();
            _cachedRepositoryCount = all.Count;
        }

        // Особый случай: запросили больше, чем есть всего в репозитории.
        // Тогда выдаём "циклами": полный перемешанный набор + остаток из нового перемешивания.
        // Это лучше, чем 1,2,1,2, потому что порядок внутри каждого цикла разный.
        while (result.Count < count) {
            if (all.IsEmpty()) break;

            if (_bag.Count == 0)
                RefillBag(all);

            // Сколько можем добрать без выхода за пределы запроса
            int need = count - result.Count;
            int take = Math.Min(need, _bag.Count);

            for (int i = 0; i < take; i++)
                result.Add(_bag.Dequeue());
        }

        return result;
    }

    private void RefillBag(IReadOnlyList<QuestionData> all) {
        // Копируем, чтобы не мутировать источник
        var shuffled = new List<QuestionData>(all);
        shuffled.FisherShuffle(_random);

        foreach (var q in shuffled)
            _bag.Enqueue(q);
    }
}



public static class ListExtensions {
    public static List<T> FisherShuffle<T>(this List<T> list, Random random) {
        for (int i = list.Count - 1; i > 0; i--) {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}


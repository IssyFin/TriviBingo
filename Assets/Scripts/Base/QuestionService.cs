using BingoTrivia.Validation;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;



public interface IQuestionService {
    QuestionData GetQuestionForTile(Tile tile);
    IReadOnlyList<QuestionData> GetAllBoardQuestions();
    bool ValidateAnswer(QuestionData question, string userAnswer);
}
public class QuestionService : IQuestionService {
    private readonly BingoBoard _board;
    private readonly IAnswerValidator _defaultValidator;

    public QuestionService(BingoBoard board) {
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
                Text = $"QuestionData #{i + 1}: Is 2 + 2 = 4?",
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
    private readonly Random _random = new();

    public QuestionProvider(IQuestionRepository repository) {
        _repository = repository;
    }

    public List<QuestionData> GetRandomQuestions(int count) {
        if (count <= 0)
            return new List<QuestionData>();

        var questions = _repository.GetAllQuestions();
        if (questions.Count == 0)
            return new List<QuestionData>();

        var list = questions.ToList();
        list.FisherShuffle(_random);

        var actualCount = Math.Min(count, list.Count);
        if (actualCount < list.Count)
            list.RemoveRange(actualCount, list.Count - actualCount);

        return list;
    }
}

public interface IQuestionRepository {
    QuestionData? GetQuestionById(string id);
    IReadOnlyList<QuestionData> GetAllQuestions();
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


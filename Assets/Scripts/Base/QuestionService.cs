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
    List<QuestionData> GetQuestions(int count);
}


public class StubQuestionProvider : IQuestionProvider {
    public List<QuestionData> GetQuestions(int count) {
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
    private readonly IDataSource<QuestionDatabase> _source;
    private readonly System.Random _random = new();

    private List<QuestionData> _questions;

    public QuestionProvider(IDataSource<QuestionDatabase> source) {
        _source = source;
    }

    public List<QuestionData> GetQuestions(int count) {
        EnsureLoaded();

        if (_questions.Count == 0)
            return new List<QuestionData>();

        var actualCount = Math.Min(count, _questions.Count);

        return _questions
            .OrderBy(_ => _random.Next())
            .Take(actualCount)
            .ToList();
    }

    private void EnsureLoaded() {
        if (_questions != null)
            return;

        var data = _source.Get();

        if (data == null || data.Questions == null)
            throw new InvalidOperationException(
                "QuestionData data is not available.");

        _questions = data.Questions;
    }
}



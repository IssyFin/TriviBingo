using BingoTrivia.Validation;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

[Serializable]
public class Question {
    public string Id = string.Empty;
    public string Text = string.Empty;
    public List<string> Answers = new List<string>();
    public int CorrectAnswerIndex;
    public List<string> CorrectAnswers = new List<string>();
    public Dictionary<string, string> Metadata = new Dictionary<string, string>();

    public bool IsCorrect(int chosenAnswerIndex) {
        return chosenAnswerIndex == CorrectAnswerIndex;
    }

    public bool IsCorrect(string userAnswer) {
        if (int.TryParse(userAnswer, out int answerIndex)) {
            return IsCorrect(answerIndex);
        }
        return false;
    }

    public override string ToString() {
        return $"[{Id}] {Text}";
    }
}

public interface IQuestionService {
    Question GetQuestionForTile(Tile tile);
    bool ValidateAnswer(Question question, string userAnswer);
    string GetCorrectAnswerText(Question question);
    IReadOnlyList<Question> GetAllQuestions();
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

    public Question GetQuestionForTile(Tile tile) {
        return _board.GetData(tile).Question;
    }

    public bool ValidateAnswer(Question question, string userAnswer) {
        var validator = _defaultValidator;
        return validator.IsCorrect(question, userAnswer);
    }

    public string GetCorrectAnswerText(Question question) {
        var validator = _defaultValidator;
        return validator.GetCorrectAnswer(question);
    }

    public IReadOnlyList<Question> GetAllQuestions() {
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
    List<Question> GetQuestions(int count);
}


public class StubQuestionProvider : IQuestionProvider {
    public List<Question> GetQuestions(int count) {
        var result = new List<Question>(count);
        for (int i = 0; i < count; i++) {
            result.Add(new Question {
                Id = $"stub_{i}",
                Text = $"Question #{i + 1}: Is 2 + 2 = 4?",
                Answers = new List<string> { "Yes", "No" },
                CorrectAnswerIndex = 0
            });
        }
        return result;
    }
}

public sealed class QuestionProvider : IQuestionProvider {
    private readonly IDataSource<QuestionData> _source;
    private readonly System.Random _random = new();

    private List<Question> _questions;

    public QuestionProvider(IDataSource<QuestionData> source) {
        _source = source;
    }

    public List<Question> GetQuestions(int count) {
        EnsureLoaded();

        if (_questions.Count == 0)
            return new List<Question>();

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
                "Question data is not available.");

        _questions = data.Questions;
    }
}

public interface IDataSource<T> {
    T Get();
}


/// <summary>
/// Универсальный провайдер. Загружает ассет, распаковывает данные и хранит их в памяти.
/// Подходит для ЛЮБЫХ данных: вопросов, уровней, конфигов.
/// </summary>
public sealed class AssetDataLoader<T> :
    IDataSource<T>,
    IDataInitializer
    where T : UnityEngine.Object {
    private readonly string _assetPath;

    private T _asset;

    public string LoaderId => $"Asset_{typeof(T).Name}";
    public int LoadPriority { get; }
    public bool IsLoaded { get; private set; }

    public AssetDataLoader(string assetPath, int priority = 10) {
        _assetPath = assetPath;
        LoadPriority = priority;
    }

    public async UniTask InitializeAsync(CancellationToken token = default) {
        if (IsLoaded)
            return;

        var request = Resources.LoadAsync<T>(_assetPath);

        await request.ToUniTask(cancellationToken: token);

        if (request.asset == null)
            throw new InvalidOperationException(
                $"Failed to load asset: {_assetPath}");

        _asset = (T)request.asset;
        IsLoaded = true;
    }

    public T Get() {
        if (!IsLoaded)
            throw new InvalidOperationException(
                $"[{LoaderId}] is not initialized.");

        return _asset;
    }

    public UniTask ResetAsync(CancellationToken token = default) {
        _asset = null;
        IsLoaded = false;

        return UniTask.CompletedTask;
    }
}
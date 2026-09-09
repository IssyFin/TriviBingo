using BingoTrivia.Validation;
using Cysharp.Threading.Tasks;
using DataManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using UnityEngine;

[Serializable]
public class QuestionData {
    public List<Question> Questions;
}
/// <summary>
/// Один вопрос с вариантами ответа.
/// [Serializable] — чтобы Unity JsonUtility мог им пользоваться напрямую,
/// а также чтобы Newtonsoft.Json/System.Text.Json тоже справлялись без проблем.
/// </summary>
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
    /// <summary>
    /// Возвращает count вопросов. Реализация сама решает: брать случайные,
    /// брать без повторов, тасовать колоду и т.д.
    /// </summary>
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

public class QuestionProvider : IQuestionProvider, IDataLoader {
    private readonly DataStore _dataStore;
    private readonly string _questionsFilePath;
    private readonly System.Random _random = new();
    private List<Question> _allQuestions;
    private bool _isLoaded = false;

    // Реализация IDataLoader
    public string LoaderId => "QuestionProvider";
    public int LoadPriority => 10; // Средний приоритет
    public bool IsLoaded => _isLoaded;

    public QuestionProvider(DataStore dataStore, string questionsFilePath) {
        _dataStore = dataStore;
        _questionsFilePath = questionsFilePath;
    }

    // Реализация IDataLoader.LoadDataAsync
    public async UniTask LoadDataAsync(CancellationToken token = default) {
        if (_isLoaded) return;

        try {
            var data = await _dataStore.LoadOrDefaultAsync<QuestionData>(
                _questionsFilePath,
                new QuestionData { Questions = new List<Question>() },
                token
            );
            _allQuestions = data.Questions;
            _isLoaded = true;

            Debug.Log($"[{LoaderId}] Loaded {_allQuestions.Count} questions");
        } catch (Exception ex) {
            Debug.LogError($"[{LoaderId}] Failed to load: {ex.Message}");
            throw;
        }
    }

    // Реализация IDataLoader.ResetAsync
    public async UniTask ResetAsync(CancellationToken token = default) {
        _isLoaded = false;
        _allQuestions = null;
        await LoadDataAsync(token);
    }

    // Бизнес-методы
    public List<Question> GetQuestions(int count) {
        if (!_isLoaded)
            throw new InvalidOperationException("QuestionProvider not loaded. Call LoadDataAsync first.");

        if (_allQuestions == null || _allQuestions.Count == 0)
            return new List<Question>();

        var actualCount = Math.Min(count, _allQuestions.Count);
        return _allQuestions.OrderBy(x => _random.Next()).Take(actualCount).ToList();
    }

    public async UniTask SaveQuestionsAsync(QuestionData data, CancellationToken token = default) {
        await _dataStore.SaveAsync(_questionsFilePath, data, token);
        _allQuestions = data.Questions;
    }
}


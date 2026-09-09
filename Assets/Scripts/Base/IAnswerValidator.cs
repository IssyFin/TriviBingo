using System;
using System.Collections.Generic;
using System.Linq;

namespace BingoTrivia.Validation {

    /// <summary>
    /// Абстракция для проверки правильности ответов.
    /// Позволяет реализовать разные стратегии проверки.
    /// </summary>
    public interface IAnswerValidator {
        bool IsCorrect(Question question, string userAnswer);
        string GetCorrectAnswer(Question question);
        string FormatAnswerOptions(Question question);
    }

    /// <summary>
    /// Валидатор, который пробует применить несколько стратегий по очереди.
    /// Возвращает true, если хотя бы одна стратегия посчитала ответ верным.
    /// </summary
    public class CompositeAnswerValidator : IAnswerValidator {
        private readonly List<IAnswerValidator> _validators;

        public CompositeAnswerValidator(params IAnswerValidator[] validators) {
            _validators = validators?.ToList() ?? new List<IAnswerValidator>();
        }

        public CompositeAnswerValidator(IEnumerable<IAnswerValidator> validators) {
            _validators = validators?.ToList() ?? new List<IAnswerValidator>();
        }

        public bool IsCorrect(Question question, string userAnswer) {
            if (string.IsNullOrWhiteSpace(userAnswer))
                return false;

            // Пробуем каждый валидатор по очереди
            foreach (var validator in _validators) {
                if (validator.IsCorrect(question, userAnswer)) {
                    return true;
                }
            }

            return false;
        }

        public string GetCorrectAnswer(Question question) {
            // Берем правильный ответ у первого валидатора в цепочке
            var firstValidator = _validators.FirstOrDefault();
            return firstValidator != null
                ? firstValidator.GetCorrectAnswer(question)
                : "Unknown";
        }

        public string FormatAnswerOptions(Question question) {
            var firstValidator = _validators.FirstOrDefault();
            return firstValidator != null
                ? firstValidator.FormatAnswerOptions(question)
                : string.Empty;
        }
    }

    /// <summary>
    /// Проверяет ответы по индексу (номер варианта ответа).
    /// </summary>
    public class IndexBasedAnswerValidator : IAnswerValidator {
        public bool IsCorrect(Question question, string userAnswer) {
            if (int.TryParse(userAnswer.Trim(), out int displayIndex)) {
                // В UI нумерация с 1, а в массиве — с 0
                int zeroBasedIndex = displayIndex - 1;
                return zeroBasedIndex == question.CorrectAnswerIndex;
            }
            return false;
        }

        public string GetCorrectAnswer(Question question) {
            if (question.CorrectAnswerIndex >= 0 && question.CorrectAnswerIndex < question.Answers.Count) {
                return question.Answers[question.CorrectAnswerIndex];
            }
            return "Unknown";
        }

        public string FormatAnswerOptions(Question question) {
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < question.Answers.Count; i++) {
                builder.AppendLine($"{i + 1}. {question.Answers[i]}");
            }
            return builder.ToString();
        }
    }

    public class TextMatchAnswerValidator : IAnswerValidator {
        private readonly StringComparison _comparisonType;

        public TextMatchAnswerValidator(StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) {
            _comparisonType = comparisonType;
        }

        public bool IsCorrect(Question question, string userAnswer) {
            if (string.IsNullOrWhiteSpace(userAnswer))
                return false;

            var correctAnswer = GetCorrectAnswer(question);
            string cleanInput = CleanInput(userAnswer);

            return string.Equals(cleanInput, correctAnswer.Trim(), _comparisonType);
        }

        public string GetCorrectAnswer(Question question) {
            if (question.CorrectAnswerIndex >= 0 && question.CorrectAnswerIndex < question.Answers.Count) {
                return question.Answers[question.CorrectAnswerIndex];
            }
            return string.Empty;
        }

        public string FormatAnswerOptions(Question question) {
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < question.Answers.Count; i++) {
                builder.AppendLine($"{question.Answers[i]}");
            }
            return builder.ToString();
        }

        private string CleanInput(string input) {
            input = input.Trim();
            // Если игрок ввел "1. Yes" или "1) Yes", отсекаем префикс с цифрой
            int dotIndex = input.IndexOfAny(new[] { '.', ')' });
            if (dotIndex > 0 && dotIndex < 3 && int.TryParse(input.Substring(0, dotIndex), out _)) {
                return input.Substring(dotIndex + 1).Trim();
            }
            return input;
        }
    }

    /// <summary>
    /// Поддерживает вопросы с несколькими правильными ответами.
    /// </summary>
    public class MultipleCorrectAnswersValidator : IAnswerValidator {
        public bool IsCorrect(Question question, string userAnswer) {
            if (string.IsNullOrWhiteSpace(userAnswer))
                return false;

            // Поддерживаем как одиночные, так и множественные ответы через запятую
            var userAnswers = userAnswer.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(a => a.Trim())
                                       .ToList();

            if (question.CorrectAnswers == null || question.CorrectAnswers.Count == 0)
                return false;

            if (userAnswers.Count != question.CorrectAnswers.Count)
                return false;

            return !userAnswers.Except(question.CorrectAnswers).Any();
        }

        public string GetCorrectAnswer(Question question) {
            return string.Join(", ", question.CorrectAnswers ?? new List<string>());
        }

        public string FormatAnswerOptions(Question question) {
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < question.Answers.Count; i++) {
                builder.AppendLine($"{i + 1}. {question.Answers[i]}");
            }
            return builder.ToString();
        }
    }
}

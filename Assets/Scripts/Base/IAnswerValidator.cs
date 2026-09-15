using System;
using System.Collections.Generic;
using System.Linq;

namespace BingoTrivia.Validation {

    /// <summary>
    /// Абстракция для проверки правильности ответов.
    /// Позволяет реализовать разные стратегии проверки.
    /// </summary>
    public interface IAnswerValidator {
        bool IsCorrect(QuestionData question, string userAnswer);
    }

    public class CompositeAnswerValidator : IAnswerValidator {
        private readonly List<IAnswerValidator> _validators;

        public CompositeAnswerValidator(params IAnswerValidator[] validators) {
            _validators = validators?.ToList() ?? new List<IAnswerValidator>();
        }

        public CompositeAnswerValidator(IEnumerable<IAnswerValidator> validators) {
            _validators = validators?.ToList() ?? new List<IAnswerValidator>();
        }

        public bool IsCorrect(QuestionData question, string userAnswer) {
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
    }

    public class IndexBasedAnswerValidator : IAnswerValidator {
        public bool IsCorrect(QuestionData question, string userAnswer) {
            if (!int.TryParse(userAnswer.Trim(), out int displayIndex))
                return false;

            int index = displayIndex - 1;

            return index >= 0 &&
                   index < question.Answers.Count &&
                   question.Answers[index].IsCorrect;
        }
    }

    public class TextMatchAnswerValidator : IAnswerValidator {
        private readonly StringComparison _comparisonType;

        public TextMatchAnswerValidator(
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) {
            _comparisonType = comparisonType;
        }

        public bool IsCorrect(QuestionData question, string userAnswer) {
            if (string.IsNullOrWhiteSpace(userAnswer))
                return false;

            string cleanInput = CleanInput(userAnswer);

            return question.Answers
                .Where(a => a.IsCorrect)
                .Any(a => string.Equals(
                    a.Text.Trim(),
                    cleanInput,
                    _comparisonType));
        }

        private string CleanInput(string input) {
            input = input.Trim();

            int index = input.IndexOfAny(new[] { '.', ')' });

            if (index > 0 &&
                index < 3 &&
                int.TryParse(input[..index], out _)) {
                return input[(index + 1)..].Trim();
            }

            return input;
        }
    }
}

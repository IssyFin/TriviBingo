using System;

public enum MessageType {
    Info,
    Success,
    Error,
    Warning
}

public interface IUIService {
    void DisplayBoard(string boardRepresentation);
    void DisplayQuestion(Question question, string answerOptions);
    void DisplayMessage(string message, MessageType type = MessageType.Info);
    string GetUserInput(string prompt);
}

public class ConsoleUIService : IUIService {
    public void DisplayBoard(string boardRepresentation) {
        Console.WriteLine(boardRepresentation);
    }

    public void DisplayQuestion(Question question, string answerOptions) {
        Console.WriteLine($"\n--- QUESTION ---");
        Console.WriteLine(question.Text);
        Console.WriteLine(answerOptions);
    }

    public void DisplayMessage(string message, MessageType type = MessageType.Info) {
        string prefix = type switch {
            MessageType.Success => "✅",
            MessageType.Error => "❌",
            MessageType.Warning => "⚠️",
            _ => "ℹ️"
        };

        Console.WriteLine($"{prefix} {message}");
    }

    public string GetUserInput(string prompt) {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }
}

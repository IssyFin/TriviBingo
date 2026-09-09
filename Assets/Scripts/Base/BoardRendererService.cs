using System;

public class BoardRendererService {
        private readonly BingoBoard _board;

        public BoardRendererService(BingoBoard board) {
            _board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public string RenderBoard() {
            int size = _board.Grid.Size;
            var builder = new System.Text.StringBuilder();

            // Header
            builder.Append("\n   ");
            for (int col = 0; col < size; col++) {
                builder.Append($" {(char)('A' + col)} ");
            }
            builder.AppendLine();

            // Rows
            for (int row = 0; row < size; row++) {
                builder.Append($" {row + 1} ");

                for (int col = 0; col < size; col++) {
                    var tile = _board.Grid.GetTile(row, col);
                    var state = _board.GetData(tile).State;

                    char symbol = state switch {
                        TileState.Unrevealed => '?',
                        TileState.Filled => 'O',
                        TileState.Dead => 'X',
                        _ => ' '
                    };

                    builder.Append($"[{symbol}]");
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }

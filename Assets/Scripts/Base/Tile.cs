public class Tile {
    public int Row { get; }
    public int Col { get; }

    public Tile(int row, int col) {
        Row = row;
        Col = col;
    }

    public override string ToString() {
        return $"{Col} {Row}";
    }
}

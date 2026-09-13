using System;
using System.Collections.Generic;

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
/// <summary>
/// Описывает структурную линию на сетке (ряд, колонка, диагональ).
/// </summary>
public class Line {
    public string Name { get; }
    public IReadOnlyList<Tile> Tiles { get; }

    public Line(string name, IReadOnlyList<Tile> tiles) {
        Name = name;
        Tiles = tiles;
    }
}

public class Grid {
    public int Size { get; }
    public IReadOnlyList<IReadOnlyList<Tile>> Rows => _tileRows;

    public int Count {
        get {
            int total = 0;
            foreach (var row in Rows) {
                total += row.Count;
            }
            return total;
        }
    }

    private readonly List<List<Tile>> _tileRows;

    public Grid(int size) {
        if (size < 2)
            throw new ArgumentException("Grid size must be at least 2.", nameof(size));

        Size = size;
        _tileRows = new List<List<Tile>>(size);

        for (int row = 0; row < size; row++) {
            var rowTiles = new List<Tile>(size);
            for (int col = 0; col < size; col++) {
                rowTiles.Add(new Tile(row, col));
            }
            _tileRows.Add(rowTiles);
        }
    }

    public Tile GetTile(int row, int col) {
        if (row < 0 || row >= Size || col < 0 || col >= Size)
            throw new ArgumentOutOfRangeException($"({row},{col}) is outside a {Size}x{Size} grid.");
        return _tileRows[row][col];
    }

    public Tile GetTileByAddress(string address) {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is empty.", nameof(address));

        address = address.Trim().ToUpperInvariant();
        char columnLetter = address[0];
        int col = columnLetter - 'A';

        string rowPart = address.Substring(1);
        if (!int.TryParse(rowPart, out int rowNumber))
            throw new ArgumentException($"Could not parse row number from address '{address}'.");

        int row = rowNumber - 1;

        if (col < 0 || col >= Size || row < 0 || row >= Size)
            throw new ArgumentException($"Address '{address}' is outside a {Size}x{Size} grid.");

        return _tileRows[row][col];
    }

    public IEnumerable<Tile> AllTiles() {
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                yield return _tileRows[row][col];
    }

    public IEnumerable<Line> AllLines() {
        for (int row = 0; row < Size; row++) {
            var rowTiles = new List<Tile>(Size);
            for (int col = 0; col < Size; col++) rowTiles.Add(_tileRows[row][col]);
            yield return new Line($"Row {row + 1}", rowTiles);
        }

        for (int col = 0; col < Size; col++) {
            var colTiles = new List<Tile>(Size);
            for (int row = 0; row < Size; row++) colTiles.Add(_tileRows[row][col]);
            yield return new Line($"Column {(char)('A' + col)}", colTiles);
        }

        var diagMain = new List<Tile>(Size);
        var diagAnti = new List<Tile>(Size);
        for (int i = 0; i < Size; i++) {
            diagMain.Add(_tileRows[i][i]);
            diagAnti.Add(_tileRows[i][Size - 1 - i]);
        }
        yield return new Line("Diagonal \u2199\u2197 (main)", diagMain);
        yield return new Line("Diagonal \u2198\u2196 (anti)", diagAnti);
    }
}

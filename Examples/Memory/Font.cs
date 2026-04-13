namespace Memory;

using SDL3;

public struct Glyph
{
    public SDL.FRect Source;
    public float XOffset;
    public float YOffset;
    public float XAdvance;
}

public class Font : IDisposable
{
    public int LineHeight { get; }
    public Texture Texture => texture;

    private readonly Texture texture;
    private readonly Dictionary<int, Glyph> glyphs;

    private Font(Texture texture, int lineHeight, Dictionary<int, Glyph> glyphs)
    {
        this.texture = texture;
        this.glyphs = glyphs;
        LineHeight = lineHeight;
    }

    public static Font Load(IntPtr sdlRenderer, string fntPath)
    {
        var resolved = Path.IsPathRooted(fntPath) ? fntPath : Path.Combine(AppContext.BaseDirectory, fntPath);
        var lines = File.ReadAllLines(resolved);
        var fntDir = Path.GetDirectoryName(resolved) ?? "";

        int lineHeight = 0;
        string? pageFile = null;
        var glyphs = new Dictionary<int, Glyph>();

        foreach (var line in lines)
        {
            if (line.Length == 0) continue;
            var tokens = ParseTokens(line, out string tag);
            switch (tag)
            {
                case "common":
                    if (tokens.TryGetValue("lineHeight", out var lh)) lineHeight = int.Parse(lh);
                    break;
                case "page":
                    if (tokens.TryGetValue("file", out var f)) pageFile = f;
                    break;
                case "char":
                    int id = int.Parse(tokens["id"]);
                    glyphs[id] = new Glyph
                    {
                        Source = new SDL.FRect
                        {
                            X = float.Parse(tokens["x"]),
                            Y = float.Parse(tokens["y"]),
                            W = float.Parse(tokens["width"]),
                            H = float.Parse(tokens["height"]),
                        },
                        XOffset = float.Parse(tokens["xoffset"]),
                        YOffset = float.Parse(tokens["yoffset"]),
                        XAdvance = float.Parse(tokens["xadvance"]),
                    };
                    break;
            }
        }

        if (pageFile == null)
            throw new Exception($"BMFont '{resolved}' has no page file");

        var texture = Texture.FromFile(sdlRenderer, Path.Combine(fntDir, pageFile));
        return new Font(texture, lineHeight, glyphs);
    }

    public bool TryGetGlyph(char c, out Glyph glyph) => glyphs.TryGetValue(c, out glyph);

    public float MeasureText(string text)
    {
        float w = 0;
        foreach (char c in text)
        {
            if (glyphs.TryGetValue(c, out var g))
                w += g.XAdvance;
        }
        return w;
    }

    public void Dispose()
    {
        texture.Dispose();
    }

    private static Dictionary<string, string> ParseTokens(string line, out string tag)
    {
        var result = new Dictionary<string, string>();
        int i = 0;
        while (i < line.Length && char.IsWhiteSpace(line[i])) i++;
        int tagStart = i;
        while (i < line.Length && !char.IsWhiteSpace(line[i])) i++;
        tag = line.Substring(tagStart, i - tagStart);

        while (i < line.Length)
        {
            while (i < line.Length && char.IsWhiteSpace(line[i])) i++;
            if (i >= line.Length) break;

            int keyStart = i;
            while (i < line.Length && line[i] != '=' && !char.IsWhiteSpace(line[i])) i++;
            if (i >= line.Length || line[i] != '=') continue;
            string key = line.Substring(keyStart, i - keyStart);
            i++;

            string value;
            if (i < line.Length && line[i] == '"')
            {
                int close = line.IndexOf('"', i + 1);
                if (close < 0) break;
                value = line.Substring(i + 1, close - i - 1);
                i = close + 1;
            }
            else
            {
                int valStart = i;
                while (i < line.Length && !char.IsWhiteSpace(line[i])) i++;
                value = line.Substring(valStart, i - valStart);
            }
            result[key] = value;
        }
        return result;
    }
}

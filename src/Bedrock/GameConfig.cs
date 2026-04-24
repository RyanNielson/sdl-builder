namespace Bedrock;

public record GameConfig(string Title, int WindowWidth, int WindowHeight, int TargetWidth, int TargetHeight, int? FixedFps = 60);
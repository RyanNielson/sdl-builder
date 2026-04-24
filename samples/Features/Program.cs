using Bedrock;
using Bedrock.Samples.Features;

using var game = new Game(new GameConfig("Memory", 512, 512, 128, 128, FixedFps: 60), new FeaturesScene());
game.Run();

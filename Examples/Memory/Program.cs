using Memory;

using var game = new Game(new("Memory", 800, 600, 128, 128), new MemoryScene());
game.Run();

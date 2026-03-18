using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "game-events",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

Console.Write("Enter your player name: ");
var playerName = Console.ReadLine()?.Trim();

if (string.IsNullOrEmpty(playerName))
{
    Console.WriteLine("No name entered, defaulting to 'PlayerOne'");
    playerName = "PlayerOne";
}

Console.WriteLine($"\n {playerName} connected. Commands: kill, score, quit\n");


while (true)
{
    var input = Console.ReadLine()?.Trim().ToLower();

    GameEvent? gameEvent = input switch
    {
        "kill"  => new GameEvent("PLAYER_KILL", playerName, 1, DateTime.Now),
        "score" => new GameEvent("SCORE_UPDATE", playerName, 100, DateTime.Now),
        "quit"  => null,
        _       => null
    };

    if (input == "quit") break;
    if (gameEvent == null)
    {
        Console.WriteLine("Unknown command. Try: kill, score, quit");
        continue;
    }

    var json = JsonSerializer.Serialize(gameEvent);
    var body = Encoding.UTF8.GetBytes(json);

    await channel.BasicPublishAsync(
        exchange: "",
        routingKey: "game-events",
        body: body
    );

    Console.WriteLine($"sent: {gameEvent.Type} by {gameEvent.Player}");
}

record GameEvent(string Type, string Player, int Value, DateTime Timestamp);
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
    exchange: "game-events-exchange",
    type: ExchangeType.Fanout
);

var playerName = args.Length > 0 ? args[0] : "Player1";
var targetPlayer = args.Length > 1 ? args[1] : "Player2";

Console.WriteLine($"\n💩 {playerName} connected. Target: {targetPlayer}. Commands: kill, score, quit\n");


while (true)
{
    var input = Console.ReadLine()?.Trim().ToLower();

    GameEvent? gameEvent = input switch
    {
        "kill"  => new GameEvent("PLAYER_KILL", playerName, targetPlayer, 1, DateTime.Now),
        "score" => new GameEvent("SCORE_UPDATE", playerName, null,  100, DateTime.Now),
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
        exchange: "game-events-exchange",
        routingKey: "",
        body: body
    );

    Console.WriteLine($"sent: {gameEvent.Type} by {gameEvent.Player}");
}

record GameEvent(string Type, string Player, string? TargetPlayer, int Value, DateTime Timestamp);
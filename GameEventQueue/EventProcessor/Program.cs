using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;


var scoreboard = new Dictionary<string, (int Kills, int Score)>();

var factory = new ConnectionFactory { HostName = "localhost"};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();


await channel.ExchangeDeclareAsync(
    exchange: "game-events-exchange",
    type: ExchangeType.Fanout
);

var queueResult = await channel.QueueDeclareAsync();
var queueName = queueResult.QueueName;

await channel.QueueBindAsync(
    queue: queueName,
    exchange: "game-events-exchange",
    routingKey: ""
);

Console.WriteLine("EventProcessor is now listening for game events... \n");

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
    var gameEvent = JsonSerializer.Deserialize<GameEvent>(json);

    if (gameEvent == null) return;

    if (!scoreboard.ContainsKey(gameEvent.Player))
        scoreboard[gameEvent.Player] = (0, 0);

    var current = scoreboard[gameEvent.Player];


    switch (gameEvent?.Type)
    {
        case "PLAYER_KILL":
            scoreboard[gameEvent.Player] = (current.Kills + 1, current.Score + 10);
            Console.WriteLine($"☠️  {gameEvent.Player} got a kill! (+{gameEvent.Value})");
            break;
        case "SCORE_UPDATE":
            scoreboard[gameEvent.Player] = (current.Kills, current.Score + gameEvent.Value);
            Console.WriteLine($"    {gameEvent.Player} scored {gameEvent.Value} points!");
            break;
        default:
            Console.WriteLine($"Unknown event: {json}");
            break;
    }

    PrintLeaderboard(scoreboard);

    await Task.CompletedTask;
};

await channel.BasicConsumeAsync(
    queue: "game-events",
    autoAck: true,
    consumer: consumer
);

Console.WriteLine("Press [enter] to exit");
Console.ReadLine();

static void PrintLeaderboard(Dictionary<string, (int Kills, int Score)> scoreboard)
{
    Console.WriteLine("\n ---- Leaderboard ----- ");
    var sorted = scoreboard.OrderByDescending(p => p.Value.Score);

    foreach (var player in sorted)
        Console.WriteLine($"  {player.Key,-12} | Kills: {player.Value.Kills} | Score: {player.Value.Score}");

    Console.WriteLine("------------------- \n");
}

record GameEvent(string Type, string Player, int Value, DateTime Timestamp);
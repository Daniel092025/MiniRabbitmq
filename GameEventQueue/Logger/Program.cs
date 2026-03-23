using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var logPath = "game-events.log";
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

Console.WriteLine($"Logger listening - writing to {logPath} \n");

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
    var gameEvent = JsonSerializer.Deserialize<GameEvent>(json);

    if (gameEvent == null) return;

    var logEntry = gameEvent.Type switch
    {
        "PLAYER_KILL"  => $"[{gameEvent.Timestamp:HH:mm:ss}] KILL  | {gameEvent.Player} killed {gameEvent.TargetPlayer}",
        "SCORE_UPDATE" => $"[{gameEvent.Timestamp:HH:mm:ss}] SCORE | {gameEvent.Player} +{gameEvent.Value} points",
        _              => $"[{gameEvent.Timestamp:HH:mm:ss}] UNKNOWN | {json}"
    };

    Console.WriteLine(logEntry);
    await File.AppendAllTextAsync(logPath, logEntry + Environment.NewLine);


};

    await channel.BasicConsumeAsync(
        queue: queueName,
        autoAck: true,
        consumer: consumer
    );

    Console.WriteLine("Press [enter] to exit");
    Console.ReadLine();



record GameEvent (string Type, string Player, string? TargetPlayer, int Value, DateTime Timestamp);


using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;


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

Console.WriteLine("Notifier is listening...");

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
    var gameEvent = JsonSerializer.Deserialize<GameEvent>(json);

    if (gameEvent == null) return;

    if (gameEvent.Type == "PLAYER_KILL" && gameEvent.TargetPlayer != null)
        Console.WriteLine($"    [{gameEvent.TargetPlayer}] You were killed by {gameEvent.Player}!");

    await Task.CompletedTask;
};

await channel.BasicConsumeAsync(
    queue: queueName,
    autoAck: true,
    consumer: consumer
);

Console.WriteLine ("Press [enter] to exit");
Console.ReadLine();


record GameEvent (string Type, string Player, string? TargetPlayer, int Value, DateTime TimeStamp);

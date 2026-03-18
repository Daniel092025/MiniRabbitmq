# GameEventQueing

Urlite prosjekt for å teste en eventlistener som RabbitMQ.

## Eventprocessor
- Tar imot og håndterer events
- Logger dem (events)

## GameServer
- publiserer events
- I dette tilfelle (foreløpig bare dette): kill og score


# For å teste / kjøre dette

## Docker
1. Installer Docker
2. Run `docker compose up -d` (bash)
3. Start EventProcessor, så GameServer

- cd EventProcessor && dotnet run (kjøre programmet fra EventProcessor)
- cd GameServer og kjør dotnet run (kjøre programmet fra GameServer)
- Trenger 2 terminaler. En for hver.

# GameEventQueing

Urlite prosjekt for å teste en eventlistener som RabbitMQ.

## Eventprocessor
- Tar imot og håndterer events
- Logger dem (events)

## GameServer
- publiserer events
- I dette tilfelle (foreløpig bare dette): kill og score

## Notifier
- For å sende notifikasjon til andre spillere
- Sende notifikasjon til Log

## Logger
- Notifikasjoner
- Skriver log / notifikasjoner til en TXT.fil / oppretter en TXT.fil med TimeStamp(s)

# For å teste / kjøre dette

## Docker
1. Installer Docker
2. Run `docker compose up -d` (bash)
3. Start EventProcessor, så GameServer

- cd EventProcessor && dotnet run (kjøre programmet fra EventProcessor)
- cd GameServer og kjør dotnet run (kjøre programmet fra GameServer)
- Trenger 2 terminaler. En for hver.

### Videreutvikling
- Trenger 4+ terminaler
- Start Eventprocessor, så Notifier, så Logger.
    - cd EventProcessor && dotnet run, cd Notifier && dotnet run, cd Logger && dotnet run
- Start GameServer, og en terminal til hver "player"
    - cd GameServer && dotnet run -- "navn" "navn på mostander"(x antall personer)
    - Eks. cd GameServer && dotnet run -- Pepsi Cola


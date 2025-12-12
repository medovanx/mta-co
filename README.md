# MTA CO Source

Conquer Online game server implementation in C#.

## Requirements

- .NET Framework 4.8
- Docker and Docker Compose

## Building

1. Open `MTA.sln` in Visual Studio
2. Restore packages (Visual Studio does this automatically, or run `nuget restore`)
3. Build the project:
   - Press `Ctrl+Shift+B` in Visual Studio
   - Or run: `msbuild MTA.sln /p:Configuration=Debug`

## Starting

1. Start the database:
   ```cmd
   docker-compose up -d
   ```

2. Start the server:
   ```cmd
   cd bin\Debug
   MTA.exe
   ```

## Taking a database dump

```cmd
mysqldump -h 127.0.0.1 -u root -pROOT --lock-all-tables --set-gtid-purged=OFF mta > mta.sql
```

## Compressing the Client
```cmd
7z a -t7z MTAConquer.7z "Client v6609  - MTA" -m0=lzma2 -mx=9 -mfb=273 -md=256m -ms=on
```
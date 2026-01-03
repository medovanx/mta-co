
# MTA CO - Private Conquer Online Server
A Conquer Online game server implementation in C#.

<div align="center">
    <img src="https://i.ibb.co/wr3HbFbc/logo.png" alt="Logo" height="100">
</div>

<div align="center">

[![C# 14](https://img.shields.io/badge/C%23-14-blue.svg)](https://learn.microsoft.com/dotnet/csharp/)
[![.NET 10](https://img.shields.io/badge/.NET-10-purple.svg)](https://dotnet.microsoft.com/)
[![MySQL 9.5](https://img.shields.io/badge/MySQL-9.5-4479A1.svg)](https://www.mysql.com/)

</div>

## Requirements

- C# v14
- .NET Framework 10
- Docker

## Building

1. Open `MTA.sln` in Visual Studio
2. Restore packages (Visual Studio does this automatically, or run `nuget restore`)
3. Build the project:
   - For debug: `msbuild MTA.sln /p:Configuration=Debug`
   - For release: `msbuild MTA.sln/p:Configuration=Release`

## Starting

1. Start the database:
   ```cmd
   docker compose up -d
   ```

2. Start the server:
   ```cmd
   cd bin\Release
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
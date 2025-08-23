# MTA CO Source

Conquer Online game server implementation in C#.

## Requirements

- Visual Studio 2019+ OR VS Code with C# extension
- .NET Framework 4.8
- MySQL Server

## How to Run

### Using Visual Studio
1. **Open the project**
   ```cmd
   Open MTA.sln in Visual Studio
   ```

2. **Restore packages** 
   - Visual Studio will restore NuGet packages automatically
   - Or run: `nuget restore`

3. **Build**
   - Press `Ctrl+Shift+B` in Visual Studio
   - Or use: `msbuild MTA.sln /p:Configuration=Debug`

### Using VS Code
1. **Open the project**
   ```cmd
   code .
   ```

2. **Restore packages**
   ```cmd
   dotnet restore
   ```

3. **Build**
   ```cmd
   dotnet build --configuration Debug
   ```

### Final Steps (Both IDEs)
4. **Setup database**
   - Install MySQL
   - Configure connection strings in config files

5. **Start server**
   ```cmd
   cd bin\Debug
   MTA.exe
   ```

## Common Issues

- **Build fails**: Make sure .NET Framework 4.8 is installed
- **Missing packages**: Run `dotnet restore` or `nuget restore`
- **Database errors**: Check MySQL connection settings
- **VS Code**: Install C# extension and .NET Framework Developer Pack
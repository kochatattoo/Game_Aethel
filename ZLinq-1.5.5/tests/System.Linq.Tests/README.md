# Run unit tests

## 1. Run tests with Visual Studio

1. Edit `Properties/launchSettings.json`
2. Select launch profile from top navigation bar.
3. Run tests with Start Without Debugging(`Ctrl+F5`)
4. Confirm unit tests execution is started.

## 2. Run tests with Visual Studio TestExplorer

It can also execute unit tests by using Test Explorer.

## 3. Run unit tests with `dotnet test` command

When running tests with `dotnet test` command.
It need to specify following parameters.

- `-tl:off`  
    This argument is required to disable TerminalLogger.
- `-p:TestingPlatformCaptureOutput=false`  
    This argument is required to show verbose logs to console.

## Example commands

### 1. Run all tests

```
dotnet test -c Release --framework net9.0 --no-build -tl:off  -p:TestingPlatformCaptureOutput=false
```

### 2. Run ZLinq tests

```
dotnet test -c Release --framework net9.0 --no-build -tl:off -p:TestingPlatformCaptureOutput=false -- --filter-namespace 'ZLinq.Tests'
```

### 3. Run System.Linq tests

```
dotnet test -c Release --framework net9.0 --no-build -tl:off -p:TestingPlatformCaptureOutput=false -- --filter-namespace 'System.Linq.Tests'
```

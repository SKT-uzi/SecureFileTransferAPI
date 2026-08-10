# Resumable File Transfer Demo

A .NET 8 sample backend for authenticated, resumable file uploads. It includes an ASP.NET Core API, Azure Functions queue processing, Azure Blob/File Share storage adapters, an optional Windows AMSI scanning service, and SQLite-backed demo accounts.

This repository is intended for local development and learning. Review the cryptography, authorization model, logging, and deployment configuration before using any part of it in production.

## Projects

- `API` - JWT authentication, upload session creation, chunk upload, health check, and Swagger UI.
- `RfuCore` - resumable upload parsing, range validation, and storage operations.
- `FunctionApp` - queue-triggered file completion and timer-based recovery jobs.
- `ScanningAPI` - optional Windows AMSI malware scanning service.
- `Entity` - Entity Framework Core models and SQLite/SQL Server configuration.
- `Common` - shared storage and utility helpers.

## Prerequisites

The complete solution is designed for Windows because `ScanningAPI` targets Windows AMSI. The API and FunctionApp projects target .NET 8.

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js LTS](https://nodejs.org/) for the npm installation commands below
- [Azurite](https://github.com/Azure/Azurite)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- Visual Studio 2022 or another .NET IDE

Install the local Azure tools with npm:

```powershell
npm install --global azurite
npm install --global azure-functions-core-tools@4 --unsafe-perm true
```

Confirm the tools are available:

```powershell
dotnet --version
azurite --version
func --version
```

If PowerShell blocks an npm-generated `.ps1` shim on Windows, run the corresponding `azurite.cmd` or `func.cmd` command instead.

## Clone and build

```powershell
git clone https://github.com/SKT-uzi/ResumableFileTransferDemo.git
Set-Location ResumableFileTransferDemo
dotnet restore ResumableFileTransferDemo.sln
dotnet build ResumableFileTransferDemo.sln --no-restore
```

## Local configuration

Create a local FunctionApp settings file. The real file is ignored by Git:

```powershell
Copy-Item FunctionApp/local.settings.example.json FunctionApp/local.settings.json
```

Generate local-only random values and configure the API with .NET user secrets:

```powershell
function New-RandomBase64([int]$byteCount) {
    $buffer = New-Object byte[] $byteCount
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($buffer) } finally { $rng.Dispose() }
    [Convert]::ToBase64String($buffer)
}

$encryptionKey = New-RandomBase64 32
$jwtKey = New-RandomBase64 64

dotnet user-secrets set "ResumableFileTransferDBConnectionString" "Data Source=../Rfu.demo.db" --project API/API.csproj
dotnet user-secrets set "RuntimefilesStorageConnectionString" "UseDevelopmentStorage=true" --project API/API.csproj
dotnet user-secrets set "EncryptionKey" $encryptionKey --project API/API.csproj
dotnet user-secrets set "JWTSecretKey" $jwtKey --project API/API.csproj

$functionSettingsPath = "FunctionApp/local.settings.json"
$functionSettings = Get-Content -Raw $functionSettingsPath | ConvertFrom-Json
$functionSettings.Values.EncryptionKey = $encryptionKey
$functionSettings | ConvertTo-Json -Depth 10 | Set-Content $functionSettingsPath -Encoding utf8
```

The SQLite database is created automatically and contains clearly marked demo accounts. The active demo software token is:

```text
00000000-0000-0000-0000-000000000001
```

## Run locally

Start each component in a separate terminal.

Terminal 1 - start Azurite from the repository root:

```powershell
azurite --silent --location .azurite
```

Terminal 2 - start the API:

```powershell
dotnet run --project API/API.csproj --launch-profile API
```

Swagger UI is available at `http://localhost:49319/swagger/index.html`.

Terminal 3 - start the function host:

```powershell
Set-Location FunctionApp
func start
```

## Verify the API

Run these commands from another PowerShell terminal:

```powershell
Invoke-RestMethod http://localhost:49319/health

$body = @{ token = "00000000-0000-0000-0000-000000000001" } | ConvertTo-Json
$auth = Invoke-RestMethod http://localhost:49319/auth -Method Post -ContentType "application/json" -Body $body
$auth
```

A successful authentication response includes a JWT. Use it as a Bearer token in Swagger when testing the file endpoints. Completing an upload places a message on the Azurite queue for FunctionApp processing.

## Optional Windows malware scanner

The scanner uses Windows AMSI and is disabled in the supplied FunctionApp example configuration. To run it locally:

```powershell
dotnet run --project ScanningAPI/ScanningAPI.csproj --launch-profile ScanningAPI
```

It listens at `http://localhost:26710/api/scanning`. Set `NeedScanning` to `true` in the local FunctionApp settings only when this service is running.

## Security

- Never commit real connection strings, tokens, keys, local databases, or `local.settings.json`.
- All committed accounts, phone numbers, paths, and software tokens are synthetic demo values.
- Private deployment documentation, generated output, local service bindings, and user-specific IDE settings are excluded from Git.
- The included encryption implementation exists for compatibility with the demo and should be replaced with a modern, reviewed design before production use.

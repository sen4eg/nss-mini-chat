param(
    [switch]$OnBuild
)

# Function to handle errors
function Handle-Error {
    param([string]$errorMessage)
    Write-Host "Error: $errorMessage"
    exit 1
}

try {
    # Get the path to the global NuGet packages
    $globalPackagesPath = & dotnet nuget locals global-packages -l | ForEach-Object { $_ -replace 'global-packages: ', '' }

    # Check if grpc.tools is installed
    $grpcToolsPath = Join-Path $globalPackagesPath "grpc.tools"
    if (-Not (Test-Path $grpcToolsPath)) {
        # Automatically install gRPC.Tools using dotnet add package if not installed
        Write-Host "Installing latest version of gRPC.Tools package..."
        & dotnet add package Grpc.Tools
        Write-Host "gRPC.Tools package installed successfully."
        # Now retrieve the path again after installation
        $grpcToolsPath = Join-Path $globalPackagesPath "grpc.tools"
    }

    # Find the latest version of gRPC.Tools
    $latestVersion = Get-ChildItem $grpcToolsPath | Sort-Object Name -Descending | Select-Object -First 1 -ExpandProperty Name
    if (-not $latestVersion) {
        Handle-Error "Failed to determine the latest version of gRPC.Tools."
    }

    # Output the latest version and the path to tools
    Write-Host "Found latest version of gRPC.Tools: $latestVersion"

    # Construct the path to protoc.exe
    $protocPath = Join-Path $grpcToolsPath "$latestVersion\tools\windows_x64\protoc.exe"
    Write-Host "Path to protoc.exe: $protocPath"

    # Ensure the protoc.exe path exists
    if (-Not (Test-Path $protocPath)) {
        Handle-Error "protoc.exe not found at $protocPath"
    }

    # Always reset the Generated folder
    $generatedPath = Join-Path $PSScriptRoot "Generated"
    if (Test-Path $generatedPath) {
        Remove-Item -Recurse -Force $generatedPath
    }
    New-Item -ItemType Directory -Path $generatedPath

    # Get the path to the gRPC C# plugin
    $grpcCsharpPluginPath = Join-Path $grpcToolsPath "$latestVersion\tools\windows_x64\grpc_csharp_plugin.exe"
    Write-Host "Path to grpc_csharp_plugin.exe: $grpcCsharpPluginPath"

    # Ensure the grpc_csharp_plugin.exe path exists
    if (-Not (Test-Path $grpcCsharpPluginPath)) {
        Handle-Error "grpc_csharp_plugin.exe not found at $grpcCsharpPluginPath"
    }

    # Generate C# files from .proto files
    $protoPath = Join-Path $PSScriptRoot "..\MiniServer\Protos\chat.proto"
    $protoDir = Split-Path $protoPath -Parent
    $protoOutputPath = $generatedPath
    & $protocPath --proto_path=$protoDir --csharp_out=$protoOutputPath --grpc_out=$protoOutputPath --plugin=protoc-gen-grpc=$grpcCsharpPluginPath $protoPath

    Write-Host "Protobuf generation complete."
}
catch {
    Handle-Error $_.Exception.Message
}

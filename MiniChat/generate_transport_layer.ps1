# Get the path to the global NuGet packages
$globalPackagesPath = & dotnet nuget locals global-packages -l | ForEach-Object { $_ -replace 'global-packages: ', '' }

# Check if grpc.tools is installed
$grpcToolsPath = Join-Path $globalPackagesPath "grpc.tools"
if (-Not (Test-Path $grpcToolsPath)) {
    Write-Host "gRPC.Tools package not found. Installing the latest version..."
    & dotnet add package Grpc.Tools
}

# Find the latest version of gRPC.Tools
$latestVersion = Get-ChildItem $grpcToolsPath | Sort-Object Name -Descending | Select-Object -First 1 -ExpandProperty Name

# Output the latest version and the path to tools
Write-Host "Found latest version of gRPC.Tools: $latestVersion"

# Construct the path to protoc.exe
$protocPath = Join-Path $grpcToolsPath "$latestVersion\tools\windows_x64\protoc.exe"
Write-Host "Path to protoc.exe: $protocPath"

# Ensure the protoc.exe path exists
if (-Not (Test-Path $protocPath)) {
    Write-Host "Error: protoc.exe not found at $protocPath"
    exit 1
}

# Reset the Generated folder
$generatedPath = "Generated"
if (Test-Path $generatedPath) {
    Remove-Item -Recurse -Force $generatedPath
}
New-Item -ItemType Directory -Path $generatedPath

# Get the path to the gRPC C# plugin
$grpcCsharpPluginPath = Join-Path $grpcToolsPath "$latestVersion\tools\windows_x64\grpc_csharp_plugin.exe"
Write-Host "Path to grpc_csharp_plugin.exe: $grpcCsharpPluginPath"

# Ensure the grpc_csharp_plugin.exe path exists
if (-Not (Test-Path $grpcCsharpPluginPath)) {
    Write-Host "Error: grpc_csharp_plugin.exe not found at $grpcCsharpPluginPath"
    exit 1
}

# Generate C# files from .proto files
$protoPath = "..\MiniServer\Protos\chat.proto"
$protoDir = Split-Path $protoPath -Parent
$protoOutputPath = "Generated"
& $protocPath --proto_path=$protoDir --csharp_out=$protoOutputPath --grpc_out=$protoOutputPath --plugin=protoc-gen-grpc=$grpcCsharpPluginPath $protoPath

Write-Host "Protobuf generation complete."

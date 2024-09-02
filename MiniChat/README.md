Frontend side of the project

Used design patterns:
   - Singleton (Model/ClientState.cs)
   - Adapter (Model/Message.cs : line 18)

Cache: no

## Setup Instructions

Before building the project, please generate the necessary transport layer files using the following steps:

1. Open a PowerShell terminal.
2. Navigate to the root directory of your project.
3. Execute the `generate_transport_layer.ps1` script by running:
   ```powershell
   .\generate_transport_layer.ps1

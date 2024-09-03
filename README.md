MiniChat NSS Project

This repository contains C# .NET server and C# MAUI application meant to be compiled both as windows and android application.
Connection between application and server is based on gRPC calls and streams.
Since repository is splet to Server part and Client part Readme's files are can be found in coresponding subdirectories.

- vyuziti spolecne DB (relacni nebo grafova) (povinné) 
- - postgre
- vyuziti cache (napriklad Hazelcast) (volitelné -2b pokud není) 
- - ne
- vyuziti messaging principu (Kafka nebo JMS) (volitelné -2b pokud není) 
- - ne
- aplikace bude zabezpecena pomoci bud basic authorization nebo pomoci OAuth2 (volitelné -2b
pokud není)
- - ano, samopsany OAuth2 (MiniServer/Utils a MiniServer/Services/AuthenticationService)
- vyuziti Inteceptors (alespon jedna trida) - napriklad na logovani (prijde request a zapiseme ho do
logu) (volitelné -2b pokud není) 
- - ne
- vyuziti jedne z technologie: SOAP, REST, graphQL, Java RMI, Corba, XML-RPC (volitelné -2b
pokud není) 
- - gRPC misto toho

Used design patterns:
   - Factory (MiniServer/Core/Events/EventFactory.cs)
   - Template Method (MiniServer/Core/Events/Eventbase.cs)
   - Singleton (MiniChat/Model/ClientState.cs)
   - Adapter (MiniChat/Model/Message.cs : line 18)
   - Builder(built-in .net framework used widely for initialization)

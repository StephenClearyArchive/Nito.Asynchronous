set ver=1.3

set WinSDK=c:\Program Files\Microsoft SDKs\Windows\v6.0A

"%WinSDK%\bin\al.exe" /link:Nito.Async.config /out:policy.1.0.Nito.Async.dll /keyfile:"..\..\..\_\Code Signing\Certificates\Microsoft Strong Name\Microsoft Strong Name Private Key.snk" /platform:anycpu /comp:"Nito Programs" /nologo /product:"Nito Libraries" /title:"Helper classes for asynchronous programming" /copyright:"Copyright (c) Nito Programs 2009" /v:%ver%.0.0
"%WinSDK%\bin\al.exe" /link:Nito.Async.config /out:policy.1.1.Nito.Async.dll /keyfile:"..\..\..\_\Code Signing\Certificates\Microsoft Strong Name\Microsoft Strong Name Private Key.snk" /platform:anycpu /comp:"Nito Programs" /nologo /product:"Nito Libraries" /title:"Helper classes for asynchronous programming" /copyright:"Copyright (c) Nito Programs 2009" /v:%ver%.0.0
"%WinSDK%\bin\al.exe" /link:Nito.Async.config /out:policy.1.2.Nito.Async.dll /keyfile:"..\..\..\_\Code Signing\Certificates\Microsoft Strong Name\Microsoft Strong Name Private Key.snk" /platform:anycpu /comp:"Nito Programs" /nologo /product:"Nito Libraries" /title:"Helper classes for asynchronous programming" /copyright:"Copyright (c) Nito Programs 2009" /v:%ver%.0.0

"%WinSDK%\bin\al.exe" /link:Nito.Async.Sockets.config /out:policy.1.0.Nito.Async.Sockets.dll /keyfile:"..\..\..\_\Code Signing\Certificates\Microsoft Strong Name\Microsoft Strong Name Private Key.snk" /platform:anycpu /comp:"Nito Programs" /nologo /product:"Nito Libraries" /title:"Helper classes for asynchronous programming" /copyright:"Copyright (c) Nito Programs 2009" /v:%ver%.0.0
"%WinSDK%\bin\al.exe" /link:Nito.Async.Sockets.config /out:policy.1.1.Nito.Async.Sockets.dll /keyfile:"..\..\..\_\Code Signing\Certificates\Microsoft Strong Name\Microsoft Strong Name Private Key.snk" /platform:anycpu /comp:"Nito Programs" /nologo /product:"Nito Libraries" /title:"Helper classes for asynchronous programming" /copyright:"Copyright (c) Nito Programs 2009" /v:%ver%.0.0
"%WinSDK%\bin\al.exe" /link:Nito.Async.Sockets.config /out:policy.1.2.Nito.Async.Sockets.dll /keyfile:"..\..\..\_\Code Signing\Certificates\Microsoft Strong Name\Microsoft Strong Name Private Key.snk" /platform:anycpu /comp:"Nito Programs" /nologo /product:"Nito Libraries" /title:"Helper classes for asynchronous programming" /copyright:"Copyright (c) Nito Programs 2009" /v:%ver%.0.0

pause
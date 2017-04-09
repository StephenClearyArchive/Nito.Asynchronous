# Nito.Asynchronous

Nito.Async is a collection of classes to assist asynchronous programming in .NET, particularly the development and use of classes adhering to the event-based asynchronous pattern.

_Note: If you're looking for Nito.AsyncEx (a library for working with the `async`/`await` and Tasks), that is a [separate project](https://github.com/StephenCleary/AsyncEx)._

## Included Classes

`Nito.Async` includes:
- `ActionThread` - A child thread with an event-driven main loop sufficient to own and synchronize event-based asynchronous components.
- `Timer` - A general-purpose timer using the asynchronous event-based model.
- `GenericSynchronizingObject` - An implementation of `ISynchronizeInvoke` that uses `SynchronizationContext` to synchronize.
- `CallbackContext` - A context manager for asynchronous callbacks.
- `AsyncResultEventArgs<T>` - An `AsyncCompletedEventArgs` with a single result value of type `T`.
- `ActionDispatcher` - A queue of actions, providing a main loop with a `SynchronizationContext` for child threads.
- `Sync.Synchronize` - Wrappers for various delegate types that provide synchronization using `AsyncOperation`. These allow fairly easy-to-write translations from the `IAsyncResult` pattern to the event-based asynchronous pattern.

## Nito.Async.Sockets Future

The current `Nito.Async.Sockets` API has been frozen. A new (v2) API will be developed that provides better separation between protocol components (e.g., type of message framing, keepalive system, etc., will all be orthogonal). The new API _may_ be based on the Rx framework, but is more likely to be tied into the .NET 4.0 Task Parallel Library.

## Nito.Async.Sockets v1 Classes
- `IAsyncTcpConnection`, `ClientTcpSocket`, `ServerTcpSocket`, and `ServerChildTcpSocket` - Sockets using the event-based asynchronous pattern.
- `SocketPacketProtocol` - A class that provides a packet protocol for socket connections, including keepalive packets.
- `ISimpleAsyncTcpConnection`, `SimpleClientTcpSocket`, `SimpleServerTcpSocket`, and `SimpleServerChildTcpSocket` - Sockets using the event-based asynchronous pattern with a packet protocol.

## More on Nito.Async.Sockets v1

One major problem faced by newcomers to TCP/IP is thread synchronization. It can be difficult to learn networking concepts and multithreading at the same time.

The `ClientTcpSocket` and `ServerTcpSocket` classes provide simple thread synchronization by exposing an event-based API for TCP/IP sockets.

There are two other common problems that are discovered when designing a TCP/IP protocol:
1. Non-packetized data. Most people read about how TCP/IP uses packets, and they believe that message boundaries are preserved. Local-machine testing confirms this misunderstanding. However, TCP/IP sockets deal with a stream of bytes, not messages.
2. Detection of dropped connections. Again, people read about TCP/IP's keepalive packets, but don't realize that they are pretty much useless.

The `SimpleClientTcpSocket` and `SimpleServerTcpSocket` solve these problems by defining a protocol with message boundaries and automatic keepalive messages.

For more information on TCP/IP socket handling in .NET, see the [TCP/IP .NET Sockets FAQ](https://blog.stephencleary.com/2009/04/tcpip-net-sockets-faq.html).

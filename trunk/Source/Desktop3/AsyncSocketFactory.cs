namespace Nito.Communication
{
    public static class AsyncSocketFactory
    {
#if !SILVERLIGHT
        public static IAsyncServerTcpSocket CreateServer()
        {
            return new EventArgsAsyncServerTcpSocket(AsyncDelegateSchedulerFactory.Create());
            return new BeginEndAsyncServerTcpSocket(AsyncDelegateSchedulerFactory.Create());
        }
#endif

        public static IAsyncClientTcpSocket CreateClient()
        {
#if SILVERLIGHT
            return new EventArgsAsyncClientTcpSocket(AsyncDelegateSchedulerFactory.Create());
#else
            return new BeginEndAsyncClientTcpSocket(AsyncDelegateSchedulerFactory.Create());
#endif
        }
    }
}

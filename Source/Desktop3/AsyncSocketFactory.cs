namespace Nito.Communication
{
    public static class AsyncSocketFactory
    {
        public static IAsyncServerTcpSocket CreateServer()
        {
#if SILVERLIGHT
            return new EventArgsAsyncServerTcpSocket(AsyncDelegateSchedulerFactory.Create());
#else
            return new BeginEndAsyncServerTcpSocket(AsyncDelegateSchedulerFactory.Create());
#endif
        }

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

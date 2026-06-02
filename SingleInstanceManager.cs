using System.Threading;

namespace PeekMemo
{
    public static class SingleInstanceManager
    {
        private static Mutex mutex;

        public static bool IsFirstInstance()
        {
            bool createdNew;

            mutex = new Mutex(
                true,
                @"Local\PeekMemo_Single_Instance_2026",
                out createdNew);

            return createdNew;
        }
    }
}

namespace Amethyst_game_engine.Core;

internal static class ManagedTask
{
    public static int maxThreadsCount = 1;
    public static int workerThreads = 0;

    public static Task Run(Task task)
    {
        if (workerThreads == maxThreadsCount)
        {
            task.RunSynchronously();

            return Task.CompletedTask;
        }
        else
        {
            Interlocked.Increment(ref workerThreads);

            return Task.Run(() =>
            {
                try
                {
                    task.Start();
                }
                finally
                {
                    Interlocked.Decrement(ref workerThreads);
                }
            });

        }
    }
}

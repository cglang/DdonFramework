//using Test.ConsoleApp;

//DelayQueueDemo.Run();

//await DelayQueueDemo.SRun();

SortedSet<long> longs = new();


List<Task> ts = new();

for (int i = 0; i < 2; i++)
{
    var t = Task.Run(() =>
     {
         Random random = new Random();
         for (int i = 0; i < 100000; i++)
         {
             longs.Add(random.NextInt64(long.MinValue, long.MaxValue));
             if (i % 1000 == 0) Console.WriteLine(i);
         }
     });
    ts.Add(t);
}

await Task.WhenAll(ts);


Console.WriteLine("=======结束=======");
Console.ReadKey();
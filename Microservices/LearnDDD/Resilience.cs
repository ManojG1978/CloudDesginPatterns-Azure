using Polly;
using Polly.Timeout;
using System;
using System.Threading;

namespace LearnDDD
{
    public class Resilience
    {
        public static void ShowTimeOutPolicy()
        {
            var timeoutPolicy = Policy
                .Timeout(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic);

            try
            {
                timeoutPolicy.Execute(
                    () =>
                    {
                        //Some long running process
                        Console.WriteLine("Sleeping for 5 seconds");
                        Thread.Sleep(5000);
                    });
            }
            catch (TimeoutRejectedException e)
            {
                Console.WriteLine("TimeOut exception encountered");
            }
            Console.WriteLine("TimeOut policy completed");

        }

        public static void ShowRetryPolicy()
        {
            var count = 0;
            var retry = Policy.Handle<ArgumentException>().WaitAndRetry(
                retryCount: 3,
                sleepDurationProvider: (c) => TimeSpan.FromSeconds(2),
                onRetry: (e, t, c) =>
                {
                    count++;
                    Console.WriteLine($"Encountered exception [{e.GetType().Name}]. " +
                                      $"Retry attempt [{count}] after wait of {t.Seconds} seconds");
                });

            retry.Execute(() =>
            {
                //Console.WriteLine($"Retry: {count} attempt");
                if (count < 3)
                    throw new ArgumentException();

                Console.WriteLine("Successfully finished operation");
            });
        }

        public static void ShowCircuitBreaker()
        {
            var breaker = Policy
                .Handle<ArgumentException>()
                .CircuitBreaker(
                    exceptionsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(6),
                    onBreak: (ex, breakDelay) =>
                    {
                        Console.Write($"Breaking the circuit for {breakDelay.TotalSeconds} seconds!");
                        Console.WriteLine($"..due to: {ex.GetType().Name}");
                    },
                    onReset: () => Console.WriteLine($"Closed the circuit now!"),
                    onHalfOpen: () => Console.WriteLine($"Half-open: Next call is a trial!")
                    );
            
            var retries = 0;
            var retry = Policy.Handle<Exception>()
                .WaitAndRetryForever(
                    attempt => TimeSpan.FromSeconds(3),
                    (exception, calculatedWaitDuration) =>
                    {
                        Console.WriteLine($"Handled {exception.GetType().Name}, retry attempt [{++retries}]. " +
                                          $"WaitDuration: [{calculatedWaitDuration.Seconds} secs]");
                    });

            retry.Execute(() =>
            {
                breaker.Execute(() =>
                {
                    Console.WriteLine("Executing an operation within the Circuit breaker");
                    //Some operation that constantly throws an exception
                    if (retries < 5) throw new ArgumentException();
                });
            });

            Console.WriteLine();
            Console.WriteLine("Completed operation successfully.");

        }



    }
}

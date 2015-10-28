# Interlocker

Interlocker is a tiny .NET library that encapsulates the boilerplate code often
needed for performing interlocked updates using
[`Interlocked.CompareExchange`][cmpxchg].

Interlocker is available from NuGet in two formats:

  * [Portable Class Library][pclpkg]  
    [![NuGet version](https://badge.fury.io/nu/Interlocker.svg)](http://badge.fury.io/nu/Interlocker)
  * [Single C# source][srcpkg] for direct inclusion in your project:  
    [![NuGet version](https://badge.fury.io/nu/Interlocker.Source.svg)](http://badge.fury.io/nu/Interlocker.Source)

Interlocker comes with the generic type `Interlocked<T>` that is designed to
hold a *logically* shared state that will be the subject of interlocked
updates. It has a single (overloaded) method called `Update` that is used to
perform the actual update.

The following example shows how to create an instance of `Interlocked<T>` and
update it:

```c#
var x = Interlocked.Create(new[] { 42 });
Console.WriteLine(x.Update(cx => new[] { cx[0] * 2 }));

// Interlocked<T>, like Interlocked.CompareExchange, does not allow value types
// (struct) as the generic type argument for T so the integer is explicitly
// boxed above via an array.
```

`Update` takes a function that computes the new state based on the current one
(`cx` in the example above). The function may be called several times during
a single `Update`, once for each time the interlocked update fails. On
each iteration of an *update attempt*, the function will then compute the new
state based on the latest one.

Following is a more involved example that creates a shared list of integers
(`nums`) and then launches 50 threads to concurrently append to that list:

```c#
var nums = Interlocked.Create(new int[0]);
var go = new ManualResetEvent(false);
var threadz =
    from n in Enumerable.Range(1, 50)
    select new Thread(() =>
    {
        go.WaitOne();
        nums.Update(ns =>
        {
            var nns = new int[ns.Length + 1];
            ns.CopyTo(nns, 0);
            nns[ns.Length] = n;
            return nns;
        });
    });
var threads = threadz.ToArray();        // Ready!
Array.ForEach(threads, t => t.Start()); // Set!
go.Set();                               // Go!
Array.ForEach(threads, t => t.Join());
Console.WriteLine("[{0}] = {{{1}}}",
                  nums.Value.Length,
                  string.Join(",", nums.Value));
```

Note that `nums` is said to be *logically* a shared *list* because the actual
storage, the integer array, changes and completely re-allocated with each
update.

If the function to the `Update` returns `null`, the update is aborted.

The return value of `Update` is the successfully updated value.

An overload of `Update` supplies the update function with a second parameter
that is the iteration number (`i` in the next example). It is also possible
to return any type of value form the update function. When you do this, you
have supply two more functions to `Update`: first which projects the update
and second which projects the return value from `Update`. The next example
builds on the previous to demonstrate these ideas.

```c#
var nums = Interlocked.Create(new int[0]);
var go = new ManualResetEvent(false);
var threadz =
    from n in Enumerable.Range(1, 50)
    select new Thread(() =>
    {
        go.WaitOne();
        var attempts = nums.Update((ns, i) =>
        {
            var nns = new int[ns.Length + 1];
            ns.CopyTo(nns, 0);
            nns[ns.Length] = n;
            return new { Update = nns, Attempts = i + 1 };
        }, e => e.Update,    // The update 
           e => e.Attempts); // Return from update
        if (attempts > 1)
            Console.WriteLine($"Thread #{n} succeeded after {attempts} attempts.");
    });
var threads = threadz.ToArray();        // Ready!
Array.ForEach(threads, t => t.Start()); // Set!
go.Set();                               // Go!
Array.ForEach(threads, t => t.Join());
Console.WriteLine("[{0}] = {{{1}}}",
                  nums.Value.Length,
                  string.Join(",", nums.Value));
```

  [pclpkg]: https://www.nuget.org/packages/Interlocker/
  [srcpkg]: https://www.nuget.org/packages/Interlocker.Source/
  [cmpxchg]: https://msdn.microsoft.com/en-us/library/bb297966(v=vs.110).aspx

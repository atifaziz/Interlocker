# Interlocker

[![Build Status][build-badge]][builds]
[![NuGet][nuget-badge]][nuget-pkg]
[![MyGet][myget-badge]][edge-pkgs]

Interlocker is a tiny [.NET Standard][netstd] library that encapsulates the
boilerplate code often needed for performing interlocked updates using
[`Interlocked.CompareExchange`][cmpxchg].

  [netstd]: https://docs.microsoft.com/en-us/dotnet/articles/standard/library

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
var samples = Enumerable.Range(1, 50).ToArray();
var nums = Interlocked.Create(new int[0]);

var start   = new Barrier(samples.Length + 1);
var finish  = new Barrier(start.ParticipantCount);
var threads =
    from n in samples
    select new Thread(() =>
    {
        start.SignalAndWait(); // wait until everyone is running/ready
        nums.Update(ns =>
        {
            var nns = new int[ns.Length + 1];
            ns.CopyTo(nns, 0);
            nns[ns.Length] = n;
            return nns;
        });
        finish.SignalAndWait();
    });

Array.ForEach(threads.ToArray(), t => t.Start());
start.SignalAndWait();
finish.SignalAndWait();

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
to return any type of value from the update function. When you do this, you
have supply two more functions to `Update`: first which projects the update
and second which projects the return value from `Update`. The next example
builds on the previous to demonstrate these ideas.

```c#
var samples = Enumerable.Range(1, 50).ToArray();
var nums = Interlocked.Create(new int[0]);

var start   = new Barrier(samples.Length + 1);
var finish  = new Barrier(start.ParticipantCount);
var threads =
    from n in samples
    select new Thread(() =>
    {
        start.SignalAndWait(); // wait until everyone is running/ready

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

        finish.SignalAndWait();
    });

Array.ForEach(threads.ToArray(), t => t.Start());
start.SignalAndWait();
finish.SignalAndWait();

Console.WriteLine("[{0}] = {{{1}}}",
                  nums.Value.Length,
                  string.Join(",", nums.Value));
```

Instead of returning an anonymous type from the updater function, yet another
`Update` overload permits returning a [tuple of two][tuple2] where the first
item is always the update (and typed such) and the second the return value of
`Update` (and where in fact the second item can also be an anonymous type).
The benefit of this is that you don't have to supply additional function's for
projecting the updater functions return value. The example below demonstrates
the approach with returning a tuple:

```c#
var samples = Enumerable.Range(1, 50).ToArray();
var nums = Interlocked.Create(new int[0]);

var start   = new Barrier(samples.Length + 1);
var finish  = new Barrier(start.ParticipantCount);
var threads =
    from n in samples
    select new Thread(() =>
    {
        start.SignalAndWait(); // wait until everyone is running/ready

        var attempts = nums.Update((ns, i) =>
        {
            var nns = new int[ns.Length + 1];
            ns.CopyTo(nns, 0);
            nns[ns.Length] = n;
            return (nns, i + 1);
        });

        if (attempts > 1)
            Console.WriteLine($"Thread #{n} succeeded after {attempts} attempts.");

        finish.SignalAndWait();
    });

Array.ForEach(threads.ToArray(), t => t.Start());
start.SignalAndWait();
finish.SignalAndWait();

Console.WriteLine("[{0}] = {{{1}}}",
                  nums.Value.Length,
                  string.Join(",", nums.Value));
```


  [cmpxchg]: https://msdn.microsoft.com/en-us/library/bb297966(v=vs.110).aspx
  [tuple2]: https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple-2?view=netstandard-2.0
  [build-badge]: https://img.shields.io/appveyor/ci/raboof/interlocker.svg
  [myget-badge]: https://img.shields.io/myget/raboof/v/Interlocker.svg?label=myget
  [edge-pkgs]: https://www.myget.org/feed/raboof/package/nuget/Interlocker
  [nuget-badge]: https://img.shields.io/nuget/v/Interlocker.svg
  [nuget-pkg]: https://www.nuget.org/packages/Interlocker
  [builds]: https://ci.appveyor.com/project/raboof/interlocker

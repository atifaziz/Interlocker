# Interlocker

Interlocker is a tiny .NET library that encapsulates the boilerplate code often
needed for performing interlocked updates using
[`Interlocked.CompareExchange`][cmpxchg].

Interlocker is available from NuGet in two formats:

  * [Portable Class Library][pclpkg]  
    [![NuGet version](https://badge.fury.io/nu/Interlocker.svg)](http://badge.fury.io/nu/Interlocker)
  * [Single C# source][srcpkg] for direct inclusion in your project:  
    [![NuGet version](https://badge.fury.io/nu/Interlocker.Source.svg)](http://badge.fury.io/nu/Interlocker.Source)


  [pclpkg]: https://www.nuget.org/packages/Interlocker/
  [srcpkg]: https://www.nuget.org/packages/Interlocker.Source/
  [cmpxchg]: https://msdn.microsoft.com/en-us/library/bb297966(v=vs.110).aspx

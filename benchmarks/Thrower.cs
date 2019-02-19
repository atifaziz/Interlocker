using System;

namespace Interlocker
{
    internal static class Thrower
    {
        internal static void ArgumentNull()
        {
            throw new ArgumentNullException("update");
        }
    }
}

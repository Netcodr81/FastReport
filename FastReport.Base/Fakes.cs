//#define USE_FAKES

// This file represent mock for System.DateTime & System.Guid (because Microsoft Fakes doesn't correct work in .Net Core)

using System.Runtime.CompilerServices;

namespace SystemFake
{
    internal struct DateTime
    {
        internal static System.DateTime Now
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.DateTime.Now;
            }
        }


        internal static System.DateTime UtcNow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.DateTime.UtcNow;
            }
        }
    }


    internal struct Guid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static System.Guid NewGuid()
        {
            return System.Guid.NewGuid();
        }
    }
}

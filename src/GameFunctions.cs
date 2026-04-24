using System.Runtime.InteropServices;
using BmSDK;

namespace Samuil1337.CharacterSwapping
{
    static class GameFunctions
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate int GetSavedDamageLevelForSkinNameDelegate(FString* skinName);

        internal static readonly GetSavedDamageLevelForSkinNameDelegate GetSavedDamageLevelForSkinName =
            Marshal.GetDelegateForFunctionPointer<GetSavedDamageLevelForSkinNameDelegate>(
                MemUtil.GetBaseAddress() + 0x821550
            );

        internal static string PlayerChosenSkinName =>
            Marshal.PtrToStructure<FString>(MemUtil.GetBaseAddress() + 0x12BB068).ToString()!;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace p5rpc.flowutils.Memory
{
    public unsafe class MemoryStuffs
    {
        // persona slot 0 starts at 0x142851d74 for steam 1.0.4
        [StructLayout(LayoutKind.Explicit, Size = 0x30)]
        public struct JokerPersona
        {
            [FieldOffset(0x0)] bool IsRegistered;
            [FieldOffset(0x2)] public short PersonaId;
            [FieldOffset(0x4)] byte Level;
            [FieldOffset(0x6)] public short TraitId;
            [FieldOffset(0x8)] int Exp;
            [FieldOffset(0xC)] fixed short SkillIds[8];
            [FieldOffset(0x1C)] byte St;
            [FieldOffset(0x1D)] byte Ma;
            [FieldOffset(0x1E)] byte En;
            [FieldOffset(0x1F)] byte Ag;
            [FieldOffset(0x20)] byte Lu;
        }

        // ryuji info starts at 0x142852016 for steam 1.0.4
        [StructLayout(LayoutKind.Explicit, Size = 0x2A0)]
        public struct PartyPersona
        {
            [FieldOffset(0x0)] short PersonaId;
            [FieldOffset(0x2)] byte Level;
            [FieldOffset(0x4)] public short TraitId;
            [FieldOffset(0x6)] int Exp;
            [FieldOffset(0xA)] fixed short SkillIds[8];
            [FieldOffset(0x1A)] byte St;
            [FieldOffset(0x1B)] byte Ma;
            [FieldOffset(0x1C)] byte En;
            [FieldOffset(0x1D)] byte Ag;
            [FieldOffset(0x1E)] byte Lu;
        }
    }
}

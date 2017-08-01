using System;

namespace CodeTemplates.Esent.Model
{
    [Flags]
    public enum EsentMakeKeyGrbit
    {
        NormalizedKey           = 0x0001,
        KeyDataZeroLength       = 0x0002,
        StrLimit                = 0x0004,
        SubStrLimit             = 0x0008,
        FullColumnStartLimit    = 0x0010,
        FullColumnEndLimit      = 0x0020,
        PartialColumnStartLimit = 0x0040,
        PartialColumnEndLimit   = 0x0080,
    }
}
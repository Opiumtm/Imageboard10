using System;

namespace CodeTemplates.Esent.Model
{
    [Flags]
    // ReSharper disable InconsistentNaming
    public enum EsentColumndefGrbit
    {
        None =               0,
        Fixed =              0x0001,
        Tagged =             0x0002,
        NotNULL =            0x0004,
        Version =            0x0008,
        Autoincrement =      0x0010,
        MultiValued =        0x0020,
        EscrowUpdate =       0x0040,
        Unversioned =        0x0080,
        MaybeNull =          0x0100,
        UserDefinedDefault = 0x0200,
        TTKey =              0x0400,
        TTDescending =       0x0800,
    }
    // ReSharper enable InconsistentNaming
}
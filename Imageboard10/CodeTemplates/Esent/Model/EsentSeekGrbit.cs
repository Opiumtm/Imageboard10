using System;

namespace CodeTemplates.Esent.Model
{
    [Flags]
    // ReSharper disable InconsistentNaming
    public enum EsentSeekGrbit
    {
        SeekEQ        = 0x0001,
        SeekLT        = 0x0002,
        SeekLE        = 0x0004,
        SeekGE        = 0x0008,
        SeekGT        = 0x0010,
        SetIndexRange = 0x0020,
    }
    // ReSharper enable InconsistentNaming
}
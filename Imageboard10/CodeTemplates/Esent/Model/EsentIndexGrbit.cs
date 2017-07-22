using System;

namespace CodeTemplates.Esent.Model
{
    [Flags]
    public enum EsentIndexGrbit
    {
        None =                  0,
        Unique =                0x0001,
        Primary =               0x0002,
        DisallowNull =          0x0004,
        IgnoreNull =            0x0008,
        IgnoreAnyNull =         0x0010,
        IgnoreFirstNull =       0x0020,
        LazyFlush =             0x0040,
        Empty =                 0x0080,
        Unversioned =           0x0100,
        SortNullsHigh =         0x0200,
    }
}
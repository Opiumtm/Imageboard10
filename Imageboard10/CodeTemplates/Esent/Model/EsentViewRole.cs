using System;

namespace CodeTemplates.Esent.Model
{
    [Flags]
    public enum EsentViewRole
    {
        None = 0,
        Fetch = 0x0001,
        Insert = 0x0002,
        Update = 0x0004,
    }
}
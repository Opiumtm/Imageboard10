using System;
using Microsoft.Isam.Esent.Interop;

namespace CodeTemplatesSandbox
{
    public class TestClass
    {
        [Flags]
        enum A
        {
            B = 1,
            C = 2
        }
        public void Test()
        {
            ushort a = 0;
            short b = -1;
            //Api.RetrieveColumnAsString()
            //Api.SetColumn();
            ColumnValue c = new FloatColumnValue();
            //Api.TrySeek(new JET_SESID(), new JET_TABLEID(), SeekGrbit.SeekEQ);
        }
    }
}
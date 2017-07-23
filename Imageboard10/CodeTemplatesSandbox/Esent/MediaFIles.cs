

// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.Isam.Esent.Interop.Windows10;
using System.Runtime.CompilerServices;
// ReSharper enable RedundantUsingDirective

// ReSharper disable once CheckNamespace
namespace Imageboard10.Core.ModelStorage.Posts.EsentTables
{
	internal sealed class MediaFiles : IDisposable
	{
        public readonly Session Session;
        public readonly JET_TABLEID Table;

		public MediaFiles(Session session, JET_TABLEID table)
        {
            Session = session;
            Table = table;
			_columnDic = null;
			Columns = new DefaultView(this);
        }

        public void Dispose()
        {
            Api.JetCloseTable(Session, Table);
        }

        public static implicit operator JET_TABLEID(MediaFiles src)
        {
            return src.Table;
        }

		public enum Column
		{
			Id,
			EntityReferences,
			SequenceNumber,
			MediaData,
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JET_COLUMNID GetColumnid(Column columnName)
        {
			switch (columnName)
			{
				case Column.Id:
					return Api.GetTableColumnid(Session, Table, "Id");
				case Column.EntityReferences:
					return Api.GetTableColumnid(Session, Table, "EntityReferences");
				case Column.SequenceNumber:
					return Api.GetTableColumnid(Session, Table, "SequenceNumber");
				case Column.MediaData:
					return Api.GetTableColumnid(Session, Table, "MediaData");
				default:
					throw new ArgumentOutOfRangeException();
			}
        }

		private IDictionary<Column, JET_COLUMNID> _columnDic;

        public IDictionary<Column, JET_COLUMNID> ColumnDictionary
        {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				if (_columnDic == null) {
					_columnDic = new Dictionary<Column, JET_COLUMNID>()
					{
						{ Column.Id, Api.GetTableColumnid(Session, Table, "Id") },
						{ Column.EntityReferences, Api.GetTableColumnid(Session, Table, "EntityReferences") },
						{ Column.SequenceNumber, Api.GetTableColumnid(Session, Table, "SequenceNumber") },
						{ Column.MediaData, Api.GetTableColumnid(Session, Table, "MediaData") },
					};
				}
				return _columnDic;
			}
        }

		public static void CreateColumnsAndIndexes(Session sid, JET_TABLEID tableid)
		{
			JET_COLUMNID tempcolid;
            Api.JetAddColumn(sid, tableid, "Id", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnNotNULL | ColumndefGrbit.ColumnAutoincrement,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "EntityReferences", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnTagged | ColumndefGrbit.ColumnMultiValued,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "SequenceNumber", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "MediaData", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongBinary,
				grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);			
			var idxDef1 = "+Id\0\0";
			Api.JetCreateIndex(sid, tableid, "Primary", CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, idxDef1, idxDef1.Length, 100);
			var idxDef2 = "+EntityReferences\0\0";
			Api.JetCreateIndex(sid, tableid, "EntityReferences", CreateIndexGrbit.IndexIgnoreAnyNull, idxDef2, idxDef2.Length, 100);
			var idxDef3 = "+EntityReferences\0+SequenceNumber\0\0";
			Api.JetCreateIndex(sid, tableid, "Sequences", CreateIndexGrbit.IndexIgnoreAnyNull, idxDef3, idxDef3.Length, 100);
		}

		public struct Multivalue<T> where T : ColumnValue, new()
        {
            private readonly MediaFiles _table;
            private readonly JET_RETRIEVECOLUMN[] _r;
            private readonly ColumnValue[] _c;
            private readonly JET_COLUMNID _columnid;

            public Multivalue(MediaFiles table, JET_COLUMNID columnid)
            {
                _table = table;
                _r = new [] { new JET_RETRIEVECOLUMN() { columnid = columnid } };
                _c = new ColumnValue[1];
                _columnid = columnid;
            }

            public void Clear()
            {
                var si = new JET_SETINFO() { itagSequence = 1 };
                for (var i = 0; i < Count; i++)
                {
                    Api.JetSetColumn(_table.Session, _table.Table, _columnid, null, 0, SetColumnGrbit.None, si);
                }
            }

            public int Count
            {
                get
                {
                    _r[0].itagSequence = 0;
                    Api.JetRetrieveColumns(_table.Session, _table, _r, 1);
                    return _r[0].itagSequence;
                }
            }

            public T this[int i]
            {
                get
                {
                    var col = new T
                    {
                        ItagSequence = i + 1,
                        Columnid = _columnid,
                        RetrieveGrbit = RetrieveColumnGrbit.None
                    };
                    _c[0] = col;
                    Api.RetrieveColumns(_table.Session, _table.Table, _c);
                    return (T)_c[0];
                }
				set
				{
                    _c[0] = value ?? throw new ArgumentNullException();
					_c[0].ItagSequence = i + 1;
                    Api.SetColumns(_table.Session, _table.Table, _c);
				}
            }

            public T[] Values
            {
                get
                {
                    var cnt = Count;
                    var r = new T[cnt];
                    for (var i = 0; i < cnt; i++)
                    {
                        var col = new T
                        {
                            ItagSequence = i + 1,
                            Columnid = _columnid,
                            RetrieveGrbit = RetrieveColumnGrbit.None
                        };
                        _c[0] = col;
                        Api.RetrieveColumns(_table.Session, _table.Table, _c);
                        r[i] = (T)_c[0];
                    }
                    return r;
                }
				set
				{
					if (value == null)
					{
						throw new ArgumentNullException();
					}
					Clear();
                    for (var i = 0; i < value.Length; i++)
                    {
						value[i].ItagSequence = i + 1;
					}
				    // ReSharper disable once CoVariantArrayConversion
                    Api.SetColumns(_table.Session, _table.Table, value);
				}
            }
        }


		public struct DefaultView
		{
			private readonly MediaFiles _table;

		    // ReSharper disable once InconsistentNaming
			private readonly Multivalue<Int32ColumnValue> __mv_EntityReferences;

			public DefaultView(MediaFiles table)
			{
				_table = table;
				__mv_EntityReferences = new Multivalue<Int32ColumnValue>(table, table.ColumnDictionary[Column.EntityReferences]);
			}

			public int Id
			{
			    // ReSharper disable once PossibleInvalidOperationException
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[Column.Id]).Value;
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[Column.Id], value);
			}

		    // ReSharper disable once ConvertToAutoProperty
			public Multivalue<Int32ColumnValue> EntityReferences => __mv_EntityReferences;

			public int SequenceNumber
			{
			    // ReSharper disable once PossibleInvalidOperationException
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[Column.SequenceNumber]).Value;
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[Column.SequenceNumber], value);
			}

			public byte[] MediaData
			{
			    // ReSharper disable once PossibleInvalidOperationException
				get => Api.RetrieveColumn(_table.Session, _table, _table.ColumnDictionary[Column.MediaData]);
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[Column.MediaData], value);
			}
		}

		public DefaultView Columns { get; }
	}
}

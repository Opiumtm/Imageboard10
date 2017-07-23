

// ReSharper disable RedundantUsingDirective
using System;
using System.Collections;
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
			Views = new TableFetchViews(this);
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
            private readonly T[] _c;
            private readonly JET_COLUMNID _columnid;

            public Multivalue(MediaFiles table, JET_COLUMNID columnid)
            {
                _table = table;
                _r = new [] { new JET_RETRIEVECOLUMN() { columnid = columnid } };
                _c = new T[1];
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
				    // ReSharper disable once CoVariantArrayConversion
                    Api.RetrieveColumns(_table.Session, _table.Table, _c);
                    return _c[0];
                }
				set
				{
                    _c[0] = value ?? throw new ArgumentNullException();
					_c[0].ItagSequence = i + 1;
				    // ReSharper disable once CoVariantArrayConversion
                    Api.SetColumns(_table.Session, _table.Table, _c);
				}
            }

			public IEnumerable<T> Enumerate()
			{
                var cnt = Count;
				if (cnt == 0)
				{
					yield break;
				}
                for (var i = 0; i < cnt; i++)
                {
                    var col = new T
                    {
                        ItagSequence = i + 1,
                        Columnid = _columnid,
                        RetrieveGrbit = RetrieveColumnGrbit.None
                    };
                    _c[0] = col;
				    // ReSharper disable once CoVariantArrayConversion
                    Api.RetrieveColumns(_table.Session, _table.Table, _c);
					yield return _c[0];
                }
			}

            public T[] Values
            {
                get
                {
                    var cnt = Count;
					if (cnt == 0)
					{
						return new T[0];
					}
                    var r = new T[cnt];
                    for (var i = 0; i < cnt; i++)
                    {
                        var col = new T
                        {
                            ItagSequence = i + 1,
                            Columnid = _columnid,
                            RetrieveGrbit = RetrieveColumnGrbit.None
                        };
                        r[i] = col;
                    }
                    // ReSharper disable once CoVariantArrayConversion
                    Api.RetrieveColumns(_table.Session, _table.Table, r);
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

	    public IEnumerable EnumerateToEnd()
	    {
	        while (Api.TryMoveNext(Session, Table))
	        {
	            yield return this;
	        }
	    }

	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    public void MoveBeforeFirst()
	    {
	        Api.MoveBeforeFirst(Session, Table);
	    }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    public bool TryMoveFirst()
	    {
	        return Api.TryMoveFirst(Session, Table);
	    }

	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    public bool TryMoveNext()
	    {
	        return Api.TryMoveNext(Session, Table);
	    }

	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    public bool TryMoveNextUniqueKey()
	    {
	        return Api.TryMove(Session, Table, JET_Move.Next, MoveGrbit.MoveKeyNE);
	    }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    public bool TryMovePrevious()
	    {
	        return Api.TryMovePrevious(Session, Table);
	    }
		
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    public void DeleteCurrentRow()
	    {
	        Api.JetDelete(Session, Table);
	    }

		public static class ViewValues
		{

			// ReSharper disable once InconsistentNaming
			public struct IdKey
			{
				public int Id;
			}

			// ReSharper disable once InconsistentNaming
			public struct SeqData
			{
				public int SequenceNumber;
				public byte[] MediaData;
			}

			// ReSharper disable once InconsistentNaming
			public struct SeqDataAll
			{
				public Int32ColumnValue[] EntityReferences;
				public int SequenceNumber;
				public byte[] MediaData;
			}

			// ReSharper disable once InconsistentNaming
			public struct ERefView
			{
				public Int32ColumnValue[] EntityReferences;
			}
		}

		public static class FetchViews {

			// ReSharper disable once InconsistentNaming
			public struct IdKey
			{
				private readonly MediaFiles _table;
				private readonly ColumnValue[] _c;

				public IdKey(MediaFiles table)
				{
					_table = table;

					_c = new ColumnValue[1];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[Column.Id],
						RetrieveGrbit = RetrieveColumnGrbit.RetrieveFromPrimaryBookmark
					};
				}

				public ViewValues.IdKey Fetch()
				{
					var r = new ViewValues.IdKey();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.Id = ((Int32ColumnValue)_c[0]).Value.Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct SeqData
			{
				private readonly MediaFiles _table;
				private readonly ColumnValue[] _c;

				public SeqData(MediaFiles table)
				{
					_table = table;

					_c = new ColumnValue[2];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[Column.MediaData],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.SeqData Fetch()
				{
					var r = new ViewValues.SeqData();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[0]).Value.Value;
					r.MediaData = ((BytesColumnValue)_c[1]).Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct SeqDataAll
			{
				private readonly MediaFiles _table;
				private readonly ColumnValue[] _c;

				public SeqDataAll(MediaFiles table)
				{
					_table = table;

					_c = new ColumnValue[2];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[Column.MediaData],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.SeqDataAll Fetch()
				{
					var r = new ViewValues.SeqDataAll();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[0]).Value.Value;
					r.MediaData = ((BytesColumnValue)_c[1]).Value;
					r.EntityReferences = _table.Columns.EntityReferences.Values;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct ERefView
			{
				private readonly MediaFiles _table;

				public ERefView(MediaFiles table)
				{
					_table = table;
				}

				public ViewValues.ERefView Fetch()
				{
					var r = new ViewValues.ERefView();
					r.EntityReferences = _table.Columns.EntityReferences.Values;
					return r;
				}
			}
	
		}

		public class TableFetchViews
		{
			private readonly MediaFiles _table;

			public TableFetchViews(MediaFiles table)
			{
				_table = table;
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.SeqData? __fv_SeqData;
			public FetchViews.SeqData SeqData
			{
				get
				{
					if (__fv_SeqData == null)
					{
						__fv_SeqData = new FetchViews.SeqData(_table);
					}
					return __fv_SeqData.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.SeqDataAll? __fv_SeqDataAll;
			public FetchViews.SeqDataAll SeqDataAll
			{
				get
				{
					if (__fv_SeqDataAll == null)
					{
						__fv_SeqDataAll = new FetchViews.SeqDataAll(_table);
					}
					return __fv_SeqDataAll.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.ERefView? __fv_ERefView;
			public FetchViews.ERefView ERefView
			{
				get
				{
					if (__fv_ERefView == null)
					{
						__fv_ERefView = new FetchViews.ERefView(_table);
					}
					return __fv_ERefView.Value;
				}
			}
		}

		public TableFetchViews Views { get; }
	}
}

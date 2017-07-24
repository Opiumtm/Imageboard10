

// ReSharper disable RedundantUsingDirective
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.Isam.Esent.Interop.Windows10;
using System.Runtime.CompilerServices;
using System.Text;
// ReSharper enable RedundantUsingDirective

// ReSharper disable RedundantNameQualifier
// ReSharper disable once CheckNamespace
namespace Imageboard10.Core.ModelStorage.Boards
{
	internal sealed class BoardReferenceTable : IDisposable
	{
        public readonly Session Session;
        public readonly JET_TABLEID Table;

		public BoardReferenceTable(Session session, JET_TABLEID table)
        {
            Session = session;
            Table = table;
			_columnDic = null;
			Columns = new DefaultView(this);
        }

	    public BoardReferenceTable(Session session, JET_DBID dbid, string tableName, OpenTableGrbit grbit)
	    {
	        Session = session;
	        JET_TABLEID tableid;
	        Api.OpenTable(session, dbid, tableName, grbit, out tableid);
	        Table = tableid;
	        _columnDic = null;
	        Columns = new DefaultView(this);
	    }

        public void Dispose()
        {
            Api.JetCloseTable(Session, Table);
        }

        public static implicit operator JET_TABLEID(BoardReferenceTable src)
        {
            return src.Table;
        }

		public enum Column
		{
			Id,
			Category,
			ShortName,
			DisplayName,
			IsAdult,
			ExtendedData,
			BumpLimit,
			DefaultName,
			Pages,
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JET_COLUMNID GetColumnid(Column columnName)
        {
			switch (columnName)
			{
				case Column.Id:
					return Api.GetTableColumnid(Session, Table, "Id");
				case Column.Category:
					return Api.GetTableColumnid(Session, Table, "Category");
				case Column.ShortName:
					return Api.GetTableColumnid(Session, Table, "ShortName");
				case Column.DisplayName:
					return Api.GetTableColumnid(Session, Table, "DisplayName");
				case Column.IsAdult:
					return Api.GetTableColumnid(Session, Table, "IsAdult");
				case Column.ExtendedData:
					return Api.GetTableColumnid(Session, Table, "ExtendedData");
				case Column.BumpLimit:
					return Api.GetTableColumnid(Session, Table, "BumpLimit");
				case Column.DefaultName:
					return Api.GetTableColumnid(Session, Table, "DefaultName");
				case Column.Pages:
					return Api.GetTableColumnid(Session, Table, "Pages");
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
						{ Column.Category, Api.GetTableColumnid(Session, Table, "Category") },
						{ Column.ShortName, Api.GetTableColumnid(Session, Table, "ShortName") },
						{ Column.DisplayName, Api.GetTableColumnid(Session, Table, "DisplayName") },
						{ Column.IsAdult, Api.GetTableColumnid(Session, Table, "IsAdult") },
						{ Column.ExtendedData, Api.GetTableColumnid(Session, Table, "ExtendedData") },
						{ Column.BumpLimit, Api.GetTableColumnid(Session, Table, "BumpLimit") },
						{ Column.DefaultName, Api.GetTableColumnid(Session, Table, "DefaultName") },
						{ Column.Pages, Api.GetTableColumnid(Session, Table, "Pages") },
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
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.ColumnNotNULL,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Category", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.ColumnNotNULL,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "ShortName", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.None,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "DisplayName", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.None,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "IsAdult", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Bit,
				grbit = ColumndefGrbit.ColumnNotNULL,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "ExtendedData", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongBinary,
				grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "BumpLimit", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "DefaultName", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.None,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Pages", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);			
			var idxDef1 = "+Id\0\0";
			Api.JetCreateIndex(sid, tableid, "PrimaryIndex", CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, idxDef1, idxDef1.Length, 100);
			var idxDef2 = "+Category\0\0";
			Api.JetCreateIndex(sid, tableid, "CategoryIndex", CreateIndexGrbit.None, idxDef2, idxDef2.Length, 100);
			var idxDef3 = "+IsAdult\0\0";
			Api.JetCreateIndex(sid, tableid, "IsAdultIndex", CreateIndexGrbit.None, idxDef3, idxDef3.Length, 100);
			var idxDef4 = "+IsAdult\0+Category\0\0";
			Api.JetCreateIndex(sid, tableid, "IsAdultAndCategoryIndex", CreateIndexGrbit.None, idxDef4, idxDef4.Length, 100);
		}

		public struct Multivalue<T> where T : ColumnValue, new()
        {
            private readonly BoardReferenceTable _table;
            private readonly JET_RETRIEVECOLUMN[] _r;
            private readonly T[] _c;
            private readonly JET_COLUMNID _columnid;

            public Multivalue(BoardReferenceTable table, JET_COLUMNID columnid)
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
				set => SetValues(value);            
			}

			public void SetValues(T[] value)
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


		public struct DefaultView
		{
			private readonly BoardReferenceTable _table;

			public DefaultView(BoardReferenceTable table)
			{
				_table = table;
			}
			public string Id
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.Id]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.Id], value, Encoding.Unicode);

			}

			public string Category
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.Category]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.Category], value, Encoding.Unicode);

			}

			public string ShortName
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.ShortName]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.ShortName], value, Encoding.Unicode);

			}

			public string DisplayName
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.DisplayName]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.DisplayName], value, Encoding.Unicode);

			}

			public bool IsAdult
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsBoolean(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.IsAdult]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.IsAdult], value);

			}

			public byte[] ExtendedData
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.ExtendedData]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.ExtendedData], value);

			}

			public int? BumpLimit
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.BumpLimit]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.BumpLimit], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.BumpLimit], null);
					}
				}
			}

			public string DefaultName
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.DefaultName]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.DefaultName], value, Encoding.Unicode);

			}

			public int? Pages
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.Pages]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.Pages], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BoardReferenceTable.Column.Pages], null);
					}
				}
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
		}

		public static class FetchViews {
	
		}

		public class TableFetchViews
		{
			private readonly BoardReferenceTable _table;

			public TableFetchViews(BoardReferenceTable table)
			{
				_table = table;
			}
		}

		// ReSharper disable once InconsistentNaming
		private TableFetchViews __views;

		public TableFetchViews Views
		{
			get
			{
				if (__views == null)
				{
					__views = new TableFetchViews(this);
				}
				return __views;
			}
		}

		public static class InsertOrUpdateViews
		{
		}

		public class TableInsertViews
		{
			private readonly BoardReferenceTable _table;

			public TableInsertViews(BoardReferenceTable table)
			{
				_table = table;
			}

			public Update CreateUpdate() => new Update(_table.Session, _table, JET_prep.Insert);
		}

		// ReSharper disable once InconsistentNaming
		private TableInsertViews __insert;

		public TableInsertViews Insert
		{
			get
			{
				if (__insert == null)
				{
					__insert = new TableInsertViews(this);
				}
				return __insert;
			}
		}

		public class TableUpdateViews
		{
			private readonly BoardReferenceTable _table;

			public TableUpdateViews(BoardReferenceTable table)
			{
				_table = table;
			}

			public Update CreateUpdate() => new Update(_table.Session, _table, JET_prep.Replace);
		}

		// ReSharper disable once InconsistentNaming
		private TableUpdateViews __update;

		public TableUpdateViews Update
		{
			get
			{
				if (__update == null)
				{
					__update = new TableUpdateViews(this);
				}
				return __update;
			}
		}

		public static class IndexDefinitions
		{

			public struct PrimaryIndex
			{
				private readonly BoardReferenceTable _table;

				public PrimaryIndex(BoardReferenceTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "PrimaryIndex");
				}

				public struct PrimaryIndexKey
				{
					public string Id;
				}

				public void SetKey(PrimaryIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.Id, Encoding.Unicode, MakeKeyGrbit.NewKey);
				}

				public bool Find(PrimaryIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ);
				}

				public IEnumerable Enumerate(PrimaryIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable EnumerateUnique(PrimaryIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

			    public int GetIndexRecordCount()
			    {
			        int r;
			        Api.JetIndexRecordCount(_table.Session, _table, out r, int.MaxValue);
			        return r;
			    }

			    public int GetIndexRecordCount(PrimaryIndexKey key)
			    {
			        Find(key);
			        return GetIndexRecordCount();
			    }
				
			}

			public struct CategoryIndex
			{
				private readonly BoardReferenceTable _table;

				public CategoryIndex(BoardReferenceTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "CategoryIndex");
				}

				public struct CategoryIndexKey
				{
					public string Category;
				}

				public void SetKey(CategoryIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.Category, Encoding.Unicode, MakeKeyGrbit.NewKey);
				}

				public bool Find(CategoryIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ);
				}

				public IEnumerable Enumerate(CategoryIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable EnumerateUnique(CategoryIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

			    public int GetIndexRecordCount()
			    {
			        int r;
			        Api.JetIndexRecordCount(_table.Session, _table, out r, int.MaxValue);
			        return r;
			    }

			    public int GetIndexRecordCount(CategoryIndexKey key)
			    {
			        Find(key);
			        return GetIndexRecordCount();
			    }
				
			}

			public struct IsAdultIndex
			{
				private readonly BoardReferenceTable _table;

				public IsAdultIndex(BoardReferenceTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "IsAdultIndex");
				}

				public struct IsAdultIndexKey
				{
					public bool IsAdult;
				}

				public void SetKey(IsAdultIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.IsAdult,  MakeKeyGrbit.NewKey);
				}

				public bool Find(IsAdultIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ);
				}

				public IEnumerable Enumerate(IsAdultIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable EnumerateUnique(IsAdultIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

			    public int GetIndexRecordCount()
			    {
			        int r;
			        Api.JetIndexRecordCount(_table.Session, _table, out r, int.MaxValue);
			        return r;
			    }

			    public int GetIndexRecordCount(IsAdultIndexKey key)
			    {
			        Find(key);
			        return GetIndexRecordCount();
			    }
				
			}

			public struct IsAdultAndCategoryIndex
			{
				private readonly BoardReferenceTable _table;

				public IsAdultAndCategoryIndex(BoardReferenceTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "IsAdultAndCategoryIndex");
				}

				public struct IsAdultAndCategoryIndexKey
				{
					public bool IsAdult;
					public string Category;
				}

				public void SetKey(IsAdultAndCategoryIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.IsAdult,  MakeKeyGrbit.NewKey);
					Api.MakeKey(_table.Session, _table, key.Category, Encoding.Unicode, MakeKeyGrbit.None);
				}

				public bool Find(IsAdultAndCategoryIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ);
				}

				public IEnumerable Enumerate(IsAdultAndCategoryIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable EnumerateUnique(IsAdultAndCategoryIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

			    public int GetIndexRecordCount()
			    {
			        int r;
			        Api.JetIndexRecordCount(_table.Session, _table, out r, int.MaxValue);
			        return r;
			    }

			    public int GetIndexRecordCount(IsAdultAndCategoryIndexKey key)
			    {
			        Find(key);
			        return GetIndexRecordCount();
			    }

				public struct IsAdultAndCategoryIndexPartialKey1
				{
					public bool IsAdult;
				}

				public void SetKey(IsAdultAndCategoryIndexPartialKey1 key, bool startRange)
				{
					var rangeFlag = startRange ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit;
					Api.MakeKey(_table.Session, _table, key.IsAdult,  MakeKeyGrbit.NewKey | rangeFlag);
				}

				public bool Find(IsAdultAndCategoryIndexPartialKey1 key)
				{
					SetKey(key, true);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE);
				}

				public bool SetPartialUpperRange(IsAdultAndCategoryIndexPartialKey1 key)
				{
					SetKey(key, false);
					return Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit);
				}

				public bool SeekPartial(IsAdultAndCategoryIndexPartialKey1 key)
				{
					if (Find(key))
					{
						if (SetPartialUpperRange(key))
						{
							return true;
						}
					}
					return false;
				}

				public IEnumerable Enumerate(IsAdultAndCategoryIndexPartialKey1 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return _table;
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}

				public int GetIndexRecordCount(IsAdultAndCategoryIndexPartialKey1 key)
			    {
			        SeekPartial(key);
			        return GetIndexRecordCount();
			    }

				
			}
		}

		public class TableIndexes
		{
			private readonly BoardReferenceTable _table;

			public TableIndexes(BoardReferenceTable table)
			{
				_table = table;
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.PrimaryIndex? __ti_PrimaryIndex;

			public IndexDefinitions.PrimaryIndex PrimaryIndex
			{
				get
				{
					if (__ti_PrimaryIndex == null)
					{
						__ti_PrimaryIndex = new IndexDefinitions.PrimaryIndex(_table);
					}
					return __ti_PrimaryIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.CategoryIndex? __ti_CategoryIndex;

			public IndexDefinitions.CategoryIndex CategoryIndex
			{
				get
				{
					if (__ti_CategoryIndex == null)
					{
						__ti_CategoryIndex = new IndexDefinitions.CategoryIndex(_table);
					}
					return __ti_CategoryIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.IsAdultIndex? __ti_IsAdultIndex;

			public IndexDefinitions.IsAdultIndex IsAdultIndex
			{
				get
				{
					if (__ti_IsAdultIndex == null)
					{
						__ti_IsAdultIndex = new IndexDefinitions.IsAdultIndex(_table);
					}
					return __ti_IsAdultIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.IsAdultAndCategoryIndex? __ti_IsAdultAndCategoryIndex;

			public IndexDefinitions.IsAdultAndCategoryIndex IsAdultAndCategoryIndex
			{
				get
				{
					if (__ti_IsAdultAndCategoryIndex == null)
					{
						__ti_IsAdultAndCategoryIndex = new IndexDefinitions.IsAdultAndCategoryIndex(_table);
					}
					return __ti_IsAdultAndCategoryIndex.Value;
				}
			}
		}

		// ReSharper disable once InconsistentNaming
		private TableIndexes __indexes;

		public TableIndexes Indexes
		{
			get
			{
				if (__indexes == null)
				{
					__indexes = new TableIndexes(this);
				}
				return __indexes;
			}
		}
	}
}
// ReSharper enable RedundantNameQualifier

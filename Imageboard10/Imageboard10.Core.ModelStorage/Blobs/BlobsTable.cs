

// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.Isam.Esent.Interop.Windows10;
using System.Runtime.CompilerServices;
using System.Text;
// ReSharper enable RedundantUsingDirective

// ReSharper disable RedundantNameQualifier
// ReSharper disable once CheckNamespace
namespace Imageboard10.Core.ModelStorage.Blobs.TableDef
{
	internal sealed class BlobsTable : IDisposable
	{
        public readonly Session Session;
        public readonly JET_TABLEID Table;

		public BlobsTable(Session session, JET_TABLEID table)
        {
            Session = session;
            Table = table;
			_columnDic = null;
			Columns = new DefaultView(this);
        }

	    public BlobsTable(Session session, JET_DBID dbid, string tableName, OpenTableGrbit grbit)
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

        public static implicit operator JET_TABLEID(BlobsTable src)
        {
            return src.Table;
        }

		public enum Column
		{
			Id,
			Name,
			Category,
			Length,
			CreatedDate,
			Data,
			ReferenceId,
			IsCompleted,
			IsFilestream,
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JET_COLUMNID GetColumnid(Column columnName)
        {
			switch (columnName)
			{
				case Column.Id:
					return Api.GetTableColumnid(Session, Table, "Id");
				case Column.Name:
					return Api.GetTableColumnid(Session, Table, "Name");
				case Column.Category:
					return Api.GetTableColumnid(Session, Table, "Category");
				case Column.Length:
					return Api.GetTableColumnid(Session, Table, "Length");
				case Column.CreatedDate:
					return Api.GetTableColumnid(Session, Table, "CreatedDate");
				case Column.Data:
					return Api.GetTableColumnid(Session, Table, "Data");
				case Column.ReferenceId:
					return Api.GetTableColumnid(Session, Table, "ReferenceId");
				case Column.IsCompleted:
					return Api.GetTableColumnid(Session, Table, "IsCompleted");
				case Column.IsFilestream:
					return Api.GetTableColumnid(Session, Table, "IsFilestream");
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
						{ Column.Name, Api.GetTableColumnid(Session, Table, "Name") },
						{ Column.Category, Api.GetTableColumnid(Session, Table, "Category") },
						{ Column.Length, Api.GetTableColumnid(Session, Table, "Length") },
						{ Column.CreatedDate, Api.GetTableColumnid(Session, Table, "CreatedDate") },
						{ Column.Data, Api.GetTableColumnid(Session, Table, "Data") },
						{ Column.ReferenceId, Api.GetTableColumnid(Session, Table, "ReferenceId") },
						{ Column.IsCompleted, Api.GetTableColumnid(Session, Table, "IsCompleted") },
						{ Column.IsFilestream, Api.GetTableColumnid(Session, Table, "IsFilestream") },
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
            Api.JetAddColumn(sid, tableid, "Name", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.ColumnNotNULL,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Category", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.None,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Length", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Currency,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "CreatedDate", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.DateTime,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Data", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongBinary,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "ReferenceId", new JET_COLUMNDEF()
            {
				coltyp = VistaColtyp.GUID,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "IsCompleted", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Bit,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "IsFilestream", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Bit,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
			var idxDef1 = "+Id\0\0";
			Api.JetCreateIndex(sid, tableid, "PrimaryIndex", CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, idxDef1, idxDef1.Length, 100);
			var idxDef2 = "+Name\0\0";
			Api.JetCreateIndex(sid, tableid, "NameIndex", CreateIndexGrbit.IndexUnique, idxDef2, idxDef2.Length, 100);
			var idxDef3 = "+Category\0+IsCompleted\0\0";
			Api.JetCreateIndex(sid, tableid, "CategoryIndex", CreateIndexGrbit.None, idxDef3, idxDef3.Length, 100);
			var idxDef4 = "+ReferenceId\0+IsCompleted\0\0";
			Api.JetCreateIndex(sid, tableid, "ReferenceIdIndex", CreateIndexGrbit.None, idxDef4, idxDef4.Length, 100);
			var idxDef5 = "+IsCompleted\0\0";
			Api.JetCreateIndex(sid, tableid, "IsCompletedIndex", CreateIndexGrbit.None, idxDef5, idxDef5.Length, 100);
		}

		public struct Multivalue<T> where T : ColumnValue, new()
        {
            private readonly BlobsTable _table;
            private readonly JET_RETRIEVECOLUMN[] _r;
            private readonly T[] _c;
            private readonly JET_COLUMNID _columnid;

            public Multivalue(BlobsTable table, JET_COLUMNID columnid)
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
			private readonly BlobsTable _table;

			public DefaultView(BlobsTable table)
			{
				_table = table;
			}
			public int Id
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Id]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Id], value);

			}

		    // ReSharper disable once InconsistentNaming
			public int Id_AutoincrementValue
			{
				// ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Id], RetrieveColumnGrbit.RetrieveCopy).Value;
			}
			public string Name
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Name]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Name], value, Encoding.Unicode);

			}

			public string Category
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Category]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Category], value, Encoding.Unicode);

			}

			public long Length
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt64(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Length]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Length], value);

			}

			public DateTime CreatedDate
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsDateTime(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.CreatedDate]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.CreatedDate], value);

			}

			public byte[] Data
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Data]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.Data], value);

			}

			public Guid? ReferenceId
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsGuid(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.ReferenceId]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.ReferenceId], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.ReferenceId], null);
					}
				}
			}

			public bool IsCompleted
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsBoolean(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.IsCompleted]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.IsCompleted], value);

			}

			public bool IsFilestream
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsBoolean(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.IsFilestream]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[BlobsTable.Column.IsFilestream], value);

			}

		}

		public DefaultView Columns { get; }

	    public IEnumerable<object> EnumerateToEnd()
	    {
	        while (Api.TryMoveNext(Session, Table))
	        {
	            yield return this;
	        }
	    }

	    public IEnumerable<object> Enumerate()
	    {
			if (Api.TryMoveFirst(Session, Table))
			{
				do {
					yield return this;
				} while (Api.TryMoveNext(Session, Table));
			}
	    }

	    public IEnumerable<object> EnumerateUnique()
	    {
			if (Api.TryMoveFirst(Session, Table))
			{
				do {
					yield return this;
				} while (Api.TryMove(Session, Table, JET_Move.Next, MoveGrbit.MoveKeyNE));
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
			private readonly BlobsTable _table;

			public TableFetchViews(BlobsTable table)
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
			private readonly BlobsTable _table;

			public TableInsertViews(BlobsTable table)
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
			private readonly BlobsTable _table;

			public TableUpdateViews(BlobsTable table)
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
				private readonly BlobsTable _table;

				public PrimaryIndex(BlobsTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "PrimaryIndex");
				}

				public struct PrimaryIndexKey
				{
					public int Id;
				}

			    // ReSharper disable InconsistentNaming
				public PrimaryIndexKey CreateKey(
						int Id
				)
			    // ReSharper enable InconsistentNaming
				{
					return new PrimaryIndexKey() {
						Id = Id,
					
					};
				}

				public void SetKey(PrimaryIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.Id,  MakeKeyGrbit.NewKey);
				}

				public bool Find(PrimaryIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ);
				}

				public IEnumerable<object> Enumerate(PrimaryIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(PrimaryIndexKey key)
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

			public struct NameIndex
			{
				private readonly BlobsTable _table;

				public NameIndex(BlobsTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "NameIndex");
				}

				public struct NameIndexKey
				{
					public string Name;
				}

			    // ReSharper disable InconsistentNaming
				public NameIndexKey CreateKey(
						string Name
				)
			    // ReSharper enable InconsistentNaming
				{
					return new NameIndexKey() {
						Name = Name,
					
					};
				}

				public void SetKey(NameIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.Name, Encoding.Unicode, MakeKeyGrbit.NewKey);
				}

				public bool Find(NameIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ);
				}

				public IEnumerable<object> Enumerate(NameIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(NameIndexKey key)
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

			    public int GetIndexRecordCount(NameIndexKey key)
			    {
			        Find(key);
			        return GetIndexRecordCount();
			    }
				
			}

			public struct CategoryIndex
			{
				private readonly BlobsTable _table;

				public CategoryIndex(BlobsTable table)
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
					public bool IsCompleted;
				}

			    // ReSharper disable InconsistentNaming
				public CategoryIndexKey CreateKey(
						string Category
						,bool IsCompleted
				)
			    // ReSharper enable InconsistentNaming
				{
					return new CategoryIndexKey() {
						Category = Category,
						IsCompleted = IsCompleted,
					
					};
				}

				public void SetKey(CategoryIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.Category, Encoding.Unicode, MakeKeyGrbit.NewKey);
					Api.MakeKey(_table.Session, _table, key.IsCompleted,  MakeKeyGrbit.None);
				}

				public bool Find(CategoryIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ);
				}

				public IEnumerable<object> Enumerate(CategoryIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(CategoryIndexKey key)
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

				public struct CategoryIndexPartialKey1
				{
					public string Category;
				}

			    // ReSharper disable InconsistentNaming
				public CategoryIndexPartialKey1 CreateKey(
						string Category
				)
			    // ReSharper enable InconsistentNaming
				{
					return new CategoryIndexPartialKey1() {
						Category = Category,
					
					};
				}

				public void SetKey(CategoryIndexPartialKey1 key, bool startRange)
				{
					var rangeFlag = startRange ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit;
					Api.MakeKey(_table.Session, _table, key.Category, Encoding.Unicode, MakeKeyGrbit.NewKey | rangeFlag);
				}

				public bool Find(CategoryIndexPartialKey1 key)
				{
					SetKey(key, true);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE);
				}

				public bool SetPartialUpperRange(CategoryIndexPartialKey1 key)
				{
					SetKey(key, false);
					return Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit);
				}

				public bool SeekPartial(CategoryIndexPartialKey1 key)
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

				public IEnumerable<object> Enumerate(CategoryIndexPartialKey1 key)
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

				public int GetIndexRecordCount(CategoryIndexPartialKey1 key)
			    {
			        SeekPartial(key);
			        return GetIndexRecordCount();
			    }

				
			}

			public struct ReferenceIdIndex
			{
				private readonly BlobsTable _table;

				public ReferenceIdIndex(BlobsTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "ReferenceIdIndex");
				}

				public struct ReferenceIdIndexKey
				{
					public Guid? ReferenceId;
					public bool IsCompleted;
				}

			    // ReSharper disable InconsistentNaming
				public ReferenceIdIndexKey CreateKey(
						Guid? ReferenceId
						,bool IsCompleted
				)
			    // ReSharper enable InconsistentNaming
				{
					return new ReferenceIdIndexKey() {
						ReferenceId = ReferenceId,
						IsCompleted = IsCompleted,
					
					};
				}

				public void SetKey(ReferenceIdIndexKey key)
				{
					if (key.ReferenceId == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.ReferenceId.Value, MakeKeyGrbit.NewKey);
					}
					Api.MakeKey(_table.Session, _table, key.IsCompleted,  MakeKeyGrbit.None);
				}

				public bool Find(ReferenceIdIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ);
				}

				public IEnumerable<object> Enumerate(ReferenceIdIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(ReferenceIdIndexKey key)
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

			    public int GetIndexRecordCount(ReferenceIdIndexKey key)
			    {
			        Find(key);
			        return GetIndexRecordCount();
			    }

				public struct ReferenceIdIndexPartialKey1
				{
					public Guid? ReferenceId;
				}

			    // ReSharper disable InconsistentNaming
				public ReferenceIdIndexPartialKey1 CreateKey(
						Guid? ReferenceId
				)
			    // ReSharper enable InconsistentNaming
				{
					return new ReferenceIdIndexPartialKey1() {
						ReferenceId = ReferenceId,
					
					};
				}

				public void SetKey(ReferenceIdIndexPartialKey1 key, bool startRange)
				{
					var rangeFlag = startRange ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit;
					if (key.ReferenceId == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey | rangeFlag);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.ReferenceId.Value, MakeKeyGrbit.NewKey | rangeFlag);
					}
				}

				public bool Find(ReferenceIdIndexPartialKey1 key)
				{
					SetKey(key, true);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE);
				}

				public bool SetPartialUpperRange(ReferenceIdIndexPartialKey1 key)
				{
					SetKey(key, false);
					return Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit);
				}

				public bool SeekPartial(ReferenceIdIndexPartialKey1 key)
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

				public IEnumerable<object> Enumerate(ReferenceIdIndexPartialKey1 key)
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

				public int GetIndexRecordCount(ReferenceIdIndexPartialKey1 key)
			    {
			        SeekPartial(key);
			        return GetIndexRecordCount();
			    }

				
			}

			public struct IsCompletedIndex
			{
				private readonly BlobsTable _table;

				public IsCompletedIndex(BlobsTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "IsCompletedIndex");
				}

				public struct IsCompletedIndexKey
				{
					public bool IsCompleted;
				}

			    // ReSharper disable InconsistentNaming
				public IsCompletedIndexKey CreateKey(
						bool IsCompleted
				)
			    // ReSharper enable InconsistentNaming
				{
					return new IsCompletedIndexKey() {
						IsCompleted = IsCompleted,
					
					};
				}

				public void SetKey(IsCompletedIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.IsCompleted,  MakeKeyGrbit.NewKey);
				}

				public bool Find(IsCompletedIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ);
				}

				public IEnumerable<object> Enumerate(IsCompletedIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(IsCompletedIndexKey key)
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

			    public int GetIndexRecordCount(IsCompletedIndexKey key)
			    {
			        Find(key);
			        return GetIndexRecordCount();
			    }
				
			}
		}

		public class TableIndexes
		{
			private readonly BlobsTable _table;

			public TableIndexes(BlobsTable table)
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
			private IndexDefinitions.NameIndex? __ti_NameIndex;

			public IndexDefinitions.NameIndex NameIndex
			{
				get
				{
					if (__ti_NameIndex == null)
					{
						__ti_NameIndex = new IndexDefinitions.NameIndex(_table);
					}
					return __ti_NameIndex.Value;
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
			private IndexDefinitions.ReferenceIdIndex? __ti_ReferenceIdIndex;

			public IndexDefinitions.ReferenceIdIndex ReferenceIdIndex
			{
				get
				{
					if (__ti_ReferenceIdIndex == null)
					{
						__ti_ReferenceIdIndex = new IndexDefinitions.ReferenceIdIndex(_table);
					}
					return __ti_ReferenceIdIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.IsCompletedIndex? __ti_IsCompletedIndex;

			public IndexDefinitions.IsCompletedIndex IsCompletedIndex
			{
				get
				{
					if (__ti_IsCompletedIndex == null)
					{
						__ti_IsCompletedIndex = new IndexDefinitions.IsCompletedIndex(_table);
					}
					return __ti_IsCompletedIndex.Value;
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



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
namespace Imageboard10.Core.ModelStorage.Blobs
{
	internal sealed class ReferencesTable : IDisposable
	{
        public readonly Session Session;
        public readonly JET_TABLEID Table;

		public ReferencesTable(Session session, JET_TABLEID table)
        {
            Session = session;
            Table = table;
			_columnDic = null;
			Columns = new DefaultView(this);
        }

	    public ReferencesTable(Session session, JET_DBID dbid, string tableName, OpenTableGrbit grbit)
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

        public static implicit operator JET_TABLEID(ReferencesTable src)
        {
            return src.Table;
        }

		public enum Column
		{
			ReferenceId,
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JET_COLUMNID GetColumnid(Column columnName)
        {
			switch (columnName)
			{
				case Column.ReferenceId:
					return Api.GetTableColumnid(Session, Table, "ReferenceId");
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
						{ Column.ReferenceId, Api.GetTableColumnid(Session, Table, "ReferenceId") },
					};
				}
				return _columnDic;
			}
        }

		public static void CreateColumnsAndIndexes(Session sid, JET_TABLEID tableid)
		{
			JET_COLUMNID tempcolid;
            Api.JetAddColumn(sid, tableid, "ReferenceId", new JET_COLUMNDEF()
            {
				coltyp = VistaColtyp.GUID,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
			var idxDef1 = "+ReferenceId\0\0";
			Api.JetCreateIndex(sid, tableid, "PrimaryIndex", CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, idxDef1, idxDef1.Length, 100);
		}

		public struct Multivalue<T> where T : ColumnValue, new()
        {
            private readonly ReferencesTable _table;
            private readonly JET_RETRIEVECOLUMN[] _r;
            private readonly T[] _c;
            private readonly JET_COLUMNID _columnid;

            public Multivalue(ReferencesTable table, JET_COLUMNID columnid)
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
			private readonly ReferencesTable _table;

			public DefaultView(ReferencesTable table)
			{
				_table = table;
			}
			public Guid ReferenceId
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsGuid(_table.Session, _table, _table.ColumnDictionary[ReferencesTable.Column.ReferenceId]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[ReferencesTable.Column.ReferenceId], value);

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
		public bool TryGotoBookmark(byte[] bookmark)
		{
			if (bookmark == null)
			{
				throw new ArgumentNullException(nameof(bookmark));
			}
			return Api.TryGotoBookmark(Session, Table, bookmark, bookmark.Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGotoBookmark(byte[] bookmark, int bookmarkSize)
		{
			if (bookmark == null)
			{
				throw new ArgumentNullException(nameof(bookmark));
			}
			return Api.TryGotoBookmark(Session, Table, bookmark, bookmarkSize);
		}
		
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    public void DeleteCurrentRow()
	    {
	        Api.JetDelete(Session, Table);
	    }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] GetBookmark()
		{
			return Api.GetBookmark(Session, Table);
		}

		public static class ViewValues
		{

			// ReSharper disable once InconsistentNaming
			public struct ReferenceView
			{
				public Guid ReferenceId;
			}
		}

		public static class FetchViews {

			// ReSharper disable once InconsistentNaming
			public struct ReferenceView
			{
				private readonly ReferencesTable _table;
				private readonly ColumnValue[] _c;

				public ReferenceView(ReferencesTable table)
				{
					_table = table;

					_c = new ColumnValue[1];
					_c[0] = new GuidColumnValue() {
						Columnid = _table.ColumnDictionary[ReferencesTable.Column.ReferenceId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.ReferenceView Fetch()
				{
					var r = new ViewValues.ReferenceView();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.ReferenceId = ((GuidColumnValue)_c[0]).Value.Value;
					return r;
				}
			}
	
		}

		public class TableFetchViews
		{
			private readonly ReferencesTable _table;

			public TableFetchViews(ReferencesTable table)
			{
				_table = table;
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.ReferenceView? __fv_ReferenceView;
			public FetchViews.ReferenceView ReferenceView
			{
				get
				{
					if (__fv_ReferenceView == null)
					{
						__fv_ReferenceView = new FetchViews.ReferenceView(_table);
					}
					return __fv_ReferenceView.Value;
				}
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
			public struct ReferenceView
			{
				private readonly ReferencesTable _table;
				private readonly ColumnValue[] _c;

				public ReferenceView(ReferencesTable table)
				{
					_table = table;

					_c = new ColumnValue[1];
					_c[0] = new GuidColumnValue() {
						Columnid = _table.ColumnDictionary[ReferencesTable.Column.ReferenceId],
						SetGrbit = SetColumnGrbit.None
					};
				}

				public void Set(ViewValues.ReferenceView value)
				{
					((GuidColumnValue)_c[0]).Value = value.ReferenceId;
					Api.SetColumns(_table.Session, _table, _c);
				}

				public void Set(ref ViewValues.ReferenceView value)
				{
					((GuidColumnValue)_c[0]).Value = value.ReferenceId;
					Api.SetColumns(_table.Session, _table, _c);
				}
			}			
		}

		public class TableInsertViews
		{
			private readonly ReferencesTable _table;

			public TableInsertViews(ReferencesTable table)
			{
				_table = table;
			}

			public Update CreateUpdate() => new Update(_table.Session, _table, JET_prep.Insert);

			private byte[] _bookmarkBuffer;

			private void EnsureBookmarkBuffer()
			{
				if (_bookmarkBuffer == null)
				{
					_bookmarkBuffer = new byte[SystemParameters.BookmarkMost];
				}
			}

			public void SaveUpdateWithBookmark(Update update, out byte[] bookmark)
			{
				EnsureBookmarkBuffer();
				int bsize;
				update.Save(_bookmarkBuffer, _bookmarkBuffer.Length, out bsize);
				bookmark = new byte[bsize];
				Array.Copy(_bookmarkBuffer, bookmark, bsize);
			}


		    // ReSharper disable once InconsistentNaming
			private InsertOrUpdateViews.ReferenceView? __iuv_ReferenceView;

			public InsertOrUpdateViews.ReferenceView ReferenceView
			{
				get
				{
					if (__iuv_ReferenceView == null)
					{
						__iuv_ReferenceView = new InsertOrUpdateViews.ReferenceView(_table);
					}
					return __iuv_ReferenceView.Value;
				}
			}

			public void InsertAsReferenceView(ViewValues.ReferenceView value)
			{
				using (var update = CreateUpdate())
				{
					ReferenceView.Set(value);
					update.Save();
				}
			}

			public void InsertAsReferenceView(ref ViewValues.ReferenceView value)
			{
				using (var update = CreateUpdate())
				{
					ReferenceView.Set(ref value);
					update.Save();
				}
			}

			public void InsertAsReferenceView(ViewValues.ReferenceView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					ReferenceView.Set(value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void InsertAsReferenceView(ref ViewValues.ReferenceView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					ReferenceView.Set(ref value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}
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
			private readonly ReferencesTable _table;

			public TableUpdateViews(ReferencesTable table)
			{
				_table = table;
			}

			public Update CreateUpdate() => new Update(_table.Session, _table, JET_prep.Replace);

			private byte[] _bookmarkBuffer;

			private void EnsureBookmarkBuffer()
			{
				if (_bookmarkBuffer == null)
				{
					_bookmarkBuffer = new byte[SystemParameters.BookmarkMost];
				}
			}

			public void SaveUpdateWithBookmark(Update update, out byte[] bookmark)
			{
				EnsureBookmarkBuffer();
				int bsize;
				update.Save(_bookmarkBuffer, _bookmarkBuffer.Length, out bsize);
				bookmark = new byte[bsize];
				Array.Copy(_bookmarkBuffer, bookmark, bsize);
			}

		    // ReSharper disable once InconsistentNaming
			private InsertOrUpdateViews.ReferenceView? __iuv_ReferenceView;

			public InsertOrUpdateViews.ReferenceView ReferenceView
			{
				get
				{
					if (__iuv_ReferenceView == null)
					{
						__iuv_ReferenceView = new InsertOrUpdateViews.ReferenceView(_table);
					}
					return __iuv_ReferenceView.Value;
				}
			}

			public void UpdateAsReferenceView(ViewValues.ReferenceView value)
			{
				using (var update = CreateUpdate())
				{
					ReferenceView.Set(value);
					update.Save();
				}
			}

			public void UpdateAsReferenceView(ref ViewValues.ReferenceView value)
			{
				using (var update = CreateUpdate())
				{
					ReferenceView.Set(ref value);
					update.Save();
				}
			}

			public void UpdateAsReferenceView(ViewValues.ReferenceView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					ReferenceView.Set(value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void UpdateAsReferenceView(ref ViewValues.ReferenceView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					ReferenceView.Set(ref value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}
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
				private readonly ReferencesTable _table;

				public PrimaryIndex(ReferencesTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "PrimaryIndex");
				}

				public struct PrimaryIndexKey
				{
					public Guid ReferenceId;
				}

			    // ReSharper disable InconsistentNaming
				public PrimaryIndexKey CreateKey(
						Guid ReferenceId
				)
			    // ReSharper enable InconsistentNaming
				{
					return new PrimaryIndexKey() {
						ReferenceId = ReferenceId,
					
					};
				}

				public void SetKey(PrimaryIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.ReferenceId,  MakeKeyGrbit.NewKey);
				}

				public bool Find(PrimaryIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
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
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }
				
			}
		}

		public class TableIndexes
		{
			private readonly ReferencesTable _table;

			public TableIndexes(ReferencesTable table)
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

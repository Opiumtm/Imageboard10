

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
namespace Imageboard10.Core.ModelStorage.Posts
{
	internal sealed class AccessLogTable : IDisposable
	{
        public readonly Session Session;
        public readonly JET_TABLEID Table;

		public AccessLogTable(Session session, JET_TABLEID table)
        {
            Session = session;
            Table = table;
			_columnDic = null;
			Columns = new DefaultView(this);
        }

	    public AccessLogTable(Session session, JET_DBID dbid, string tableName, OpenTableGrbit grbit)
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

        public static implicit operator JET_TABLEID(AccessLogTable src)
        {
            return src.Table;
        }

		public enum Column
		{
			Id,
			EntityId,
			AccessTime,
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JET_COLUMNID GetColumnid(Column columnName)
        {
			switch (columnName)
			{
				case Column.Id:
					return Api.GetTableColumnid(Session, Table, "Id");
				case Column.EntityId:
					return Api.GetTableColumnid(Session, Table, "EntityId");
				case Column.AccessTime:
					return Api.GetTableColumnid(Session, Table, "AccessTime");
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
						{ Column.EntityId, Api.GetTableColumnid(Session, Table, "EntityId") },
						{ Column.AccessTime, Api.GetTableColumnid(Session, Table, "AccessTime") },
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
				coltyp = VistaColtyp.GUID,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "EntityId", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "AccessTime", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.DateTime,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
			var idxDef1 = "+Id\0\0";
			Api.JetCreateIndex(sid, tableid, "PrimaryIndex", CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, idxDef1, idxDef1.Length, 100);
			var idxDef2 = "+EntityId\0\0";
			Api.JetCreateIndex(sid, tableid, "EntityIdIndex", CreateIndexGrbit.None, idxDef2, idxDef2.Length, 100);
			var idxDef3 = "+EntityId\0+AccessTime\0\0";
			Api.JetCreateIndex(sid, tableid, "EntityIdAndAccessTimeIndex", CreateIndexGrbit.None, idxDef3, idxDef3.Length, 100);
		}

		public struct Multivalue<T> where T : ColumnValue, new()
        {
            private readonly AccessLogTable _table;
            private readonly JET_RETRIEVECOLUMN[] _r;
            private readonly T[] _c;
            private readonly JET_COLUMNID _columnid;

            public Multivalue(AccessLogTable table, JET_COLUMNID columnid)
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
			private readonly AccessLogTable _table;

			public DefaultView(AccessLogTable table)
			{
				_table = table;
			}
			public Guid Id
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsGuid(_table.Session, _table, _table.ColumnDictionary[AccessLogTable.Column.Id]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[AccessLogTable.Column.Id], value);

			}

			public int EntityId
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[AccessLogTable.Column.EntityId]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[AccessLogTable.Column.EntityId], value);

			}

			public DateTime AccessTime
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsDateTime(_table.Session, _table, _table.ColumnDictionary[AccessLogTable.Column.AccessTime]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[AccessLogTable.Column.AccessTime], value);

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

	    public IEnumerable<object> EnumerateToEnd(int skip, int? maxCount)
	    {
			if (skip > 0)
			{
				if (!TryMove(skip))
				{
					yield break;
				}
			}
	        while (Api.TryMoveNext(Session, Table) && (maxCount > 0 || maxCount == null))
	        {
	            yield return this;
				if (maxCount != null)
				{
					maxCount--;
				}
	        }
	    }

	    public IEnumerable<object> EnumerateToEnd(int? maxCount)
	    {
	        while (Api.TryMoveNext(Session, Table) && (maxCount > 0 || maxCount == null))
	        {
	            yield return this;
				if (maxCount != null)
				{
					maxCount--;
				}
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
	    public bool TryMove(int delta)
	    {
	        return Api.TryMove(Session, Table, (JET_Move)delta, MoveGrbit.None);
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
			public struct AccessTimeAndId
			{
				public Guid Id;
				public DateTime AccessTime;
			}

			// ReSharper disable once InconsistentNaming
			public struct InsertAllColumnsView
			{
				public Guid Id;
				public int EntityId;
				public DateTime AccessTime;
			}
		}

		public static class FetchViews {

			// ReSharper disable once InconsistentNaming
			public struct AccessTimeAndId
			{
				private readonly AccessLogTable _table;
				private readonly ColumnValue[] _c;

				public AccessTimeAndId(AccessLogTable table)
				{
					_table = table;

					_c = new ColumnValue[2];
					_c[0] = new GuidColumnValue() {
						Columnid = _table.ColumnDictionary[AccessLogTable.Column.Id],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new DateTimeColumnValue() {
						Columnid = _table.ColumnDictionary[AccessLogTable.Column.AccessTime],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.AccessTimeAndId Fetch()
				{
					var r = new ViewValues.AccessTimeAndId();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.Id = ((GuidColumnValue)_c[0]).Value.Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.AccessTime = ((DateTimeColumnValue)_c[1]).Value.Value;
					return r;
				}
			}
	
		}

		public class TableFetchViews
		{
			private readonly AccessLogTable _table;

			public TableFetchViews(AccessLogTable table)
			{
				_table = table;
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.AccessTimeAndId? __fv_AccessTimeAndId;
			public FetchViews.AccessTimeAndId AccessTimeAndId
			{
				get
				{
					if (__fv_AccessTimeAndId == null)
					{
						__fv_AccessTimeAndId = new FetchViews.AccessTimeAndId(_table);
					}
					return __fv_AccessTimeAndId.Value;
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
			public struct InsertAllColumnsView
			{
				private readonly AccessLogTable _table;
				private readonly ColumnValue[] _c;

				public InsertAllColumnsView(AccessLogTable table)
				{
					_table = table;

					_c = new ColumnValue[3];
					_c[0] = new GuidColumnValue() {
						Columnid = _table.ColumnDictionary[AccessLogTable.Column.Id],
						SetGrbit = SetColumnGrbit.None
					};
					_c[1] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[AccessLogTable.Column.EntityId],
						SetGrbit = SetColumnGrbit.None
					};
					_c[2] = new DateTimeColumnValue() {
						Columnid = _table.ColumnDictionary[AccessLogTable.Column.AccessTime],
						SetGrbit = SetColumnGrbit.None
					};
				}

				public void Set(ViewValues.InsertAllColumnsView value)
				{
					((GuidColumnValue)_c[0]).Value = value.Id;
					((Int32ColumnValue)_c[1]).Value = value.EntityId;
					((DateTimeColumnValue)_c[2]).Value = value.AccessTime;
					Api.SetColumns(_table.Session, _table, _c);
				}

				public void Set(ref ViewValues.InsertAllColumnsView value)
				{
					((GuidColumnValue)_c[0]).Value = value.Id;
					((Int32ColumnValue)_c[1]).Value = value.EntityId;
					((DateTimeColumnValue)_c[2]).Value = value.AccessTime;
					Api.SetColumns(_table.Session, _table, _c);
				}
			}			
		}

		public class TableInsertViews
		{
			private readonly AccessLogTable _table;

			public TableInsertViews(AccessLogTable table)
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
			private InsertOrUpdateViews.InsertAllColumnsView? __iuv_InsertAllColumnsView;

			public InsertOrUpdateViews.InsertAllColumnsView InsertAllColumnsView
			{
				get
				{
					if (__iuv_InsertAllColumnsView == null)
					{
						__iuv_InsertAllColumnsView = new InsertOrUpdateViews.InsertAllColumnsView(_table);
					}
					return __iuv_InsertAllColumnsView.Value;
				}
			}

			public void InsertAsInsertAllColumnsView(ViewValues.InsertAllColumnsView value)
			{
				using (var update = CreateUpdate())
				{
					InsertAllColumnsView.Set(value);
					update.Save();
				}
			}

			public void InsertAsInsertAllColumnsView(ref ViewValues.InsertAllColumnsView value)
			{
				using (var update = CreateUpdate())
				{
					InsertAllColumnsView.Set(ref value);
					update.Save();
				}
			}

			public void InsertAsInsertAllColumnsView(ViewValues.InsertAllColumnsView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					InsertAllColumnsView.Set(value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void InsertAsInsertAllColumnsView(ref ViewValues.InsertAllColumnsView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					InsertAllColumnsView.Set(ref value);
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
			private readonly AccessLogTable _table;

			public TableUpdateViews(AccessLogTable table)
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
				private readonly AccessLogTable _table;

				public PrimaryIndex(AccessLogTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "PrimaryIndex");
				}

				public struct PrimaryIndexKey
				{
					public Guid Id;
				}

			    // ReSharper disable InconsistentNaming
				public PrimaryIndexKey CreateKey(
						Guid Id
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

			public struct EntityIdIndex
			{
				private readonly AccessLogTable _table;

				public EntityIdIndex(AccessLogTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "EntityIdIndex");
				}

				public struct EntityIdIndexKey
				{
					public int EntityId;
				}

			    // ReSharper disable InconsistentNaming
				public EntityIdIndexKey CreateKey(
						int EntityId
				)
			    // ReSharper enable InconsistentNaming
				{
					return new EntityIdIndexKey() {
						EntityId = EntityId,
					
					};
				}

				public void SetKey(EntityIdIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.EntityId,  MakeKeyGrbit.NewKey);
				}

				public bool Find(EntityIdIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(EntityIdIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(EntityIdIndexKey key)
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

			    public int GetIndexRecordCount(EntityIdIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }
				
			}

			public struct EntityIdAndAccessTimeIndex
			{
				private readonly AccessLogTable _table;

				public EntityIdAndAccessTimeIndex(AccessLogTable table)
				{
					_table = table;
					_views = new IndexFetchViews(_table);
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "EntityIdAndAccessTimeIndex");
				}

				public struct EntityIdAndAccessTimeIndexKey
				{
					public int EntityId;
					public DateTime AccessTime;
				}

			    // ReSharper disable InconsistentNaming
				public EntityIdAndAccessTimeIndexKey CreateKey(
						int EntityId
						,DateTime AccessTime
				)
			    // ReSharper enable InconsistentNaming
				{
					return new EntityIdAndAccessTimeIndexKey() {
						EntityId = EntityId,
						AccessTime = AccessTime,
					
					};
				}

				public void SetKey(EntityIdAndAccessTimeIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.EntityId,  MakeKeyGrbit.NewKey);
					Api.MakeKey(_table.Session, _table, key.AccessTime,  MakeKeyGrbit.None);
				}

				public bool Find(EntityIdAndAccessTimeIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(EntityIdAndAccessTimeIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(EntityIdAndAccessTimeIndexKey key)
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

			    public int GetIndexRecordCount(EntityIdAndAccessTimeIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public struct EntityIdAndAccessTimeIndexPartialKey1
				{
					public int EntityId;
				}

			    // ReSharper disable InconsistentNaming
				public EntityIdAndAccessTimeIndexPartialKey1 CreateKey(
						int EntityId
				)
			    // ReSharper enable InconsistentNaming
				{
					return new EntityIdAndAccessTimeIndexPartialKey1() {
						EntityId = EntityId,
					
					};
				}

				public void SetKey(EntityIdAndAccessTimeIndexPartialKey1 key, bool startRange)
				{
					var rangeFlag = startRange ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit;
					Api.MakeKey(_table.Session, _table, key.EntityId,  MakeKeyGrbit.NewKey | rangeFlag);
				}

				public bool Find(EntityIdAndAccessTimeIndexPartialKey1 key)
				{
					SetKey(key, true);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE);
				}

				public bool SetPartialUpperRange(EntityIdAndAccessTimeIndexPartialKey1 key)
				{
					SetKey(key, false);
					return Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit);
				}

				public bool SeekPartial(EntityIdAndAccessTimeIndexPartialKey1 key)
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

				public IEnumerable<object> Enumerate(EntityIdAndAccessTimeIndexPartialKey1 key)
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

				public int GetIndexRecordCount(EntityIdAndAccessTimeIndexPartialKey1 key)
			    {
					if (!SeekPartial(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public class IndexFetchViews
				{
					private readonly AccessLogTable _table;

					public IndexFetchViews(AccessLogTable table)
					{
						_table = table;
					}

					// ReSharper disable once InconsistentNaming
					private FetchViews.AccessTimeAndId? __fv_AccessTimeAndId;
					public FetchViews.AccessTimeAndId AccessTimeAndId
					{
						get
						{
							if (__fv_AccessTimeAndId == null)
							{
								__fv_AccessTimeAndId = new FetchViews.AccessTimeAndId(_table);
							}
							return __fv_AccessTimeAndId.Value;
						}
					}
				}

				private readonly IndexFetchViews _views;
			    // ReSharper disable once ConvertToAutoProperty
				public IndexFetchViews Views => _views;

				public IEnumerable<ViewValues.AccessTimeAndId> EnumerateAsAccessTimeAndId()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.AccessTimeAndId.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.AccessTimeAndId> EnumerateAsAccessTimeAndId(EntityIdAndAccessTimeIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.AccessTimeAndId.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.AccessTimeAndId> EnumerateUniqueAsAccessTimeAndId()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.AccessTimeAndId.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.AccessTimeAndId> EnumerateUniqueAsAccessTimeAndId(EntityIdAndAccessTimeIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.AccessTimeAndId.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.AccessTimeAndId> EnumerateAsAccessTimeAndId(EntityIdAndAccessTimeIndexPartialKey1 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return Views.AccessTimeAndId.Fetch();
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}
								
				
			}
		}

		public class TableIndexes
		{
			private readonly AccessLogTable _table;

			public TableIndexes(AccessLogTable table)
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
			private IndexDefinitions.EntityIdIndex? __ti_EntityIdIndex;

			public IndexDefinitions.EntityIdIndex EntityIdIndex
			{
				get
				{
					if (__ti_EntityIdIndex == null)
					{
						__ti_EntityIdIndex = new IndexDefinitions.EntityIdIndex(_table);
					}
					return __ti_EntityIdIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.EntityIdAndAccessTimeIndex? __ti_EntityIdAndAccessTimeIndex;

			public IndexDefinitions.EntityIdAndAccessTimeIndex EntityIdAndAccessTimeIndex
			{
				get
				{
					if (__ti_EntityIdAndAccessTimeIndex == null)
					{
						__ti_EntityIdAndAccessTimeIndex = new IndexDefinitions.EntityIdAndAccessTimeIndex(_table);
					}
					return __ti_EntityIdAndAccessTimeIndex.Value;
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

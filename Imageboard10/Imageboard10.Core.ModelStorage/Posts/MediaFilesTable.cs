

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
	internal sealed class MediaFilesTable : IDisposable
	{
        public readonly Session Session;
        public readonly JET_TABLEID Table;

		public MediaFilesTable(Session session, JET_TABLEID table)
        {
            Session = session;
            Table = table;
			_columnDic = null;
			Columns = new DefaultView(this);
        }

	    public MediaFilesTable(Session session, JET_DBID dbid, string tableName, OpenTableGrbit grbit)
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

        public static implicit operator JET_TABLEID(MediaFilesTable src)
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
				coltyp = JET_coltyp.Currency,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "MediaData", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongBinary,
				grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);			
			var idxDef1 = "+Id\0\0";
			Api.JetCreateIndex(sid, tableid, "PrimaryIndex", CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, idxDef1, idxDef1.Length, 100);
			var idxDef2 = "+EntityReferences\0\0";
			Api.JetCreateIndex(sid, tableid, "EntityReferencesIndex", CreateIndexGrbit.IndexIgnoreAnyNull, idxDef2, idxDef2.Length, 100);
			var idxDef3 = "+EntityReferences\0+SequenceNumber\0\0";
			Api.JetCreateIndex(sid, tableid, "SequencesIndex", CreateIndexGrbit.IndexIgnoreFirstNull, idxDef3, idxDef3.Length, 100);
		}

		public struct Multivalue<T> where T : ColumnValue, new()
        {
            private readonly MediaFilesTable _table;
            private readonly JET_RETRIEVECOLUMN[] _r;
            private readonly T[] _c;
            private readonly JET_COLUMNID _columnid;

            public Multivalue(MediaFilesTable table, JET_COLUMNID columnid)
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
					value[i].Columnid = _columnid;
				}
				// ReSharper disable once CoVariantArrayConversion
                Api.SetColumns(_table.Session, _table.Table, value);
			}
        }


		public struct DefaultView
		{
			private readonly MediaFilesTable _table;

		    // ReSharper disable once InconsistentNaming
			private readonly Multivalue<Int32ColumnValue> __mv_EntityReferences;

			public DefaultView(MediaFilesTable table)
			{
				_table = table;
				__mv_EntityReferences = new Multivalue<Int32ColumnValue>(table, table.ColumnDictionary[MediaFilesTable.Column.EntityReferences]);
			}
			public int Id
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[MediaFilesTable.Column.Id]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[MediaFilesTable.Column.Id], value);

			}

		    // ReSharper disable once InconsistentNaming
			public int Id_AutoincrementValue
			{
				// ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[MediaFilesTable.Column.Id], RetrieveColumnGrbit.RetrieveCopy).Value;
			}

		    // ReSharper disable once ConvertToAutoProperty
			public Multivalue<Int32ColumnValue> EntityReferences => __mv_EntityReferences;
			
			public void SetEntityReferencesValueArr(Int32ColumnValue[] v)
			{
			    // ReSharper disable once ImpureMethodCallOnReadonlyValueField
				__mv_EntityReferences.SetValues(v);
			}
			public long SequenceNumber
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt64(_table.Session, _table, _table.ColumnDictionary[MediaFilesTable.Column.SequenceNumber]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[MediaFilesTable.Column.SequenceNumber], value);

			}

			public byte[] MediaData
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumn(_table.Session, _table, _table.ColumnDictionary[MediaFilesTable.Column.MediaData]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[MediaFilesTable.Column.MediaData], value);

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
			public struct MediaDataView
			{
				public byte[] MediaData;
			}

			// ReSharper disable once InconsistentNaming
			public struct InsertView
			{
				public Int32ColumnValue[] EntityReferences;
				public long SequenceNumber;
				public byte[] MediaData;
			}
		}

		public static class FetchViews {

			// ReSharper disable once InconsistentNaming
			public struct MediaDataView
			{
				private readonly MediaFilesTable _table;
				private readonly ColumnValue[] _c;

				public MediaDataView(MediaFilesTable table)
				{
					_table = table;

					_c = new ColumnValue[1];
					_c[0] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[MediaFilesTable.Column.MediaData],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.MediaDataView Fetch()
				{
					var r = new ViewValues.MediaDataView();
					Api.RetrieveColumns(_table.Session, _table, _c);
					r.MediaData = ((BytesColumnValue)_c[0]).Value;
					return r;
				}
			}
	
		}

		public class TableFetchViews
		{
			private readonly MediaFilesTable _table;

			public TableFetchViews(MediaFilesTable table)
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
			public struct InsertView
			{
				private readonly MediaFilesTable _table;
				private readonly ColumnValue[] _c;

				public InsertView(MediaFilesTable table)
				{
					_table = table;

					_c = new ColumnValue[2];
					_c[0] = new Int64ColumnValue() {
						Columnid = _table.ColumnDictionary[MediaFilesTable.Column.SequenceNumber],
						SetGrbit = SetColumnGrbit.None
					};
					_c[1] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[MediaFilesTable.Column.MediaData],
						SetGrbit = SetColumnGrbit.None
					};
				}

				public void Set(ViewValues.InsertView value)
				{
					((Int64ColumnValue)_c[0]).Value = value.SequenceNumber;
					((BytesColumnValue)_c[1]).Value = value.MediaData;
					Api.SetColumns(_table.Session, _table, _c);
					_table.Columns.SetEntityReferencesValueArr(value.EntityReferences);
				}

				public void Set(ref ViewValues.InsertView value)
				{
					((Int64ColumnValue)_c[0]).Value = value.SequenceNumber;
					((BytesColumnValue)_c[1]).Value = value.MediaData;
					Api.SetColumns(_table.Session, _table, _c);
					_table.Columns.SetEntityReferencesValueArr(value.EntityReferences);
				}
			}			
		}

		public class TableInsertViews
		{
			private readonly MediaFilesTable _table;

			public TableInsertViews(MediaFilesTable table)
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
			private InsertOrUpdateViews.InsertView? __iuv_InsertView;

			public InsertOrUpdateViews.InsertView InsertView
			{
				get
				{
					if (__iuv_InsertView == null)
					{
						__iuv_InsertView = new InsertOrUpdateViews.InsertView(_table);
					}
					return __iuv_InsertView.Value;
				}
			}

			public void InsertAsInsertView(ViewValues.InsertView value)
			{
				using (var update = CreateUpdate())
				{
					InsertView.Set(value);
					update.Save();
				}
			}

			public void InsertAsInsertView(ref ViewValues.InsertView value)
			{
				using (var update = CreateUpdate())
				{
					InsertView.Set(ref value);
					update.Save();
				}
			}

			public void InsertAsInsertView(ViewValues.InsertView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					InsertView.Set(value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void InsertAsInsertView(ref ViewValues.InsertView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					InsertView.Set(ref value);
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
			private readonly MediaFilesTable _table;

			public TableUpdateViews(MediaFilesTable table)
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
				private readonly MediaFilesTable _table;

				public PrimaryIndex(MediaFilesTable table)
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

			public struct EntityReferencesIndex
			{
				private readonly MediaFilesTable _table;

				public EntityReferencesIndex(MediaFilesTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "EntityReferencesIndex");
				}

				public struct EntityReferencesIndexKey
				{
					public int? EntityReferences;
				}

			    // ReSharper disable InconsistentNaming
				public EntityReferencesIndexKey CreateKey(
						int? EntityReferences
				)
			    // ReSharper enable InconsistentNaming
				{
					return new EntityReferencesIndexKey() {
						EntityReferences = EntityReferences,
					
					};
				}

				public void SetKey(EntityReferencesIndexKey key)
				{
					if (key.EntityReferences == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.EntityReferences.Value, MakeKeyGrbit.NewKey);
					}
				}

				public bool Find(EntityReferencesIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(EntityReferencesIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(EntityReferencesIndexKey key)
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

			    public int GetIndexRecordCount(EntityReferencesIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }
				
			}

			public struct SequencesIndex
			{
				private readonly MediaFilesTable _table;

				public SequencesIndex(MediaFilesTable table)
				{
					_table = table;
					_views = new IndexFetchViews(_table);
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "SequencesIndex");
				}

				public struct SequencesIndexKey
				{
					public int? EntityReferences;
					public long SequenceNumber;
				}

			    // ReSharper disable InconsistentNaming
				public SequencesIndexKey CreateKey(
						int? EntityReferences
						,long SequenceNumber
				)
			    // ReSharper enable InconsistentNaming
				{
					return new SequencesIndexKey() {
						EntityReferences = EntityReferences,
						SequenceNumber = SequenceNumber,
					
					};
				}

				public void SetKey(SequencesIndexKey key)
				{
					if (key.EntityReferences == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.EntityReferences.Value, MakeKeyGrbit.NewKey);
					}
					Api.MakeKey(_table.Session, _table, key.SequenceNumber,  MakeKeyGrbit.None);
				}

				public bool Find(SequencesIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(SequencesIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(SequencesIndexKey key)
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

			    public int GetIndexRecordCount(SequencesIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public struct SequencesIndexPartialKey1
				{
					public int? EntityReferences;
				}

			    // ReSharper disable InconsistentNaming
				public SequencesIndexPartialKey1 CreateKey(
						int? EntityReferences
				)
			    // ReSharper enable InconsistentNaming
				{
					return new SequencesIndexPartialKey1() {
						EntityReferences = EntityReferences,
					
					};
				}

				public void SetKey(SequencesIndexPartialKey1 key, bool startRange)
				{
					var rangeFlag = startRange ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit;
					if (key.EntityReferences == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey | rangeFlag);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.EntityReferences.Value, MakeKeyGrbit.NewKey | rangeFlag);
					}
				}

				public bool Find(SequencesIndexPartialKey1 key)
				{
					SetKey(key, true);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE);
				}

				public bool SetPartialUpperRange(SequencesIndexPartialKey1 key)
				{
					SetKey(key, false);
					return Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit);
				}

				public bool SeekPartial(SequencesIndexPartialKey1 key)
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

				public IEnumerable<object> Enumerate(SequencesIndexPartialKey1 key)
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

				public int GetIndexRecordCount(SequencesIndexPartialKey1 key)
			    {
					if (!SeekPartial(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public class IndexFetchViews
				{
					private readonly MediaFilesTable _table;

					public IndexFetchViews(MediaFilesTable table)
					{
						_table = table;
					}

					// ReSharper disable once InconsistentNaming
					private FetchViews.MediaDataView? __fv_MediaDataView;
					public FetchViews.MediaDataView MediaDataView
					{
						get
						{
							if (__fv_MediaDataView == null)
							{
								__fv_MediaDataView = new FetchViews.MediaDataView(_table);
							}
							return __fv_MediaDataView.Value;
						}
					}
				}

				private readonly IndexFetchViews _views;
			    // ReSharper disable once ConvertToAutoProperty
				public IndexFetchViews Views => _views;

				public IEnumerable<ViewValues.MediaDataView> EnumerateAsMediaDataView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.MediaDataView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.MediaDataView> EnumerateAsMediaDataView(SequencesIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.MediaDataView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.MediaDataView> EnumerateUniqueAsMediaDataView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.MediaDataView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.MediaDataView> EnumerateUniqueAsMediaDataView(SequencesIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.MediaDataView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.MediaDataView> EnumerateAsMediaDataView(SequencesIndexPartialKey1 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return Views.MediaDataView.Fetch();
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}
								
				
			}
		}

		public class TableIndexes
		{
			private readonly MediaFilesTable _table;

			public TableIndexes(MediaFilesTable table)
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
			private IndexDefinitions.EntityReferencesIndex? __ti_EntityReferencesIndex;

			public IndexDefinitions.EntityReferencesIndex EntityReferencesIndex
			{
				get
				{
					if (__ti_EntityReferencesIndex == null)
					{
						__ti_EntityReferencesIndex = new IndexDefinitions.EntityReferencesIndex(_table);
					}
					return __ti_EntityReferencesIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.SequencesIndex? __ti_SequencesIndex;

			public IndexDefinitions.SequencesIndex SequencesIndex
			{
				get
				{
					if (__ti_SequencesIndex == null)
					{
						__ti_SequencesIndex = new IndexDefinitions.SequencesIndex(_table);
					}
					return __ti_SequencesIndex.Value;
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

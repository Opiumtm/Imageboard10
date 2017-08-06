

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
	internal sealed class PostsTable : IDisposable
	{
        public readonly Session Session;
        public readonly JET_TABLEID Table;

		public PostsTable(Session session, JET_TABLEID table)
        {
            Session = session;
            Table = table;
			_columnDic = null;
			Columns = new DefaultView(this);
        }

	    public PostsTable(Session session, JET_DBID dbid, string tableName, OpenTableGrbit grbit)
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

        public static implicit operator JET_TABLEID(PostsTable src)
        {
            return src.Table;
        }

		public enum Column
		{
			Id,
			ParentId,
			DirectParentId,
			EntityType,
			DataLoaded,
			ChildrenLoadStage,
			BoardId,
			SequenceNumber,
			ParentSequenceNumber,
			Subject,
			Thumbnail,
			Date,
			BoardSpecificDate,
			Flags,
			ThreadTags,
			Likes,
			Dislikes,
			Document,
			QuotedPosts,
			LoadedTime,
			Etag,
			PosterName,
			OtherDataBinary,
			PreviewCounts,
			LastServerUpdate,
			NumberOfPostsOnServer,
			NumberOfReadPosts,
			LastPostLinkOnServer,
			OnServerSequenceCounter,
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JET_COLUMNID GetColumnid(Column columnName)
        {
			switch (columnName)
			{
				case Column.Id:
					return Api.GetTableColumnid(Session, Table, "Id");
				case Column.ParentId:
					return Api.GetTableColumnid(Session, Table, "ParentId");
				case Column.DirectParentId:
					return Api.GetTableColumnid(Session, Table, "DirectParentId");
				case Column.EntityType:
					return Api.GetTableColumnid(Session, Table, "EntityType");
				case Column.DataLoaded:
					return Api.GetTableColumnid(Session, Table, "DataLoaded");
				case Column.ChildrenLoadStage:
					return Api.GetTableColumnid(Session, Table, "ChildrenLoadStage");
				case Column.BoardId:
					return Api.GetTableColumnid(Session, Table, "BoardId");
				case Column.SequenceNumber:
					return Api.GetTableColumnid(Session, Table, "SequenceNumber");
				case Column.ParentSequenceNumber:
					return Api.GetTableColumnid(Session, Table, "ParentSequenceNumber");
				case Column.Subject:
					return Api.GetTableColumnid(Session, Table, "Subject");
				case Column.Thumbnail:
					return Api.GetTableColumnid(Session, Table, "Thumbnail");
				case Column.Date:
					return Api.GetTableColumnid(Session, Table, "Date");
				case Column.BoardSpecificDate:
					return Api.GetTableColumnid(Session, Table, "BoardSpecificDate");
				case Column.Flags:
					return Api.GetTableColumnid(Session, Table, "Flags");
				case Column.ThreadTags:
					return Api.GetTableColumnid(Session, Table, "ThreadTags");
				case Column.Likes:
					return Api.GetTableColumnid(Session, Table, "Likes");
				case Column.Dislikes:
					return Api.GetTableColumnid(Session, Table, "Dislikes");
				case Column.Document:
					return Api.GetTableColumnid(Session, Table, "Document");
				case Column.QuotedPosts:
					return Api.GetTableColumnid(Session, Table, "QuotedPosts");
				case Column.LoadedTime:
					return Api.GetTableColumnid(Session, Table, "LoadedTime");
				case Column.Etag:
					return Api.GetTableColumnid(Session, Table, "Etag");
				case Column.PosterName:
					return Api.GetTableColumnid(Session, Table, "PosterName");
				case Column.OtherDataBinary:
					return Api.GetTableColumnid(Session, Table, "OtherDataBinary");
				case Column.PreviewCounts:
					return Api.GetTableColumnid(Session, Table, "PreviewCounts");
				case Column.LastServerUpdate:
					return Api.GetTableColumnid(Session, Table, "LastServerUpdate");
				case Column.NumberOfPostsOnServer:
					return Api.GetTableColumnid(Session, Table, "NumberOfPostsOnServer");
				case Column.NumberOfReadPosts:
					return Api.GetTableColumnid(Session, Table, "NumberOfReadPosts");
				case Column.LastPostLinkOnServer:
					return Api.GetTableColumnid(Session, Table, "LastPostLinkOnServer");
				case Column.OnServerSequenceCounter:
					return Api.GetTableColumnid(Session, Table, "OnServerSequenceCounter");
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
						{ Column.ParentId, Api.GetTableColumnid(Session, Table, "ParentId") },
						{ Column.DirectParentId, Api.GetTableColumnid(Session, Table, "DirectParentId") },
						{ Column.EntityType, Api.GetTableColumnid(Session, Table, "EntityType") },
						{ Column.DataLoaded, Api.GetTableColumnid(Session, Table, "DataLoaded") },
						{ Column.ChildrenLoadStage, Api.GetTableColumnid(Session, Table, "ChildrenLoadStage") },
						{ Column.BoardId, Api.GetTableColumnid(Session, Table, "BoardId") },
						{ Column.SequenceNumber, Api.GetTableColumnid(Session, Table, "SequenceNumber") },
						{ Column.ParentSequenceNumber, Api.GetTableColumnid(Session, Table, "ParentSequenceNumber") },
						{ Column.Subject, Api.GetTableColumnid(Session, Table, "Subject") },
						{ Column.Thumbnail, Api.GetTableColumnid(Session, Table, "Thumbnail") },
						{ Column.Date, Api.GetTableColumnid(Session, Table, "Date") },
						{ Column.BoardSpecificDate, Api.GetTableColumnid(Session, Table, "BoardSpecificDate") },
						{ Column.Flags, Api.GetTableColumnid(Session, Table, "Flags") },
						{ Column.ThreadTags, Api.GetTableColumnid(Session, Table, "ThreadTags") },
						{ Column.Likes, Api.GetTableColumnid(Session, Table, "Likes") },
						{ Column.Dislikes, Api.GetTableColumnid(Session, Table, "Dislikes") },
						{ Column.Document, Api.GetTableColumnid(Session, Table, "Document") },
						{ Column.QuotedPosts, Api.GetTableColumnid(Session, Table, "QuotedPosts") },
						{ Column.LoadedTime, Api.GetTableColumnid(Session, Table, "LoadedTime") },
						{ Column.Etag, Api.GetTableColumnid(Session, Table, "Etag") },
						{ Column.PosterName, Api.GetTableColumnid(Session, Table, "PosterName") },
						{ Column.OtherDataBinary, Api.GetTableColumnid(Session, Table, "OtherDataBinary") },
						{ Column.PreviewCounts, Api.GetTableColumnid(Session, Table, "PreviewCounts") },
						{ Column.LastServerUpdate, Api.GetTableColumnid(Session, Table, "LastServerUpdate") },
						{ Column.NumberOfPostsOnServer, Api.GetTableColumnid(Session, Table, "NumberOfPostsOnServer") },
						{ Column.NumberOfReadPosts, Api.GetTableColumnid(Session, Table, "NumberOfReadPosts") },
						{ Column.LastPostLinkOnServer, Api.GetTableColumnid(Session, Table, "LastPostLinkOnServer") },
						{ Column.OnServerSequenceCounter, Api.GetTableColumnid(Session, Table, "OnServerSequenceCounter") },
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
            Api.JetAddColumn(sid, tableid, "ParentId", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnTagged | ColumndefGrbit.ColumnMultiValued,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "DirectParentId", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "EntityType", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.UnsignedByte,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "DataLoaded", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Bit,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "ChildrenLoadStage", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.UnsignedByte,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "BoardId", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Text,
				grbit = ColumndefGrbit.ColumnNotNULL,
				cp = JET_CP.Unicode,
				cbMax = 50,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "SequenceNumber", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnNotNULL,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "ParentSequenceNumber", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Subject", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.ColumnTagged,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Thumbnail", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongBinary,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Date", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.DateTime,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "BoardSpecificDate", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.ColumnTagged,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Flags", new JET_COLUMNDEF()
            {
				coltyp = VistaColtyp.GUID,
				grbit = ColumndefGrbit.ColumnTagged | ColumndefGrbit.ColumnMultiValued,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "ThreadTags", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.ColumnTagged | ColumndefGrbit.ColumnMultiValued,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Likes", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Dislikes", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Document", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongBinary,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "QuotedPosts", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnTagged | ColumndefGrbit.ColumnMultiValued,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "LoadedTime", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.DateTime,
				grbit = ColumndefGrbit.None,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "Etag", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Text,
				grbit = ColumndefGrbit.ColumnTagged,
				cp = JET_CP.Unicode,
				cbMax = 100,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "PosterName", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongText,
				grbit = ColumndefGrbit.ColumnTagged,
				cp = JET_CP.Unicode,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "OtherDataBinary", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongBinary,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "PreviewCounts", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.LongBinary,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "LastServerUpdate", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.DateTime,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "NumberOfPostsOnServer", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "NumberOfReadPosts", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "LastPostLinkOnServer", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
            Api.JetAddColumn(sid, tableid, "OnServerSequenceCounter", new JET_COLUMNDEF()
            {
				coltyp = JET_coltyp.Long,
				grbit = ColumndefGrbit.ColumnTagged,
            }, null, 0, out tempcolid);			
			var idxDef1 = "+Id\0\0";
			Api.JetCreateIndex(sid, tableid, "PrimaryIndex", CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, idxDef1, idxDef1.Length, 100);
			var idxDef2 = "+ParentId\0\0";
			Api.JetCreateIndex(sid, tableid, "ParentIdIndex", CreateIndexGrbit.IndexIgnoreAnyNull, idxDef2, idxDef2.Length, 100);
			var idxDef3 = "+EntityType\0\0";
			Api.JetCreateIndex(sid, tableid, "TypeIndex", CreateIndexGrbit.None, idxDef3, idxDef3.Length, 100);
			var idxDef4 = "+ChildrenLoadStage\0\0";
			Api.JetCreateIndex(sid, tableid, "ChildrenLoadStageIndex", CreateIndexGrbit.None, idxDef4, idxDef4.Length, 100);
			var idxDef5 = "+EntityType\0+BoardId\0+SequenceNumber\0\0";
			Api.JetCreateIndex(sid, tableid, "TypeAndPostIdIndex", CreateIndexGrbit.None, idxDef5, idxDef5.Length, 100);
			var idxDef6 = "+Flags\0\0";
			Api.JetCreateIndex(sid, tableid, "FlagsIndex", CreateIndexGrbit.None, idxDef6, idxDef6.Length, 100);
			var idxDef7 = "+DirectParentId\0+Flags\0\0";
			Api.JetCreateIndex(sid, tableid, "DirectParentFlagsIndex", CreateIndexGrbit.IndexIgnoreAnyNull, idxDef7, idxDef7.Length, 100);
			var idxDef8 = "+DirectParentId\0+QuotedPosts\0\0";
			Api.JetCreateIndex(sid, tableid, "QuotedPostsIndex", CreateIndexGrbit.IndexIgnoreAnyNull, idxDef8, idxDef8.Length, 100);
			var idxDef9 = "+DirectParentId\0+SequenceNumber\0\0";
			Api.JetCreateIndex(sid, tableid, "InThreadPostLinkIndex", CreateIndexGrbit.IndexIgnoreAnyNull, idxDef9, idxDef9.Length, 100);
		}

		public struct Multivalue<T> where T : ColumnValue, new()
        {
            private readonly PostsTable _table;
            private readonly JET_RETRIEVECOLUMN[] _r;
            private readonly T[] _c;
            private readonly JET_COLUMNID _columnid;

            public Multivalue(PostsTable table, JET_COLUMNID columnid)
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
					_c[0].Columnid = _columnid;
				    // ReSharper disable once CoVariantArrayConversion
                    Api.SetColumns(_table.Session, _table.Table, _c);
				}
            }

			public void Add(T value)
			{
                _c[0] = value ?? throw new ArgumentNullException();
				_c[0].ItagSequence = 0;
				_c[0].Columnid = _columnid;
				// ReSharper disable once CoVariantArrayConversion
                Api.SetColumns(_table.Session, _table.Table, _c);
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
				set => SetValues(value, false);
			}

			public void SetValues(T[] value, bool isInsert)
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (!isInsert)
				{
					Clear();
				}
				if (value.Length == 0)
				{
					return;
				}
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
			private readonly PostsTable _table;

		    // ReSharper disable once InconsistentNaming
			private readonly Multivalue<Int32ColumnValue> __mv_ParentId;

		    // ReSharper disable once InconsistentNaming
			private readonly Multivalue<GuidColumnValue> __mv_Flags;

		    // ReSharper disable once InconsistentNaming
			private readonly Multivalue<StringColumnValue> __mv_ThreadTags;

		    // ReSharper disable once InconsistentNaming
			private readonly Multivalue<Int32ColumnValue> __mv_QuotedPosts;

			public DefaultView(PostsTable table)
			{
				_table = table;
				__mv_ParentId = new Multivalue<Int32ColumnValue>(table, table.GetColumnid(PostsTable.Column.ParentId));
				__mv_Flags = new Multivalue<GuidColumnValue>(table, table.GetColumnid(PostsTable.Column.Flags));
				__mv_ThreadTags = new Multivalue<StringColumnValue>(table, table.GetColumnid(PostsTable.Column.ThreadTags));
				__mv_QuotedPosts = new Multivalue<Int32ColumnValue>(table, table.GetColumnid(PostsTable.Column.QuotedPosts));
			}
			public int Id
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Id]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Id], value);

			}

		    // ReSharper disable once InconsistentNaming
			public int Id_AutoincrementValue
			{
				// ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Id], RetrieveColumnGrbit.RetrieveCopy).Value;
			}

		    // ReSharper disable once ConvertToAutoProperty
			public Multivalue<Int32ColumnValue> ParentId => __mv_ParentId;
			
			public void SetParentIdValueArr(Int32ColumnValue[] v, bool isInsert)
			{
			    // ReSharper disable once ImpureMethodCallOnReadonlyValueField
				__mv_ParentId.SetValues(v, isInsert);
			}
			public int? DirectParentId
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.DirectParentId]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.DirectParentId], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.DirectParentId], null);
					}
				}
			}

			public byte EntityType
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsByte(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.EntityType]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.EntityType], value);

			}

			public bool DataLoaded
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsBoolean(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.DataLoaded]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.DataLoaded], value);

			}

			public byte ChildrenLoadStage
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsByte(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.ChildrenLoadStage]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.ChildrenLoadStage], value);

			}

			public string BoardId
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.BoardId]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.BoardId], value, Encoding.Unicode);

			}

			public int SequenceNumber
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.SequenceNumber]).Value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.SequenceNumber], value);

			}

			public int? ParentSequenceNumber
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber], null);
					}
				}
			}

			public string Subject
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Subject]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Subject], value, Encoding.Unicode);

			}

			public byte[] Thumbnail
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Thumbnail]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Thumbnail], value);

			}

			public DateTime? Date
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsDateTime(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Date]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Date], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Date], null);
					}
				}
			}

			public string BoardSpecificDate
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.BoardSpecificDate]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.BoardSpecificDate], value, Encoding.Unicode);

			}


		    // ReSharper disable once ConvertToAutoProperty
			public Multivalue<GuidColumnValue> Flags => __mv_Flags;
			
			public void SetFlagsValueArr(GuidColumnValue[] v, bool isInsert)
			{
			    // ReSharper disable once ImpureMethodCallOnReadonlyValueField
				__mv_Flags.SetValues(v, isInsert);
			}

		    // ReSharper disable once ConvertToAutoProperty
			public Multivalue<StringColumnValue> ThreadTags => __mv_ThreadTags;
			
			public void SetThreadTagsValueArr(StringColumnValue[] v, bool isInsert)
			{
			    // ReSharper disable once ImpureMethodCallOnReadonlyValueField
				__mv_ThreadTags.SetValues(v, isInsert);
			}
			public int? Likes
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Likes]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Likes], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Likes], null);
					}
				}
			}

			public int? Dislikes
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Dislikes]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Dislikes], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Dislikes], null);
					}
				}
			}

			public byte[] Document
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Document]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Document], value);

			}


		    // ReSharper disable once ConvertToAutoProperty
			public Multivalue<Int32ColumnValue> QuotedPosts => __mv_QuotedPosts;
			
			public void SetQuotedPostsValueArr(Int32ColumnValue[] v, bool isInsert)
			{
			    // ReSharper disable once ImpureMethodCallOnReadonlyValueField
				__mv_QuotedPosts.SetValues(v, isInsert);
			}
			public DateTime? LoadedTime
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsDateTime(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.LoadedTime]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.LoadedTime], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.LoadedTime], null);
					}
				}
			}

			public string Etag
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Etag]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.Etag], value, Encoding.Unicode);

			}

			public string PosterName
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsString(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.PosterName]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.PosterName], value, Encoding.Unicode);

			}

			public byte[] OtherDataBinary
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.OtherDataBinary]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.OtherDataBinary], value);

			}

			public byte[] PreviewCounts
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.PreviewCounts]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.PreviewCounts], value);

			}

			public DateTime? LastServerUpdate
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsDateTime(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.LastServerUpdate]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.LastServerUpdate], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.LastServerUpdate], null);
					}
				}
			}

			public int? NumberOfPostsOnServer
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.NumberOfPostsOnServer]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.NumberOfPostsOnServer], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.NumberOfPostsOnServer], null);
					}
				}
			}

			public int? NumberOfReadPosts
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.NumberOfReadPosts]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.NumberOfReadPosts], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.NumberOfReadPosts], null);
					}
				}
			}

			public int? LastPostLinkOnServer
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.LastPostLinkOnServer]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.LastPostLinkOnServer], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.LastPostLinkOnServer], null);
					}
				}
			}

			public int? OnServerSequenceCounter
			{
			    // ReSharper disable once PossibleInvalidOperationException
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => Api.RetrieveColumnAsInt32(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.OnServerSequenceCounter]);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
				set {
					if (value != null)
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.OnServerSequenceCounter], value.Value);
					} else
					{
						Api.SetColumn(_table.Session, _table, _table.ColumnDictionary[PostsTable.Column.OnServerSequenceCounter], null);
					}
				}
			}

		}

		public DefaultView Columns { get; }

	    public IEnumerable<object> EnumerateToEnd()
	    {
			do {
	            yield return this;
			} while (Api.TryMoveNext(Session, Table));
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
			do {
	            yield return this;
				if (maxCount != null)
				{
					maxCount--;
					if (maxCount <= 0)
					{
						break;
					}
				}
			} while (Api.TryMoveNext(Session, Table));
	    }

	    public IEnumerable<object> EnumerateToEnd(int? maxCount)
	    {
			do {
	            yield return this;
				if (maxCount != null)
				{
					maxCount--;
					if (maxCount <= 0)
					{
						break;
					}
				}
			} while (Api.TryMoveNext(Session, Table));
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySeek(SeekGrbit grbit)
		{
			return Api.TrySeek(Session, Table, grbit);
		}

		public static class ViewValues
		{

			// ReSharper disable once InconsistentNaming
			public struct ChildrenLoadStageView
			{
				public byte ChildrenLoadStage;
			}

			// ReSharper disable once InconsistentNaming
			public struct RetrieveIdFromIndexView
			{
				public int Id;
			}

			// ReSharper disable once InconsistentNaming
			public struct SequenceNumberView
			{
				public int SequenceNumber;
			}

			// ReSharper disable once InconsistentNaming
			public struct LinkInfoView
			{
				public string BoardId;
				public int SequenceNumber;
				public int? ParentSequenceNumber;
			}

			// ReSharper disable once InconsistentNaming
			public struct LastLinkInfoView
			{
				public string BoardId;
				public int SequenceNumber;
				public int? LastPostLinkOnServer;
			}

			// ReSharper disable once InconsistentNaming
			public struct BasicLoadInfoView
			{
				public int Id;
				public byte EntityType;
				public string BoardId;
				public int SequenceNumber;
				public int? ParentSequenceNumber;
				public int? DirectParentId;
				public static implicit operator ViewValues.LinkInfoView(ViewValues.BasicLoadInfoView src)
				{
					return new ViewValues.LinkInfoView()
					{
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
					};
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct BareEntityLoadInfoView
			{
				public int Id;
				public byte EntityType;
				public string BoardId;
				public int SequenceNumber;
				public int? ParentSequenceNumber;
				public int? DirectParentId;
				public string Subject;
				public byte[] Thumbnail;
				public static implicit operator ViewValues.LinkInfoView(ViewValues.BareEntityLoadInfoView src)
				{
					return new ViewValues.LinkInfoView()
					{
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
					};
				}
				public static implicit operator ViewValues.BasicLoadInfoView(ViewValues.BareEntityLoadInfoView src)
				{
					return new ViewValues.BasicLoadInfoView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
					};
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct PostLightLoadView
			{
				public int Id;
				public byte EntityType;
				public string BoardId;
				public int SequenceNumber;
				public int? ParentSequenceNumber;
				public int? DirectParentId;
				public string Subject;
				public byte[] Thumbnail;
				public string BoardSpecificDate;
				public DateTime? Date;
				public GuidColumnValue[] Flags;
				public StringColumnValue[] ThreadTags;
				public int? Likes;
				public int? Dislikes;
				public static implicit operator ViewValues.LinkInfoView(ViewValues.PostLightLoadView src)
				{
					return new ViewValues.LinkInfoView()
					{
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
					};
				}
				public static implicit operator ViewValues.BasicLoadInfoView(ViewValues.PostLightLoadView src)
				{
					return new ViewValues.BasicLoadInfoView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
					};
				}
				public static implicit operator ViewValues.BareEntityLoadInfoView(ViewValues.PostLightLoadView src)
				{
					return new ViewValues.BareEntityLoadInfoView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
						Subject = src.Subject,
						Thumbnail = src.Thumbnail,
					};
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct PostFullLoadView
			{
				public int Id;
				public byte EntityType;
				public string BoardId;
				public int SequenceNumber;
				public int? ParentSequenceNumber;
				public int? DirectParentId;
				public string Subject;
				public byte[] Thumbnail;
				public string BoardSpecificDate;
				public DateTime? Date;
				public GuidColumnValue[] Flags;
				public StringColumnValue[] ThreadTags;
				public int? Likes;
				public int? Dislikes;
				public string PosterName;
				public byte[] OtherDataBinary;
				public byte[] Document;
				public DateTime? LoadedTime;
				public static implicit operator ViewValues.LinkInfoView(ViewValues.PostFullLoadView src)
				{
					return new ViewValues.LinkInfoView()
					{
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
					};
				}
				public static implicit operator ViewValues.BasicLoadInfoView(ViewValues.PostFullLoadView src)
				{
					return new ViewValues.BasicLoadInfoView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
					};
				}
				public static implicit operator ViewValues.BareEntityLoadInfoView(ViewValues.PostFullLoadView src)
				{
					return new ViewValues.BareEntityLoadInfoView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
						Subject = src.Subject,
						Thumbnail = src.Thumbnail,
					};
				}
				public static implicit operator ViewValues.PostLightLoadView(ViewValues.PostFullLoadView src)
				{
					return new ViewValues.PostLightLoadView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
						Subject = src.Subject,
						Thumbnail = src.Thumbnail,
						BoardSpecificDate = src.BoardSpecificDate,
						Date = src.Date,
						Flags = src.Flags,
						ThreadTags = src.ThreadTags,
						Likes = src.Likes,
						Dislikes = src.Dislikes,
					};
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct PostCollectionLoadInfoView
			{
				public int Id;
				public byte EntityType;
				public string BoardId;
				public int SequenceNumber;
				public int? ParentSequenceNumber;
				public int? DirectParentId;
				public string Subject;
				public byte[] Thumbnail;
				public string Etag;
				public byte[] OtherDataBinary;
				public byte ChildrenLoadStage;
				public static implicit operator ViewValues.LinkInfoView(ViewValues.PostCollectionLoadInfoView src)
				{
					return new ViewValues.LinkInfoView()
					{
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
					};
				}
				public static implicit operator ViewValues.BasicLoadInfoView(ViewValues.PostCollectionLoadInfoView src)
				{
					return new ViewValues.BasicLoadInfoView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
					};
				}
				public static implicit operator ViewValues.BareEntityLoadInfoView(ViewValues.PostCollectionLoadInfoView src)
				{
					return new ViewValues.BareEntityLoadInfoView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
						Subject = src.Subject,
						Thumbnail = src.Thumbnail,
					};
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct ThreadPreviewLoadInfoView
			{
				public int Id;
				public byte EntityType;
				public string BoardId;
				public int SequenceNumber;
				public int? ParentSequenceNumber;
				public int? DirectParentId;
				public string Subject;
				public byte[] Thumbnail;
				public string Etag;
				public byte[] OtherDataBinary;
				public byte ChildrenLoadStage;
				public byte[] PreviewCounts;
				public static implicit operator ViewValues.LinkInfoView(ViewValues.ThreadPreviewLoadInfoView src)
				{
					return new ViewValues.LinkInfoView()
					{
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
					};
				}
				public static implicit operator ViewValues.BasicLoadInfoView(ViewValues.ThreadPreviewLoadInfoView src)
				{
					return new ViewValues.BasicLoadInfoView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
					};
				}
				public static implicit operator ViewValues.BareEntityLoadInfoView(ViewValues.ThreadPreviewLoadInfoView src)
				{
					return new ViewValues.BareEntityLoadInfoView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
						Subject = src.Subject,
						Thumbnail = src.Thumbnail,
					};
				}
				public static implicit operator ViewValues.PostCollectionLoadInfoView(ViewValues.ThreadPreviewLoadInfoView src)
				{
					return new ViewValues.PostCollectionLoadInfoView()
					{
						Id = src.Id,
						EntityType = src.EntityType,
						BoardId = src.BoardId,
						SequenceNumber = src.SequenceNumber,
						ParentSequenceNumber = src.ParentSequenceNumber,
						DirectParentId = src.DirectParentId,
						Subject = src.Subject,
						Thumbnail = src.Thumbnail,
						Etag = src.Etag,
						OtherDataBinary = src.OtherDataBinary,
						ChildrenLoadStage = src.ChildrenLoadStage,
					};
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct ParentsAndIdViewForIndex
			{
				public int Id;
				public Int32ColumnValue[] ParentId;
			}

			// ReSharper disable once InconsistentNaming
			public struct NumberOfReadPostsUpdateView
			{
				public int? NumberOfReadPosts;
			}

			// ReSharper disable once InconsistentNaming
			public struct DirectParentAndSequenceNumberView
			{
				public int? DirectParentId;
				public int SequenceNumber;
			}

			// ReSharper disable once InconsistentNaming
			public struct PostDataIdentityUpdateView
			{
				public Int32ColumnValue[] ParentId;
				public int? DirectParentId;
				public byte EntityType;
				public bool DataLoaded;
				public byte ChildrenLoadStage;
				public string BoardId;
				public int? ParentSequenceNumber;
				public int SequenceNumber;
			}

			// ReSharper disable once InconsistentNaming
			public struct PostDataUpdateView
			{
				public string Subject;
				public byte[] Thumbnail;
				public DateTime? Date;
				public string BoardSpecificDate;
				public GuidColumnValue[] Flags;
				public StringColumnValue[] ThreadTags;
				public int? Likes;
				public int? Dislikes;
				public byte[] Document;
				public Int32ColumnValue[] QuotedPosts;
				public DateTime? LoadedTime;
				public string PosterName;
				public int? OnServerSequenceCounter;
				public byte[] OtherDataBinary;
			}

			// ReSharper disable once InconsistentNaming
			public struct PostCollectionUpdateInfoView
			{
				public DateTime? LastServerUpdate;
				public int? NumberOfPostsOnServer;
				public int? LastPostLinkOnServer;
			}

			// ReSharper disable once InconsistentNaming
			public struct LikesUpdateView
			{
				public int? Likes;
				public int? Dislikes;
			}

			// ReSharper disable once InconsistentNaming
			public struct PostDataBareEntityInsertView
			{
				public Int32ColumnValue[] ParentId;
				public int? DirectParentId;
				public byte EntityType;
				public bool DataLoaded;
				public byte ChildrenLoadStage;
				public string BoardId;
				public int? ParentSequenceNumber;
				public int SequenceNumber;
				public string Subject;
				public byte[] Thumbnail;
			}
		}

		public static class FetchViews {

			// ReSharper disable once InconsistentNaming
			public struct RetrieveIdFromIndexView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public RetrieveIdFromIndexView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[1];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Id],
						RetrieveGrbit = RetrieveColumnGrbit.RetrieveFromPrimaryBookmark
					};
				}

				public ViewValues.RetrieveIdFromIndexView Fetch()
				{
					var r = new ViewValues.RetrieveIdFromIndexView();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.Id = ((Int32ColumnValue)_c[0]).Value.Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct SequenceNumberView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public SequenceNumberView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[1];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.SequenceNumberView Fetch()
				{
					var r = new ViewValues.SequenceNumberView();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[0]).Value.Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct LinkInfoView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public LinkInfoView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[3];
					_c[0] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[2] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.LinkInfoView Fetch()
				{
					var r = new ViewValues.LinkInfoView();
					Api.RetrieveColumns(_table.Session, _table, _c);
					r.BoardId = ((StringColumnValue)_c[0]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[1]).Value.Value;
					r.ParentSequenceNumber = ((Int32ColumnValue)_c[2]).Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct LastLinkInfoView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public LastLinkInfoView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[3];
					_c[0] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[2] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.LastPostLinkOnServer],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.LastLinkInfoView Fetch()
				{
					var r = new ViewValues.LastLinkInfoView();
					Api.RetrieveColumns(_table.Session, _table, _c);
					r.BoardId = ((StringColumnValue)_c[0]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[1]).Value.Value;
					r.LastPostLinkOnServer = ((Int32ColumnValue)_c[2]).Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct BasicLoadInfoView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public BasicLoadInfoView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[6];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Id],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.EntityType],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[2] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[3] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[4] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[5] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DirectParentId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.BasicLoadInfoView Fetch()
				{
					var r = new ViewValues.BasicLoadInfoView();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.Id = ((Int32ColumnValue)_c[0]).Value.Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.EntityType = ((ByteColumnValue)_c[1]).Value.Value;
					r.BoardId = ((StringColumnValue)_c[2]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[3]).Value.Value;
					r.ParentSequenceNumber = ((Int32ColumnValue)_c[4]).Value;
					r.DirectParentId = ((Int32ColumnValue)_c[5]).Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct BareEntityLoadInfoView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public BareEntityLoadInfoView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[8];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Id],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.EntityType],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[2] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[3] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[4] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[5] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DirectParentId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[6] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Subject],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[7] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Thumbnail],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.BareEntityLoadInfoView Fetch()
				{
					var r = new ViewValues.BareEntityLoadInfoView();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.Id = ((Int32ColumnValue)_c[0]).Value.Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.EntityType = ((ByteColumnValue)_c[1]).Value.Value;
					r.BoardId = ((StringColumnValue)_c[2]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[3]).Value.Value;
					r.ParentSequenceNumber = ((Int32ColumnValue)_c[4]).Value;
					r.DirectParentId = ((Int32ColumnValue)_c[5]).Value;
					r.Subject = ((StringColumnValue)_c[6]).Value;
					r.Thumbnail = ((BytesColumnValue)_c[7]).Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct PostLightLoadView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public PostLightLoadView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[12];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Id],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.EntityType],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[2] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[3] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[4] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[5] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DirectParentId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[6] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Subject],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[7] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Thumbnail],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[8] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardSpecificDate],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[9] = new DateTimeColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Date],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[10] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Likes],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[11] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Dislikes],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.PostLightLoadView Fetch()
				{
					var r = new ViewValues.PostLightLoadView();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.Id = ((Int32ColumnValue)_c[0]).Value.Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.EntityType = ((ByteColumnValue)_c[1]).Value.Value;
					r.BoardId = ((StringColumnValue)_c[2]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[3]).Value.Value;
					r.ParentSequenceNumber = ((Int32ColumnValue)_c[4]).Value;
					r.DirectParentId = ((Int32ColumnValue)_c[5]).Value;
					r.Subject = ((StringColumnValue)_c[6]).Value;
					r.Thumbnail = ((BytesColumnValue)_c[7]).Value;
					r.BoardSpecificDate = ((StringColumnValue)_c[8]).Value;
					r.Date = ((DateTimeColumnValue)_c[9]).Value;
					r.Likes = ((Int32ColumnValue)_c[10]).Value;
					r.Dislikes = ((Int32ColumnValue)_c[11]).Value;
					r.Flags = _table.Columns.Flags.Values;
					r.ThreadTags = _table.Columns.ThreadTags.Values;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct PostFullLoadView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public PostFullLoadView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[16];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Id],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.EntityType],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[2] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[3] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[4] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[5] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DirectParentId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[6] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Subject],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[7] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Thumbnail],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[8] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardSpecificDate],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[9] = new DateTimeColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Date],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[10] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Likes],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[11] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Dislikes],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[12] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.PosterName],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[13] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.OtherDataBinary],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[14] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Document],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[15] = new DateTimeColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.LoadedTime],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.PostFullLoadView Fetch()
				{
					var r = new ViewValues.PostFullLoadView();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.Id = ((Int32ColumnValue)_c[0]).Value.Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.EntityType = ((ByteColumnValue)_c[1]).Value.Value;
					r.BoardId = ((StringColumnValue)_c[2]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[3]).Value.Value;
					r.ParentSequenceNumber = ((Int32ColumnValue)_c[4]).Value;
					r.DirectParentId = ((Int32ColumnValue)_c[5]).Value;
					r.Subject = ((StringColumnValue)_c[6]).Value;
					r.Thumbnail = ((BytesColumnValue)_c[7]).Value;
					r.BoardSpecificDate = ((StringColumnValue)_c[8]).Value;
					r.Date = ((DateTimeColumnValue)_c[9]).Value;
					r.Likes = ((Int32ColumnValue)_c[10]).Value;
					r.Dislikes = ((Int32ColumnValue)_c[11]).Value;
					r.PosterName = ((StringColumnValue)_c[12]).Value;
					r.OtherDataBinary = ((BytesColumnValue)_c[13]).Value;
					r.Document = ((BytesColumnValue)_c[14]).Value;
					r.LoadedTime = ((DateTimeColumnValue)_c[15]).Value;
					r.Flags = _table.Columns.Flags.Values;
					r.ThreadTags = _table.Columns.ThreadTags.Values;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct PostCollectionLoadInfoView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public PostCollectionLoadInfoView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[11];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Id],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.EntityType],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[2] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[3] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[4] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[5] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DirectParentId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[6] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Subject],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[7] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Thumbnail],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[8] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Etag],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[9] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.OtherDataBinary],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[10] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ChildrenLoadStage],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.PostCollectionLoadInfoView Fetch()
				{
					var r = new ViewValues.PostCollectionLoadInfoView();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.Id = ((Int32ColumnValue)_c[0]).Value.Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.EntityType = ((ByteColumnValue)_c[1]).Value.Value;
					r.BoardId = ((StringColumnValue)_c[2]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[3]).Value.Value;
					r.ParentSequenceNumber = ((Int32ColumnValue)_c[4]).Value;
					r.DirectParentId = ((Int32ColumnValue)_c[5]).Value;
					r.Subject = ((StringColumnValue)_c[6]).Value;
					r.Thumbnail = ((BytesColumnValue)_c[7]).Value;
					r.Etag = ((StringColumnValue)_c[8]).Value;
					r.OtherDataBinary = ((BytesColumnValue)_c[9]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.ChildrenLoadStage = ((ByteColumnValue)_c[10]).Value.Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct ThreadPreviewLoadInfoView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public ThreadPreviewLoadInfoView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[12];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Id],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.EntityType],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[2] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[3] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[4] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[5] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DirectParentId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[6] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Subject],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[7] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Thumbnail],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[8] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Etag],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[9] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.OtherDataBinary],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[10] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ChildrenLoadStage],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[11] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.PreviewCounts],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.ThreadPreviewLoadInfoView Fetch()
				{
					var r = new ViewValues.ThreadPreviewLoadInfoView();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.Id = ((Int32ColumnValue)_c[0]).Value.Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.EntityType = ((ByteColumnValue)_c[1]).Value.Value;
					r.BoardId = ((StringColumnValue)_c[2]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[3]).Value.Value;
					r.ParentSequenceNumber = ((Int32ColumnValue)_c[4]).Value;
					r.DirectParentId = ((Int32ColumnValue)_c[5]).Value;
					r.Subject = ((StringColumnValue)_c[6]).Value;
					r.Thumbnail = ((BytesColumnValue)_c[7]).Value;
					r.Etag = ((StringColumnValue)_c[8]).Value;
					r.OtherDataBinary = ((BytesColumnValue)_c[9]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.ChildrenLoadStage = ((ByteColumnValue)_c[10]).Value.Value;
					r.PreviewCounts = ((BytesColumnValue)_c[11]).Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct ParentsAndIdViewForIndex
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public ParentsAndIdViewForIndex(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[1];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Id],
						RetrieveGrbit = RetrieveColumnGrbit.RetrieveFromPrimaryBookmark
					};
				}

				public ViewValues.ParentsAndIdViewForIndex Fetch()
				{
					var r = new ViewValues.ParentsAndIdViewForIndex();
					Api.RetrieveColumns(_table.Session, _table, _c);
				    // ReSharper disable once PossibleInvalidOperationException
					r.Id = ((Int32ColumnValue)_c[0]).Value.Value;
					r.ParentId = _table.Columns.ParentId.Values;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct NumberOfReadPostsUpdateView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public NumberOfReadPostsUpdateView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[1];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.NumberOfReadPosts],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.NumberOfReadPostsUpdateView Fetch()
				{
					var r = new ViewValues.NumberOfReadPostsUpdateView();
					Api.RetrieveColumns(_table.Session, _table, _c);
					r.NumberOfReadPosts = ((Int32ColumnValue)_c[0]).Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct DirectParentAndSequenceNumberView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public DirectParentAndSequenceNumberView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[2];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DirectParentId],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.DirectParentAndSequenceNumberView Fetch()
				{
					var r = new ViewValues.DirectParentAndSequenceNumberView();
					Api.RetrieveColumns(_table.Session, _table, _c);
					r.DirectParentId = ((Int32ColumnValue)_c[0]).Value;
				    // ReSharper disable once PossibleInvalidOperationException
					r.SequenceNumber = ((Int32ColumnValue)_c[1]).Value.Value;
					return r;
				}
			}

			// ReSharper disable once InconsistentNaming
			public struct LikesUpdateView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public LikesUpdateView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[2];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Likes],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
					_c[1] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Dislikes],
						RetrieveGrbit = RetrieveColumnGrbit.None
					};
				}

				public ViewValues.LikesUpdateView Fetch()
				{
					var r = new ViewValues.LikesUpdateView();
					Api.RetrieveColumns(_table.Session, _table, _c);
					r.Likes = ((Int32ColumnValue)_c[0]).Value;
					r.Dislikes = ((Int32ColumnValue)_c[1]).Value;
					return r;
				}
			}
	
		}

		public class TableFetchViews
		{
			private readonly PostsTable _table;

			public TableFetchViews(PostsTable table)
			{
				_table = table;
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.SequenceNumberView? __fv_SequenceNumberView;
			public FetchViews.SequenceNumberView SequenceNumberView
			{
				get
				{
					if (__fv_SequenceNumberView == null)
					{
						__fv_SequenceNumberView = new FetchViews.SequenceNumberView(_table);
					}
					return __fv_SequenceNumberView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.LinkInfoView? __fv_LinkInfoView;
			public FetchViews.LinkInfoView LinkInfoView
			{
				get
				{
					if (__fv_LinkInfoView == null)
					{
						__fv_LinkInfoView = new FetchViews.LinkInfoView(_table);
					}
					return __fv_LinkInfoView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.LastLinkInfoView? __fv_LastLinkInfoView;
			public FetchViews.LastLinkInfoView LastLinkInfoView
			{
				get
				{
					if (__fv_LastLinkInfoView == null)
					{
						__fv_LastLinkInfoView = new FetchViews.LastLinkInfoView(_table);
					}
					return __fv_LastLinkInfoView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.BasicLoadInfoView? __fv_BasicLoadInfoView;
			public FetchViews.BasicLoadInfoView BasicLoadInfoView
			{
				get
				{
					if (__fv_BasicLoadInfoView == null)
					{
						__fv_BasicLoadInfoView = new FetchViews.BasicLoadInfoView(_table);
					}
					return __fv_BasicLoadInfoView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.BareEntityLoadInfoView? __fv_BareEntityLoadInfoView;
			public FetchViews.BareEntityLoadInfoView BareEntityLoadInfoView
			{
				get
				{
					if (__fv_BareEntityLoadInfoView == null)
					{
						__fv_BareEntityLoadInfoView = new FetchViews.BareEntityLoadInfoView(_table);
					}
					return __fv_BareEntityLoadInfoView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.PostLightLoadView? __fv_PostLightLoadView;
			public FetchViews.PostLightLoadView PostLightLoadView
			{
				get
				{
					if (__fv_PostLightLoadView == null)
					{
						__fv_PostLightLoadView = new FetchViews.PostLightLoadView(_table);
					}
					return __fv_PostLightLoadView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.PostFullLoadView? __fv_PostFullLoadView;
			public FetchViews.PostFullLoadView PostFullLoadView
			{
				get
				{
					if (__fv_PostFullLoadView == null)
					{
						__fv_PostFullLoadView = new FetchViews.PostFullLoadView(_table);
					}
					return __fv_PostFullLoadView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.PostCollectionLoadInfoView? __fv_PostCollectionLoadInfoView;
			public FetchViews.PostCollectionLoadInfoView PostCollectionLoadInfoView
			{
				get
				{
					if (__fv_PostCollectionLoadInfoView == null)
					{
						__fv_PostCollectionLoadInfoView = new FetchViews.PostCollectionLoadInfoView(_table);
					}
					return __fv_PostCollectionLoadInfoView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.ThreadPreviewLoadInfoView? __fv_ThreadPreviewLoadInfoView;
			public FetchViews.ThreadPreviewLoadInfoView ThreadPreviewLoadInfoView
			{
				get
				{
					if (__fv_ThreadPreviewLoadInfoView == null)
					{
						__fv_ThreadPreviewLoadInfoView = new FetchViews.ThreadPreviewLoadInfoView(_table);
					}
					return __fv_ThreadPreviewLoadInfoView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.NumberOfReadPostsUpdateView? __fv_NumberOfReadPostsUpdateView;
			public FetchViews.NumberOfReadPostsUpdateView NumberOfReadPostsUpdateView
			{
				get
				{
					if (__fv_NumberOfReadPostsUpdateView == null)
					{
						__fv_NumberOfReadPostsUpdateView = new FetchViews.NumberOfReadPostsUpdateView(_table);
					}
					return __fv_NumberOfReadPostsUpdateView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.DirectParentAndSequenceNumberView? __fv_DirectParentAndSequenceNumberView;
			public FetchViews.DirectParentAndSequenceNumberView DirectParentAndSequenceNumberView
			{
				get
				{
					if (__fv_DirectParentAndSequenceNumberView == null)
					{
						__fv_DirectParentAndSequenceNumberView = new FetchViews.DirectParentAndSequenceNumberView(_table);
					}
					return __fv_DirectParentAndSequenceNumberView.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private FetchViews.LikesUpdateView? __fv_LikesUpdateView;
			public FetchViews.LikesUpdateView LikesUpdateView
			{
				get
				{
					if (__fv_LikesUpdateView == null)
					{
						__fv_LikesUpdateView = new FetchViews.LikesUpdateView(_table);
					}
					return __fv_LikesUpdateView.Value;
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
			public struct ChildrenLoadStageView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public ChildrenLoadStageView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[1];
					_c[0] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ChildrenLoadStage],
						SetGrbit = SetColumnGrbit.None
					};
				}

				public void Set(ViewValues.ChildrenLoadStageView value, bool isInsert = false)
				{
					((ByteColumnValue)_c[0]).Value = value.ChildrenLoadStage;
					Api.SetColumns(_table.Session, _table, _c);
				}

				public void Set(ref ViewValues.ChildrenLoadStageView value, bool isInsert = false)
				{
					((ByteColumnValue)_c[0]).Value = value.ChildrenLoadStage;
					Api.SetColumns(_table.Session, _table, _c);
				}
			}			
			public struct NumberOfReadPostsUpdateView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public NumberOfReadPostsUpdateView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[1];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.NumberOfReadPosts],
						SetGrbit = SetColumnGrbit.None
					};
				}

				public void Set(ViewValues.NumberOfReadPostsUpdateView value, bool isInsert = false)
				{
					((Int32ColumnValue)_c[0]).Value = value.NumberOfReadPosts;
					Api.SetColumns(_table.Session, _table, _c);
				}

				public void Set(ref ViewValues.NumberOfReadPostsUpdateView value, bool isInsert = false)
				{
					((Int32ColumnValue)_c[0]).Value = value.NumberOfReadPosts;
					Api.SetColumns(_table.Session, _table, _c);
				}
			}			
			public struct PostDataIdentityUpdateView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public PostDataIdentityUpdateView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[7];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DirectParentId],
						SetGrbit = SetColumnGrbit.None
					};
					_c[1] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.EntityType],
						SetGrbit = SetColumnGrbit.None
					};
					_c[2] = new BoolColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DataLoaded],
						SetGrbit = SetColumnGrbit.None
					};
					_c[3] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ChildrenLoadStage],
						SetGrbit = SetColumnGrbit.None
					};
					_c[4] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardId],
						SetGrbit = SetColumnGrbit.None
					};
					_c[5] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber],
						SetGrbit = SetColumnGrbit.None
					};
					_c[6] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						SetGrbit = SetColumnGrbit.None
					};
				}

				public void Set(ViewValues.PostDataIdentityUpdateView value, bool isInsert = false)
				{
					((Int32ColumnValue)_c[0]).Value = value.DirectParentId;
					((ByteColumnValue)_c[1]).Value = value.EntityType;
					((BoolColumnValue)_c[2]).Value = value.DataLoaded;
					((ByteColumnValue)_c[3]).Value = value.ChildrenLoadStage;
					((StringColumnValue)_c[4]).Value = value.BoardId;
					((Int32ColumnValue)_c[5]).Value = value.ParentSequenceNumber;
					((Int32ColumnValue)_c[6]).Value = value.SequenceNumber;
					Api.SetColumns(_table.Session, _table, _c);
					_table.Columns.SetParentIdValueArr(value.ParentId, isInsert);
				}

				public void Set(ref ViewValues.PostDataIdentityUpdateView value, bool isInsert = false)
				{
					((Int32ColumnValue)_c[0]).Value = value.DirectParentId;
					((ByteColumnValue)_c[1]).Value = value.EntityType;
					((BoolColumnValue)_c[2]).Value = value.DataLoaded;
					((ByteColumnValue)_c[3]).Value = value.ChildrenLoadStage;
					((StringColumnValue)_c[4]).Value = value.BoardId;
					((Int32ColumnValue)_c[5]).Value = value.ParentSequenceNumber;
					((Int32ColumnValue)_c[6]).Value = value.SequenceNumber;
					Api.SetColumns(_table.Session, _table, _c);
					_table.Columns.SetParentIdValueArr(value.ParentId, isInsert);
				}
			}			
			public struct PostDataUpdateView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public PostDataUpdateView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[11];
					_c[0] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Subject],
						SetGrbit = SetColumnGrbit.None
					};
					_c[1] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Thumbnail],
						SetGrbit = SetColumnGrbit.None
					};
					_c[2] = new DateTimeColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Date],
						SetGrbit = SetColumnGrbit.None
					};
					_c[3] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardSpecificDate],
						SetGrbit = SetColumnGrbit.None
					};
					_c[4] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Likes],
						SetGrbit = SetColumnGrbit.None
					};
					_c[5] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Dislikes],
						SetGrbit = SetColumnGrbit.None
					};
					_c[6] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Document],
						SetGrbit = SetColumnGrbit.None
					};
					_c[7] = new DateTimeColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.LoadedTime],
						SetGrbit = SetColumnGrbit.None
					};
					_c[8] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.PosterName],
						SetGrbit = SetColumnGrbit.None
					};
					_c[9] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.OnServerSequenceCounter],
						SetGrbit = SetColumnGrbit.None
					};
					_c[10] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.OtherDataBinary],
						SetGrbit = SetColumnGrbit.None
					};
				}

				public void Set(ViewValues.PostDataUpdateView value, bool isInsert = false)
				{
					((StringColumnValue)_c[0]).Value = value.Subject;
					((BytesColumnValue)_c[1]).Value = value.Thumbnail;
					((DateTimeColumnValue)_c[2]).Value = value.Date;
					((StringColumnValue)_c[3]).Value = value.BoardSpecificDate;
					((Int32ColumnValue)_c[4]).Value = value.Likes;
					((Int32ColumnValue)_c[5]).Value = value.Dislikes;
					((BytesColumnValue)_c[6]).Value = value.Document;
					((DateTimeColumnValue)_c[7]).Value = value.LoadedTime;
					((StringColumnValue)_c[8]).Value = value.PosterName;
					((Int32ColumnValue)_c[9]).Value = value.OnServerSequenceCounter;
					((BytesColumnValue)_c[10]).Value = value.OtherDataBinary;
					Api.SetColumns(_table.Session, _table, _c);
					_table.Columns.SetFlagsValueArr(value.Flags, isInsert);
					_table.Columns.SetThreadTagsValueArr(value.ThreadTags, isInsert);
					_table.Columns.SetQuotedPostsValueArr(value.QuotedPosts, isInsert);
				}

				public void Set(ref ViewValues.PostDataUpdateView value, bool isInsert = false)
				{
					((StringColumnValue)_c[0]).Value = value.Subject;
					((BytesColumnValue)_c[1]).Value = value.Thumbnail;
					((DateTimeColumnValue)_c[2]).Value = value.Date;
					((StringColumnValue)_c[3]).Value = value.BoardSpecificDate;
					((Int32ColumnValue)_c[4]).Value = value.Likes;
					((Int32ColumnValue)_c[5]).Value = value.Dislikes;
					((BytesColumnValue)_c[6]).Value = value.Document;
					((DateTimeColumnValue)_c[7]).Value = value.LoadedTime;
					((StringColumnValue)_c[8]).Value = value.PosterName;
					((Int32ColumnValue)_c[9]).Value = value.OnServerSequenceCounter;
					((BytesColumnValue)_c[10]).Value = value.OtherDataBinary;
					Api.SetColumns(_table.Session, _table, _c);
					_table.Columns.SetFlagsValueArr(value.Flags, isInsert);
					_table.Columns.SetThreadTagsValueArr(value.ThreadTags, isInsert);
					_table.Columns.SetQuotedPostsValueArr(value.QuotedPosts, isInsert);
				}
			}			
			public struct PostCollectionUpdateInfoView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public PostCollectionUpdateInfoView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[3];
					_c[0] = new DateTimeColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.LastServerUpdate],
						SetGrbit = SetColumnGrbit.None
					};
					_c[1] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.NumberOfPostsOnServer],
						SetGrbit = SetColumnGrbit.None
					};
					_c[2] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.LastPostLinkOnServer],
						SetGrbit = SetColumnGrbit.None
					};
				}

				public void Set(ViewValues.PostCollectionUpdateInfoView value, bool isInsert = false)
				{
					((DateTimeColumnValue)_c[0]).Value = value.LastServerUpdate;
					((Int32ColumnValue)_c[1]).Value = value.NumberOfPostsOnServer;
					((Int32ColumnValue)_c[2]).Value = value.LastPostLinkOnServer;
					Api.SetColumns(_table.Session, _table, _c);
				}

				public void Set(ref ViewValues.PostCollectionUpdateInfoView value, bool isInsert = false)
				{
					((DateTimeColumnValue)_c[0]).Value = value.LastServerUpdate;
					((Int32ColumnValue)_c[1]).Value = value.NumberOfPostsOnServer;
					((Int32ColumnValue)_c[2]).Value = value.LastPostLinkOnServer;
					Api.SetColumns(_table.Session, _table, _c);
				}
			}			
			public struct LikesUpdateView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public LikesUpdateView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[2];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Likes],
						SetGrbit = SetColumnGrbit.None
					};
					_c[1] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Dislikes],
						SetGrbit = SetColumnGrbit.None
					};
				}

				public void Set(ViewValues.LikesUpdateView value, bool isInsert = false)
				{
					((Int32ColumnValue)_c[0]).Value = value.Likes;
					((Int32ColumnValue)_c[1]).Value = value.Dislikes;
					Api.SetColumns(_table.Session, _table, _c);
				}

				public void Set(ref ViewValues.LikesUpdateView value, bool isInsert = false)
				{
					((Int32ColumnValue)_c[0]).Value = value.Likes;
					((Int32ColumnValue)_c[1]).Value = value.Dislikes;
					Api.SetColumns(_table.Session, _table, _c);
				}
			}			
			public struct PostDataBareEntityInsertView
			{
				private readonly PostsTable _table;
				private readonly ColumnValue[] _c;

				public PostDataBareEntityInsertView(PostsTable table)
				{
					_table = table;

					_c = new ColumnValue[9];
					_c[0] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DirectParentId],
						SetGrbit = SetColumnGrbit.None
					};
					_c[1] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.EntityType],
						SetGrbit = SetColumnGrbit.None
					};
					_c[2] = new BoolColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.DataLoaded],
						SetGrbit = SetColumnGrbit.None
					};
					_c[3] = new ByteColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ChildrenLoadStage],
						SetGrbit = SetColumnGrbit.None
					};
					_c[4] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.BoardId],
						SetGrbit = SetColumnGrbit.None
					};
					_c[5] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.ParentSequenceNumber],
						SetGrbit = SetColumnGrbit.None
					};
					_c[6] = new Int32ColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.SequenceNumber],
						SetGrbit = SetColumnGrbit.None
					};
					_c[7] = new StringColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Subject],
						SetGrbit = SetColumnGrbit.None
					};
					_c[8] = new BytesColumnValue() {
						Columnid = _table.ColumnDictionary[PostsTable.Column.Thumbnail],
						SetGrbit = SetColumnGrbit.None
					};
				}

				public void Set(ViewValues.PostDataBareEntityInsertView value, bool isInsert = false)
				{
					((Int32ColumnValue)_c[0]).Value = value.DirectParentId;
					((ByteColumnValue)_c[1]).Value = value.EntityType;
					((BoolColumnValue)_c[2]).Value = value.DataLoaded;
					((ByteColumnValue)_c[3]).Value = value.ChildrenLoadStage;
					((StringColumnValue)_c[4]).Value = value.BoardId;
					((Int32ColumnValue)_c[5]).Value = value.ParentSequenceNumber;
					((Int32ColumnValue)_c[6]).Value = value.SequenceNumber;
					((StringColumnValue)_c[7]).Value = value.Subject;
					((BytesColumnValue)_c[8]).Value = value.Thumbnail;
					Api.SetColumns(_table.Session, _table, _c);
					_table.Columns.SetParentIdValueArr(value.ParentId, isInsert);
				}

				public void Set(ref ViewValues.PostDataBareEntityInsertView value, bool isInsert = false)
				{
					((Int32ColumnValue)_c[0]).Value = value.DirectParentId;
					((ByteColumnValue)_c[1]).Value = value.EntityType;
					((BoolColumnValue)_c[2]).Value = value.DataLoaded;
					((ByteColumnValue)_c[3]).Value = value.ChildrenLoadStage;
					((StringColumnValue)_c[4]).Value = value.BoardId;
					((Int32ColumnValue)_c[5]).Value = value.ParentSequenceNumber;
					((Int32ColumnValue)_c[6]).Value = value.SequenceNumber;
					((StringColumnValue)_c[7]).Value = value.Subject;
					((BytesColumnValue)_c[8]).Value = value.Thumbnail;
					Api.SetColumns(_table.Session, _table, _c);
					_table.Columns.SetParentIdValueArr(value.ParentId, isInsert);
				}
			}			
		}

		public class TableInsertViews
		{
			private readonly PostsTable _table;

			public TableInsertViews(PostsTable table)
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
			private InsertOrUpdateViews.PostDataIdentityUpdateView? __iuv_PostDataIdentityUpdateView;

			public InsertOrUpdateViews.PostDataIdentityUpdateView PostDataIdentityUpdateView
			{
				get
				{
					if (__iuv_PostDataIdentityUpdateView == null)
					{
						__iuv_PostDataIdentityUpdateView = new InsertOrUpdateViews.PostDataIdentityUpdateView(_table);
					}
					return __iuv_PostDataIdentityUpdateView.Value;
				}
			}

			public void InsertAsPostDataIdentityUpdateView(ViewValues.PostDataIdentityUpdateView value)
			{
				using (var update = CreateUpdate())
				{
					PostDataIdentityUpdateView.Set(value, true);
					update.Save();
				}
			}

			public void InsertAsPostDataIdentityUpdateView(ref ViewValues.PostDataIdentityUpdateView value)
			{
				using (var update = CreateUpdate())
				{
					PostDataIdentityUpdateView.Set(ref value, true);
					update.Save();
				}
			}

			public void InsertAsPostDataIdentityUpdateView(ViewValues.PostDataIdentityUpdateView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					PostDataIdentityUpdateView.Set(value, true);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void InsertAsPostDataIdentityUpdateView(ref ViewValues.PostDataIdentityUpdateView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					PostDataIdentityUpdateView.Set(ref value, true);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

		    // ReSharper disable once InconsistentNaming
			private InsertOrUpdateViews.PostDataUpdateView? __iuv_PostDataUpdateView;

			public InsertOrUpdateViews.PostDataUpdateView PostDataUpdateView
			{
				get
				{
					if (__iuv_PostDataUpdateView == null)
					{
						__iuv_PostDataUpdateView = new InsertOrUpdateViews.PostDataUpdateView(_table);
					}
					return __iuv_PostDataUpdateView.Value;
				}
			}

			public void InsertAsPostDataUpdateView(ViewValues.PostDataUpdateView value)
			{
				using (var update = CreateUpdate())
				{
					PostDataUpdateView.Set(value, true);
					update.Save();
				}
			}

			public void InsertAsPostDataUpdateView(ref ViewValues.PostDataUpdateView value)
			{
				using (var update = CreateUpdate())
				{
					PostDataUpdateView.Set(ref value, true);
					update.Save();
				}
			}

			public void InsertAsPostDataUpdateView(ViewValues.PostDataUpdateView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					PostDataUpdateView.Set(value, true);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void InsertAsPostDataUpdateView(ref ViewValues.PostDataUpdateView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					PostDataUpdateView.Set(ref value, true);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

		    // ReSharper disable once InconsistentNaming
			private InsertOrUpdateViews.PostDataBareEntityInsertView? __iuv_PostDataBareEntityInsertView;

			public InsertOrUpdateViews.PostDataBareEntityInsertView PostDataBareEntityInsertView
			{
				get
				{
					if (__iuv_PostDataBareEntityInsertView == null)
					{
						__iuv_PostDataBareEntityInsertView = new InsertOrUpdateViews.PostDataBareEntityInsertView(_table);
					}
					return __iuv_PostDataBareEntityInsertView.Value;
				}
			}

			public void InsertAsPostDataBareEntityInsertView(ViewValues.PostDataBareEntityInsertView value)
			{
				using (var update = CreateUpdate())
				{
					PostDataBareEntityInsertView.Set(value, true);
					update.Save();
				}
			}

			public void InsertAsPostDataBareEntityInsertView(ref ViewValues.PostDataBareEntityInsertView value)
			{
				using (var update = CreateUpdate())
				{
					PostDataBareEntityInsertView.Set(ref value, true);
					update.Save();
				}
			}

			public void InsertAsPostDataBareEntityInsertView(ViewValues.PostDataBareEntityInsertView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					PostDataBareEntityInsertView.Set(value, true);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void InsertAsPostDataBareEntityInsertView(ref ViewValues.PostDataBareEntityInsertView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					PostDataBareEntityInsertView.Set(ref value, true);
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
			private readonly PostsTable _table;

			public TableUpdateViews(PostsTable table)
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
			private InsertOrUpdateViews.ChildrenLoadStageView? __iuv_ChildrenLoadStageView;

			public InsertOrUpdateViews.ChildrenLoadStageView ChildrenLoadStageView
			{
				get
				{
					if (__iuv_ChildrenLoadStageView == null)
					{
						__iuv_ChildrenLoadStageView = new InsertOrUpdateViews.ChildrenLoadStageView(_table);
					}
					return __iuv_ChildrenLoadStageView.Value;
				}
			}

			public void UpdateAsChildrenLoadStageView(ViewValues.ChildrenLoadStageView value)
			{
				using (var update = CreateUpdate())
				{
					ChildrenLoadStageView.Set(value);
					update.Save();
				}
			}

			public void UpdateAsChildrenLoadStageView(ref ViewValues.ChildrenLoadStageView value)
			{
				using (var update = CreateUpdate())
				{
					ChildrenLoadStageView.Set(ref value);
					update.Save();
				}
			}

			public void UpdateAsChildrenLoadStageView(ViewValues.ChildrenLoadStageView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					ChildrenLoadStageView.Set(value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void UpdateAsChildrenLoadStageView(ref ViewValues.ChildrenLoadStageView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					ChildrenLoadStageView.Set(ref value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

		    // ReSharper disable once InconsistentNaming
			private InsertOrUpdateViews.NumberOfReadPostsUpdateView? __iuv_NumberOfReadPostsUpdateView;

			public InsertOrUpdateViews.NumberOfReadPostsUpdateView NumberOfReadPostsUpdateView
			{
				get
				{
					if (__iuv_NumberOfReadPostsUpdateView == null)
					{
						__iuv_NumberOfReadPostsUpdateView = new InsertOrUpdateViews.NumberOfReadPostsUpdateView(_table);
					}
					return __iuv_NumberOfReadPostsUpdateView.Value;
				}
			}

			public void UpdateAsNumberOfReadPostsUpdateView(ViewValues.NumberOfReadPostsUpdateView value)
			{
				using (var update = CreateUpdate())
				{
					NumberOfReadPostsUpdateView.Set(value);
					update.Save();
				}
			}

			public void UpdateAsNumberOfReadPostsUpdateView(ref ViewValues.NumberOfReadPostsUpdateView value)
			{
				using (var update = CreateUpdate())
				{
					NumberOfReadPostsUpdateView.Set(ref value);
					update.Save();
				}
			}

			public void UpdateAsNumberOfReadPostsUpdateView(ViewValues.NumberOfReadPostsUpdateView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					NumberOfReadPostsUpdateView.Set(value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void UpdateAsNumberOfReadPostsUpdateView(ref ViewValues.NumberOfReadPostsUpdateView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					NumberOfReadPostsUpdateView.Set(ref value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

		    // ReSharper disable once InconsistentNaming
			private InsertOrUpdateViews.PostDataUpdateView? __iuv_PostDataUpdateView;

			public InsertOrUpdateViews.PostDataUpdateView PostDataUpdateView
			{
				get
				{
					if (__iuv_PostDataUpdateView == null)
					{
						__iuv_PostDataUpdateView = new InsertOrUpdateViews.PostDataUpdateView(_table);
					}
					return __iuv_PostDataUpdateView.Value;
				}
			}

			public void UpdateAsPostDataUpdateView(ViewValues.PostDataUpdateView value)
			{
				using (var update = CreateUpdate())
				{
					PostDataUpdateView.Set(value);
					update.Save();
				}
			}

			public void UpdateAsPostDataUpdateView(ref ViewValues.PostDataUpdateView value)
			{
				using (var update = CreateUpdate())
				{
					PostDataUpdateView.Set(ref value);
					update.Save();
				}
			}

			public void UpdateAsPostDataUpdateView(ViewValues.PostDataUpdateView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					PostDataUpdateView.Set(value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void UpdateAsPostDataUpdateView(ref ViewValues.PostDataUpdateView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					PostDataUpdateView.Set(ref value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

		    // ReSharper disable once InconsistentNaming
			private InsertOrUpdateViews.PostCollectionUpdateInfoView? __iuv_PostCollectionUpdateInfoView;

			public InsertOrUpdateViews.PostCollectionUpdateInfoView PostCollectionUpdateInfoView
			{
				get
				{
					if (__iuv_PostCollectionUpdateInfoView == null)
					{
						__iuv_PostCollectionUpdateInfoView = new InsertOrUpdateViews.PostCollectionUpdateInfoView(_table);
					}
					return __iuv_PostCollectionUpdateInfoView.Value;
				}
			}

			public void UpdateAsPostCollectionUpdateInfoView(ViewValues.PostCollectionUpdateInfoView value)
			{
				using (var update = CreateUpdate())
				{
					PostCollectionUpdateInfoView.Set(value);
					update.Save();
				}
			}

			public void UpdateAsPostCollectionUpdateInfoView(ref ViewValues.PostCollectionUpdateInfoView value)
			{
				using (var update = CreateUpdate())
				{
					PostCollectionUpdateInfoView.Set(ref value);
					update.Save();
				}
			}

			public void UpdateAsPostCollectionUpdateInfoView(ViewValues.PostCollectionUpdateInfoView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					PostCollectionUpdateInfoView.Set(value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void UpdateAsPostCollectionUpdateInfoView(ref ViewValues.PostCollectionUpdateInfoView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					PostCollectionUpdateInfoView.Set(ref value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

		    // ReSharper disable once InconsistentNaming
			private InsertOrUpdateViews.LikesUpdateView? __iuv_LikesUpdateView;

			public InsertOrUpdateViews.LikesUpdateView LikesUpdateView
			{
				get
				{
					if (__iuv_LikesUpdateView == null)
					{
						__iuv_LikesUpdateView = new InsertOrUpdateViews.LikesUpdateView(_table);
					}
					return __iuv_LikesUpdateView.Value;
				}
			}

			public void UpdateAsLikesUpdateView(ViewValues.LikesUpdateView value)
			{
				using (var update = CreateUpdate())
				{
					LikesUpdateView.Set(value);
					update.Save();
				}
			}

			public void UpdateAsLikesUpdateView(ref ViewValues.LikesUpdateView value)
			{
				using (var update = CreateUpdate())
				{
					LikesUpdateView.Set(ref value);
					update.Save();
				}
			}

			public void UpdateAsLikesUpdateView(ViewValues.LikesUpdateView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					LikesUpdateView.Set(value);
					SaveUpdateWithBookmark(update, out bookmark);
				}
			}

			public void UpdateAsLikesUpdateView(ref ViewValues.LikesUpdateView value, out byte[] bookmark)
			{
				using (var update = CreateUpdate())
				{
					LikesUpdateView.Set(ref value);
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
				private readonly PostsTable _table;

				public PrimaryIndex(PostsTable table)
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

			public struct ParentIdIndex
			{
				private readonly PostsTable _table;

				public ParentIdIndex(PostsTable table)
				{
					_table = table;
					_views = new IndexFetchViews(_table);
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "ParentIdIndex");
				}

				public struct ParentIdIndexKey
				{
					public int? ParentId;
				}

			    // ReSharper disable InconsistentNaming
				public ParentIdIndexKey CreateKey(
						int? ParentId
				)
			    // ReSharper enable InconsistentNaming
				{
					return new ParentIdIndexKey() {
						ParentId = ParentId,
					
					};
				}

				public void SetKey(ParentIdIndexKey key)
				{
					if (key.ParentId == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.ParentId.Value, MakeKeyGrbit.NewKey);
					}
				}

				public bool Find(ParentIdIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(ParentIdIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(ParentIdIndexKey key)
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

			    public int GetIndexRecordCount(ParentIdIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }
				public class IndexFetchViews
				{
					private readonly PostsTable _table;

					public IndexFetchViews(PostsTable table)
					{
						_table = table;
					}

					// ReSharper disable once InconsistentNaming
					private FetchViews.RetrieveIdFromIndexView? __fv_RetrieveIdFromIndexView;
					public FetchViews.RetrieveIdFromIndexView RetrieveIdFromIndexView
					{
						get
						{
							if (__fv_RetrieveIdFromIndexView == null)
							{
								__fv_RetrieveIdFromIndexView = new FetchViews.RetrieveIdFromIndexView(_table);
							}
							return __fv_RetrieveIdFromIndexView.Value;
						}
					}
				}

				private readonly IndexFetchViews _views;
			    // ReSharper disable once ConvertToAutoProperty
				public IndexFetchViews Views => _views;

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView(ParentIdIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateUniqueAsRetrieveIdFromIndexView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateUniqueAsRetrieveIdFromIndexView(ParentIdIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}
				
			}

			public struct TypeIndex
			{
				private readonly PostsTable _table;

				public TypeIndex(PostsTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "TypeIndex");
				}

				public struct TypeIndexKey
				{
					public byte EntityType;
				}

			    // ReSharper disable InconsistentNaming
				public TypeIndexKey CreateKey(
						byte EntityType
				)
			    // ReSharper enable InconsistentNaming
				{
					return new TypeIndexKey() {
						EntityType = EntityType,
					
					};
				}

				public void SetKey(TypeIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.EntityType,  MakeKeyGrbit.NewKey);
				}

				public bool Find(TypeIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(TypeIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(TypeIndexKey key)
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

			    public int GetIndexRecordCount(TypeIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }
				
			}

			public struct ChildrenLoadStageIndex
			{
				private readonly PostsTable _table;

				public ChildrenLoadStageIndex(PostsTable table)
				{
					_table = table;
					_views = new IndexFetchViews(_table);
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "ChildrenLoadStageIndex");
				}

				public struct ChildrenLoadStageIndexKey
				{
					public byte ChildrenLoadStage;
				}

			    // ReSharper disable InconsistentNaming
				public ChildrenLoadStageIndexKey CreateKey(
						byte ChildrenLoadStage
				)
			    // ReSharper enable InconsistentNaming
				{
					return new ChildrenLoadStageIndexKey() {
						ChildrenLoadStage = ChildrenLoadStage,
					
					};
				}

				public void SetKey(ChildrenLoadStageIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.ChildrenLoadStage,  MakeKeyGrbit.NewKey);
				}

				public bool Find(ChildrenLoadStageIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(ChildrenLoadStageIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(ChildrenLoadStageIndexKey key)
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

			    public int GetIndexRecordCount(ChildrenLoadStageIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }
				public class IndexFetchViews
				{
					private readonly PostsTable _table;

					public IndexFetchViews(PostsTable table)
					{
						_table = table;
					}

					// ReSharper disable once InconsistentNaming
					private FetchViews.RetrieveIdFromIndexView? __fv_RetrieveIdFromIndexView;
					public FetchViews.RetrieveIdFromIndexView RetrieveIdFromIndexView
					{
						get
						{
							if (__fv_RetrieveIdFromIndexView == null)
							{
								__fv_RetrieveIdFromIndexView = new FetchViews.RetrieveIdFromIndexView(_table);
							}
							return __fv_RetrieveIdFromIndexView.Value;
						}
					}
				}

				private readonly IndexFetchViews _views;
			    // ReSharper disable once ConvertToAutoProperty
				public IndexFetchViews Views => _views;

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView(ChildrenLoadStageIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateUniqueAsRetrieveIdFromIndexView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateUniqueAsRetrieveIdFromIndexView(ChildrenLoadStageIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}
				
			}

			public struct TypeAndPostIdIndex
			{
				private readonly PostsTable _table;

				public TypeAndPostIdIndex(PostsTable table)
				{
					_table = table;
					_views = new IndexFetchViews(_table);
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "TypeAndPostIdIndex");
				}

				public struct TypeAndPostIdIndexKey
				{
					public byte EntityType;
					public string BoardId;
					public int SequenceNumber;
				}

			    // ReSharper disable InconsistentNaming
				public TypeAndPostIdIndexKey CreateKey(
						byte EntityType
						,string BoardId
						,int SequenceNumber
				)
			    // ReSharper enable InconsistentNaming
				{
					return new TypeAndPostIdIndexKey() {
						EntityType = EntityType,
						BoardId = BoardId,
						SequenceNumber = SequenceNumber,
					
					};
				}

				public void SetKey(TypeAndPostIdIndexKey key)
				{
					Api.MakeKey(_table.Session, _table, key.EntityType,  MakeKeyGrbit.NewKey);
					Api.MakeKey(_table.Session, _table, key.BoardId, Encoding.Unicode, MakeKeyGrbit.None);
					Api.MakeKey(_table.Session, _table, key.SequenceNumber,  MakeKeyGrbit.None);
				}

				public bool Find(TypeAndPostIdIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(TypeAndPostIdIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(TypeAndPostIdIndexKey key)
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

			    public int GetIndexRecordCount(TypeAndPostIdIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public struct TypeAndPostIdIndexPartialKey1
				{
					public byte EntityType;
					public string BoardId;
				}

			    // ReSharper disable InconsistentNaming
				public TypeAndPostIdIndexPartialKey1 CreateKey(
						byte EntityType
						,string BoardId
				)
			    // ReSharper enable InconsistentNaming
				{
					return new TypeAndPostIdIndexPartialKey1() {
						EntityType = EntityType,
						BoardId = BoardId,
					
					};
				}

				public void SetKey(TypeAndPostIdIndexPartialKey1 key, bool startRange)
				{
					var rangeFlag = startRange ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit;
					Api.MakeKey(_table.Session, _table, key.EntityType,  MakeKeyGrbit.NewKey);
					Api.MakeKey(_table.Session, _table, key.BoardId, Encoding.Unicode, rangeFlag);
				}

				public bool Find(TypeAndPostIdIndexPartialKey1 key)
				{
					SetKey(key, true);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE);
				}

				public bool SetPartialUpperRange(TypeAndPostIdIndexPartialKey1 key)
				{
					SetKey(key, false);
					return Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit);
				}

				public bool SeekPartial(TypeAndPostIdIndexPartialKey1 key)
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

				public IEnumerable<object> Enumerate(TypeAndPostIdIndexPartialKey1 key)
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

				public int GetIndexRecordCount(TypeAndPostIdIndexPartialKey1 key)
			    {
					if (!SeekPartial(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }


				public struct TypeAndPostIdIndexPartialKey2
				{
					public byte EntityType;
				}

			    // ReSharper disable InconsistentNaming
				public TypeAndPostIdIndexPartialKey2 CreateKey(
						byte EntityType
				)
			    // ReSharper enable InconsistentNaming
				{
					return new TypeAndPostIdIndexPartialKey2() {
						EntityType = EntityType,
					
					};
				}

				public void SetKey(TypeAndPostIdIndexPartialKey2 key, bool startRange)
				{
					var rangeFlag = startRange ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit;
					Api.MakeKey(_table.Session, _table, key.EntityType,  MakeKeyGrbit.NewKey | rangeFlag);
				}

				public bool Find(TypeAndPostIdIndexPartialKey2 key)
				{
					SetKey(key, true);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE);
				}

				public bool SetPartialUpperRange(TypeAndPostIdIndexPartialKey2 key)
				{
					SetKey(key, false);
					return Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit);
				}

				public bool SeekPartial(TypeAndPostIdIndexPartialKey2 key)
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

				public IEnumerable<object> Enumerate(TypeAndPostIdIndexPartialKey2 key)
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

				public int GetIndexRecordCount(TypeAndPostIdIndexPartialKey2 key)
			    {
					if (!SeekPartial(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public class IndexFetchViews
				{
					private readonly PostsTable _table;

					public IndexFetchViews(PostsTable table)
					{
						_table = table;
					}

					// ReSharper disable once InconsistentNaming
					private FetchViews.RetrieveIdFromIndexView? __fv_RetrieveIdFromIndexView;
					public FetchViews.RetrieveIdFromIndexView RetrieveIdFromIndexView
					{
						get
						{
							if (__fv_RetrieveIdFromIndexView == null)
							{
								__fv_RetrieveIdFromIndexView = new FetchViews.RetrieveIdFromIndexView(_table);
							}
							return __fv_RetrieveIdFromIndexView.Value;
						}
					}

					// ReSharper disable once InconsistentNaming
					private FetchViews.ParentsAndIdViewForIndex? __fv_ParentsAndIdViewForIndex;
					public FetchViews.ParentsAndIdViewForIndex ParentsAndIdViewForIndex
					{
						get
						{
							if (__fv_ParentsAndIdViewForIndex == null)
							{
								__fv_ParentsAndIdViewForIndex = new FetchViews.ParentsAndIdViewForIndex(_table);
							}
							return __fv_ParentsAndIdViewForIndex.Value;
						}
					}
				}

				private readonly IndexFetchViews _views;
			    // ReSharper disable once ConvertToAutoProperty
				public IndexFetchViews Views => _views;

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView(TypeAndPostIdIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateUniqueAsRetrieveIdFromIndexView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateUniqueAsRetrieveIdFromIndexView(TypeAndPostIdIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView(TypeAndPostIdIndexPartialKey1 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return Views.RetrieveIdFromIndexView.Fetch();
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}
								

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView(TypeAndPostIdIndexPartialKey2 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return Views.RetrieveIdFromIndexView.Fetch();
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}
								
				public IEnumerable<ViewValues.ParentsAndIdViewForIndex> EnumerateAsParentsAndIdViewForIndex()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.ParentsAndIdViewForIndex.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.ParentsAndIdViewForIndex> EnumerateAsParentsAndIdViewForIndex(TypeAndPostIdIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.ParentsAndIdViewForIndex.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.ParentsAndIdViewForIndex> EnumerateUniqueAsParentsAndIdViewForIndex()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.ParentsAndIdViewForIndex.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.ParentsAndIdViewForIndex> EnumerateUniqueAsParentsAndIdViewForIndex(TypeAndPostIdIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.ParentsAndIdViewForIndex.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.ParentsAndIdViewForIndex> EnumerateAsParentsAndIdViewForIndex(TypeAndPostIdIndexPartialKey1 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return Views.ParentsAndIdViewForIndex.Fetch();
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}
								

				public IEnumerable<ViewValues.ParentsAndIdViewForIndex> EnumerateAsParentsAndIdViewForIndex(TypeAndPostIdIndexPartialKey2 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return Views.ParentsAndIdViewForIndex.Fetch();
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}
								
				
			}

			public struct FlagsIndex
			{
				private readonly PostsTable _table;

				public FlagsIndex(PostsTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "FlagsIndex");
				}

				public struct FlagsIndexKey
				{
					public Guid? Flags;
				}

			    // ReSharper disable InconsistentNaming
				public FlagsIndexKey CreateKey(
						Guid? Flags
				)
			    // ReSharper enable InconsistentNaming
				{
					return new FlagsIndexKey() {
						Flags = Flags,
					
					};
				}

				public void SetKey(FlagsIndexKey key)
				{
					if (key.Flags == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.Flags.Value, MakeKeyGrbit.NewKey);
					}
				}

				public bool Find(FlagsIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(FlagsIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(FlagsIndexKey key)
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

			    public int GetIndexRecordCount(FlagsIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }
				
			}

			public struct DirectParentFlagsIndex
			{
				private readonly PostsTable _table;

				public DirectParentFlagsIndex(PostsTable table)
				{
					_table = table;
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "DirectParentFlagsIndex");
				}

				public struct DirectParentFlagsIndexKey
				{
					public int? DirectParentId;
					public Guid? Flags;
				}

			    // ReSharper disable InconsistentNaming
				public DirectParentFlagsIndexKey CreateKey(
						int? DirectParentId
						,Guid? Flags
				)
			    // ReSharper enable InconsistentNaming
				{
					return new DirectParentFlagsIndexKey() {
						DirectParentId = DirectParentId,
						Flags = Flags,
					
					};
				}

				public void SetKey(DirectParentFlagsIndexKey key)
				{
					if (key.DirectParentId == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.DirectParentId.Value, MakeKeyGrbit.NewKey);
					}
					if (key.Flags == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.None);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.Flags.Value, MakeKeyGrbit.None);
					}
				}

				public bool Find(DirectParentFlagsIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(DirectParentFlagsIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(DirectParentFlagsIndexKey key)
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

			    public int GetIndexRecordCount(DirectParentFlagsIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public struct DirectParentFlagsIndexPartialKey1
				{
					public int? DirectParentId;
				}

			    // ReSharper disable InconsistentNaming
				public DirectParentFlagsIndexPartialKey1 CreateKey(
						int? DirectParentId
				)
			    // ReSharper enable InconsistentNaming
				{
					return new DirectParentFlagsIndexPartialKey1() {
						DirectParentId = DirectParentId,
					
					};
				}

				public void SetKey(DirectParentFlagsIndexPartialKey1 key, bool startRange)
				{
					var rangeFlag = startRange ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit;
					if (key.DirectParentId == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey | rangeFlag);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.DirectParentId.Value, MakeKeyGrbit.NewKey | rangeFlag);
					}
				}

				public bool Find(DirectParentFlagsIndexPartialKey1 key)
				{
					SetKey(key, true);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE);
				}

				public bool SetPartialUpperRange(DirectParentFlagsIndexPartialKey1 key)
				{
					SetKey(key, false);
					return Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit);
				}

				public bool SeekPartial(DirectParentFlagsIndexPartialKey1 key)
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

				public IEnumerable<object> Enumerate(DirectParentFlagsIndexPartialKey1 key)
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

				public int GetIndexRecordCount(DirectParentFlagsIndexPartialKey1 key)
			    {
					if (!SeekPartial(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				
			}

			public struct QuotedPostsIndex
			{
				private readonly PostsTable _table;

				public QuotedPostsIndex(PostsTable table)
				{
					_table = table;
					_views = new IndexFetchViews(_table);
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "QuotedPostsIndex");
				}

				public struct QuotedPostsIndexKey
				{
					public int? DirectParentId;
					public int? QuotedPosts;
				}

			    // ReSharper disable InconsistentNaming
				public QuotedPostsIndexKey CreateKey(
						int? DirectParentId
						,int? QuotedPosts
				)
			    // ReSharper enable InconsistentNaming
				{
					return new QuotedPostsIndexKey() {
						DirectParentId = DirectParentId,
						QuotedPosts = QuotedPosts,
					
					};
				}

				public void SetKey(QuotedPostsIndexKey key)
				{
					if (key.DirectParentId == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.DirectParentId.Value, MakeKeyGrbit.NewKey);
					}
					if (key.QuotedPosts == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.None);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.QuotedPosts.Value, MakeKeyGrbit.None);
					}
				}

				public bool Find(QuotedPostsIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(QuotedPostsIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(QuotedPostsIndexKey key)
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

			    public int GetIndexRecordCount(QuotedPostsIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public struct QuotedPostsIndexPartialKey1
				{
					public int? DirectParentId;
				}

			    // ReSharper disable InconsistentNaming
				public QuotedPostsIndexPartialKey1 CreateKey(
						int? DirectParentId
				)
			    // ReSharper enable InconsistentNaming
				{
					return new QuotedPostsIndexPartialKey1() {
						DirectParentId = DirectParentId,
					
					};
				}

				public void SetKey(QuotedPostsIndexPartialKey1 key, bool startRange)
				{
					var rangeFlag = startRange ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit;
					if (key.DirectParentId == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey | rangeFlag);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.DirectParentId.Value, MakeKeyGrbit.NewKey | rangeFlag);
					}
				}

				public bool Find(QuotedPostsIndexPartialKey1 key)
				{
					SetKey(key, true);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE);
				}

				public bool SetPartialUpperRange(QuotedPostsIndexPartialKey1 key)
				{
					SetKey(key, false);
					return Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit);
				}

				public bool SeekPartial(QuotedPostsIndexPartialKey1 key)
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

				public IEnumerable<object> Enumerate(QuotedPostsIndexPartialKey1 key)
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

				public int GetIndexRecordCount(QuotedPostsIndexPartialKey1 key)
			    {
					if (!SeekPartial(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public class IndexFetchViews
				{
					private readonly PostsTable _table;

					public IndexFetchViews(PostsTable table)
					{
						_table = table;
					}

					// ReSharper disable once InconsistentNaming
					private FetchViews.RetrieveIdFromIndexView? __fv_RetrieveIdFromIndexView;
					public FetchViews.RetrieveIdFromIndexView RetrieveIdFromIndexView
					{
						get
						{
							if (__fv_RetrieveIdFromIndexView == null)
							{
								__fv_RetrieveIdFromIndexView = new FetchViews.RetrieveIdFromIndexView(_table);
							}
							return __fv_RetrieveIdFromIndexView.Value;
						}
					}

					// ReSharper disable once InconsistentNaming
					private FetchViews.SequenceNumberView? __fv_SequenceNumberView;
					public FetchViews.SequenceNumberView SequenceNumberView
					{
						get
						{
							if (__fv_SequenceNumberView == null)
							{
								__fv_SequenceNumberView = new FetchViews.SequenceNumberView(_table);
							}
							return __fv_SequenceNumberView.Value;
						}
					}
				}

				private readonly IndexFetchViews _views;
			    // ReSharper disable once ConvertToAutoProperty
				public IndexFetchViews Views => _views;

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView(QuotedPostsIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateUniqueAsRetrieveIdFromIndexView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateUniqueAsRetrieveIdFromIndexView(QuotedPostsIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView(QuotedPostsIndexPartialKey1 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return Views.RetrieveIdFromIndexView.Fetch();
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}
								
				public IEnumerable<ViewValues.SequenceNumberView> EnumerateAsSequenceNumberView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.SequenceNumberView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.SequenceNumberView> EnumerateAsSequenceNumberView(QuotedPostsIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.SequenceNumberView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.SequenceNumberView> EnumerateUniqueAsSequenceNumberView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.SequenceNumberView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.SequenceNumberView> EnumerateUniqueAsSequenceNumberView(QuotedPostsIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.SequenceNumberView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.SequenceNumberView> EnumerateAsSequenceNumberView(QuotedPostsIndexPartialKey1 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return Views.SequenceNumberView.Fetch();
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}
								
				
			}

			public struct InThreadPostLinkIndex
			{
				private readonly PostsTable _table;

				public InThreadPostLinkIndex(PostsTable table)
				{
					_table = table;
					_views = new IndexFetchViews(_table);
				}

				public void SetAsCurrentIndex()
				{
					Api.JetSetCurrentIndex(_table.Session, _table, "InThreadPostLinkIndex");
				}

				public struct InThreadPostLinkIndexKey
				{
					public int? DirectParentId;
					public int SequenceNumber;
				}

			    // ReSharper disable InconsistentNaming
				public InThreadPostLinkIndexKey CreateKey(
						int? DirectParentId
						,int SequenceNumber
				)
			    // ReSharper enable InconsistentNaming
				{
					return new InThreadPostLinkIndexKey() {
						DirectParentId = DirectParentId,
						SequenceNumber = SequenceNumber,
					
					};
				}

				public void SetKey(InThreadPostLinkIndexKey key)
				{
					if (key.DirectParentId == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.DirectParentId.Value, MakeKeyGrbit.NewKey);
					}
					Api.MakeKey(_table.Session, _table, key.SequenceNumber,  MakeKeyGrbit.None);
				}

				public bool Find(InThreadPostLinkIndexKey key)
				{
					SetKey(key);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange);
				}

				public IEnumerable<object> Enumerate(InThreadPostLinkIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return _table;
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<object> EnumerateUnique(InThreadPostLinkIndexKey key)
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

			    public int GetIndexRecordCount(InThreadPostLinkIndexKey key)
			    {
			        if (!Find(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public struct InThreadPostLinkIndexPartialKey1
				{
					public int? DirectParentId;
				}

			    // ReSharper disable InconsistentNaming
				public InThreadPostLinkIndexPartialKey1 CreateKey(
						int? DirectParentId
				)
			    // ReSharper enable InconsistentNaming
				{
					return new InThreadPostLinkIndexPartialKey1() {
						DirectParentId = DirectParentId,
					
					};
				}

				public void SetKey(InThreadPostLinkIndexPartialKey1 key, bool startRange)
				{
					var rangeFlag = startRange ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit;
					if (key.DirectParentId == null)
					{
						Api.MakeKey(_table.Session, _table, null, MakeKeyGrbit.NewKey | rangeFlag);
					} else
					{
						Api.MakeKey(_table.Session, _table, key.DirectParentId.Value, MakeKeyGrbit.NewKey | rangeFlag);
					}
				}

				public bool Find(InThreadPostLinkIndexPartialKey1 key)
				{
					SetKey(key, true);
					return Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE);
				}

				public bool SetPartialUpperRange(InThreadPostLinkIndexPartialKey1 key)
				{
					SetKey(key, false);
					return Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit);
				}

				public bool SeekPartial(InThreadPostLinkIndexPartialKey1 key)
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

				public IEnumerable<object> Enumerate(InThreadPostLinkIndexPartialKey1 key)
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

				public int GetIndexRecordCount(InThreadPostLinkIndexPartialKey1 key)
			    {
					if (!SeekPartial(key))
					{
						return 0;
					}
			        return GetIndexRecordCount();
			    }

				public class IndexFetchViews
				{
					private readonly PostsTable _table;

					public IndexFetchViews(PostsTable table)
					{
						_table = table;
					}

					// ReSharper disable once InconsistentNaming
					private FetchViews.RetrieveIdFromIndexView? __fv_RetrieveIdFromIndexView;
					public FetchViews.RetrieveIdFromIndexView RetrieveIdFromIndexView
					{
						get
						{
							if (__fv_RetrieveIdFromIndexView == null)
							{
								__fv_RetrieveIdFromIndexView = new FetchViews.RetrieveIdFromIndexView(_table);
							}
							return __fv_RetrieveIdFromIndexView.Value;
						}
					}

					// ReSharper disable once InconsistentNaming
					private FetchViews.SequenceNumberView? __fv_SequenceNumberView;
					public FetchViews.SequenceNumberView SequenceNumberView
					{
						get
						{
							if (__fv_SequenceNumberView == null)
							{
								__fv_SequenceNumberView = new FetchViews.SequenceNumberView(_table);
							}
							return __fv_SequenceNumberView.Value;
						}
					}
				}

				private readonly IndexFetchViews _views;
			    // ReSharper disable once ConvertToAutoProperty
				public IndexFetchViews Views => _views;

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView(InThreadPostLinkIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateUniqueAsRetrieveIdFromIndexView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateUniqueAsRetrieveIdFromIndexView(InThreadPostLinkIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.RetrieveIdFromIndexView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.RetrieveIdFromIndexView> EnumerateAsRetrieveIdFromIndexView(InThreadPostLinkIndexPartialKey1 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return Views.RetrieveIdFromIndexView.Fetch();
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}
								
				public IEnumerable<ViewValues.SequenceNumberView> EnumerateAsSequenceNumberView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.SequenceNumberView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.SequenceNumberView> EnumerateAsSequenceNumberView(InThreadPostLinkIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.SequenceNumberView.Fetch();
						} while (Api.TryMoveNext(_table.Session, _table));
					}
				}

				public IEnumerable<ViewValues.SequenceNumberView> EnumerateUniqueAsSequenceNumberView()
				{
					if (Api.TryMoveFirst(_table.Session, _table))
					{
						do {
							yield return Views.SequenceNumberView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.SequenceNumberView> EnumerateUniqueAsSequenceNumberView(InThreadPostLinkIndexKey key)
				{
					SetKey(key);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange))
					{
						do {
							yield return Views.SequenceNumberView.Fetch();
						} while (Api.TryMove(_table.Session, _table, JET_Move.Next, MoveGrbit.MoveKeyNE));
					}
				}

				public IEnumerable<ViewValues.SequenceNumberView> EnumerateAsSequenceNumberView(InThreadPostLinkIndexPartialKey1 key)
				{
					SetKey(key, true);
					if (Api.TrySeek(_table.Session, _table, SeekGrbit.SeekGE))
					{
						SetKey(key, false);
						if (Api.TrySetIndexRange(_table.Session, _table, SetIndexRangeGrbit.RangeUpperLimit))
						{
							do {
								yield return Views.SequenceNumberView.Fetch();
							} while (Api.TryMoveNext(_table.Session, _table));
						}
					}
				}
								
				
			}
		}

		public class TableIndexes
		{
			private readonly PostsTable _table;

			public TableIndexes(PostsTable table)
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
			private IndexDefinitions.ParentIdIndex? __ti_ParentIdIndex;

			public IndexDefinitions.ParentIdIndex ParentIdIndex
			{
				get
				{
					if (__ti_ParentIdIndex == null)
					{
						__ti_ParentIdIndex = new IndexDefinitions.ParentIdIndex(_table);
					}
					return __ti_ParentIdIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.TypeIndex? __ti_TypeIndex;

			public IndexDefinitions.TypeIndex TypeIndex
			{
				get
				{
					if (__ti_TypeIndex == null)
					{
						__ti_TypeIndex = new IndexDefinitions.TypeIndex(_table);
					}
					return __ti_TypeIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.ChildrenLoadStageIndex? __ti_ChildrenLoadStageIndex;

			public IndexDefinitions.ChildrenLoadStageIndex ChildrenLoadStageIndex
			{
				get
				{
					if (__ti_ChildrenLoadStageIndex == null)
					{
						__ti_ChildrenLoadStageIndex = new IndexDefinitions.ChildrenLoadStageIndex(_table);
					}
					return __ti_ChildrenLoadStageIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.TypeAndPostIdIndex? __ti_TypeAndPostIdIndex;

			public IndexDefinitions.TypeAndPostIdIndex TypeAndPostIdIndex
			{
				get
				{
					if (__ti_TypeAndPostIdIndex == null)
					{
						__ti_TypeAndPostIdIndex = new IndexDefinitions.TypeAndPostIdIndex(_table);
					}
					return __ti_TypeAndPostIdIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.FlagsIndex? __ti_FlagsIndex;

			public IndexDefinitions.FlagsIndex FlagsIndex
			{
				get
				{
					if (__ti_FlagsIndex == null)
					{
						__ti_FlagsIndex = new IndexDefinitions.FlagsIndex(_table);
					}
					return __ti_FlagsIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.DirectParentFlagsIndex? __ti_DirectParentFlagsIndex;

			public IndexDefinitions.DirectParentFlagsIndex DirectParentFlagsIndex
			{
				get
				{
					if (__ti_DirectParentFlagsIndex == null)
					{
						__ti_DirectParentFlagsIndex = new IndexDefinitions.DirectParentFlagsIndex(_table);
					}
					return __ti_DirectParentFlagsIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.QuotedPostsIndex? __ti_QuotedPostsIndex;

			public IndexDefinitions.QuotedPostsIndex QuotedPostsIndex
			{
				get
				{
					if (__ti_QuotedPostsIndex == null)
					{
						__ti_QuotedPostsIndex = new IndexDefinitions.QuotedPostsIndex(_table);
					}
					return __ti_QuotedPostsIndex.Value;
				}
			}

		    // ReSharper disable once InconsistentNaming
			private IndexDefinitions.InThreadPostLinkIndex? __ti_InThreadPostLinkIndex;

			public IndexDefinitions.InThreadPostLinkIndex InThreadPostLinkIndex
			{
				get
				{
					if (__ti_InThreadPostLinkIndex == null)
					{
						__ti_InThreadPostLinkIndex = new IndexDefinitions.InThreadPostLinkIndex(_table);
					}
					return __ti_InThreadPostLinkIndex.Value;
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



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
			var idxDef2 = "+Flags\0\0";
			Api.JetCreateIndex(sid, tableid, "FlagsIndex", CreateIndexGrbit.None, idxDef2, idxDef2.Length, 100);
			var idxDef3 = "+DirectParentId\0+Flags\0\0";
			Api.JetCreateIndex(sid, tableid, "DirectParentFlagsIndex", CreateIndexGrbit.IndexIgnoreAnyNull, idxDef3, idxDef3.Length, 100);
			var idxDef4 = "+DirectParentId\0+QuotedPosts\0\0";
			Api.JetCreateIndex(sid, tableid, "QuotedPostsIndex", CreateIndexGrbit.IndexIgnoreAnyNull, idxDef4, idxDef4.Length, 100);
			var idxDef5 = "+DirectParentId\0+SequenceNumber\0\0";
			Api.JetCreateIndex(sid, tableid, "InThreadPostLinkIndex", CreateIndexGrbit.IndexIgnoreAnyNull, idxDef5, idxDef5.Length, 100);
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
				__mv_ParentId = new Multivalue<Int32ColumnValue>(table, table.ColumnDictionary[PostsTable.Column.ParentId]);
				__mv_Flags = new Multivalue<GuidColumnValue>(table, table.ColumnDictionary[PostsTable.Column.Flags]);
				__mv_ThreadTags = new Multivalue<StringColumnValue>(table, table.ColumnDictionary[PostsTable.Column.ThreadTags]);
				__mv_QuotedPosts = new Multivalue<Int32ColumnValue>(table, table.ColumnDictionary[PostsTable.Column.QuotedPosts]);
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
			
			public void SetParentIdValueArr(Int32ColumnValue[] v)
			{
			    // ReSharper disable once ImpureMethodCallOnReadonlyValueField
				__mv_ParentId.SetValues(v);
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
			
			public void SetFlagsValueArr(GuidColumnValue[] v)
			{
			    // ReSharper disable once ImpureMethodCallOnReadonlyValueField
				__mv_Flags.SetValues(v);
			}

		    // ReSharper disable once ConvertToAutoProperty
			public Multivalue<StringColumnValue> ThreadTags => __mv_ThreadTags;
			
			public void SetThreadTagsValueArr(StringColumnValue[] v)
			{
			    // ReSharper disable once ImpureMethodCallOnReadonlyValueField
				__mv_ThreadTags.SetValues(v);
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
			
			public void SetQuotedPostsValueArr(Int32ColumnValue[] v)
			{
			    // ReSharper disable once ImpureMethodCallOnReadonlyValueField
				__mv_QuotedPosts.SetValues(v);
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
		}

		public static class FetchViews {
	
		}

		public class TableFetchViews
		{
			private readonly PostsTable _table;

			public TableFetchViews(PostsTable table)
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

				
			}

			public struct InThreadPostLinkIndex
			{
				private readonly PostsTable _table;

				public InThreadPostLinkIndex(PostsTable table)
				{
					_table = table;
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

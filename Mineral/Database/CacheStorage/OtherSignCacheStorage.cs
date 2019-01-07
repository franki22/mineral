﻿using Mineral.Core;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using System.Collections.Generic;

namespace Mineral.Database.CacheStorage
{
    internal class OtherSignCacheStorage
    {
        DbCache<UInt256, OtherSignTransactionState> _cache;

        public OtherSignCacheStorage(DB db)
        {
            _cache = new DbCache<UInt256, OtherSignTransactionState>(db, DataEntryPrefix.ST_OtherSign);
        }

        public void Add(UInt256 txhash, HashSet<string> others)
        {
            _cache.Add(txhash, new OtherSignTransactionState(txhash, others));
        }

        public OtherSignTransactionState GetAndChange(UInt256 txhash)
        {
            return _cache.GetAndChange(txhash, null);
        }

        public void Commit(WriteBatch batch)
        {
            _cache.Commit(batch);
        }
    }
}

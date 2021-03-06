﻿using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Protocol;

namespace Mineral.Core.Database
{
    public class ExchangeStore : MineralStoreWithRevoking<ExchangeCapsule, Exchange>
    {
        public class ExchangeSortCompare : IComparer<ExchangeCapsule>
        {
            public int Compare(ExchangeCapsule x, ExchangeCapsule y)
            {
                return x.CreateTime <= y.CreateTime ? 1 : -1;
            }
        }

        #region Field
        #endregion


        #region Property
        public List<ExchangeCapsule> AllExchanges
        {
            get
            {
                List<ExchangeCapsule> result = new List<ExchangeCapsule>();
                IEnumerator<KeyValuePair<byte[], ExchangeCapsule>> it = GetEnumerator();

                while (it.MoveNext())
                {
                    result.Add(it.Current.Value);
                }
                result.Sort(new ExchangeSortCompare());

                return result;
            }
        }
        #endregion


        #region Contructor
        public ExchangeStore(IRevokingDatabase revoking_database, string db_name = "exchange")
            : base(revoking_database, db_name)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override ExchangeCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.Get(key);

            return new ExchangeCapsule(value);
        }
        #endregion
    }
}

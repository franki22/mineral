﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database
{
    public class AssetIssueV2Store : AssetIssueStore
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public AssetIssueV2Store(IRevokingDatabase revoking_database, string db_name = "asset-issue-v2")
            : base(revoking_database, db_name)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Database2.Common;

namespace Mineral.Core.Database2.Core
{
    public abstract class AbstractSnapshot<T, V> : ISnapshot
    {
        #region Field
        protected ISnapshot previous;
        protected WeakReference next = null;
        protected IBaseDB<T, V> db { get; set; }

        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public ISnapshot Advance()
        {
            throw new NotImplementedException();
        }

        public void SetPrevious(ISnapshot snapshot)
        {
            this.previous = snapshot;
        }

        public void SetNext(ISnapshot next)
        {
            this.next = new WeakReference(next);
        }

        public ISnapshot GetPrevious()
        {
            return this.previous;
        }

        public ISnapshot GetNext()
        {
            return (ISnapshot)this.next.Target;
        }

        #region Abstract -ISnapshot
        public abstract ISnapshot GetSolidity();
        public abstract ISnapshot Retreat();
        public abstract byte[] Get(byte[] key);
        public abstract void Put(byte[] key, byte[] value);
        public abstract void Merge(ISnapshot snapshot);
        public abstract void Remove(byte[] key);
        public abstract void Reset();
        public abstract void ResetSolidity();
        public abstract void UpdateSolidity();
        public abstract void Close();
        #endregion
        #endregion
    }
}
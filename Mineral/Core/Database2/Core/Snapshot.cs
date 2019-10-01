﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mineral.Core.Database2.Common;

namespace Mineral.Core.Database2.Core
{
    public class Snapshot : AbstractSnapshot<Key, Value>
    {
        #region Field
        private ISnapshot root = null;
        #endregion


        #region Property
        protected ISnapshot Root { get { return this.root; } }
        #endregion


        #region Constructor
        public Snapshot(ISnapshot snapshot)
        {
            this.root = snapshot.GetRoot();
            this.previous = snapshot;
            snapshot.SetNext(this);
            lock (this)
            {
                this.db = new HashDB();
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static bool IsRoot(ISnapshot snapshot)
        {
            return snapshot != null && snapshot.GetType() == typeof(SnapshotRoot);
        }

        public static bool IsImplement(ISnapshot snapshot)
        {
            return snapshot != null && snapshot.GetType() == typeof(Snapshot);
        }

        public IEnumerator<KeyValuePair<Key, Value>> GetEnumerator()
        {
            return ((HashDB)(this.db)).GetEnumerator();
        }

        public override ISnapshot GetRoot()
        {
            return this.root;
        }

        public byte[] Get(ISnapshot head, byte[] key)
        {
            ISnapshot snapshot = head;

            Value result = null;
            while (IsImplement(snapshot))
            {
                result = ((Snapshot)(snapshot)).db.Get(Key.Of(key));
                if (result != null)
                {
                    return result.Data;
                }
                snapshot = snapshot.GetPrevious();
            }
            return snapshot?.Get(key);
        }

        public override ISnapshot GetSolidity()
        {
            return this.root.GetSolidity();
        }

        public override ISnapshot Retreat()
        {
            return this.previous;
        }

        public override byte[] Get(byte[] key)
        {
            return Get(this, key);
        }

        public override void Put(byte[] key, byte[] value)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            Helper.IsNotNull(value, "Key must be not null.");

            this.db.Put(Key.CopyOf(key), Value.CopyOf(Value.Operator.PUT, value));
        }

        public override void Merge(ISnapshot snapshot)
        {
            HashDB hash_db = null;
            if (((Snapshot)snapshot).db is HashDB)
            {
                hash_db = (HashDB)((Snapshot)snapshot).db;
                IEnumerator<KeyValuePair<Key, Value>> it = hash_db.GetEnumerator();

                while (it.MoveNext())
                    this.db.Put(it.Current.Key, it.Current.Value);
            }
        }

        public override void Remove(byte[] key)
        {
            Helper.IsNotNull(key, "Key must be not null.");
            this.db.Put(Key.Of(key), Value.Of(Value.Operator.DELETE, null));
        }

        public override void Reset()
        {
            this.root.Reset();
        }

        public override void Close()
        {
            this.root.Close();
        }

        public override void ResetSolidity()
        {
            this.root.ResetSolidity();
        }

        public override void UpdateSolidity()
        {
            this.root.UpdateSolidity();
        }

        public void Collect(Dictionary<WrappedByteArray, WrappedByteArray> collect)
        {
            ISnapshot next = GetRoot().GetNext();
            while (next != null)
            {
                IEnumerator<KeyValuePair<Key, Value>> values = ((Snapshot)next).GetEnumerator();
                while (values.MoveNext())
                {
                    collect.Add(WrappedByteArray.Of(values.Current.Key.Data), WrappedByteArray.Of(values.Current.Value.Data));
                }
                next = next.GetNext();
            }
        }
        #endregion
    }
}

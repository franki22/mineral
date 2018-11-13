﻿using Mineral.Database.LevelDB;
using System;
using System.Collections.Generic;

namespace Mineral.Core
{
    public abstract class Blockchain : IDisposable
    {
        public enum BLOCK_ERROR
        {
            E_NO_ERROR = 0,
            E_ERROR = 1,
            E_ERROR_HEIGHT = 2
        };

        static private Blockchain _instance = null;
        static public Blockchain Instance => _instance;

        static public void SetInstance(Blockchain chain)
        {
            _instance = chain;
        }

        protected Proof _proof;
        public Proof Proof => _proof;
        public void SetProof(Proof proof)
        {
            _proof = proof;
        }

        public object PersistLock { get; } = new object();
        public event EventHandler<Block> PersistCompleted;

        public abstract Block GenesisBlock { get; }
        public abstract int CurrentBlockHeight { get; }
        public abstract UInt256 CurrentBlockHash { get; }

        public abstract int CurrentHeaderHeight { get; }
        public abstract UInt256 CurrentHeaderHash { get; }

        protected object PoolLock { get; } = new object();
        protected Dictionary<UInt256, Transaction> _rxPool = new Dictionary<UInt256, Transaction>();
        protected Dictionary<UInt256, Transaction> _txPool = new Dictionary<UInt256, Transaction>();

        public abstract void Run();
        public abstract void Dispose();
        public abstract BLOCK_ERROR AddBlock(Block block);
        public abstract bool AddBlockDirectly(Block block);
        public abstract BlockHeader GetHeader(UInt256 hash);
        public abstract BlockHeader GetHeader(int height);
        public abstract BlockHeader GetNextHeader(UInt256 hash);
        public abstract bool ContainsBlock(UInt256 hash);
        public abstract Block GetBlock(UInt256 hash);
        public abstract Block GetBlock(int height);
        public abstract Block GetNextBlock(UInt256 hash);
        public abstract bool VerityBlock(Block block);

        public abstract Storage storage { get; }

        public bool HasTransactionPool(UInt256 hash)
        {
            lock (PoolLock)
            {
                if (_rxPool.ContainsKey(hash))
                    return true;
                if (_txPool.ContainsKey(hash))
                    return true;
                return false;
            }
        }

        public bool AddTransactionPool(Transaction tx)
        {
            if (!tx.Verify())
                return false;

            lock (PoolLock)
            {
                if (_rxPool.ContainsKey(tx.Hash))
                    return false;
                if (_txPool.ContainsKey(tx.Hash))
                    return false;
                if (storage.GetTransaction(tx.Hash) != null)
                    return false;
                _rxPool.Add(tx.Hash, tx);
                return true;
            }
        }

        public void AddTxPool(List<Transaction> txs)
        {
            lock (PoolLock)
            {
                foreach (var tx in txs)
                {
                    if (_rxPool.ContainsKey(tx.Hash))
                        continue;
                    if (_txPool.ContainsKey(tx.Hash))
                        continue;
                    _txPool.Add(tx.Hash, tx);
                }
            }
        }

        public void AddTransactionPool(List<Transaction> txs)
        {
            foreach (var tx in txs)
                AddTransactionPool(tx);
        }

        public int RemoveTransactionPool(List<Transaction> txs)
        {
            int nRemove = 0;
            lock (PoolLock)
            {
                foreach (Transaction tx in txs)
                {
                    if (_txPool.ContainsKey(tx.Hash))
                    {
                        _txPool.Remove(tx.Hash);
                        nRemove++;
                    }
                    if (_rxPool.ContainsKey(tx.Hash))
                    {
                        _rxPool.Remove(tx.Hash);
                        nRemove++;
                    }
                }
            }
            return nRemove;
        }

        public void LoadTransactionPool(ref List<Transaction> txs)
        {
            if (txs == null)
                txs = new List<Transaction>();
            lock (PoolLock)
            {
                foreach (Transaction tx in _rxPool.Values)
                {
                    txs.Add(tx);
                    _txPool.Add(tx.Hash, tx);
                    if (txs.Count >= Config.Instance.MaxTransactions)
                    {
                        foreach (Transaction rx in _txPool.Values)
                            _rxPool.Remove(rx.Hash);
                        return;
                    }
                }
                _rxPool.Clear();
            }
        }

        public abstract List<DelegateState> GetDelegateStateAll();
        public abstract List<DelegateState> GetDelegateStateMakers();

        protected void OnPersistCompleted(Block block)
        {
            RemoveTransactionPool(block.Transactions);
            PersistCompleted?.Invoke(this, block);
        }

        public abstract void NormalizeTransactions(ref List<Transaction> txs);
        public abstract void PersistTurnTable(List<UInt160> addrs, int height);
        public abstract TurnTableState GetTurnTable(int height);
    }
}
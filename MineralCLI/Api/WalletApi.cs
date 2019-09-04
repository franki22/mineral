﻿using Google.Protobuf;
using Mineral;
using Mineral.Common.Utils;
using Mineral.Core.Capsule.Util;
using Mineral.Cryptography;
using Mineral.Wallets.KeyStore;
using MineralCLI.Shell;
using MineralCLI.Util;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MineralCLI.Api
{
    public static class WalletApi
    {
        private static readonly string file_path = "Wallet";

        public static TransferContract CreateTransaferContract(byte[] owner, byte[] to, long amount)
        {
            TransferContract contract = new TransferContract();
            contract.ToAddress = ByteString.CopyFrom(to);
            contract.OwnerAddress = ByteString.CopyFrom(owner);
            contract.Amount = amount;

            return contract;
        }

        public static Transaction SignatureTransaction(Transaction transaction)
        {
            if (transaction.RawData.Timestamp == 0)
            {
                transaction.RawData.Timestamp = Helper.CurrentTimeMillis();
            }

            ProtocolUtil.SetExpirationTime(ref transaction);
            ProtocolUtil.SetPermissionId(ref transaction);

            while (true)
            {
                Console.WriteLine("Please choose keystore for signature.");
                KeyStore key_store = SelectKeyStore();

                string password = ConsoleServiceBase.ReadPasswordString("Please input password");
                if (KeyStoreService.DecryptKeyStore(password, key_store, out byte[] privatekey))
                {
                    ECKey key = ECKey.FromPrivateKey(privatekey);
                    ECDSASignature signature = key.Sign(SHA256Hash.ToHash(transaction.RawData.ToByteArray()));

                    transaction.Signature.Add(ByteString.CopyFrom(signature.ToByteArray()));

                    Console.WriteLine("current transaction hex string is " + transaction.ToByteArray().ToHexString());

                    //break;
                }
            }

            return transaction;
        }

        public static KeyStore SelectKeyStore()
        {
            DirectoryInfo info = new DirectoryInfo(file_path);
            if (!info.Exists)
            {
                return null;
            }

            FileInfo[] wallets = info.GetFiles();
            if (wallets.Length <= 0)
            {
                return null;
            }

            for (int i = 0; i < wallets.Length; i++)
            {
                Console.WriteLine("The " + "[" + i + 1 + "]" + " Keystore file name : " + wallets[i].Name);
            }

            FileInfo wallet = null;

            while (true)
            {
                int index = -1;
                string input = Console.ReadLine().Trim();
                try
                {
                    index = int.Parse(input);
                }
                catch (System.Exception)
                {
                    Console.WriteLine("Invalid number of " + input);
                    Console.WriteLine("Please choose again between 1 to " + wallets.Length);
                    continue;
                }

                if (index < 1 || index > wallets.Length)
                {
                    Console.WriteLine("Please choose again between 1 to " + wallets.Length);
                    continue;
                }

                wallet = wallets[index];
                break;
            }

            try
            {
                KeyStore keystore = null;
                using (var file = File.OpenText(wallet.FullName))
                {
                    KeyStore.FromJson(file.ReadToEnd());
                }

                return keystore;
            }
            catch (System.Exception)
            {
                Console.WriteLine("load fail keystore file : " + wallet.FullName);
            }

            return null;
        }
    }
}

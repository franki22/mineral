﻿using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

namespace Mineral.Cryptography
{
    public class ECDSASignature
    {
        private const string InvalidDERSignature = "Invalid DER signature";
        public static readonly BigInteger SECP256K1N = new BigInteger("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141", 16);


        public ECDSASignature(BigInteger r, BigInteger s)
        {
            R = r;
            S = s;
        }

        public ECDSASignature(BigInteger[] rs)
        {
            R = rs[0];
            S = rs[1];
        }

        public ECDSASignature(byte[] derSig)
        {
            try
            {
                var decoder = new Asn1InputStream(derSig);
                var seq = decoder.ReadObject() as DerSequence;
                if (seq == null || seq.Count != 2)
                    throw new FormatException(InvalidDERSignature);
                R = ((DerInteger)seq[0]).Value;
                S = ((DerInteger)seq[1]).Value;
            }
            catch (Exception ex)
            {
                throw new FormatException(InvalidDERSignature, ex);
            }
        }

        public BigInteger R { get; }

        public BigInteger S { get; }

        public byte[] V { get; set; }

        public bool IsLowS => S.CompareTo(ECKey.HALF_CURVE_ORDER) <= 0;

        public static ECDSASignature FromDER(byte[] sig)
        {
            return new ECDSASignature(sig);
        }

        public static bool IsValidDER(byte[] bytes)
        {
            try
            {
                FromDER(bytes);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ValidateComponents()
        {
            return ECDSASignatureFactory.ValidateComponents(R, S, V[0]);
        }

        public ECDSASignature MakeCanonical()
        {
            if (!IsLowS)
                return new ECDSASignature(R, ECKey.CURVE_ORDER.Subtract(S));
            return this;
        }

        public byte[] ToDER()
        {
            // Usually 70-72 bytes.
            var bos = new MemoryStream(72);
            var seq = new DerSequenceGenerator(bos);
            seq.AddObject(new DerInteger(R));
            seq.AddObject(new DerInteger(S));
            seq.Close();
            return bos.ToArray();
        }
    }
}

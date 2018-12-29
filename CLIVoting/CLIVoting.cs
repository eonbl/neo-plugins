using Akka.Actor;
using Neo.Consensus;
using Neo.IO;
using Neo.Ledger;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Persistence.LevelDB;
using Neo.Plugins;
using Neo.Services;
using Neo.SmartContract;
using Neo.Wallets;
using Neo.Wallets.NEP6;
using Neo.Wallets.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ECCurve = Neo.Cryptography.ECC.ECCurve;
using ECPoint = Neo.Cryptography.ECC.ECPoint;
using System.Text.RegularExpressions;

namespace Neo.Plugins
{
    public class CLIVoting : Plugin
    {

        public CLIVoting()
        {
            
        }

        public override void Configure()
        {
        }

        private bool OnVote(string[] args)
        {
            if (args.Length < 3) return false;
            if (NoWallet()) return true;
            UInt160 scriptHash = args[1].ToScriptHash();
            Transaction tx = Program.Wallet.MakeTransaction(new StateTransaction
            {
                Version = 0,
                Descriptors = new[]
                {
                    new StateDescriptor
                    {
                        Type = StateType.Account,
                        Key = scriptHash.ToArray(),
                        Field = "Votes",
                        Value = new List<string>(args).GetRange(2, args.Length - 2).Select(p => ECPoint.Parse(p, ECCurve.Secp256r1)).ToArray().ToByteArray()
                    }
                }
            });
            if (tx == null)
            {
                Console.WriteLine("Insufficient funds");
                return true;
            }
            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(tx);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Invalid Operation attempted. Unsynchronized Block");
                return true;
            }
            Program.Wallet.Sign(context);
            if (context.Completed)
            {
                tx.Witnesses = context.GetWitnesses();
                Program.Wallet.ApplyTransaction(tx);
                system.LocalNode.Tell(new LocalNode.Relay { Inventory = tx });
                Console.WriteLine($"Transaction successful\nTXID: {tx.Hash}");
            }
            else
            {
                Console.WriteLine("Incompleted signature\nSignatureContext:");
                Console.WriteLine(context.ToString());
            }
            return true;

        }

        private bool OnHelp(string[] args)
        {
            if (args.Length < 2) return false;
            if (!string.Equals(args[1], Name, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(args[1], "vote", StringComparison.OrdinalIgnoreCase))
                return false;
            Console.Write($"{Name} Command:\n" + "\tvote <your address> <list of candidates' public keys separated by spaces>\n");
            return true;
        }

        protected override bool OnMessage(object message)
        {
            if (!(message is string[] args)) return false;
            if (args.Length == 0) return false;
            switch (args[0].ToLower())
            {
                case "help":
                    return OnHelp(args);
                case "vote":
                    return OnVote(args);
            }
            return false;
        }
    }
}

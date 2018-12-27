using Akka.Actor;
using Neo.IO;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            throw new NotImplementedException();
        }

        private bool OnHelp(string[] args)
        {
            throw new NotImplementedException();
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

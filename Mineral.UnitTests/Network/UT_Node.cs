﻿using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Network;
using Mineral.Network.Payload;

namespace Mineral.UnitTests.Network
{
    public class UnitTestNode : RemoteNode
    {
        public UnitTestNode()
        {

        }

        public void SetInfo(NodeInfo info)
        {
            Info = info;
        }

        protected override Task<Message> ReceiveMessageAsync(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> SendMessageAsync(Message message)
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class UT_Node
    {
        [TestMethod]
        public void ContainsNodeInfo()
        {
            Guid guid = Guid.NewGuid();

            UnitTestNode n1 = new UnitTestNode();
            n1.Info.EndPoint = new IPEndPoint(123, 456);
            n1.Info.Version = VersionPayload.Create(456, guid);

            UnitTestNode n2 = new UnitTestNode();
            ConcurrentDictionary<NodeInfo, RemoteNode> peers = new ConcurrentDictionary<NodeInfo, RemoteNode>();
            peers.TryAdd(n1.Info, n1).Should().BeTrue();
            peers.TryAdd(n1.Info, n2).Should().BeFalse();
        }
    }
}

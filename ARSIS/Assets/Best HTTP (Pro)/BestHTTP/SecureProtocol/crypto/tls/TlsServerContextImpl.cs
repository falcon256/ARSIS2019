#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Tls
{
    internal class TlsServerContextImpl
        : AbstractTlsContext, TlsServerContext
    {
        internal TlsServerContextImpl(SecureRandom secureRandom, SecurityParameters securityParameters)
            : base(secureRandom, securityParameters)
        {
        }

        public override bool IsServer
        {
            get { return true; }
        }
    }
}

#endif

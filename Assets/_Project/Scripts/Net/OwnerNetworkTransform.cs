#if SIMCITY_NETCODE
using Unity.Netcode.Components;

namespace Simcity.Net
{
    /// <summary>
    /// A NetworkTransform that trusts the OWNING client instead of the server. Players
    /// move themselves locally (FirstPersonController on the owner) and this streams that
    /// motion to everyone else, which feels responsive for co-op movement. NPCs use the
    /// stock server-authoritative NetworkTransform instead (they're driven on the host).
    /// </summary>
    public class OwnerNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
#endif

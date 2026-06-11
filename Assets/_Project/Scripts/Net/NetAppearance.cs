#if SIMCITY_NETCODE
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Simcity.Stats;

namespace Simcity.Net
{
    /// <summary>
    /// Network-friendly snapshot of an <see cref="AppearanceConfig"/>. NetworkVariable
    /// can't carry the managed AppearanceConfig class, so we pack the look into a small
    /// blittable struct (name as a FixedString, colors/floats inline) that replicates to
    /// every client — that's how friends see each other's chosen body, and how clients
    /// rebuild a server-spawned NPC's randomized look.
    /// </summary>
    public struct NetAppearance : INetworkSerializable, IEquatable<NetAppearance>
    {
        public FixedString64Bytes name;
        public byte gender;
        public float height;
        public float build;
        public Color skin;
        public Color hair;
        public Color shirt;
        public Color pants;

        public static NetAppearance From(AppearanceConfig c)
        {
            c ??= AppearanceConfig.CreateDefault();
            return new NetAppearance
            {
                name = new FixedString64Bytes(Trim(c.characterName)),
                gender = (byte)c.gender,
                height = c.height,
                build = c.build,
                skin = c.skin,
                hair = c.hair,
                shirt = c.shirt,
                pants = c.pants,
            };
        }

        public AppearanceConfig ToConfig() => new AppearanceConfig
        {
            characterName = name.ToString(),
            gender = (Gender)gender,
            height = height,
            build = build,
            skin = skin,
            hair = hair,
            shirt = shirt,
            pants = pants,
        };

        // FixedString64Bytes holds up to 61 UTF-8 bytes; keep names well under that.
        private static string Trim(string s) =>
            string.IsNullOrEmpty(s) ? "Newcomer" : (s.Length > 20 ? s.Substring(0, 20) : s);

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref name);
            s.SerializeValue(ref gender);
            s.SerializeValue(ref height);
            s.SerializeValue(ref build);
            // Color isn't a memcpy-serializable type to NGO, so send the channels as floats.
            SerializeColor(s, ref skin);
            SerializeColor(s, ref hair);
            SerializeColor(s, ref shirt);
            SerializeColor(s, ref pants);
        }

        private static void SerializeColor<T>(BufferSerializer<T> s, ref Color c) where T : IReaderWriter
        {
            s.SerializeValue(ref c.r);
            s.SerializeValue(ref c.g);
            s.SerializeValue(ref c.b);
        }

        public bool Equals(NetAppearance o) =>
            name.Equals(o.name) && gender == o.gender && height == o.height && build == o.build &&
            skin == o.skin && hair == o.hair && shirt == o.shirt && pants == o.pants;

        public override bool Equals(object o) => o is NetAppearance a && Equals(a);
        public override int GetHashCode() => name.GetHashCode() ^ gender ^ skin.GetHashCode();
    }
}
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nexauth.Protocol.Packets {
    public class PacketStore {
        public PacketStore() {
            _idAddressableDictionary = new Dictionary<int, PacketMetadata>();
            _typeAddressableDictionary = new Dictionary<Type, PacketMetadata>();
            LoadPackets();
        }

        public PacketMetadata GetMetadataFor<T>() {
            PacketMetadata metadata = null;
            _typeAddressableDictionary.TryGetValue(typeof(T), out metadata);
            return metadata;
        }

        public PacketMetadata GetMetadataFor(int Id) {
            PacketMetadata metadata = null;
            _idAddressableDictionary.TryGetValue(Id, out metadata);
            return metadata;
        }

        private void LoadPackets() {
            var packets = (from type in typeof(PacketStore).Assembly.GetTypes()
                        where type.GetCustomAttributes<PacketAttribute>().Any()
                        select type).ToList<Type>();
            foreach (Type type in packets) {
                var attribute = type.GetCustomAttribute<PacketAttribute>();
                PacketMetadata metadata = new PacketMetadata {
                    Id = attribute.Id,
                    Class = type,
                    Encryption = attribute.Encryption,
                    Type = attribute.Type
                };
                _idAddressableDictionary.Add(metadata.Id, metadata);
                _typeAddressableDictionary.Add(metadata.Class, metadata);
            }
        }

        Dictionary<int, PacketMetadata> _idAddressableDictionary;
        Dictionary<Type, PacketMetadata> _typeAddressableDictionary;
    }
}
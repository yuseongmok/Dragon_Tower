using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DragonTower
{
    // Only the save representation is compressed. Original float ticks are lossless,
    // so replay preserves damage, cooldowns and seeded random decisions exactly.
    public static class BattleResumeCodec
    {
        public static BattleResumeData Pack(BattleResumeData source)
        {
            if (source == null) return null;
            using (var buffer = new MemoryStream())
            {
                using (var zip = new GZipStream(buffer, CompressionMode.Compress, true))
                using (var writer = new BinaryWriter(zip))
                {
                    writer.Write(source.inputs.Count);
                    foreach (var input in source.inputs)
                    {
                        writer.Write((byte)input.action);
                        if (input.action == 0) writer.Write(input.delta);
                        if (input.action == 4) writer.Write(input.itemId ?? "");
                    }
                }
                return new BattleResumeData
                {
                    initialRunJson = source.initialRunJson, seed = source.seed,
                    enemy = source.enemy, monsterId = source.monsterId,
                    packedInputs = Convert.ToBase64String(buffer.ToArray())
                };
            }
        }

        public static BattleResumeData Unpack(BattleResumeData saved)
        {
            if (string.IsNullOrEmpty(saved.packedInputs)) return saved;
            var restored = new BattleResumeData
            {
                initialRunJson = saved.initialRunJson, seed = saved.seed,
                enemy = saved.enemy, monsterId = saved.monsterId
            };
            using (var buffer = new MemoryStream(Convert.FromBase64String(saved.packedInputs)))
            using (var zip = new GZipStream(buffer, CompressionMode.Decompress))
            using (var reader = new BinaryReader(zip))
            {
                int count = reader.ReadInt32();
                if (count < 0 || count > 10000000) throw new InvalidDataException("Invalid battle record length.");
                restored.inputs = new List<BattleInputRecord>(count);
                for (int i = 0; i < count; i++)
                {
                    int action = reader.ReadByte();
                    if (action > 4) throw new InvalidDataException("Invalid battle action.");
                    restored.inputs.Add(new BattleInputRecord
                    {
                        action = action, delta = action == 0 ? reader.ReadSingle() : 0,
                        itemId = action == 4 ? reader.ReadString() : null
                    });
                }
            }
            return restored;
        }
    }
}

using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Attributes;

namespace WTA.Shared.GuidGenerators;

[Implement<IGuidGenerator>(ServiceLifetime.Singleton)]
public class GuidGenerator : IGuidGenerator
{
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
    private readonly SequentialGuidType _guidType;

    public GuidGenerator(IConfiguration cfg)
    {
        this._guidType = cfg.GetValue("SequentialGuidType", SequentialGuidType.SequentialAsString);
    }

    public Guid Create()
    {
        var randomBytes = new byte[10];
        _rng.GetBytes(randomBytes);

        var timestamp = DateTime.UtcNow.Ticks / 10000L;
        var timestampBytes = BitConverter.GetBytes(timestamp);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timestampBytes);
        }

        var guidBytes = new byte[16];

        switch (this._guidType)
        {
            case SequentialGuidType.SequentialAsString:
            case SequentialGuidType.SequentialAsBinary:
                Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);
                Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);

                // If formatting as a string, we have to reverse the order
                // of the Data1 and Data2 blocks on little-endian systems.
                if (this._guidType == SequentialGuidType.SequentialAsString && BitConverter.IsLittleEndian)
                {
                    Array.Reverse(guidBytes, 0, 4);
                    Array.Reverse(guidBytes, 4, 2);
                }
                break;

            case SequentialGuidType.SequentialAtEnd:
                Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
                Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
                break;
        }

        return new Guid(guidBytes);
    }
}

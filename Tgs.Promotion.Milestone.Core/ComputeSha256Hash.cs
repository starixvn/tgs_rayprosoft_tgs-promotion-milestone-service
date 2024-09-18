using System.Security.Cryptography;
using System.Text;

namespace Tgs.Promotion.Milestone.Core;

public static class ComputeSha256Hash
{
    public static long ComputeSha256HashToLong(string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

        var hashLong = BitConverter.ToInt64(bytes, 0);
        return hashLong;
    }
}
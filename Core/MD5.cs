using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyward.Skygrate.Core
{
    public class MD5
    {
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes);
            }
        }

        public static string CreateMD5(IEnumerable<string> inputLines, string seed)
        {
            var lines = inputLines.ToList();

            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                int offset = 0;
                for(var i = 0; i < lines.Count; ++i)
                {
                    var input = lines[i];
                    byte[] block = Encoding.UTF8.GetBytes(input);
                    offset += md5.TransformBlock(block, 0, block.Length, null, 0);
                }
                var finalBlock = Encoding.UTF8.GetBytes(seed);
                md5.TransformFinalBlock(finalBlock, 0, finalBlock.Length);
                var hashBytes = md5.Hash;
                return Convert.ToHexString(hashBytes);
            }
        }
    }
}

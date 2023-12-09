using System;
using System.IO;
using ComponentAce.Compression.Libs.zlib;

namespace Aki.Common.Utils
{
	public enum ZlibCompression
	{
		Store = 0,
		Fastest = 1,
		Fast = 3,
		Normal = 5,
		Ultra = 7,
		Maximum = 9
	}

	public static class Zlib
	{
		// Level | CM/CI FLG
		// ----- | ---------
		// 1     | 78 01
		// 2     | 78 5E
		// 3     | 78 5E
		// 4     | 78 5E
		// 5     | 78 5E
		// 6     | 78 9C
		// 7     | 78 DA
		// 8     | 78 DA
		// 9     | 78 DA

		/// <summary>
		/// Check if the file is ZLib compressed
		/// </summary>
		/// <param name="Data">Data</param>
		/// <returns>If the file is Zlib compressed</returns>
		public static bool IsCompressed(byte[] Data)
		{
			// We need the first two bytes;
			// First byte:  Info (CM/CINFO) Header, should always be 0x78
			// Second byte: Flags (FLG) Header, should define our compression level.

			if (Data == null || Data.Length < 3 || Data[0] != 0x78)
			{
				return false;
			}

			switch (Data[1])
			{
				case 0x01:  // fastest
				case 0x5E:  // low
				case 0x9C:  // normal
				case 0xDA:  // max
					return true;
			}

			return false;
		}

		private static byte[] Run(byte[] data, ZlibCompression level)
		{
		    // ZOutputStream.Close() flushes itself.
            // ZOutputStream.Flush() flushes the target stream.
            // It's fucking stupid, but whatever.
            // -- Waffle.Lord, 2022-12-01

			using (var ms = new MemoryStream())
            {
				using (var zs = (level > ZlibCompression.Store)
					? new ZOutputStream(ms, (int)level)
					: new ZOutputStream(ms))
				{
					zs.Write(data, 0, data.Length);
				}
				// <-- zs flushes everything here

				return ms.ToArray();
			}
		}

		/// <summary>
		/// Deflate data.
		/// </summary>
		public static byte[] Compress(byte[] data, ZlibCompression level)
		{
			return Run(data, level);
		}

        /// <summary>
        /// Inflate data.
        /// </summary>
        public static byte[] Decompress(byte[] data)
		{
			return Run(data, ZlibCompression.Store);
		}
	}
}
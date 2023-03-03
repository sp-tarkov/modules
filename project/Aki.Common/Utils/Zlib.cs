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

		/// <summary>
		/// Deflate data.
		/// </summary>
		public static byte[] Compress(byte[] data, ZlibCompression level)
		{
			byte[] buffer = new byte[data.Length + 24];

			ZStream zs = new ZStream()
			{
				avail_in = data.Length,
				next_in = data,
				next_in_index = 0,
				avail_out = buffer.Length,
				next_out = buffer,
				next_out_index = 0
			};

			zs.deflateInit((int)level);
			zs.deflate(zlibConst.Z_FINISH);

			data = new byte[zs.next_out_index];
			Array.Copy(zs.next_out, 0, data, 0, zs.next_out_index);

			return data;
		}

        /// <summary>
        /// Inflate data.
        /// </summary>
        public static byte[] Decompress(byte[] data)
		{
			byte[] buffer = new byte[4096];

			ZStream zs = new ZStream()
			{
				avail_in = data.Length,
				next_in = data,
				next_in_index = 0,
				avail_out = buffer.Length,
				next_out = buffer,
				next_out_index = 0
			};

			zs.inflateInit();

			using (MemoryStream ms = new MemoryStream())
			{
				do
				{
					zs.avail_out = buffer.Length;
					zs.next_out = buffer;
					zs.next_out_index = 0;

					int result = zs.inflate(0);

					if (result != 0 && result != 1)
					{
						break;
					}

					ms.Write(zs.next_out, 0, zs.next_out_index);
				}
				while (zs.avail_in > 0 || zs.avail_out == 0);

				return ms.ToArray();
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	public class LargeScaleQueue : IDisposable
	{
		public static long TEMPORARY_FILE_SIZE_MAX = 1000000000L; // 1 GB

		private string BufferDir;
		private List<string> BufferFiles = new List<string>();
		private long ReadPosition = 0L;

		public long Count { private set; get; } = 0L;

		public LargeScaleQueue(string temporaryDir)
		{
			if (
				string.IsNullOrEmpty(temporaryDir) ||
				!Directory.Exists(temporaryDir)
				)
				throw new Exception("Bad temporaryDir");

			this.BufferDir = temporaryDir;
		}

		private string MakeBufferFilePath()
		{
			return Path.Combine(this.BufferDir, SCommon.GetCUID());
		}

		public void Enqueue(byte[] element)
		{
			if (element == null)
				throw new Exception("Bad element");

			if (
				this.BufferFiles.Count == 0 ||
				new FileInfo(this.BufferFiles.Last()).Length >= TEMPORARY_FILE_SIZE_MAX
				)
				this.BufferFiles.Add(this.MakeBufferFilePath());

			using (FileStream writer = new FileStream(this.BufferFiles.Last(), FileMode.Append, FileAccess.Write))
			{
				SCommon.WritePart(writer, element);
			}
			this.Count++;
		}

		public byte[] Dequeue()
		{
			if (this.Count == 0L)
				throw new Exception("no elements");

			byte[] element;
			bool eofReached = false;

			using (FileStream reader = new FileStream(this.BufferFiles[0], FileMode.Open, FileAccess.Read))
			{
				reader.Position = this.ReadPosition;

				element = SCommon.ReadPart(reader);
				eofReached = reader.Length <= reader.Position;

				this.ReadPosition = reader.Position;
			}
			if (eofReached)
			{
				SCommon.DeletePath(this.BufferFiles[0]);

				this.BufferFiles.RemoveAt(0);
				this.ReadPosition = 0L;
			}
			this.Count--;
			return element;
		}

		public void Dispose()
		{
			foreach (string file in this.BufferFiles)
				SCommon.DeletePath(file);

			this.BufferFiles.Clear();
			this.ReadPosition = 0L;
			this.Count = 0L;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	public class LargeScaleStringBuilder
	{
		public static long BUFFER_FILE_SIZE_MAX = 30000000L; // 30 MB
		public static Encoding BUFFER_FILE_ENCODING = Encoding.UTF8;

		private string TemporaryDir;
		private List<string> BufferFiles = new List<string>();

		public LargeScaleStringBuilder()
			: this(WorkingDir.GetCommonDir())
		{ }

		public LargeScaleStringBuilder(string tempraryDir)
		{
			this.TemporaryDir = tempraryDir;
		}

		public LargeScaleStringBuilder Append(string value)
		{
			if (
				this.BufferFiles.Count == 0 ||
				new FileInfo(this.BufferFiles.Last()).Length >= BUFFER_FILE_SIZE_MAX
				)
				this.BufferFiles.Add(this.MakeBufferFilePath());

			using (FileStream writer = new FileStream(this.BufferFiles.Last(), FileMode.Append, FileAccess.Write))
			{
				SCommon.Write(writer.Write, BUFFER_FILE_ENCODING.GetBytes(value));
			}
			return this;
		}

		private string MakeBufferFilePath()
		{
			return Path.Combine(this.TemporaryDir, SCommon.GetCUID());
		}

		public IEnumerable<string> E_GetString()
		{
			foreach (string file in this.BufferFiles)
			{
				yield return File.ReadAllText(file, BUFFER_FILE_ENCODING);
			}
		}

		public string GetString()
		{
			return string.Join("", this.E_GetString());
		}

		public override string ToString()
		{
			return $"LSSB/{this.BufferFiles.Count}/{BUFFER_FILE_SIZE_MAX}/{this.BufferFiles.Count * BUFFER_FILE_SIZE_MAX}/{BUFFER_FILE_ENCODING}";
		}
	}
}

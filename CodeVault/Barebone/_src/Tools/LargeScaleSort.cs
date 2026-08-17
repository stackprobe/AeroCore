using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	public static class LargeScaleSort
	{
		/// <summary>
		/// メモリにロード可能な総データサイズの上限
		/// </summary>
		public static int MEMORY_ELEMENT_TOTAL_SIZE_MAX = 50000000; // 50 MB

		/// <summary>
		/// メモリにロード可能な総データ数の上限
		/// </summary>
		public static int MEMORY_ELEMENT_COUNT_MAX = 1000000; // 1 M

		/// <summary>
		/// K-ウェイマージの K の値
		/// </summary>
		public static int MERGE_WAY_K = 256;

		/// <summary>
		/// 巨大データのソートを行う。
		/// </summary>
		/// <param name="reader">1レコードを読み込む(終端に達した場合 null を返すこと)</param>
		/// <param name="writer">1レコードを書き出す</param>
		/// <param name="temporaryDir">一時ディレクトリ</param>
		/// <param name="comp">比較メソッド</param>
		public static void Run(Func<byte[]> reader, Action<byte[]> writer, string temporaryDir, Comparison<byte[]> comp)
		{
			if (
				reader == null ||
				writer == null ||
				string.IsNullOrEmpty(temporaryDir) ||
				!Directory.Exists(temporaryDir) ||
				comp == null
				)
				throw new Exception("不正な引数");

			Queue<string> q = new Queue<string>();

			{
				List<byte[]> buff = new List<byte[]>();
				int totalSize = 0;

				Action flushBuff = () =>
				{
					buff.Sort(comp);

					string file = MakeFilePath(temporaryDir);
					WriteFile(file, buff);
					q.Enqueue(file);
				};

				for (; ; )
				{
					byte[] element = reader();

					if (element == null) // ? 読み込み終了
						break;

					buff.Add(element);
					totalSize += element.Length;

					if (MEMORY_ELEMENT_TOTAL_SIZE_MAX < totalSize || MEMORY_ELEMENT_COUNT_MAX < buff.Count)
					{
						flushBuff();

						buff.Clear();
						totalSize = 0;
					}
				}
				if (1 <= buff.Count)
				{
					flushBuff();
				}
			}

			while (MERGE_WAY_K < q.Count)
			{
				string file = MakeFilePath(temporaryDir);
				MergeToFile(q, MERGE_WAY_K, comp, file);
				q.Enqueue(file);
			}

			Merge(q, q.Count, comp, writer);
		}

		private static string MakeFilePath(string dir)
		{
			return Path.Combine(dir, SCommon.GetCUID());
		}

		private static void WriteFile(string file, IEnumerable<byte[]> elements)
		{
			using (FileStream writer = new FileStream(file, FileMode.Create, FileAccess.Write))
			{
				foreach (byte[] element in elements)
					SCommon.WritePart(writer, element);
			}
		}

		private static void MergeToFile(Queue<string> q, int k, Comparison<byte[]> comp, string file)
		{
			using (FileStream writer = new FileStream(file, FileMode.Create, FileAccess.Write))
			{
				Merge(q, k, comp, element => SCommon.WritePart(writer, element));
			}
		}

		private class Reader_t
		{
			public FileStream Reader;
			public byte[] Element;

			public bool Read()
			{
				if (this.Reader.Position < this.Reader.Length)
				{
					this.Element = SCommon.ReadPart(this.Reader);
					return true;
				}
				else
				{
					this.Element = null;
					return false;
				}
			}
		}

		private static void Merge(Queue<string> q, int k, Comparison<byte[]> comp, Action<byte[]> writer) // k: 0 ～ MERGE_WAY_K
		{
			string[] files = new string[k];

			for (int index = 0; index < k; index++)
				files[index] = q.Dequeue();

			FileStream[] rStreams = new FileStream[k];
			try
			{
				for (int index = 0; index < k; index++)
					rStreams[index] = new FileStream(files[index], FileMode.Open, FileAccess.Read);

				List<Reader_t> readers = rStreams
					.Select(reader => new Reader_t() { Reader = reader })
					.Where(reader => reader.Read())
					.OrderBy((a, b) => comp(a.Element, b.Element))
					.ToList();

				while (1 <= readers.Count)
				{
					writer(readers[0].Element);

					if (readers[0].Read())
						Reorder(readers, comp);
					else
						readers.RemoveAt(0);
				}
			}
			finally
			{
				foreach (FileStream rStream in rStreams)
					if (rStream != null)
						rStream.Dispose();
			}

			foreach (string file in files)
				SCommon.DeletePath(file);
		}

		private static void Reorder(List<Reader_t> readers, Comparison<byte[]> comp)
		{
			for (int index = 0; index + 1 < readers.Count; index++)
			{
				if (comp(readers[index].Element, readers[index + 1].Element) <= 0)
					break;

				SCommon.Swap(readers, index, index + 1);
			}
		}
	}
}

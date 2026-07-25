using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	// クラスタ書式：
	// -- https://github.com/stackprobe/notehub/blob/main/DevSourceRef/DirToCluster_Format_Specification.txt

	/// <summary>
	/// 指定ディレクトリを圧縮して単一ファイルにまとめる機能と、
	/// 圧縮ファイルを展開する機能を提供する。
	/// 圧縮形式(圧縮ファイルの内容)は apps/Compress と互換性がある。
	/// 圧縮ファイルの推奨拡張子 = .cmp-gz
	/// </summary>
	public static class DirToClusterFileTools
	{
		public static void DirToClusterFile(string rDir, string wFile)
		{
			rDir = SCommon.MakeFullPath(rDir);
			wFile = SCommon.MakeFullPath(wFile);

			if (!Directory.Exists(rDir))
				throw new Exception("no rDir");

			if (Directory.Exists(wFile))
				throw new Exception("Bad wFile");

			ProcMain.WriteLog("DirToClusterFile-ST");
			ProcMain.WriteLog("< " + rDir);
			ProcMain.WriteLog("> " + wFile);

			using (FileStream fileWriter = new FileStream(wFile, FileMode.Create, FileAccess.Write))
			using (GZipStream gz = new GZipStream(fileWriter, CompressionMode.Compress, true))
			{
				Writer = gz;

				foreach (string dir in Directory.GetDirectories(rDir, "*", SearchOption.AllDirectories))
				{
					ProcMain.WriteLog("< " + dir);

					string relPath = SCommon.EraseRoot(dir, rDir);

					WriteString("D"); // Directory
					WriteString(relPath);
				}
				foreach (string file in Directory.GetFiles(rDir, "*", SearchOption.AllDirectories))
				{
					ProcMain.WriteLog("< " + file);

					string relPath = SCommon.EraseRoot(file, rDir);

					WriteString("F"); // File
					WriteString(relPath);

					FileInfo fileInfo = new FileInfo(file);

					WriteString(new SimpleDateTime(fileInfo.CreationTime).ToTimeStamp().ToString());
					WriteString(new SimpleDateTime(fileInfo.LastWriteTime).ToTimeStamp().ToString());
					WriteString(new SimpleDateTime(fileInfo.LastAccessTime).ToTimeStamp().ToString());
					WriteString(fileInfo.Length.ToString());

					using (FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read))
					{
						SCommon.ReadToEnd(reader.Read, Writer.Write);
					}
				}
				WriteString("E"); // End
				Writer = null;
			}
			ProcMain.WriteLog("DirToClusterFile-ED");
		}

		private static Stream Writer;

		private static void WriteString(string str)
		{
			byte[] bStr = Encoding.UTF8.GetBytes(str);

			Write(SCommon.UIntToBytes((uint)bStr.Length));
			Write(bStr);
		}

		private static void Write(byte[] data)
		{
			SCommon.Write(Writer, data);
		}

		public static void ClusterFileToDir(string rFile, string wDir)
		{
			rFile = SCommon.MakeFullPath(rFile);
			wDir = SCommon.MakeFullPath(wDir);

			ProcMain.WriteLog("ClusterFileToDir-ST");
			ProcMain.WriteLog("< " + rFile);
			ProcMain.WriteLog("> " + wDir);

			if (!File.Exists(rFile))
				throw new Exception("no rFile");

			if (File.Exists(wDir))
				throw new Exception("Bad wDir");

			SCommon.CreateDir(wDir);

			using (FileStream fileReader = new FileStream(rFile, FileMode.Open, FileAccess.Read))
			using (GZipStream gz = new GZipStream(fileReader, CompressionMode.Decompress, true))
			{
				Reader = gz;

				for (; ; )
				{
					string label = ReadString();

					if (label == "D") // Directory
					{
						string relPath = ReadString();
						CheckRelPath(relPath);
						string dir = Path.Combine(wDir, relPath);

						ProcMain.WriteLog("> " + dir);

						SCommon.CreateDir(dir);
					}
					else if (label == "F") // File
					{
						string relPath = ReadString();
						CheckRelPath(relPath);
						string file = Path.Combine(wDir, relPath);

						ProcMain.WriteLog("> " + file);

						SimpleDateTime creationTime = SimpleDateTime.FromTimeStamp(long.Parse(ReadString()));
						SimpleDateTime lastWriteTime = SimpleDateTime.FromTimeStamp(long.Parse(ReadString()));
						SimpleDateTime lastAccessTime = SimpleDateTime.FromTimeStamp(long.Parse(ReadString()));
						long fileSize = long.Parse(ReadString());

						ProcMain.WriteLog(creationTime);
						ProcMain.WriteLog(lastWriteTime);
						ProcMain.WriteLog(lastAccessTime);
						ProcMain.WriteLog(fileSize);

						if (fileSize < 0)
							throw new Exception("Bad fileSize: " + fileSize);

						using (FileStream writer = new FileStream(file, FileMode.Create, FileAccess.Write))
						{
							for (long count = 0L; count < fileSize;)
							{
								const int READ_SIZE_MAX = 2000000; // 2 MB

								int size = (int)Math.Min((long)READ_SIZE_MAX, fileSize - count);
								SCommon.Write(writer, SCommon.Read(Reader, size));
								count += size;
							}
						}

						{
							FileInfo fileInfo = new FileInfo(file);

							fileInfo.CreationTime = creationTime.ToDateTime();
							fileInfo.LastWriteTime = lastWriteTime.ToDateTime();
							fileInfo.LastAccessTime = lastAccessTime.ToDateTime();
						}
					}
					else if (label == "E") // End
					{
						break;
					}
					else
					{
						throw new Exception("不明なラベル");
					}
				}
				Reader = null;
			}
			ProcMain.WriteLog("ClusterFileToDir-ED");
		}

		private static Stream Reader;

		private static string ReadString()
		{
			int size = (int)SCommon.ToUInt(Read(4));

			if (size < 0 || SCommon.IMAX < size) // rough limit
				throw new Exception("Bad size: " + size);

			byte[] bStr = Read(size);
			string str = Encoding.UTF8.GetString(bStr);
			return str;
		}

		private static byte[] Read(int size)
		{
			return SCommon.Read(Reader, size);
		}

		private static void CheckRelPath(string relPath)
		{
			string[] pTkns = SCommon.Tokenize(relPath, "\\/");

			foreach (string pTkn in pTkns)
			{
				if (
					pTkn == "" ||
					pTkn == "." ||
					pTkn == ".." ||
					pTkn.Contains(':')
					)
					throw new Exception("Bad pTkn");
			}
		}
	}
}

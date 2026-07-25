using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;

namespace HLTStudio.Tools
{
	// memo: プロジェクト右クリック -> NuGet パッケージの監理 -> 参照 -> System.Data.SQLite 1.0.117
	//
	// バージョンに関するメモ @ 2025.11.19
	// --
	// 2.0.2 -- 自力でネイティブ部分(sqlite3.dll)をダウンロードしてコピーする方式に変更された。
	// 2.0.1 -- 同上
	// 1.0.119 -- クリティカルなバグあり
	// 1.0.118 -- 同上
	// 1.0.117 -- 消去法でこれ！

	public static class SQLiteTools
	{
		public static string GetConnectionString(string dbFile)
		{
			return $"Data Source=\"{dbFile}\"; Version=3;";
		}

		public static void Connection(string dbFile, Action<SQLiteConnection, SQLiteCommand> routine)
		{
			using (SQLiteConnection conn = new SQLiteConnection(GetConnectionString(dbFile)))
			{
				conn.Open();

				using (SQLiteCommand cmd = new SQLiteCommand(conn))
				{
					routine(conn, cmd);
				}
			}
		}

		public static void Transaction(string dbFile, Action<SQLiteConnection, SQLiteCommand> routine)
		{
			Connection(dbFile, (conn, cmd) =>
			{
				using (var tran = conn.BeginTransaction())
				{
					cmd.Transaction = tran;
					try
					{
						routine(conn, cmd);
						tran.Commit();
					}
					catch
					{
						tran.Rollback();
						throw;
					}
				}
			});
		}

#if false // 不使用
		public static void ExecuteReader(string dbFile, string query, Action<SQLiteDataReader> routine)
		{
			ExecuteReader(dbFile, query, new Parameter_t[0], routine);
		}
#endif

		public static void ExecuteReader(string dbFile, string query, Parameter_t[] parameters, Action<SQLiteDataReader> routine)
		{
			Connection(dbFile, (conn, cmd) =>
			{
				cmd.CommandText = query;
				cmd.Parameters.Clear();

				AddParameters(cmd, parameters);

				using (SQLiteDataReader reader = cmd.ExecuteReader())
				{
					routine(reader);
				}

				// memo:
				// 上記処理を繰り返し書けば複数のクエリを同一コネクション内で実行できる。
				// cmd.ExecuteNonQuery を実行するには Transaction を使うこと！
			});
		}

		public static string[][] GetRows(string dbFile, string query)
		{
			return GetRows(dbFile, query, new Parameter_t[0]);
		}

		public static string[][] GetRows(string dbFile, string query, Parameter_t[] parameters)
		{
			List<string[]> rows = new List<string[]>();

			ExecuteReader(dbFile, query, parameters, reader =>
			{
				while (reader.Read())
				{
					string[] row = new string[reader.FieldCount];

					for (int index = 0; index < reader.FieldCount; index++)
						row[index] = P_GetCellValueString(reader, index);

					rows.Add(row);
				}
			});

			return rows.ToArray();
		}

		private static string P_GetCellValueString(SQLiteDataReader reader, int index)
		{
			if (reader.IsDBNull(index))
				return "";

			object value = reader.GetValue(index);

			if (value == null)
				throw null; // never

			switch (value)
			{
				case long longValue:
					return P_GetLongValueString(longValue);

				case int intValue:
					return P_GetLongValueString((long)intValue);

				case short shortValue:
					return P_GetLongValueString((long)shortValue);

				case sbyte sbyteValue:
					return P_GetLongValueString((long)sbyteValue);

				case byte byteValue:
					return P_GetLongValueString((long)byteValue);

				case bool boolValue:
					return P_GetLongValueString(boolValue ? 1L : 0L);

				case double doubleValue:
					return P_GetDoubleValueString(doubleValue);

				case float floatValue:
					return P_GetDoubleValueString((double)floatValue);

				case decimal decimalValue:
					//return P_GetDoubleValueString((double)decimalValue);
					return P_GetStringValueString(decimalValue.ToString());

				case string stringValue:
					return P_GetStringValueString(stringValue);

				case byte[] byteArrayValue:
					return P_GetByteArrayValueString(byteArrayValue);

				default:
					//throw new Exception("不明な列タイプ");
					return P_GetStringValueString(value.ToString());
			}
		}

		private static string P_GetLongValueString(long value)
		{
			return value.ToString();
		}

		private static string P_GetDoubleValueString(double value)
		{
			return value.ToString("F16");
		}

		private static string P_GetStringValueString(string value)
		{
			return value;
		}

		private static string P_GetByteArrayValueString(byte[] value)
		{
			return SCommon.Hex.I.GetString(value);
			//return SCommon.Base64.I.Encode(value);
		}

		public static void ExecuteNonQuery(string dbFile, string query)
		{
			ExecuteNonQuery(dbFile, query, new Parameter_t[0]);
		}

		public static void ExecuteNonQuery(string dbFile, string query, Parameter_t[] parameters)
		{
			Transaction(dbFile, (conn, cmd) =>
			{
				cmd.CommandText = query;
				cmd.Parameters.Clear();

				AddParameters(cmd, parameters);

				cmd.ExecuteNonQuery();

				// memo:
				// 上記処理を繰り返し書けば複数のクエリを同一トランザクション内で実行できる。
				// cmd.ExecuteReader も実行可能！
			});
		}

		public class Parameter_t
		{
			public string Name;
			public DbType ValueType;
			public object Value;

			public Parameter_t(string name, DbType valueType, object value)
			{
				this.Name = name;
				this.ValueType = valueType;
				this.Value = value;
			}
		}

		private static void AddParameters(SQLiteCommand cmd, Parameter_t[] parameters)
		{
			foreach (Parameter_t parameter in parameters)
			{
				//cmd.Parameters.AddWithValue(parameter.Name, parameter.Value);
				cmd.Parameters.Add(parameter.Name, parameter.ValueType).Value = parameter.Value;
			}
		}
	}
}

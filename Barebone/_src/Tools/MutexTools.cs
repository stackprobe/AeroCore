using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HLTStudio.Tools
{
	public static class MutexTools
	{
		public static Mutex Create(string name, bool global)
		{
			if (global)
				return CreateGlobal(name);
			else
				return CreateLocal(name);
		}

		public static Mutex CreateLocal(string name)
		{
			return new Mutex(false, name);
		}

		public static Mutex CreateGlobal(string name)
		{
			MutexSecurity security = new MutexSecurity();

			security.AddAccessRule(
				new MutexAccessRule(
					new SecurityIdentifier(
						WellKnownSidType.WorldSid,
						null
						),
					MutexRights.FullControl,
					AccessControlType.Allow
					)
				);

			return new Mutex(false, $"Global\\{name}", out _, security);
		}

		public static void Section(string name, bool global, Action routine)
		{
			if (!Section(name, global, Timeout.Infinite, routine))
			{
				throw null; // never
			}
		}

		public static bool Section(string name, bool global, int millis, Action routine)
		{
			using (Mutex mutex = Create(name, global))
			{
				if (mutex.WaitOne(millis))
				{
					try
					{
						routine();
					}
					finally
					{
						mutex.ReleaseMutex();
					}
					return true;
				}
				return false;
			}
		}
	}
}

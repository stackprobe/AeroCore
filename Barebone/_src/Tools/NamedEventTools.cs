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
	public static class NamedEventTools
	{
		public static EventWaitHandle Create(string name, bool autoReset, bool global)
		{
			if (global)
				return CreateGlobal(name, autoReset);
			else
				return CreateLocal(name, autoReset);
		}

		public static EventWaitHandle CreateLocal(string name, bool autoReset)
		{
			return new EventWaitHandle(false, GetResetMode(autoReset), name);
		}

		public static EventWaitHandle CreateGlobal(string name, bool autoReset)
		{
			EventWaitHandleSecurity security = new EventWaitHandleSecurity();

			security.AddAccessRule(
				new EventWaitHandleAccessRule(
					new SecurityIdentifier(
						WellKnownSidType.WorldSid,
						null
						),
					EventWaitHandleRights.FullControl,
					AccessControlType.Allow
					)
				);

			return new EventWaitHandle(false, GetResetMode(autoReset), $"Global\\{name}", out _, security);
		}

		private static EventResetMode GetResetMode(bool autoReset)
		{
			return autoReset ? EventResetMode.AutoReset : EventResetMode.ManualReset;
		}

		public static void Set(string name, bool autoReset, bool global)
		{
			using (EventWaitHandle namedEvent = Create(name, autoReset, global))
			{
				namedEvent.Set();
			}
		}

		public static void Reset(string name, bool autoReset, bool global)
		{
			using (EventWaitHandle namedEvent = Create(name, autoReset, global))
			{
				namedEvent.Reset();
			}
		}

		public static void Wait(string name, bool autoReset, bool global)
		{
			if (!Wait(name, autoReset, global, Timeout.Infinite))
			{
				throw null; // never
			}
		}

		public static bool Wait(string name, bool autoReset, bool global, int millis)
		{
			using (EventWaitHandle namedEvent = Create(name, autoReset, global))
			{
				return namedEvent.WaitOne(millis);
			}
		}
	}
}

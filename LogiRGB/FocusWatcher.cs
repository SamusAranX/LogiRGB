using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiRGB {
	class FocusWatcher {

		WinApi.WinEventDelegate dele = null;
		public FocusWatcher() {
			dele = new WinApi.WinEventDelegate(WinEventProc);

			IntPtr m_hhook = WinApi.SetWinEventHook(WinApi.EVENT_SYSTEM_FOREGROUND, WinApi.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WinApi.WINEVENT_OUTOFCONTEXT);
		}

		public void StartWatching() {
			IntPtr m_hhook = WinApi.SetWinEventHook(WinApi.EVENT_SYSTEM_FOREGROUND, WinApi.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WinApi.WINEVENT_OUTOFCONTEXT);
		}



		public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
			var windowHandle = WinApi.GetForegroundWindow();

			uint processID;
			WinApi.GetWindowThreadProcessId(windowHandle, out processID);
			Debug.WriteLine(Process.GetProcessById((int)processID).MainModule.FileName);
		}
	}
}

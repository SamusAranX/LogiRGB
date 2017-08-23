using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LogiRGB {
	public static class WinApi {
		[DllImport("user32.dll")]
		public static extern void GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

		public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

		[DllImport("user32.dll")]
		public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

		[DllImport("user32.dll")]
		public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

		public const uint WINEVENT_OUTOFCONTEXT = 0;
		public const uint WINEVENT_SKIPOWNTHREAD = 1;
		public const uint WINEVENT_SKIPOWNPROCESS = 2;
		public const uint EVENT_SYSTEM_FOREGROUND = 3;

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		// stuff for icon extraction

		public const int GCL_HICONSM = -34;
		public const int GCL_HICON = -14;

		public const int ICON_SMALL = 0;
		public const int ICON_BIG = 1;
		public const int ICON_SMALL2 = 2;

		public const int WM_GETICON = 0x7F;

		[DllImport("user32.dll")]
		public static extern IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
	}
}

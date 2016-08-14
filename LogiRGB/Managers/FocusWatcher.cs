using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogiRGB.Managers {
	/// <summary>
	/// Waits for window focus changes and notifies all event subscribers
	/// Excludes the 
	/// </summary>
	public class FocusWatcher {

		WinApi.WinEventDelegate dele = null;
		IntPtr eventHook;

		// "System Idle Process" and "System" processes
		// When quickly minimizing and restoring windows from the taskbar, Windows sometimes brings these into focus
		// So we'll filter those out to avoid crashes
		uint[] forbiddenProcIDs = { 0, 4, 8 };

		public FocusWatcher() {
			dele = new WinApi.WinEventDelegate(WinEventProc);
		}

		public void StartWatching() {
			eventHook = WinApi.SetWinEventHook(WinApi.EVENT_SYSTEM_FOREGROUND, WinApi.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WinApi.WINEVENT_OUTOFCONTEXT);
		}

		// Not really necessary because Windows automatically cleans up after us, but what the heck
		public void StopWatching() {
			WinApi.UnhookWinEvent(eventHook);
		}

		public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
			var windowHandle = WinApi.GetForegroundWindow();

			uint processID;
			WinApi.GetWindowThreadProcessId(windowHandle, out processID);
			Debug.WriteLine($"Process ID: {processID}");

			if (forbiddenProcIDs.Contains(processID))
				return;

			try {
				string filename = Process.GetProcessById((int)processID).MainModule.FileName;

				OnFocusChanged((int)processID, windowHandle, filename);
			} catch (Exception ex) {
				Debug.WriteLine(ex.Message);
			}
		}

		protected virtual void OnFocusChanged(int processID, IntPtr windowHandle, string filename) {
			var handler = FocusChanged;
			if (handler == null)
				return;

			Debug.WriteLine($"FocusWatcher: {filename}");

			if (filename == Assembly.GetExecutingAssembly().Location || filename.EndsWith("LogiRGB.vshost.exe"))
				return; // Exclude LogiRGB

			var eventArgs = new FocusChangedEventArgs(processID, windowHandle, filename);
			handler(this, eventArgs);
		}

		public event EventHandler<FocusChangedEventArgs> FocusChanged;
	}

	public class FocusChangedEventArgs : EventArgs {
		private readonly int _processID;
		private readonly IntPtr _windowHandle;
		private readonly string _filename;

		public FocusChangedEventArgs(int processID, IntPtr windowHandle, string filename) {
			_processID = processID;
			_windowHandle = windowHandle;
			_filename = filename;
		}

		public int ProcessID {
			get {
				return _processID;
			}
		}

		public IntPtr WindowHandle {
			get {
				return _windowHandle;
			}
		}

		public string Filename {
			get {
				return _filename;
			}
		}
	}
}

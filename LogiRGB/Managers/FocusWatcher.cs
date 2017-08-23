using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		public struct ProcessInfo {
			public uint ProcessID;
			public string ProcessName;

		}

		WinApi.WinEventDelegate dele = null;
		IntPtr eventHook;

		// "System Idle Process" and "System" processes
		// When quickly minimizing and restoring windows from the taskbar, Windows sometimes brings these into focus
		// So we'll filter those out to avoid crashes
		uint[] forbiddenProcIDs = { 0, 4, 8 };

		public FocusWatcher() {
			this.dele = new WinApi.WinEventDelegate(WinEventProc);
		}

		public void StartWatching() {
			this.eventHook = WinApi.SetWinEventHook(WinApi.EVENT_SYSTEM_FOREGROUND, WinApi.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, this.dele, 0, 0, WinApi.WINEVENT_OUTOFCONTEXT | WinApi.WINEVENT_SKIPOWNPROCESS);
		}

		// Not really necessary because Windows automatically cleans up after us, but what the heck
		public void StopWatching() {
			WinApi.UnhookWinEvent(this.eventHook);
		}

		public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
			var windowHandle = WinApi.GetForegroundWindow();

			WinApi.GetWindowThreadProcessId(windowHandle, out uint processID);
			Debug.WriteLine($"Process ID: {processID}");

			if (this.forbiddenProcIDs.Contains(processID))
				return;

			try {
				var process = Process.GetProcessById((int)processID).MainModule;
				string filename = process.FileName;

				OnFocusChanged((int)processID, windowHandle, filename);
			} catch (Win32Exception ex) {
				Debug.WriteLine($"FocusWatcher: {ex.Message}");
				Debug.WriteLine($"FocusWatcher: Native Error Code: {ex.NativeErrorCode}");
			} catch (Exception ex) {
				Debug.WriteLine(ex.Message);
			}
		}

		protected virtual void OnFocusChanged(int processID, IntPtr windowHandle, string filename) {
			var handler = FocusChanged;
			if (handler == null)
				return;

			Debug.WriteLine($"FocusWatcher: {filename}");

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
			this._processID = processID;
			this._windowHandle = windowHandle;
			this._filename = filename;
		}

		public int ProcessID {
			get {
				return this._processID;
			}
		}

		public IntPtr WindowHandle {
			get {
				return this._windowHandle;
			}
		}

		public string Filename {
			get {
				return this._filename;
			}
		}
	}
}

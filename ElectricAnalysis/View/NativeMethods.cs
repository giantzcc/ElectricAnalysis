using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MIV.Bus.WPF.UIShell
{
	internal static class NativeMethods
	{
		public const int WM_ACTIVATE = 0x0006;
		public const int WM_SETCURSOR = 0x20;
		public const int WM_NCHITTEST = 0x0084;

		//user32

		[DllImport( "user32.dll", CharSet = CharSet.Auto, ExactSpelling = true )]
		public static extern IntPtr GetActiveWindow();

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "0" ), DllImport( "user32.dll" )]
		public static extern IntPtr WindowFromPoint( POINT Point );

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool GetCursorPos( out POINT lpPoint );


		[DllImport( "user32.dll", CharSet = CharSet.Auto )]
		public static extern IntPtr SendMessage( IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam );

		[DllImport( "user32.dll" )]
		public static extern bool ReleaseCapture();

		[StructLayout( LayoutKind.Sequential )]
		public struct POINT
		{
			public int X;
			public int Y;
		}

		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;



		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		internal static extern bool FlashWindowEx( ref FLASHWINFO pwfi );

		[StructLayout( LayoutKind.Sequential )]
		public struct FLASHWINFO
		{
			public UInt32 cbSize;
			public IntPtr hwnd;
			public UInt32 dwFlags;
			public UInt32 uCount;
			public UInt32 dwTimeout;
		}
		public const UInt32 FLASHW_STOP = 0;
		public const UInt32 FLASHW_CAPTION = 1;
		public const UInt32 FLASHW_TRAY = 2;
		public const UInt32 FLASHW_ALL = 3;
		public const UInt32 FLASHW_TIMER = 4;
		public const UInt32 FLASHW_TIMERNOFG = 12;

		// Sent to a window when the size or position of the window is about to change
		public const int WM_GETMINMAXINFO = 0x0024;

		// Retrieves a handle to the display monitor that is nearest to the window
		public const int MONITOR_DEFAULTTONEAREST = 2;

		// Retrieves a handle to the display monitor
		[DllImport( "user32.dll" )]
		public static extern IntPtr MonitorFromWindow( IntPtr hwnd, int dwFlags );

		// RECT structure, Rectangle used by MONITORINFOEX
		[StructLayout( LayoutKind.Sequential )]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		// MONITORINFOEX structure, Monitor information used by GetMonitorInfo function
		[StructLayout( LayoutKind.Sequential )]
		public class MONITORINFOEX
		{
			public int cbSize;
			public RECT rcMonitor; // The display monitor rectangle
			public RECT rcWork; // The working area rectangle
			public int dwFlags;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 0x20 )]
			public char[] szDevice;
		}

		// MINMAXINFO structure, Window's maximum size and position information
		[StructLayout( LayoutKind.Sequential )]
		public struct MINMAXINFO
		{
			public POINT ptReserved;
			public POINT ptMaxSize; // The maximized size of the window
			public POINT ptMaxPosition; // The position of the maximized window
			public POINT ptMinTrackSize; // The mini size of window
			public POINT ptMaxTrackSize; // The maxi size of window
		}

		// Get the working area of the specified monitor
		[DllImport( "user32.dll" )]
		public static extern bool GetMonitorInfo( HandleRef hmonitor, [In, Out] MONITORINFOEX monitorInfo );
		
		/// <summary>
		/// 鼠标的各种消息
		/// </summary>
		public enum HitTest : int
		{
			HTERROR = -2,

			HTTRANSPARENT = -1,

			HTNOWHERE = 0,

			HTCLIENT = 1,

			HTCAPTION = 2,

			HTSYSMENU = 3,

			HTGROWBOX = 4,

			HTSIZE = HTGROWBOX,

			HTMENU = 5,

			HTHSCROLL = 6,

			HTVSCROLL = 7,

			HTMINBUTTON = 8,

			HTMAXBUTTON = 9,

			HTLEFT = 10,

			HTRIGHT = 11,

			HTTOP = 12,

			HTTOPLEFT = 13,

			HTTOPRIGHT = 14,

			HTBOTTOM = 15,

			HTBOTTOMLEFT = 16,

			HTBOTTOMRIGHT = 17,

			HTBORDER = 18,

			HTREDUCE = HTMINBUTTON,

			HTZOOM = HTMAXBUTTON,

			HTSIZEFIRST = HTLEFT,

			HTSIZELAST = HTBOTTOMRIGHT,

			HTOBJECT = 19,

			HTCLOSE = 20,

			HTHELP = 21

		}


		// kernel32


		[DllImport( "kernel32.dll", SetLastError = true )]
		static extern bool GetProcessHandleCount( IntPtr hProcess, ref UInt32 dwHandleCount );

		/// <summary>
		/// Determines whether the specified process is running under WOW64. Some WinAPI functions work differently when running through WOW64, so you will sometimes need to know if a process is under the thunking layer.
		/// </summary>
		/// <param name="hProcess"></param>
		/// <param name="wow64Process"></param>
		/// <returns></returns>
		[DllImport( "kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi )]
		[return: MarshalAs( UnmanagedType.Bool )]
		internal static extern bool IsWow64Process( [In] IntPtr process, [Out] out bool wow64Process );


	}


	public static class API
	{
		//public static IntPtr GetHandle<TWndow>( this TWndow wnd ) where TWndow : Window
		//{
		//	return new System.Windows.Interop.WindowInteropHelper( wnd ).Handle;
		//}

		public static IntPtr GetHandle<TVisual>( this TVisual visual ) where TVisual : System.Windows.Media.Visual
		{
			if ( null == visual )
				return IntPtr.Zero;

			HwndSource hwndSource = PresentationSource.FromVisual( visual ) as HwndSource;

			if ( null != hwndSource )
				return hwndSource.Handle;

			return IntPtr.Zero;
		}

		public static bool FlashWindowEx<TWndow>( this TWndow wnd ) where TWndow : Window
		{
			IntPtr hwnd = wnd.GetHandle();

			NativeMethods.FLASHWINFO flashinfo = new NativeMethods.FLASHWINFO();

			flashinfo.cbSize = Convert.ToUInt32( Marshal.SizeOf( flashinfo ) );
			flashinfo.hwnd = hwnd;
			flashinfo.dwFlags = NativeMethods.FLASHW_ALL | NativeMethods.FLASHW_TIMERNOFG;//这里是闪动窗标题和任务栏按钮,直到用户激活窗体
			flashinfo.uCount = UInt32.MaxValue;
			flashinfo.dwTimeout = 0;

			return MIV.Bus.WPF.UIShell.NativeMethods.FlashWindowEx( ref flashinfo );
		}
	}
}

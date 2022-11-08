using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

// Wallpaper Engine

// This script sets a build to render behind the files on the desktop and applications but ahead of the actual desktop wallpaper

// All code written by Walter Keiler 2022

public class Wallpaper : MonoBehaviour
{
    // This region holds everything needed to interface with windows and render a window
    #region Window Vars
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    [DllImport("User32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("User32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    
    [DllImport("User32.dll")]
    private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    
    [DllImport("User32.dll")]
    private static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);
    
    [Flags]
    enum SendMessageTimeoutFlags : uint
    {
        SMTO_NORMAL             = 0x0,
        SMTO_BLOCK              = 0x1,
        SMTO_ABORTIFHUNG        = 0x2,
        SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
        SMTO_ERRORONEXIT = 0x20
    }
    
    [DllImport("User32.dll")]
    private static extern IntPtr FindWindow(string windowName, bool wait);
    
    [DllImport("User32.dll")]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint Msg,
        IntPtr wParam,
        IntPtr lParam,
        SendMessageTimeoutFlags fuFlags,
        uint uTimeout,
        out IntPtr lpdwResult);
    
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("User32.dll")]
    static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    
    [DllImport("User32.dll")]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className,  string windowTitle);
    
    [DllImport("User32.dll")]
    static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    
    private struct Margins
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cxTopHeight;
        public int cxBottomHeight;
    }

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins margins);

    private const int GWL_EXSTYLE = -20;
    
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(0);
    
    private const uint LWA_COLORKEY = 0x00000001;

    private IntPtr hWnd;
    

    #endregion

    [SerializeField] private LayerMask clickableMask;
    
    // When the application starts set it to render on the propper layer
    private void Start()
    {
#if !UNITY_EDITOR
        hWnd = GetActiveWindow();
        
        Margins margins = new Margins {cxLeftWidth = -1};
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        IntPtr program = FindWindow("Progman", false);
        IntPtr result = IntPtr.Zero;
        SendMessageTimeout(program, 
            0x052C, 
            new IntPtr(0), 
            IntPtr.Zero, 
            SendMessageTimeoutFlags.SMTO_NORMAL, 
            1000, 
            out result);
        
        IntPtr workerw = IntPtr.Zero;
        
        EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
        {
            IntPtr p = FindWindowEx(tophandle, 
                IntPtr.Zero, 
                "SHELLDLL_DefView", 
                null);

            if (p != IntPtr.Zero)
            {
                workerw = FindWindowEx(IntPtr.Zero, 
                    tophandle, 
                    "WorkerW", 
                    null);
            }

            return true;
        }), IntPtr.Zero);
        
        SetWindowPos(hWnd, workerw, 0, 0, 0, 0, 0);
        SetParent(hWnd, workerw);
#endif
    }

    // This function is for if you want to set certain things to be clickable 
    private void SetClickThrough(bool clickThrough)
    {
        if (clickThrough)
        {
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
        else
        {
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        }
    }
}

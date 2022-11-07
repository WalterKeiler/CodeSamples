using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

// Desktop Breakout

// This script controls how the application is rendered and if you can click through it or not

// All code written by Walter Keiler 2022

public class TransparentWindow : MonoBehaviour
{
    // Holds everything necessary to interface with windows and the render layers
    #region Window Vars
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    [DllImport("User32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("User32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    
    [DllImport("User32.dll")]
    private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    
    [DllImport("User32.dll")]
    private static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);
    
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

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    
    private const uint LWA_COLORKEY = 0x00000001;

    private IntPtr hWnd;
    

    #endregion

    [SerializeField] private LayerMask clickableMask;
    
    // Find the application and set it to render over all other applications
    private void Start()
    {
#if !UNITY_EDITOR
        hWnd = FindWindow(null,"FunnyDesktopThing");
        
        Margins margins = new Margins {cxLeftWidth = -1};
        DwmExtendFrameIntoClientArea(hWnd, ref margins);
        
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);

        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0);
#endif
    }

    // Check to see if you are allowed to click through this object
    private void Update()
    {
        SetClickThrough(Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition),clickableMask) == null && !EventSystem.current.IsPointerOverGameObject());
    }

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

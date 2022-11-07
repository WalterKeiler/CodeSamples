using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

// Desktop Breakout

// This script controls how the application is rendered and if you can click through it or not

// All code written by Walter Keiler 2022

public class WindowController : MonoBehaviour
{
    [SerializeField] private GameObject gameObj;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Vector2 startPos;
    
    private System.Diagnostics.Process player;
    private IntPtr playerHandle;
    private IntPtr zPos;
    
    // Holds all of the relevant windows variables 
    #region DllImport

    [DllImport("User32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);
 
    // Used to resize and position a window
    [DllImport("User32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner
    }
    
    [DllImport("User32.dll")]
    static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("User32.dll")]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className,  string windowTitle);

    // Flag to not give the other window focus
    private const int SWP_NOACTIVATE = 0x0010;
    public const int SWP_NOMOVE = 0x0002;
    public const int SWP_NOSENDCHANGING = 0x0400;
    public const int SWP_HIDEWINDOW = 0x0080;
 
    public enum GWL
    {
        GWL_WNDPROC =    (-4),
        GWL_HINSTANCE =  (-6),
        GWL_HWNDPARENT = (-8),
        GWL_STYLE =      (-16),
        GWL_EXSTYLE =    (-20),
        GWL_USERDATA =   (-21),
        GWL_ID =     (-12)
    }
    
    [DllImport("user32.dll", EntryPoint="GetWindowLong")]
    static extern IntPtr GetWindowLongPtr(IntPtr hWnd, GWL nIndex);

    #endregion
    
    // Simple two state toggle
    bool toggle = true;

    // Create the notepad and name it
    private void Start()
    {
        player = new System.Diagnostics.Process();
        player.StartInfo = new System.Diagnostics.ProcessStartInfo("notepad.exe", "Player");
        player.EnableRaisingEvents = true;
        player.Start();
        playerHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Player.txt - Notepad");
    }

    // This moves the player window to follow the mouse and moves the notepad
    void Update () 
    {
        playerHandle = IntPtr.Zero;
        playerHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Player.txt - Notepad");
        Vector2 targetPos = ConvertWorldToPixel(startPos);

        SetWindowPos(playerHandle, IntPtr.Zero, (int) Input.mousePosition.x - 100, (int) targetPos.y, width, height,
            SWP_NOACTIVATE);
        
        IntPtr handle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Player.txt - Notepad");
        RECT pos = new RECT();
        GetWindowRect(handle, out pos);

        Vector2 bottomLft = new Vector2(pos.Left, pos.Bottom);
        Vector2 bottomRt = new Vector2(pos.Right, pos.Bottom);
        Vector2 topRt = new Vector2(pos.Right, pos.Top);
        Vector2 topLft = new Vector2(pos.Left, pos.Top);
        
        float midX = (bottomRt.x + bottomLft.x) * .5f;
        float midY = (topLft.y + bottomLft.y) * .5f;
        
        Vector2 posWorldPoint = new Vector2(midX, midY);
        Vector2 worldPoint = ConvertPixelToWorld((posWorldPoint));
        
        Vector2 worldBottomLft = ConvertPixelToWorld((bottomLft));
        Vector2 worldTopLft = ConvertPixelToWorld((topLft));
        Vector2 worldTopRt = ConvertPixelToWorld((topRt));
        
        gameObj.transform.position = new Vector3(worldPoint.x,-worldPoint.y,0);
        gameObj.transform.localScale = new Vector3(Vector2.Distance(worldTopLft,worldTopRt), Vector2.Distance(worldTopLft,worldBottomLft));
    }

    Vector3 ConvertPixelToWorld(Vector3 input)
    {
        return Camera.main.ScreenToWorldPoint(input);
    }
    Vector3 ConvertWorldToPixel(Vector3 input)
    {
        return Camera.main.WorldToScreenPoint(input);
    }

}

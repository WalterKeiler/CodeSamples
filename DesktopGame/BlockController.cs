using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] private GameObject gameObj;
    [SerializeField] private GameObject xObj;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int num;
    public Vector2 startPos;
    [SerializeField] private Vector2 offset;
    
    private System.Diagnostics.Process player;
    private IntPtr playerHandle;

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
    public const int SWP_SHOWWINDOW = 0x0040;
 
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
    
    private const int TS_TRUE = 1;
    private const int CBS_NORMAL = 1;
    private const int WP_CLOSEBUTTON = 18;
    private const string VSCLASS_WINDOW = "WINDOW";
    
    #endregion
    
    // Simple two state toggle
    public bool hit = false;

    private void Start()
    {
        player = new System.Diagnostics.Process();
        player.StartInfo = new System.Diagnostics.ProcessStartInfo("notepad.exe", num.ToString());
        player.EnableRaisingEvents = true;
        player.Start();
        playerHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, num.ToString() + ".txt - Notepad");
        //Debug.Log(GetWindowLongPtr(playerHandle,GWL.GWL_HINSTANCE));
    }

    void Update () 
    {
        playerHandle = IntPtr.Zero;
        playerHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, num.ToString() + ".txt - Notepad");
        Vector2 targetPos = ConvertWorldToPixel(startPos);
        //Debug.Log(player.Id);
        
        SetWindowPos(playerHandle, IntPtr.Zero, (int) targetPos.x, (int) targetPos.y, width, height,
            SWP_NOACTIVATE);

        if (hit)
        {
            SetWindowPos(playerHandle, IntPtr.Zero, (int) targetPos.x, (int) targetPos.y, width, height,
                SWP_HIDEWINDOW);
            gameObj.SetActive(false);
            xObj.SetActive(false);
        }
        
        //IntPtr handle = FindWindow(null, "Sticky Notes");
        //IntPtr handle = FindWindow(null, "Untitled - Notepad");
        IntPtr handle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, num.ToString() + ".txt - Notepad");
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

        xObj.transform.position = new Vector3((float)(worldTopRt.x - offset.x), (float)(worldTopRt.y - offset.y));
    }

    Vector3 ConvertPixelToWorld(Vector3 input)
    {
        return Camera.main.ScreenToWorldPoint(input);
    }
    Vector3 ConvertWorldToPixel(Vector3 input)
    {
        return Camera.main.WorldToScreenPoint(input);
    }

    public void Reset()
    {
        hit = false;
        gameObj.SetActive(true);
        xObj.SetActive(true);
        
        Vector2 targetPos = ConvertWorldToPixel(startPos);
        SetWindowPos(playerHandle, IntPtr.Zero, (int) targetPos.x, (int) targetPos.y, width, height,
            SWP_SHOWWINDOW);
    }
}


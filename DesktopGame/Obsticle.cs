using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Random = UnityEngine.Random;

public class Obsticle : MonoBehaviour
{
    [SerializeField] Vector2 sizeMax;
    [SerializeField] Vector2 sizeMin;
    private Vector2 randSize;
    
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

    // Flag to not give the other window focus
    private const int SWP_NOACTIVATE = 0x0010;
    private IntPtr handle;
    void Start()
    {
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo = new System.Diagnostics.ProcessStartInfo("explorer.exe");
        p.Start();
        
        randSize = new Vector2((int)Random.Range(sizeMin.x,sizeMax.x), (int) Random.Range(sizeMin.y, sizeMax.y));
        
        handle = p.Handle;
        Vector2 startPos = ConvertWorldToPixel(transform.position);
        SetWindowPos(handle, IntPtr.Zero, 0, 0,
            (int)randSize.x, (int)randSize.y, SWP_NOACTIVATE);
    }

    public bool on = true;
    void Update()
    {
        Invoke("UpdateThing", .1f);
        
    }

    void UpdateThing()
    {
        if (on)
        {
            handle = FindWindow(null, "File Explorer");

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

            gameObject.transform.position = new Vector3(worldPoint.x, -worldPoint.y, 0);
            gameObject.transform.localScale = new Vector3(Vector2.Distance(worldTopLft, worldTopRt),
                Vector2.Distance(worldTopLft, worldBottomLft));
            on = false;
        }
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

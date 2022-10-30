using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Scripts.Systems;

// Jubilite

// This script drives the visual end of the HeatMap script changing a cube to visually represent the output of the map

// All code written by Walter Keiler 2022
public class BlockScript : MonoBehaviour
{
    // What position on the board this cube is this correlates with the HeatMap output array
    [SerializeField] private Vector2 position;

    [SerializeField] private Material defaultMat;
    private Gradient colorGradient;

    private Renderer rend;
    private int beatNum = 1;
    private HeatMap _hm;
    private Material mat;
    
    private BeatManager _beatManager;
   
   // Subscribe the script to the universal Beat event
    private void OnEnable()
    {
        SetInitialReferences();

        BeatManager.OnBeat += Beat;
    }

    private void OnDisable()
    {
        BeatManager.OnBeat -= Beat;
    }

    void SetInitialReferences()
    {
        _beatManager = FindObjectOfType<BeatManager>();
    }

    // Setup the variables for the script
    void Start()
    {
        _hm = FindObjectOfType<HeatMap>();
        rend = GetComponent<MeshRenderer>();
        mat = new Material(defaultMat);
        rend.material = mat;
        colorGradient = _hm.colorGradient;
        Beat();
    }

    // Each beat we check the heatmap array for that beat find our value and sample the gradient for the color it should be
    void Beat()
    {
        colorGradient = _hm.colorGradient;
        rend.material.color = colorGradient.Evaluate(_hm.heatMaps[beatNum].heatMapOutput[(int) (position.x), (int) -position.y].r);
        
        transform.localScale = new Vector3(Mathf.Clamp(_hm.heatMaps[beatNum].heatMapOutput[(int) (position.x), (int) -position.y].r,0,.85f),
            1.1f, Mathf.Clamp(_hm.heatMaps[beatNum].heatMapOutput[(int) (position.x), (int) -position.y].r,0,.85f));
        
        if (_hm.heatMaps[beatNum].heatMapOutput[(int) (position.x), (int) -position.y].r < .1f)
        {
            rend.material.color = Color.clear;
        }
        
        beatNum++;
    }
}

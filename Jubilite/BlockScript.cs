using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Scripts.Systems;

public class BlockScript : MonoBehaviour
{
    [SerializeField] private Vector2 position;
    [SerializeField] private Material defaultMat;
    private Gradient colorGradient;

    private Renderer rend;
    private int beatNum = 1;
    private HeatMap _hm;
    private Material mat;
    
    private BeatManager _beatManager;
   
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

    void Start()
    {
        _hm = FindObjectOfType<HeatMap>();
        rend = GetComponent<MeshRenderer>();
        mat = new Material(defaultMat);
        rend.material = mat;
        colorGradient = _hm.colorGradient;
        Beat();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            //Beat();
        }
    }

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

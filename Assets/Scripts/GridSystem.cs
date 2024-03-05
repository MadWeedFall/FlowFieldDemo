using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer _meshRenderer;
    [SerializeField]
    private MeshFilter _meshFilter;

    [SerializeField]
    private Transform _pathFinder;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private Transform _funyGuy;

    [SerializeField]
    private GameObject _blockOject;

    private float _desiredVelocity = 100f/(float)GRID_WIDTH;

    public const int GRID_WIDTH = 50;
    public const int GRID_HEIGHT = 50;

    private Vector2Int CELL_INDEX_INVALID = new Vector2Int(GRID_WIDTH+1,GRID_HEIGHT+1);

    private VectorField _vf;

    private HashSet<Vector2Int> Blocks;
    private List<GameObject> _blockGOs;


    public Vector2Int GetGridCellIndexByLocHitPos(Vector3 pos)
    {
        Vector2Int ret = new Vector2Int((int)Mathf.Floor((pos.x+5.0f)/10.0f*GRID_WIDTH),(int)Mathf.Floor((5.0f-pos.z)/10.0f*GRID_HEIGHT));
        return ret;
    }

    private void InitBlocks()
    {
        if(Blocks==null) Blocks = new HashSet<Vector2Int>();
        Blocks.Clear();
        if(_blockGOs==null) _blockGOs = new List<GameObject>();
        _blockGOs.Clear();
        Blocks.Add(new Vector2Int(30,30));
        Blocks.Add(new Vector2Int(30,29));
        Blocks.Add(new Vector2Int(30,28));
        Blocks.Add(new Vector2Int(30,27));
        Blocks.Add(new Vector2Int(30,26));
        Blocks.Add(new Vector2Int(30,25));
        Blocks.Add(new Vector2Int(30,24));
        Blocks.Add(new Vector2Int(30,23));
        Blocks.Add(new Vector2Int(29,30));
        Blocks.Add(new Vector2Int(28,30));
        Blocks.Add(new Vector2Int(27,30));
        Blocks.Add(new Vector2Int(26,30));
        Blocks.Add(new Vector2Int(25,30));
        Blocks.Add(new Vector2Int(24,30));
        Blocks.Add(new Vector2Int(23,30));
        Blocks.Add(new Vector2Int(22,30));
        Blocks.Add(new Vector2Int(21,30));
        Blocks.Add(new Vector2Int(20,30));

        foreach(var block in Blocks)
        {
            var go = Instantiate(_blockOject);
            go.transform.parent = transform;
            go.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
            go.transform.position = transform.TransformPoint(new Vector3(-5.0f+(block.x+0.5f)/(float)GRID_WIDTH*10.0f,0,5.0f-(block.y+0.5f)/(float)GRID_HEIGHT*10.0f));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitBlocks();
        _vf = new VectorField();
        _vf.SetBlocks(Blocks);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
               var locHitPos = transform.InverseTransformPoint(hit.point);
               var cellIdx = GetGridCellIndexByLocHitPos(locHitPos);
               if(!Blocks.Contains(cellIdx))
               {
                    _target.position = hit.point;
                    _vf.GenerateHeatMap(cellIdx);
                    _vf.GertnerateVectorField(cellIdx);
                    // _vf.ShowConsoleHeatMap();
                    // _vf.ShowConsoleVectorFiled();
               }
            }
        }

        var localPathFinderPos =  transform.InverseTransformPoint(_pathFinder.position);
        var delta = Time.deltaTime;
        var cellIndx = GetGridCellIndexByLocHitPos(localPathFinderPos);
        var vi = _vf.GetVelocity(cellIndx);
        var v = new Vector2(vi.x*_desiredVelocity,vi.y*_desiredVelocity);
        var wolrdPos = transform.TransformPoint(new Vector3(localPathFinderPos.x+v.x*delta,localPathFinderPos.y,localPathFinderPos.z-v.y*delta));
        _pathFinder.position = wolrdPos;

        var localPathFinderPos1 =  transform.InverseTransformPoint(_funyGuy.position);
        var cellIndx1 = GetGridCellIndexByLocHitPos(localPathFinderPos1);
        var vi1 = _vf.GetVelocity(cellIndx1);
        var v1 = new Vector2(vi1.x*_desiredVelocity*Mathf.Abs(Mathf.Sin(Time.time)),vi1.y*_desiredVelocity*Mathf.Abs(Mathf.Cos(Time.time)));
        var wolrdPos1 = transform.TransformPoint(new Vector3(localPathFinderPos1.x+v1.x*delta,localPathFinderPos.y,localPathFinderPos1.z-v1.y*delta));
        _funyGuy.position = wolrdPos1;

    }
}

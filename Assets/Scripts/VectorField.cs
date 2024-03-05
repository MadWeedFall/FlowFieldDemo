/// Naive implentation of Goal Based Vector Field Path Finding Algoritm 
/// Grid Width Height Should Less than 1000
/// Use Euclidean Distance for heat map generation
/// Works like charm

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VectorField
{
    private int[,] _heatMap = new int[GridSystem.GRID_WIDTH,GridSystem.GRID_HEIGHT];
    private Vector2Int[,] _vectorField = new Vector2Int[GridSystem.GRID_WIDTH,GridSystem.GRID_HEIGHT];

    private HashSet<int> _openSet;
    private List<int> _closeSet;

    private HashSet<int> _blockSet;

    private const int BLOCK_DISTANCE = int.MaxValue/2;

    public Vector2Int GetVelocity(Vector2Int cellIdx)
    {
        if(cellIdx.x>=0&&cellIdx.y>=0&&cellIdx.x<GridSystem.GRID_WIDTH&&cellIdx.y<GridSystem.GRID_HEIGHT)
        {
            return _vectorField[cellIdx.x,cellIdx.y];
        }
        return new Vector2Int(cellIdx.x>=GridSystem.GRID_WIDTH?-1:(cellIdx.x<0?1:0),cellIdx.y>=GridSystem.GRID_HEIGHT?-1:(cellIdx.y<0?1:0));
    }

    public void SetBlocks(HashSet<Vector2Int> blocks)
    {
        if(_blockSet==null) _blockSet = new HashSet<int>();
        _blockSet.Clear();
        foreach(Vector2Int block in blocks)
        {
            _blockSet.Add(block.x+1000*block.y);
        }
    }


    void CalSurroundCellPathLength(Vector2Int target)
    {
        var upIdx = target.x+(target.y-1)*1000;
        var downIdx = target.x+(target.y+1)*1000;
        var leftIdx = target.x-1+target.y*1000;
        var rightIdx = target.x+1+target.y*1000;
        var targetHeat = _heatMap[target.x,target.y];
        if(_openSet.Contains(upIdx))
        {
            _heatMap[target.x,target.y-1] = targetHeat+1;
            _openSet.Remove(upIdx);
            _closeSet.Add(upIdx);
        }
        if(_openSet.Contains(downIdx))
        {
            _heatMap[target.x,target.y+1] = targetHeat+1;
            _openSet.Remove(downIdx);
            _closeSet.Add(downIdx);
        }
        if(_openSet.Contains(leftIdx))
        {
            _heatMap[target.x-1,target.y] = targetHeat+1;
            _openSet.Remove(leftIdx);
            _closeSet.Add(leftIdx);
        }
        if(_openSet.Contains(rightIdx))
        {
            _heatMap[target.x+1,target.y] = targetHeat+1;
            _openSet.Remove(rightIdx);
            _closeSet.Add(rightIdx);
        }
    }

    Vector2Int SetIdx2CellIdx(int setIdx)
    {
        var y = setIdx/1000;
        var x = setIdx - y*1000;
        return new Vector2Int(x,y);
    }

    public void ShowConsoleHeatMap()
    {  
        StringBuilder sb = new StringBuilder();
        for(int i=0;i<GridSystem.GRID_HEIGHT;i++)
        {  
            for(int j=0;j<GridSystem.GRID_WIDTH;j++)
            {
                string sPart = _heatMap[j,i]<10?"0"+_heatMap[j,i]:(_heatMap[j,i]==BLOCK_DISTANCE?"xx":""+_heatMap[j,i]);
                sb.Append(""+sPart+" ");
            }
            sb.Append("\n");
        }
        Debug.Log(sb.ToString());
    }

    public void GenerateHeatMap(Vector2Int targetCellIdx)
    {  
        if(_openSet==null) _openSet = new HashSet<int>(GridSystem.GRID_HEIGHT*GridSystem.GRID_WIDTH);
        if(_closeSet==null) _closeSet = new List<int>(GridSystem.GRID_HEIGHT*GridSystem.GRID_WIDTH);
        Vector2Int target = targetCellIdx;
        for(int i=0;i<GridSystem.GRID_HEIGHT;i++)
            for(int j=0;j<GridSystem.GRID_WIDTH;j++)
            {
                var setIdx = i*1000+j;
                if(_blockSet.Contains(setIdx))
                {
                    _heatMap[j,i] = BLOCK_DISTANCE;
                    _closeSet.Add(setIdx);
                }
                else
                {
                    _openSet.Add(i*1000+j);
                }
            }
        _openSet.Remove(target.x+target.y*1000);
        _closeSet.Add(target.x+target.y*1000);
        _heatMap[target.x,target.y] = 0;
        var lastCloseIdx = _closeSet.Count;
        CalSurroundCellPathLength(target);
        while(_openSet.Any())
        {
            for(int i=lastCloseIdx;i<_closeSet.Count;i++)
            {
                lastCloseIdx = i;
                CalSurroundCellPathLength(SetIdx2CellIdx(_closeSet[i]));
            }
        }    
        _closeSet.Clear();
    }

    bool IsNotBlock(int x,int y)
    {
        return !_blockSet.Contains(x+y*1000);
    }

    bool IsInBounds(int x,int y)
    {
        bool inBounds = x>=0 && y>=0 && x<GridSystem.GRID_WIDTH && y<GridSystem.GRID_HEIGHT;
        return inBounds;
    }

    bool IsInBounds(Vector2Int idx)
    {
        return IsInBounds(idx.x,idx.y);
    }

    bool IsAvliable(int x,int y)
    {
        return IsInBounds(x,y)&&IsNotBlock(x,y);
    }

    bool IsAvliable(Vector2Int idx)
    {
        return IsAvliable(idx.x,idx.y);
    }

    void CalCellVelocityVector(Vector2Int target)
    {
        var upIdx = new Vector2Int(target.x,target.y-1);
        var downIdx = new Vector2Int(target.x,target.y+1);
        var leftIdx = new Vector2Int(target.x-1,target.y);
        var rightIdx = new Vector2Int(target.x+1,target.y);

        var upDis = IsInBounds(upIdx)?_heatMap[upIdx.x,upIdx.y]:_heatMap[target.x,target.y];
        var downDis = IsInBounds(downIdx)?_heatMap[downIdx.x,downIdx.y]:_heatMap[target.x,target.y];
        var leftDis = IsInBounds(leftIdx)?_heatMap[leftIdx.x,leftIdx.y]:_heatMap[target.x,target.y];
        var rightDis = IsInBounds(rightIdx)?_heatMap[rightIdx.x,rightIdx.y]:_heatMap[target.x,target.y];
        var x = leftDis-rightDis==0?0:Math.Sign(leftDis-rightDis);
        var y = upDis-downDis==0?0:Math.Sign(upDis-downDis);
        _vectorField[target.x,target.y] = new Vector2Int(x,y);
    }

    void RedirectVelocityNearBlockByCellIdx(int x,int y,Vector2Int target)
    {
        if(target.x==x&&target.y==y)
        {
            _vectorField[x,y] = Vector2Int.zero;
            return; //no need to process target cell
        }

        if(!IsNotBlock(x,y)) return; 
       
        var v = _vectorField[x,y];
        if(v.x*v.y==0)
        {
            // local optima
            // In heat map up equals down and left equals right, thus velocity at that cell equals zero
            // In this case When path finder move to current cell, it will stop moving  
            if(v.x==v.y&&v.y==0)
            {
                if(IsAvliable(x-1,y)) _vectorField[x,y] = _vectorField[x-1,y];
                else if(IsAvliable(x+1,y)) _vectorField[x,y] = _vectorField[x+1,y];
                else if(IsAvliable(x,y-1)) _vectorField[x,y] = _vectorField[x,y-1];
                else if(IsAvliable(x,y-1)) _vectorField[x,y] = _vectorField[x,y+1];
                else Debug.LogError("Hey buddy how did you get here "+x+","+y);
                return;
            }
        }

        //TODO: near block procession
        // near block local optima
        // Unlike problem above, only have two neighbors has samev value,but the other neighbor have one side blocked
        // So even velovtiy at current cell have value, in direction or current velocity, the next cell have an oppsite velocity
        // Pathfinder will stuck between two cells, infinitely move between.
        var nextIdx = new Vector2Int(x+v.x,y+v.y);
        if(IsInBounds(nextIdx))
        {
            var nextV = _vectorField[nextIdx.x,nextIdx.y];

            if(!IsNotBlock(nextIdx.x,nextIdx.y))
            {
                //Not a corner and not target
                if(nextV.x*nextV.y==0&&nextV.x!=nextV.y)
                {
                    _vectorField[x,y] = new Vector2Int(v.x==nextV.x?0:v.x,v.y==nextV.y?0:v.y);
                }
            }

            if(_vectorField[x,y]+nextV==Vector2Int.zero)
            {
                _vectorField[x,y] = new Vector2Int(v.y,v.x);
            }
        }
    }

    public void GertnerateVectorField(Vector2Int target)
    {
        for(int i=0;i<GridSystem.GRID_HEIGHT;i++)
            for(int j=0;j<GridSystem.GRID_WIDTH;j++)
            {
                CalCellVelocityVector(new Vector2Int(j,i));
            }
        for(int i=0;i<GridSystem.GRID_HEIGHT;i++)
            for(int j=0;j<GridSystem.GRID_WIDTH;j++)
            {
                RedirectVelocityNearBlockByCellIdx(j,i,target);
            }
    }

    public void ShowConsoleVectorFiled()
    {
        StringBuilder sb = new StringBuilder();
        for(int i=0;i<GridSystem.GRID_HEIGHT;i++)
        {  
            for(int j=0;j<GridSystem.GRID_WIDTH;j++)
            {
                string arrow;
                switch(_vectorField[j,i])
                {
                    case Vector2Int v when v.Equals(new Vector2Int(0,1)):
                        arrow = "↓";
                        break;
                    case Vector2Int v when v.Equals(new Vector2Int(1,1)):
                        arrow = "↘︎";
                        break;
                    case Vector2Int v when v.Equals(new Vector2Int(0,-1)):
                        arrow = "↑";
                        break;
                    case Vector2Int v when v.Equals(new Vector2Int(-1,-1)):
                        arrow = "↖︎";
                        break;
                    case Vector2Int v when v.Equals(new Vector2Int(-1,0)):
                        arrow = "←";
                        break;
                    case Vector2Int v when v.Equals(new Vector2Int(-1,1)):
                        arrow = "↙";
                        break;
                    case Vector2Int v when v.Equals(new Vector2Int(1,0)):
                        arrow = "→";
                        break;
                    case Vector2Int v when v.Equals(new Vector2Int(1,-1)):
                        arrow = "↗";
                        break;
                    default:
                        Debug.Log(_vectorField[j,i].ToString()+" "+j+","+i);
                        arrow = ".";
                        break;
                }
                sb.Append(""+arrow+" ");
            }
            sb.Append("\n");
        }
        Debug.Log(sb.ToString());
    }

}

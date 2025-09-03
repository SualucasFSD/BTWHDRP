using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _father;
    [SerializeField] private PathNode[] _fatherPathNucleo;    
    [SerializeField] private Transform _Parent;
    [SerializeField] private MazeCell[] _wallWithoutTower;
    [SerializeField] private MazeCell[] _wallWithTower;
    [SerializeField] private int _mazeWidth;
    [SerializeField] private int _mazeDepth;
    MazeCell prefabToUse;
    Vector3 spawnPosition;
    private MazeCell[,] _mazeGrid;
    private int _iti = 0;
    [SerializeField] private LayerMask _nodeLayer;
    private bool f=false;

    private void Start()
    {
        _mazeGrid = new MazeCell[_mazeWidth, _mazeDepth];
        StartCoroutine(GenerateGridFragmented());
    }
    private IEnumerator GenerateGridFragmented()
    {
        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                if (_mazeGrid[x, z] != null)
                    continue;

                prefabToUse = Par(x)
                    ? (Par(z) ? _wallWithTower[Random.Range(0, _wallWithTower.Length)] : _wallWithoutTower[Random.Range(0, _wallWithTower.Length)])
                    : (Par(z) ? _wallWithoutTower[Random.Range(0, _wallWithTower.Length)] : _wallWithTower[Random.Range(0, _wallWithTower.Length)]);

                if (x == 0 && z == 0)
                {
                    spawnPosition = transform.position;
                }
                else if (z > 0)
                {
                    spawnPosition = _mazeGrid[x, z - 1].RigtLeftSpawnVector[1].position;
                }
                else if (x > 0)
                {
                    spawnPosition = _mazeGrid[x - 1, 0].RigtLeftSpawnVector[0].position;
                }

                _mazeGrid[x, z] = Instantiate(prefabToUse, spawnPosition, Quaternion.identity);
                _mazeGrid[x, z].MazeArrayPosition[0] = x;
                _mazeGrid[x, z].MazeArrayPosition[1] = z;
                _mazeGrid[x, z].transform.SetParent(_Parent);
                OptimizerScript.instance.MazeCells.Add(_mazeGrid[x, z]);
                yield return null;
            }
        }

        _mazeGrid[_mazeWidth / 2, 0].ClearWall(3);
        StartCoroutine(GenerateMaze(null, _mazeGrid[0, 0]));
    }

    private IEnumerator GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);
        yield return null;
        MazeCell nextCell;
        do
        {
            nextCell = GetNextUnvisitedCell(currentCell);

            if (nextCell != null)
            {
                yield return GenerateMaze(currentCell, nextCell);
            }

        } while (nextCell != null);
        _iti++;
        if(_iti>=_mazeDepth*_mazeWidth)
        {
            foreach(PathNode n in _fatherPathNucleo)
            {
                n.Neighbords.Add(_mazeGrid[_mazeWidth / 2, 0]._primalPathNode[3]);
                _mazeGrid[_mazeWidth / 2, 0]._primalPathNode[3].Neighbords.Add(n);
            }
            f=true;
            OptimizerScript.instance.Activate(_mazeGrid[_mazeWidth / 2, 0]);
            print("Finish");
        }
    }
    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        var unvisitedCells = GetUnvisitedCells(currentCell);

        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }
    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        int x = currentCell.MazeArrayPosition[0];
        int z = currentCell.MazeArrayPosition[1];
        if (x + 1 < _mazeWidth)
        {
            var cellToRight = _mazeGrid[x + 1, z];

            if (cellToRight.IsVisited == false)
            {
                yield return cellToRight;
            }
        }

        if (x - 1 >= 0)
        {
            var cellToLeft = _mazeGrid[x - 1, z];

            if (cellToLeft.IsVisited == false)
            {
                yield return cellToLeft;
            }
        }

        if (z + 1 < _mazeDepth)
        {
            var cellToFront = _mazeGrid[x, z + 1];

            if (cellToFront.IsVisited == false)
            {
                yield return cellToFront;
            }
        }

        if (z - 1 >= 0)
        {
            var cellToBack = _mazeGrid[x, z - 1];

            if (cellToBack.IsVisited == false)
            {
                yield return cellToBack;
            }
        }
    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null)
        {
            return;
        }

        if (previousCell.transform.position.x < currentCell.transform.position.x)
        {
            previousCell.ClearWall(1);
            currentCell.ClearWall(0);
            previousCell._neighbords.Add(currentCell);
            currentCell._neighbords.Add(previousCell);
            previousCell._primalPathNode[1].Neighbords.Add(currentCell._primalPathNode[0]);
            currentCell._primalPathNode[0].Neighbords.Add(previousCell._primalPathNode[1]);
            return;
        }

        if (previousCell.transform.position.x > currentCell.transform.position.x)
        {
            previousCell.ClearWall(0);
            currentCell.ClearWall(1);
            previousCell._neighbords.Add(currentCell);
            currentCell._neighbords.Add(previousCell);
            previousCell._primalPathNode[0].Neighbords.Add(currentCell._primalPathNode[1]);
            currentCell._primalPathNode[1].Neighbords.Add(previousCell._primalPathNode[0]);
            return;
        }

        if (previousCell.transform.position.z < currentCell.transform.position.z)
        {
            previousCell.ClearWall(2);
            currentCell.ClearWall(3);
            previousCell._neighbords.Add(currentCell);
            currentCell._neighbords.Add(previousCell);
            previousCell._primalPathNode[2].Neighbords.Add(currentCell._primalPathNode[3]);
            currentCell._primalPathNode[3].Neighbords.Add(previousCell._primalPathNode[2]);
            return;
        }

        if (previousCell.transform.position.z > currentCell.transform.position.z)
        {
            previousCell.ClearWall(3);
            currentCell.ClearWall(2);
            previousCell._neighbords.Add(currentCell);
            currentCell._neighbords.Add(previousCell);
            previousCell._primalPathNode[3].Neighbords.Add(currentCell._primalPathNode[2]);
            currentCell._primalPathNode[2].Neighbords.Add(previousCell._primalPathNode[3]);
            return;
        }
    }
    private bool Par(int i)
    {
        if(i%2==0) return true;
        else return false;
    }
    private void OnDrawGizmos()
    {
        if(f)
        {
            Gizmos.DrawWireSphere(_mazeGrid[_mazeWidth / 2, 0].transform.position - _mazeGrid[_mazeWidth / 2, 0].transform.forward * 30f, 10f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    [SerializeField] private List<Entity> _enemies;
    [SerializeField] private Transform[] _enemiesSpawns;
    [SerializeField] private MazeCell _mazeCell;
    
    public void Spawn()
    {
        
    }
}

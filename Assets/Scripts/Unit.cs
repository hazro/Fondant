using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitStats unitStats;

    void Start()
    {
        // unitStatsからステータスを読み込む
        int currentJob = unitStats.job;
        int currentLevel = unitStats.level;
    }
}

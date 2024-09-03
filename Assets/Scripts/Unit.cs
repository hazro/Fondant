using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public UnitStats unitStats;
    private JobStatus jobStatus;

    [SerializeField] public int currentLevel;
    [SerializeField] public float Hp;
    [SerializeField] public float physicalAttackPower;
    [SerializeField] public float magicalAttackPower;
    [SerializeField] public float physicalDefensePower;
    [SerializeField] public float magicalDefensePower;
    [SerializeField] public float resistCondition;
    [SerializeField] public float attackDelay;
    [SerializeField] public float Speed;
    [SerializeField] public int attackUnitThrough;
    [SerializeField] public int attackObjectThrough;
    [SerializeField] public float attackSize;
    [SerializeField] public float knockBack;
    [SerializeField] public int targetJob;
    [SerializeField] public bool teleportation;
    [SerializeField] public bool escape;

    public Slider hpBar;
    public Image hpColor;
    private float maxHp;
    private float currentHp;

    void Start()
    {
        jobStatus = Resources.Load<JobStatus>("JobStatus" + unitStats.job.ToString("D2"));
        // unitStatsからステータスを読み込む
        int currentJob = unitStats.job;
        int totalExp = unitStats.totalExp;
        int currentWeapons = unitStats.weapons;
        int currentShields = unitStats.shields;
        int currentArmor = unitStats.armor;
        int currentAccessories = unitStats.accessories;
        // JobStatusからステータスを読み込む
        float currentLevelScaleFactor = jobStatus.levelScaleFactor;
        float currentMagic = jobStatus.Magic;
        float currentStr = jobStatus.Str;
        float currentDex = jobStatus.Dex;
        float currentResidtCondition = jobStatus.ResidtCondition;
        float currentAttackUnitThrough = jobStatus.AttackUnitThrough;
        float currentAttackObjectThrough = jobStatus.AttackObjectThrough;
        float currentKnockBack = jobStatus.KnockBack;
        float levelMagic = jobStatus.levelMagic;
        float levelStr = jobStatus.levelStr;
        float levelDex = jobStatus.levelDex;
        float levelResidtCondition = jobStatus.levelResidtCondition;
        float levelAttackUnitThrough = jobStatus.levelAttackUnitThrough;
        float levelAttackObjectThrough = jobStatus.levelAttackObjectThrough;
        float levelKnockBack = jobStatus.levelKnockBack;
        int targetJob = jobStatus.targetJob;
        float teleportation = jobStatus.teleportation;
        float escape = jobStatus.escape;

        // ステータスを計算する
        int baseExperience = 10; // レベルアップに必要な基本経験値
        currentLevel = (int)Math.Sqrt(totalExp / (baseExperience * currentLevelScaleFactor)) + 1;
        Hp = (currentStr + currentLevel * levelStr) * 10;
        physicalAttackPower = (currentStr + currentLevel * levelStr) / 1;
        magicalAttackPower = (currentMagic + currentLevel * levelMagic) / 1;
        physicalDefensePower = (currentStr + currentLevel * levelStr) / 10;
        magicalDefensePower = (currentMagic + currentLevel * levelMagic) / 10;
        resistCondition = (currentResidtCondition + currentLevel * levelResidtCondition) / 10;
        attackDelay =  (currentDex + currentLevel * levelDex) / 10;
        Speed =  3 + (currentDex + currentLevel * levelDex) / 10;
        attackUnitThrough = (int)(currentAttackUnitThrough + currentLevel * levelAttackUnitThrough);
        attackObjectThrough = (int)(currentAttackObjectThrough + currentLevel * levelAttackObjectThrough);
        attackSize = 1;
        knockBack = (currentKnockBack + currentLevel * levelKnockBack) / 10;
        targetJob = targetJob;
        teleportation = teleportation;
        escape = escape;

        // HPの初期化
        maxHp = (currentStr + currentLevel * levelStr) * 10;
        currentHp = maxHp;

        // HPバーの初期化
        UpdateHpBar();
    }

    void UpdateHpBar()
    {
        if (hpBar != null)
        {
            float hpPercentage = currentHp / maxHp;
            hpBar.value = hpPercentage;
            if (hpColor != null)
            {
                // HPの割合に応じて色を変更
                if (hpPercentage > 0.6f)
                {
                    hpColor.color = Color.green;
                }
                else if (hpPercentage > 0.4f)
                {
                    hpColor.color = new Color(1.0f, 0.65f, 0.0f); // オレンジ色
                }
                else
                {
                    hpColor.color = Color.red;
                }
            }
        }
    }

    // HPを減少させるメソッド
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp < 0)
        {
            currentHp = 0;
            Die();
        };
        UpdateHpBar();
    } 

    // HPを回復させるメソッド
    public void Heal(float amount)
    {
        currentHp += amount;
        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }
        UpdateHpBar();
    }

    // ユニットが死亡したら呼び出されるメソッド
    public void Die()
    {
        Destroy(gameObject);
    }
}

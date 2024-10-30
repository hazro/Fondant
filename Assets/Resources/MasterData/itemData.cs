using System;
using System.Collections.Generic;

[Serializable]
public class ItemData {
    [Serializable]
    public class JobListData {
        public int ID;
        public string name;
        public float magic;
        public float str;
        public float dex;
        public float residCondition;
        public float attackUnitThrough;
        public float attackObjectThrough;
        public float initKnockBack;
        public float levelScaleFactor;
        public float levelMagic;
        public float levelStr;
        public float levelDex;
        public float levelResidtCondition;
        public float levelAttackUnitThrough;
        public float levelAttackObjectThrough;
        public float levelKnockBack;
        public int targetJob;
        public float teleportation;
        public float escape;
        public int equeLargeSwordAxe;
        public int equeSwordDagger;
        public int equeStickStaffBook;
        public int arrow;
        public int spWeapon;
        public int equeShield;
        public int equeHeavyArmor;
        public int equeLightArmor;
        public int equeRobe;
        public int equeAccessories;
        public int initWeapon;
        public int initShield;
        public int initArmor;
        public int initAccessories;
    }

    public List<JobListData> jobList = new List<JobListData>();
    [Serializable]
    public class EqpListData {
        public int ID;
        public string name;
        public string ability;
        public int world;
        public int price;
        public int socketCount;
        public int hp;
        public float physicalAttackPower;
        public float magicalAttackPower;
        public float physicalDefensePower;
        public float magicalDefensePower;
        public float resistCondition;
        public float guardChance;
        public float criticalChance;
        public float criticalDamage;
        public float attackDelay;
        public float speed;
        public int attackUnitThrough;
        public int attackObjectThrough;
        public float attackSize;
        public float knockBack;
        public int targetJob;
        public float teleportation;
        public float escape;
        public float attackRange;
        public float attackStanceDuration;
        public float attackStanceDelay;
    }

    public List<EqpListData> eqpList = new List<EqpListData>();
    [Serializable]
    public class WpnListData {
        public int ID;
        public string name;
        public string ability;
        public int world;
        public int price;
        public int socketCount;
        public int hp;
        public float physicalAttackPower;
        public float magicalAttackPower;
        public float physicalDefensePower;
        public float magicalDefensePower;
        public float resistCondition;
        public float guardChance;
        public float criticalChance;
        public float criticalDamage;
        public float attackDelay;
        public float speed;
        public int attackUnitThrough;
        public int attackObjectThrough;
        public float attackSize;
        public float knockBack;
        public int targetJob;
        public float teleportation;
        public float escape;
        public float attackRange;
        public float attackStanceDuration;
        public float attackStanceDelay;
    }

    public List<WpnListData> wpnList = new List<WpnListData>();
    [Serializable]
    public class EnemyListData {
        public int ID;
        public string name;
        public int job;
        public int addLevel;
        public int weapons;
        public int shields;
        public int armor;
        public int accessories;
        public int drop1;
        public int drop2;
        public int drop3;
        public float drop1Rate;
        public float drop2Rate;
        public float drop3Rate;
        public int dropExp;
        public int dropGold;
    }

    public List<EnemyListData> enemyList = new List<EnemyListData>();
    [Serializable]
    public class RuneListData {
        public int ID;
        public string name;
        public int world;
        public int price;
        public int maxLevel;
        public string ability;
        public int addLevelLv1;
        public int addLevelLv2;
        public int addLevelLv3;
        public float delayLv1;
        public float delayLv2;
        public float delayLv3;
        public float scaleLv1;
        public float scaleLv2;
        public float scaleLv3;
        public float distanceLv1;
        public float distanceLv2;
        public float distanceLv3;
        public float timeLv1;
        public float timeLv2;
        public float timeLv3;
        public float speedLv1;
        public float speedLv2;
        public float speedLv3;
        public float moveSpeedLv1;
        public float moveSpeedLv2;
        public float moveSpeedLv3;
        public float physicalDefenseLv1;
        public float physicalDefenseLv2;
        public float physicalDefenseLv3;
        public float magicalDefenseLv1;
        public float magicalDefenseLv2;
        public float magicalDefenseLv3;
        public float pysicalPowerLv1;
        public float pysicalPowerLv2;
        public float pysicalPowerLv3;
        public float magicalPowerLv1;
        public float magicalPowerLv2;
        public float magicalPowerLv3;
        public int attackUnitThroughLv1;
        public int attackUnitThroughLv2;
        public int attackUnitThroughLv3;
        public int attackObjectThroughLv1;
        public int attackObjectThroughLv2;
        public int attackObjectThroughLv3;
        public float bloodTimeLv1;
        public float bloodTimeLv2;
        public float bloodTimeLv3;
        public float poisonTimeLv1;
        public float poisonTimeLv2;
        public float poisonTimeLv3;
        public float cretecalChanceLv1;
        public float cretecalChanceLv2;
        public float cretecalChanceLv3;
        public float guardChanceLv1;
        public float guardChanceLv2;
        public float guardChanceLv3;
        public float bloodSuckLv1;
        public float bloodSuckLv2;
        public float bloodSuckLv3;
        public float poisonGuardLv1;
        public float poisonGuardLv2;
        public float poisonGuardLv3;
        public float bleedGuardLv1;
        public float bleedGuardLv2;
        public float bleedGuardLv3;
        public float stunGuardLv1;
        public float stunGuardLv2;
        public float stunGuardLv3;
        public float paralysisGuardLv1;
        public float paralysisGuardLv2;
        public float paralysisGuardLv3;
        public float wakeGuardLv1;
        public float wakeGuardLv2;
        public float wakeGuardLv3;
        public float defenceDownGuardLv1;
        public float defenceDownGuardLv2;
        public float defenceDownGuardLv3;
        public float conditionGuardLv1;
        public float conditionGuardLv2;
        public float conditionGuardLv3;
        public float comboDamageLv1;
        public float comboDamageLv2;
        public float comboDamageLv3;
        public float comboCriticalLv1;
        public float comboCriticalLv2;
        public float comboCriticalLv3;
        public float criticalDamageLv1;
        public float criticalDamageLv2;
        public float criticalDamageLv3;
        public float poisonLv1;
        public float poisonLv2;
        public float poisonLv3;
        public float bleedLv1;
        public float bleedLv2;
        public float bleedLv3;
        public float stunLv1;
        public float stunLv2;
        public float stunLv3;
        public float paralysisLv1;
        public float paralysisLv2;
        public float paralysisLv3;
        public float wakeLv1;
        public float wakeLv2;
        public float wakeLv3;
        public float defenceDownLv1;
        public float defenceDownLv2;
        public float defenceDownLv3;
    }

    public List<RuneListData> runeList = new List<RuneListData>();
    [Serializable]
    public class EnemySpawnSettingsData {
        public int world;
        public int stage;
        public int ID;
        public int minCount;
        public int maxCount;
        public int preferredColumn;
        public int priority;
    }

    public List<EnemySpawnSettingsData> enemySpawnSettings = new List<EnemySpawnSettingsData>();
    [Serializable]
    public class RuneSpawnSettingsData {
        public int world;
        public int ID;
        public string name;
    }

    public List<RuneSpawnSettingsData> runeSpawnSettings = new List<RuneSpawnSettingsData>();
}

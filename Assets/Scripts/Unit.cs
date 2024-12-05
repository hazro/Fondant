using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Unit : MonoBehaviour
{
    [Header("Debug")]
    public bool isNoHpReduction = false; // HP減少なし
    //public UnitStatus unitStatus;
    public SpriteRenderer unitSprite; // ユニットのスプライト
    private Animator animator; // Animatorコンポーネントの参照
    public bool live = true; // ユニットが生存しているかどうか
    private int RuneDropChance = 70; // ルーンドロップの確率(%)
    public int ID; // ユニットのID
    public int positionID; // ユニットの位置ID
    public List<GameObject> IventrySkillList; // ユニットのスキルリスト

    [SerializeField] public string unitName;
    [SerializeField] public bool[] condition = new bool[9]; // 状態異常（各要素がtrueの時） 0:死亡、1:毒、2:出血、3:スタン、4:麻痺、5:脆弱、6:弱体、7:吸血、8:リジェネ
    [SerializeField] public int job;
    [SerializeField] public int totalExp;
    [SerializeField] public int currentLevel;
    [SerializeField] public float nextLevelExp;
    [SerializeField] public int remainingExp;
    [SerializeField] public int addLevel;
    [SerializeField] public float Hp;
    public float currentHp; // 現在のHP
    [SerializeField] public float physicalAttackPower;
    [SerializeField] public float magicalAttackPower;
    [SerializeField] public float physicalDefensePower;
    [SerializeField] public float magicalDefensePower;
    [SerializeField] public float resistCondition;
    [SerializeField] public float attackDelay;
    [SerializeField] public float Speed;
    [SerializeField] public int attackUnitThrough;
    [SerializeField] public int attackObjectThrough;
    [SerializeField] public int easilyTargeted = 50; //50%を平均値として低いほど狙われやすい
    [SerializeField] public float attackRange;
    [SerializeField] public float attackSize;
    [SerializeField] public float knockBack;

    /// 追加ステータス
    [Header("追加ステータス")]
    [SerializeField] public float attackLifetime;
    [SerializeField] public float attackDistance;
    [SerializeField] public float attackSpeed;
    [SerializeField] public float moveSpeed;
    [SerializeField] public float guardChance;
    [SerializeField] public float criticalChance;
    [SerializeField] public float criticalDamage;
    [SerializeField] public float comboDamage; // count x 10% ダメージアップ
    [SerializeField] public float comboCriticalCount; // コンボ何回毎にクリティカルするか
    public int comboCount = 0; // コンボカウント
    public float lastHitTime = 0; // 最後にダメージを与えた時間
    public int spreadCount = 0; // スプレッドカウント
    public float spreadDamage = 0; // スプレッドダメージ倍率
    public bool conditionRecavery = false; // 状態異常回復を行うか
    public int conditionRecaveryChance = 0; // 状態異常回復の確率(%)

    [Header("///////////////")]

    [Header("------ 状態異常の能力値-------")]
    public float conditionChance = 0; // 状態異常を与える確率(%)
    public bool poisonAttack = false; // 毒攻撃を行うか
    public float poisonAmount = 0; // 毒の効果量
    public float poisonDealTime = 0; // 毒の効果時間
    public bool bleedAttack = false; //　出血攻撃を行うか
    public float bleedAmount = 0; // 出血の効果量
    public float bleedDealTime = 0; // 出血の効果時間
    public bool stunAttack = false; // スタン攻撃を行うか
    public float stunDealTime = 0; // スタンの効果時間
    public bool paralysisAttack = false; // 麻痺攻撃を行うか
    public float paralysisDealTime = 0; // 麻痺の効果時間
    public bool weakAttack = false; // 脆弱攻撃を行うか
    public float weakDealTime = 0; // 脆弱の効果時間
    public bool defenceDownAttack = false; // 弱体攻撃を行うか
    public float defenceDownDealTime = 0; // 弱体の効果時間
    public bool bloodSuckAttack = false; // 吸血攻撃を行うか
    public float bloodSuckAmount = 0; // 吸血の効果量
    public bool regeneAttack = false; // リジェネ攻撃を行うか
    public float regeneAmount = 0; // リジェネの効果量
    public float regeneDealTime = 0; // リジェネの効果時間

    [Header("--- 鉄壁 ---")]
    [SerializeField] public bool ironWall = false; // 鉄壁状態であるか
    [Header("--- 範囲持続攻撃 ---")]
    [SerializeField] public bool areaAttack = false; // 範囲持続攻撃を行うか
    [SerializeField] public float areaAttackDamage = 0; // 範囲持続攻撃のダメージ
    [SerializeField] public GameObject areaAttackPrefab=null; // 範囲持続攻撃のプレハブのClone
    [SerializeField] public float areaDistance = 0.6f; // 範囲持続攻撃の範囲
    [Header("--- 範囲持続スピードダウン ---")]
    [SerializeField] public bool areaSlow = false; // 範囲持続スピードダウンを行うか
    [SerializeField] public float areaSlowAmount = 0; // 範囲持続スピードダウンの効果量
    [SerializeField] public GameObject areaSlowPrefab=null; // 範囲持続攻撃のプレハブのClone
    [SerializeField] public float areaSlowDistance = 0.6f; // 範囲持続攻撃の範囲

    [Header("///////////////")]
    ///

    [Header("///////////////")]
    [Header("------ 状態異常のステータス -------")]
    [Header("--- 毒 ---")]
    [SerializeField] public float poisonTime; // 毒の効果時間累積
    [SerializeField] public float poisonDamage; // くらった時にLVx0.5を加算する
    [SerializeField] public int poisonCount; // 毒の重ねがけ回数 max3
    [SerializeField] private Unit poisonAttacker; // 毒を与えたユニット
    [Header("--- 出血 ---")]
    [SerializeField] public float bleedTime; // 出血の効果時間累積
    [SerializeField] public float bleedDamage; // くらった時にLVx0.8を加算する
    [SerializeField] public int bleedCount; // 出血の重ねがけ回数 max3
    [SerializeField] private Unit bleedAttacker; // 出血を与えたユニット
    [Header("--- スタン ---")]
    [SerializeField] public float stunTime; // スタンの効果時間累積(その間動けないが、防御力2倍)
    [SerializeField] public bool previousStunCondition = false; // 前の時間の状態を記録
  
    [Header("--- 麻痺 ---")]
    [SerializeField] public float paralysisTime; // 麻痺の効果時間累積(その間DelayとmoveSpeedとattackSpeedを30%低下)
    [SerializeField] public bool previousParalysisCondition = false; // 前の時間の状態を記録

    [Header("--- 脆弱 ---")]
    [SerializeField] public float weakeTime; // 脆弱の効果時間累積(攻撃力が30%下がる)
    [SerializeField] public bool previousWeakeCondition = false; // 前の時間の状態を記録
    [Header("--- 弱体 ---")]
    [SerializeField] public float defenceDownTime; // 弱体の効果時間累積(防御が30%下がる)
    [SerializeField] public bool previousDefenceDownCondition = false; // 前の時間の状態を記録
    [Header("--- 吸血 ---")]
    [SerializeField] public float bloodDamage; // 吸血の効果量 ルーンレベルの累積x0.1 x 与えるダメージ
    [Header("--- リジェネ ---")]
    [SerializeField] public float regeneTime; // リジェネの効果時間累積
    [SerializeField] public float regeneDamage; // リジェネの効果量(%) ルーンレベルの累積x1.2 2秒毎に回復 
    [SerializeField] private Unit regeneAttacker; // リジェネを与えたユニット
    [Header("--- スロー ---")]
    [SerializeField] public float slowTime; // スローの効果時間累積
    [SerializeField] public float beforeMoveSpeed=0; // 移動速度を一時的に保存
    [SerializeField] public float beforeAttackDelay=0; // 攻撃速度を一時的に保存


    [Header("///////////////")]

    [SerializeField] public int targetJob;
    public bool enableEscape = false; // 逃走を行うか
    public bool enableTeleport = false; // テレポートを行うか
    [SerializeField] public float teleportation;
    [SerializeField] public float escape;
    [SerializeField] public float attackStanceDuration;
    [SerializeField] public float attackStanceDelay;
    [SerializeField] public int socketCount;
    [SerializeField] public int currentWeapons;
    [SerializeField] public int currentAttackEffects;
    [SerializeField] public int currentShields;
    [SerializeField] public int currentArmor;
    [SerializeField] public int currentAccessories;
    [SerializeField] public int mainSocket;
    [SerializeField] public int[] subSocket = new int[11];

    // 武器prefab設定
    public GameObject weaponPosition; // 武器の生成位置
    public TextMeshProUGUI hpText; // HP数値表示テキスト
    public GameObject HPBarRoot; // HPバーのルートオブジェクト
    private Animator HPBarAnimator; // HPバーのAnimatorコンポーネント
    public Slider hpBar;
    public Image hpColor;
    public TextMeshProUGUI damageText; // ダメージ数値表示テキスト
    public TextMeshProUGUI comboText; // コンボ数値表示テキスト
    [HideInInspector] public float maxHp;

    private GameManager gameManager; // GameManagerの参照
    private IventryUI iventryUI; // IventryUIの参照
    private WorldManager worldManager; // WorldManagerの参照
    private BattleManager battleManager; // BattleManagerの参照

    [Header("calc Condition")]
    private float lastDamageTime = 0; // 最後にダメージを受けた時間
    private float lastOneSecondTime = 0f; // 1秒ごとの処理タイマー

    void Start()
    {
        // 初期化
        condition = new bool[9]; 
        // Animatorコンポーネントを取得
        animator = GetComponent<Animator>();
        // ランダムなオフセットを設定 (0から1の範囲で)
        float randomOffset = UnityEngine.Random.Range(0f, animator.GetCurrentAnimatorStateInfo(0).length);

        // HPバーのAnimatorコンポーネントを取得
        HPBarAnimator = HPBarRoot.GetComponent<Animator>();

        // GameManagerのインスタンスを取得
        gameManager = GameManager.Instance;
        // IventryItemを取得
        iventryUI = gameManager.GetComponent<IventryUI>();
        // WorldManagerがあればインスタンスを取得
        if (WorldManager.Instance != null)
        {
            worldManager = WorldManager.Instance;
        }
        // BattleManagerがあればインスタンスを取得
        if (BattleManager.Instance != null)
        {
            battleManager = BattleManager.Instance;
        }

        // Playerの場合はスキルリストの取得
        if (gameObject.tag == "Ally")
        {
            if(iventryUI != null)
            {
                switch(ID)
                {
                    case 1:
                        IventrySkillList = iventryUI.IventrySkillList1;
                        break;
                    case 2:
                        IventrySkillList = iventryUI.IventrySkillList2;
                        break;
                    case 3:
                        IventrySkillList = iventryUI.IventrySkillList3;
                        break;
                    case 4:
                        IventrySkillList = iventryUI.IventrySkillList4;
                        break;
                    case 5:
                        IventrySkillList = iventryUI.IventrySkillList5;
                        break;
                    default:
                        Debug.LogError("unitNum is invalid");
                        return;
                }
            }
        }

        // tagがEnemyの場合はEnemyListDataを取得
        if (gameObject.tag == "Enemy")
        {
            // オブジェクト名から(Clone)を削除
            gameObject.name = gameObject.name.Replace("(Clone)", "");
            // オブジェクト名末尾2桁の数字を取得
            int unitId = int.Parse(gameObject.name.Substring(gameObject.name.Length - 2));
            // Enemyの場合はenemyListからunitIdに対応するデータを取得
            ItemData.EnemyListData enemyListData = gameManager.itemData.enemyList.Find(x => x.ID == unitId);
            if(enemyListData != null)
            {
                unitName = enemyListData.name;
                job = enemyListData.job;
                addLevel = enemyListData.addLevel;
                currentWeapons = enemyListData.weapons;
                currentShields = enemyListData.shields;
                currentArmor = enemyListData.armor;
                currentAccessories = enemyListData.accessories;
            }
        }
        else
        {
            // Allyの場合はjobListからjobに対応する初期装備を取得
            ChangeEqpByJob();
        }

        updateStatus();
        InitHp();

        // アニメーションを再生
        string stateName = job.ToString("D2") + "_idle";
        if(PlayAnimatorStateIfExists(stateName))
        {
            animator.Play(stateName); // 動いていない場合はIdleステートをセット
        }
    }

    void Update()
    {
        // 1秒起きの処理
        if (Time.time - lastOneSecondTime > 1)
        {
            // 1秒ごとの処理タイマーを更新(一番上にしないと何度も実行されてしまうため注意)
            lastOneSecondTime = Time.time;
            // 処理を追加予定
            //////////////////////////
            CalcOneSecond();
            //////////////////////////
        }
        // 2秒起きに状態効果の計算
        if (Time.time - lastDamageTime > 2)
        {
            // lastDamageTimeの更新(一番上にしないと何度も実行されてしまうため注意)
            lastDamageTime = Time.time;

            CalcCondition();
        }

    }

    /// <summary>
    /// 1秒ごとの処理
    /// </summary>
    private void CalcOneSecond()
    {
        // 範囲攻撃持続ダメージ
        if (areaAttack)
        {
            // 現在のシーン名を取得
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "BattleScene")
            {
                // SpriteRendererを表示する
                if (areaAttackPrefab != null) areaAttackPrefab.GetComponent<SpriteRenderer>().enabled = true;
                // 範囲攻撃の範囲内の敵にダメージを与える
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, areaDistance);
                foreach (Collider2D hitCollider in hitColliders)
                {
                    // ダメージが0.5以上で自分と異なるtagで、自分自身は無視
                    if (areaAttackDamage > 0.5f && (hitCollider.tag == "Enemy" || hitCollider.tag == "Ally") && hitCollider.tag != gameObject.tag && hitCollider.gameObject != gameObject)
                    {
                        // ダメージを与える
                        hitCollider.GetComponent<Unit>().TakeDamage(areaAttackDamage, this, true);
                    }
                }
            }
            else
            {
                // SpriteRendererを非表示にする
                if (areaAttackPrefab != null) areaAttackPrefab.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        // 範囲持続スピードダウン
        if (areaSlow)
        {
            // 現在のシーン名を取得
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "BattleScene")
            {
                // SpriteRendererを表示する
                if (areaSlowPrefab != null) areaSlowPrefab.GetComponent<SpriteRenderer>().enabled = true;
                // 範囲内の敵に効果を与える
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, areaSlowDistance);
                foreach (Collider2D hitCollider in hitColliders)
                {
                    // 自分と異なるtagで、自分自身は無視
                    if ((hitCollider.tag == "Enemy" || hitCollider.tag == "Ally") && hitCollider.tag != gameObject.tag && hitCollider.gameObject != gameObject)
                    {
                        // スピードを減少させる
                        hitCollider.GetComponent<Unit>().attackDelay = beforeAttackDelay - (beforeAttackDelay * areaSlowAmount);
                        hitCollider.GetComponent<Unit>().moveSpeed = beforeMoveSpeed - (beforeMoveSpeed * areaSlowAmount);
                        hitCollider.GetComponent<Unit>().slowTime = 1.5f; // 1.5秒持続
                    }
                }
            }
            else
            {
                // SpriteRendererを非表示にする
                if (areaSlowPrefab != null) areaSlowPrefab.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        slowTime -= 1; // スローの効果時間を1秒減少
        // スローの効果時間が0になったら速度を元に戻す
        if (slowTime < 0)
        {
            slowTime = 0;
            attackDelay = beforeAttackDelay;
            moveSpeed = beforeMoveSpeed;
        }
    }

    /// <summary>
    /// 2秒起きの状態異常効果の計算
    /// </summary>
    private void CalcCondition()
    {
        if (condition[1]) // 毒
        {
            // 効果時間が0になったら解除
            if (poisonTime <= 0)
            {
                condition[1] = false;
                // 初期化
                poisonTime = 0;
                poisonDamage = 0;
                poisonCount = 0;
                //  ステータスを更新
                updateStatus();
            }
            else
            {
                poisonTime -= 2;
                // 無属性（防御無視）ダメージを与える
                TakeDamage(poisonDamage, poisonAttacker, true);
            }
        }
        if (condition[2]) // 出血
        {
            // 効果時間が0になったら解除
            if (bleedTime <= 0)
            {
                condition[2] = false;
                // 初期化
                bleedTime = 0;
                bleedDamage = 0;
                bleedCount = 0;
                //  ステータスを更新
                updateStatus();
            }
            else
            {
                // 2秒毎LVx0.8のダメージ、重ねがけ3回までダメージを1.5倍、初回6秒、攻撃を受けるたびに効果時間を6秒延長(1分間で5回の確率でHit)
                bleedTime -= 2;
                // 無属性（防御無視）ダメージを与える
                TakeDamage(bleedDamage, bleedAttacker, true);
            }
        }
        if (condition[3]) // スタン
        {
            // 効果時間が0になったら解除
            if (stunTime <= 0)
            {
                condition[3] = false;
                // 初期化
                stunTime = 0;
                previousStunCondition = false;
                //  ステータスを更新
                updateStatus();
            }
            else
            {
                stunTime -= 2;
            }
            // 状態記録
            previousStunCondition = condition[3];
        }
        if (condition[4]) // 麻痺
        {
            // 効果時間が0になったら解除
            if (paralysisTime <= 0)
            {
                condition[4] = false;
                // 初期化
                paralysisTime = 0;
                previousParalysisCondition = false;
                //  ステータスを更新
                updateStatus();
            }
            else
            {
                paralysisTime -= 2;
            }
            // 状態記録
            previousParalysisCondition = condition[4];
        }
        if (condition[5]) // 脆弱
        {
            // 効果時間が0になったら解除
            if (weakeTime <= 0)
            {
                condition[5] = false;
                // 初期化
                weakeTime = 0;
                previousWeakeCondition = false;
                //  ステータスを更新
                updateStatus();
            }
            else
            {
                weakeTime -= 2;
            }
        }
        if (condition[6]) // 弱体
        {
            // 効果時間が0になったら解除
            if (defenceDownTime <= 0)
            {
                condition[6] = false;
                // 初期化
                defenceDownTime = 0;
                previousDefenceDownCondition = false;
                //  ステータスを更新
                updateStatus();
            }
            else
            {
                defenceDownTime -= 2;
            }
        }
        if (condition[8]) // リジェネ
        {
            // 効果時間が0になったら解除
            if (regeneTime <= 0)
            {
                condition[8] = false;
                // 初期化
                regeneTime = 0;
                regeneDamage = 0;
                //  ステータスを更新
                updateStatus();
            }
            else
            {
                regeneTime -= 2;
                // 2秒毎に最大Hp x regeneAmount%回復
                float amount = maxHp/100 * regeneDamage;
                Heal(amount, regeneAttacker, true);
            }
        }
    }

    /// <summary>
    /// 指定されたステートがAnimatorに存在する場合に再生します。
    /// </summary>
    /// <param name="stateName">再生するステートの名前</param>
    private bool PlayAnimatorStateIfExists(string stateName)
    {
        // state名をハッシュ値に変換
        int stateID = Animator.StringToHash(stateName);

        // 指定したレイヤーの中にstateIDのステートが存在するか確認
        return animator.HasState(0, stateID);
    }

    // ステータスの更新
    public void updateStatus()
    {
        // ユニットコントローラーを参照
        UnitController unitController = GetComponent<UnitController>();
        // jobListからjobに対応するステータスを取得
        ItemData.JobListData jobListData = gameManager.itemData.jobList.Find(x => x.ID == job);
        if (jobListData == null)
        {
            Debug.LogError("jobListData is not assigned.");
            return;
        }
        // JobListからステータスを読み込む
        float currentLevelScaleFactor = jobListData.levelScaleFactor;
        float currentMagic = jobListData.magic;
        float currentStr = jobListData.str;
        float currentDex = jobListData.dex;
        float currentResidtCondition = jobListData.levelResidtCondition;
        float currentAttackUnitThrough = jobListData.attackUnitThrough;
        float currentAttackObjectThrough = jobListData.attackObjectThrough;
        float currentKnockBack = jobListData.initKnockBack;
        float levelMagic = jobListData.levelMagic;
        float levelStr = jobListData.levelStr;
        float levelDex = jobListData.levelDex;
        float levelResidtCondition = jobListData.levelResidtCondition;
        float levelAttackUnitThrough = jobListData.levelAttackUnitThrough;
        float levelAttackObjectThrough = jobListData.levelAttackObjectThrough;
        float levelKnockBack = jobListData.levelKnockBack;
        int JtargetJob = jobListData.targetJob;
        float teleportations = jobListData.teleportation;
        float escapes = jobListData.escape;

        // itemDataからステータスを読み込む
        // 武器のステータスを取得
        ItemData.WpnListData wpnListData = gameManager.itemData.wpnList.Find(x => x.ID == currentWeapons);
        if (wpnListData == null)
        {
            Debug.LogError("unit name: " + gameObject.name + " currentWeapons: " + currentWeapons + " wpnListData is not assigned.");
            return;
        }
        // シールドのステータスを取得
        ItemData.EqpListData shieldListData = gameManager.itemData.eqpList.Find(x => x.ID == currentShields);
        if (shieldListData == null)
        {
            Debug.LogError("unit name: " + gameObject.name + " currentShields: " + currentShields + " shieldListData is not assigned.");
            return;
        }
        // 防具のステータスを取得
        ItemData.EqpListData armorListData = gameManager.itemData.eqpList.Find(x => x.ID == currentArmor);
        if (armorListData == null)
        {
            Debug.LogError("unit name: " + gameObject.name + " currentArmor: " + currentArmor + "armorListData is not assigned.");
            return;
        }// アクセサリのステータスを取得
        ItemData.EqpListData accessoriesListData = gameManager.itemData.eqpList.Find(x => x.ID == currentAccessories);
        if (accessoriesListData == null)
        {
            Debug.LogError("unit name: " + gameObject.name + " currentAccessories: " + currentAccessories + "accessoriesListData is not assigned.");
            return;
        }

        // 装備中の武器に固定ルーンがあった場合、敵味方共に固定ルーンを装備する。 (武器やルーンの交換処理はIventryUI.changeEquipmentで行う)
        // 武器に固定メインルーンがあるか確認
        if( wpnListData.fixMainRune !=0 )
        {
            Debug.Log(this.unitName + " が固定メインルーンを装備: " + wpnListData.fixMainRune);
            // mainSocketに固定メインルーンを装備
            mainSocket = wpnListData.fixMainRune;
        }
        // 事前にリストでサブルーンのIDを取得
        List<int> fixSubRunes = new List<int>{wpnListData.fixSubRune1,wpnListData.fixSubRune2,wpnListData.fixSubRune3};
        for(int i = 0; i < fixSubRunes.Count; i++)
        {
            if( fixSubRunes[i] != 0)
            {
                Debug.Log(this.unitName + " が固定サブルーン" + i + "を装備: " + fixSubRunes[i]);
                // subSocket[i]に固定サブルーンを装備
                subSocket[i] = fixSubRunes[i];
            }
        }

        // ルーンのステータスの初期化
        int addLevel = 0;
        float pysicalAttackMultiplier = 1.0f;
        float magicAttackMultiplier = 1.0f;
        float physicalDefenseMultiplier = 1.0f;
        float magicalDefenseMultiplier = 1.0f;
        float delayMultiplier = 1.0f;
        float aefSpeedMultiplier = 1.0f;
        float moveSpeedMultiplier = 1.0f;
        float maxDistanceMultiplier = 1.0f;
        float weaponScaleMultiplier = 1.0f;
        int attackCharThroughAdd = 0;
        int attackObjectThroughAdd = 0;
        float projectileLifetime = 1.0f;
        float conditionGuard = 0.0f;
        float guardChanceMultiplier = 1.0f;
        float comboDamageUp = 0.0f;
        float comboCriticalCountUp = 0.0f;
        float criticalChanceMult = 1.0f;
        float criticalDamageMult = 1.0f;
        float knockBackMultiplier = 1.0f;
        float poison = 0;
        float bleed = 0;
        float stun = 0;
        float paralysis = 0;
        float weak = 0;
        float defenceDown = 0;
        float bloodSuck = 0;
        float regene = 0;
        ironWall = false; // 鉄壁状態であるかを初期化
        areaAttack = false; // 範囲持続攻撃を行うかを初期化
        int areaAttackLv = 0; // 範囲持続攻撃のレベルを初期化
        conditionRecavery = false; // 状態異常回復を行うかを初期化
        conditionRecaveryChance = 0; // 状態異常回復の確率を初期化
        int easyTargetLv = 0; // 狙われやすさのレベルを初期化
        int targetOthersLv = 0; // 他を狙う確率のレベルを初期化
        enableEscape = false; // 逃走を行うかを初期化
        enableTeleport = false; // テレポートを行うかを初期化

        if(areaAttackPrefab != null)
        {
            Destroy(areaAttackPrefab);
            areaAttackPrefab = null;
        } // 範囲持続攻撃のプレハブを削除
        areaSlow = false; // 範囲持続スピードダウンを行うかを初期化
        int areaSlowLv = 0; // 範囲持続スピードダウンのレベルを初期化
        if (areaSlowPrefab != null)
        {
            Destroy(areaSlowPrefab);
            areaSlowPrefab = null;
        } // 範囲持続スピードダウンのプレハブを削除

        // 以下未設定
        float poisonGuard = 0.0f;
        float bleedGuard = 0.0f;
        float stunGuard = 0.0f;
        float paralysisGuard = 0.0f;
        float wakeGuard = 0.0f;
        float defenceDownGuard = 0.0f;

        // その他
        int spreadTotalLv = 0;

        // ルーンのステータスを取得
        spreadCount = 0; // スプレッドカウントの初期化

        // mainSocketとsubSocketに装備されているルーンのIDが0でなければリストに追加
        List<int> eqpRuneIDList = new List<int>{};
        if(mainSocket != 0) eqpRuneIDList.Add(mainSocket);
        for(int i = 0; i < subSocket.Length; i++)
        {
            if(subSocket[i] != 0) eqpRuneIDList.Add(subSocket[i]);
        }

        foreach(int eqpRuneId in eqpRuneIDList)
        {
            int runeId = eqpRuneId / 10;
            int runeLevel = eqpRuneId % 10;
            
            ItemData.RuneListData runeListData = gameManager.itemData.runeList.Find(x => x.ID == runeId);
            string runeName = runeListData.name;

            if (runeListData != null)
            {
                if(runeLevel == 1)
                {
                    if (runeListData.addLevelLv1 != 0) addLevel += runeListData.addLevelLv1;
                    if (runeListData.pysicalPowerLv1 != 0) pysicalAttackMultiplier *= runeListData.pysicalPowerLv1;
                    if (runeListData.magicalPowerLv1 != 0) magicAttackMultiplier *= runeListData.magicalPowerLv1;
                    if (runeListData.physicalDefenseLv1 != 0) physicalDefenseMultiplier *= runeListData.physicalDefenseLv1;
                    if (runeListData.magicalDefenseLv1 != 0) magicalDefenseMultiplier *= runeListData.magicalDefenseLv1;
                    if (runeListData.delayLv1 != 0) delayMultiplier *= runeListData.delayLv1;
                    if (runeListData.speedLv1 != 0) aefSpeedMultiplier *= runeListData.speedLv1;
                    if (runeListData.moveSpeedLv1 != 0) moveSpeedMultiplier *= runeListData.moveSpeedLv1;
                    if (runeListData.distanceLv1 != 0) maxDistanceMultiplier *= runeListData.distanceLv1;
                    if (runeListData.scaleLv1 != 0) weaponScaleMultiplier *= runeListData.scaleLv1;
                    if (runeListData.attackUnitThroughLv1 != 0) attackCharThroughAdd += runeListData.attackUnitThroughLv1;
                    if (runeListData.attackObjectThroughLv1 != 0) attackObjectThroughAdd += runeListData.attackObjectThroughLv1;
                    if (runeListData.timeLv1 != 0) projectileLifetime *= runeListData.timeLv1;
                    if (runeListData.conditionGuardLv1 != 0) conditionGuard += runeListData.conditionGuardLv1;
                    if (runeListData.guardChanceLv1 != 0) guardChanceMultiplier *= runeListData.guardChanceLv1;
                    if (runeListData.comboDamageLv1 != 0) comboDamageUp += runeListData.comboDamageLv1;
                    if (runeListData.comboCriticalLv1 != 0) comboCriticalCountUp += runeListData.comboCriticalLv1;
                    if (runeListData.criticalChanceLv1 != 0) criticalChanceMult *= runeListData.criticalChanceLv1;
                    if (runeListData.criticalDamageLv1 != 0) criticalDamageMult *= runeListData.criticalDamageLv1;
                    if (runeListData.knockBackLv1 != 0) knockBackMultiplier *= runeListData.knockBackLv1;
                    if (runeListData.poisonLv1 != 0) poison += runeListData.poisonLv1;
                    if (runeListData.bleedLv1 != 0) bleed += runeListData.bleedLv1;
                    if (runeListData.stunLv1 != 0) stun += runeListData.stunLv1;
                    if (runeListData.paralysisLv1 != 0) paralysis += runeListData.paralysisLv1;
                    if (runeListData.wakeLv1 != 0) weak += runeListData.wakeLv1;
                    if (runeListData.defenceDownLv1 != 0) defenceDown += runeListData.defenceDownLv1;
                    if (runeListData.bloodSuckLv1 != 0) bloodSuck += runeListData.bloodSuckLv1;
                    if (runeListData.regeneLv1 != 0) regene += runeListData.regeneLv1;

                    // 以下未設定
                    //if (runeListData.bloodTimeLv1 != 0) bloodTime *= runeListData.bloodTimeLv1;
                    if (runeListData.poisonGuardLv1 != 0) poisonGuard += runeListData.poisonGuardLv1;
                    if (runeListData.bleedGuardLv1 != 0) bleedGuard += runeListData.bleedGuardLv1;
                    if (runeListData.stunGuardLv1 != 0) stunGuard += runeListData.stunGuardLv1;
                    if (runeListData.paralysisGuardLv1 != 0) paralysisGuard += runeListData.paralysisGuardLv1;
                    if (runeListData.wakeGuardLv1 != 0) wakeGuard += runeListData.wakeGuardLv1;
                    if (runeListData.defenceDownGuardLv1 != 0) defenceDownGuard += runeListData.defenceDownGuardLv1;

                }
                if(runeLevel == 2)
                {
                    if (runeListData.addLevelLv2 != 0) addLevel += runeListData.addLevelLv2;
                    if (runeListData.pysicalPowerLv2 != 0) pysicalAttackMultiplier *= runeListData.pysicalPowerLv2;
                    if (runeListData.magicalPowerLv2 != 0) magicAttackMultiplier *= runeListData.magicalPowerLv2;
                    if (runeListData.physicalDefenseLv2 != 0) physicalDefenseMultiplier *= runeListData.physicalDefenseLv2;
                    if (runeListData.magicalDefenseLv2 != 0) magicalDefenseMultiplier *= runeListData.magicalDefenseLv2;
                    if (runeListData.delayLv2 != 0) delayMultiplier *= runeListData.delayLv2;
                    if (runeListData.speedLv2 != 0) aefSpeedMultiplier *= runeListData.speedLv2;
                    if (runeListData.moveSpeedLv2 != 0) moveSpeedMultiplier *= runeListData.moveSpeedLv2;
                    if (runeListData.distanceLv2 != 0) maxDistanceMultiplier *= runeListData.distanceLv2;
                    if (runeListData.scaleLv2 != 0) weaponScaleMultiplier *= runeListData.scaleLv2;
                    if (runeListData.attackUnitThroughLv2 != 0) attackCharThroughAdd += runeListData.attackUnitThroughLv2;
                    if (runeListData.attackObjectThroughLv2 != 0) attackObjectThroughAdd += runeListData.attackObjectThroughLv2;
                    if (runeListData.timeLv2 != 0) projectileLifetime *= runeListData.timeLv2;
                    if (runeListData.conditionGuardLv2 != 0) conditionGuard += runeListData.conditionGuardLv2;
                    if (runeListData.guardChanceLv2 != 0) guardChanceMultiplier *= runeListData.guardChanceLv2;
                    if (runeListData.comboDamageLv2 != 0) comboDamageUp += runeListData.comboDamageLv2;
                    if (runeListData.comboCriticalLv2 != 0) comboCriticalCountUp += runeListData.comboCriticalLv2;
                    if (runeListData.criticalChanceLv2 != 0) criticalChanceMult *= runeListData.criticalChanceLv2;
                    if (runeListData.criticalDamageLv2 != 0) criticalDamageMult *= runeListData.criticalDamageLv2;
                    if (runeListData.knockBackLv2 != 0) knockBackMultiplier *= runeListData.knockBackLv2;
                    if (runeListData.poisonLv2 != 0) poison += runeListData.poisonLv2;
                    if (runeListData.bleedLv2 != 0) bleed += runeListData.bleedLv2;
                    if (runeListData.stunLv2 != 0) stun += runeListData.stunLv2;
                    if (runeListData.paralysisLv2 != 0) paralysis += runeListData.paralysisLv2;
                    if (runeListData.wakeLv2 != 0) weak += runeListData.wakeLv2;
                    if (runeListData.defenceDownLv2 != 0) defenceDown += runeListData.defenceDownLv2;
                    if (runeListData.bloodSuckLv2 != 0) bloodSuck += runeListData.bloodSuckLv2;
                    if (runeListData.regeneLv2 != 0) regene += runeListData.regeneLv2;

                    // 以下未設定
                    //if (runeListData.bloodTimeLv2 != 0) bloodTime *= runeListData.bloodTimeLv2;
                    //if (runeListData.poisonTimeLv2 != 0) poisonTime *= runeListData.poisonTimeLv2;
                    if (runeListData.poisonGuardLv2 != 0) poisonGuard += runeListData.poisonGuardLv2;
                    if (runeListData.bleedGuardLv2 != 0) bleedGuard += runeListData.bleedGuardLv2;
                    if (runeListData.stunGuardLv2 != 0) stunGuard += runeListData.stunGuardLv2;
                    if (runeListData.paralysisGuardLv2 != 0) paralysisGuard += runeListData.paralysisGuardLv2;
                    if (runeListData.wakeGuardLv2 != 0) wakeGuard += runeListData.wakeGuardLv2;
                    if (runeListData.defenceDownGuardLv2 != 0) defenceDownGuard += runeListData.defenceDownGuardLv2;
                }


                if(runeLevel == 3)
                {
                    if (runeListData.addLevelLv3 != 0) addLevel += runeListData.addLevelLv3;
                    if (runeListData.pysicalPowerLv3 != 0) pysicalAttackMultiplier *= runeListData.pysicalPowerLv3;
                    if (runeListData.magicalPowerLv3 != 0) magicAttackMultiplier *= runeListData.magicalPowerLv3;
                    if (runeListData.physicalDefenseLv3 != 0) physicalDefenseMultiplier *= runeListData.physicalDefenseLv3;
                    if (runeListData.magicalDefenseLv3 != 0) magicalDefenseMultiplier *= runeListData.magicalDefenseLv3;
                    if (runeListData.delayLv3 != 0) delayMultiplier *= runeListData.delayLv3;
                    if (runeListData.speedLv3 != 0) aefSpeedMultiplier *= runeListData.speedLv3;
                    if (runeListData.moveSpeedLv3 != 0) moveSpeedMultiplier *= runeListData.moveSpeedLv3;
                    if (runeListData.distanceLv3 != 0) maxDistanceMultiplier *= runeListData.distanceLv3;
                    if (runeListData.scaleLv3 != 0) weaponScaleMultiplier *= runeListData.scaleLv3;
                    if (runeListData.attackUnitThroughLv3 != 0) attackCharThroughAdd += runeListData.attackUnitThroughLv3;
                    if (runeListData.attackObjectThroughLv3 != 0) attackObjectThroughAdd += runeListData.attackObjectThroughLv3;
                    if (runeListData.timeLv3 != 0) projectileLifetime *= runeListData.timeLv3;
                    if (runeListData.conditionGuardLv3 != 0) conditionGuard += runeListData.conditionGuardLv3;
                    if (runeListData.guardChanceLv3 != 0) guardChanceMultiplier *= runeListData.guardChanceLv3;
                    if (runeListData.comboDamageLv3 != 0) comboDamageUp += runeListData.comboDamageLv3;
                    if (runeListData.comboCriticalLv3 != 0) comboCriticalCountUp += runeListData.comboCriticalLv3;
                    if (runeListData.criticalChanceLv3 != 0) criticalChanceMult *= runeListData.criticalChanceLv3;
                    if (runeListData.criticalDamageLv3 != 0) criticalDamageMult *= runeListData.criticalDamageLv3;
                    if (runeListData.knockBackLv3 != 0) knockBackMultiplier *= runeListData.knockBackLv3;
                    if (runeListData.poisonLv3 != 0) poison += runeListData.poisonLv3;
                    if (runeListData.bleedLv3 != 0) bleed += runeListData.bleedLv3;
                    if (runeListData.stunLv3 != 0) stun += runeListData.stunLv3;
                    if (runeListData.paralysisLv3 != 0) paralysis += runeListData.paralysisLv3;
                    if (runeListData.wakeLv3 != 0) weak += runeListData.wakeLv3;
                    if (runeListData.defenceDownLv3 != 0) defenceDown += runeListData.defenceDownLv3;
                    if (runeListData.bloodSuckLv2 != 0) bloodSuck += runeListData.bloodSuckLv2;
                    if (runeListData.regeneLv3 != 0) regene += runeListData.regeneLv3;

                    // 以下未設定
                    //if (runeListData.bloodTimeLv3 != 0) bloodTime *= runeListData.bloodTimeLv3;
                    //if (runeListData.poisonTimeLv3 != 0) poisonTime *= runeListData.poisonTimeLv3;
                    if (runeListData.poisonGuardLv3 != 0) poisonGuard += runeListData.poisonGuardLv3;
                    if (runeListData.bleedGuardLv3 != 0) bleedGuard += runeListData.bleedGuardLv3;
                    if (runeListData.stunGuardLv3 != 0) stunGuard += runeListData.stunGuardLv3;
                    if (runeListData.paralysisGuardLv3 != 0) paralysisGuard += runeListData.paralysisGuardLv3;
                    if (runeListData.wakeGuardLv3 != 0) wakeGuard += runeListData.wakeGuardLv3;
                    if (runeListData.defenceDownGuardLv3 != 0) defenceDownGuard += runeListData.defenceDownGuardLv3;
                }
            }
            // ルーン名がspreadの場合はspreadCountを1増やす
            if(runeName == "spread")
            {
                spreadCount++;
                spreadTotalLv += runeLevel;
            }
            spreadDamage = spreadTotalLv * 0.25f;

            // --- 鉄壁 --- ルーン名がironWallならironWallをtrueにし、job=4(タンク)の場合「鉄壁」の効果を2倍にする。
            if(runeName == "ironWall")
            {
                ironWall = true;
                if (job == 4)
                {                        
                    physicalDefenseMultiplier *= 2;
                    magicalDefenseMultiplier *= 2;
                    guardChanceMultiplier *= 2;
                    poisonGuard *= 2;
                    bleedGuard *= 2;
                    stunGuard *= 2;
                    paralysisGuard *= 2;
                    wakeGuard *= 2;
                    defenceDownGuard *= 2;
                }
            }
            // --- 範囲持続攻撃 --- ルーンID名がareaAttackならareaAttackをtrueにする
            if(runeName == "areaAttack")
            {
                areaAttack = true; // １つでも装備していたら範囲持続攻撃を行う
                // ルーンのトータルLvを取得
                areaAttackLv += runeLevel;
            }
            // --- 範囲持続スピードダウン --- ルーンID名がareaSlowならareaSlowをtrueにする
            if(runeName == "areaSlow")
            {
                areaSlow = true; // １つでも装備していたら範囲持続スピードダウンを行う
                // ルーンのトータルLvを取得
                areaSlowLv += runeLevel;
            }
            // --- 状態異常回復 --- ルーンID名がconditionRecaveryならconditionRecaveryをtrueにする
            if(runeName == "conditionRecavery")
            {
                conditionRecavery = true; // １つでも装備していたら状態異常回復を行う
                // ルーンのトータルLvを取得
                conditionRecaveryChance += runeLevel * 15;
                if (runeLevel == 3) conditionRecaveryChance += 5;
            }
            // --- 狙われやすい --- ルーンID名がeasyTargetならeasyTargetLvを加算
            if(runeName == "easyTarget")
            {
                easyTargetLv += runeLevel;
            }
            // --- 狙われにくい --- ルーンID名がtargetOthersならtargetOthersLvを加算
            if(runeName == "targetOthers")
            {
                targetOthersLv += runeLevel;
            }
            // --- 逃走 --- ルーンID名がescape又はescapeが1以上ならenableEscapeをtrue
            if(runeName == "escape")
            {
                enableEscape = true;
            }
            // --- テレポ --- ルーンID名がteleportation又はteleportationが1以上ならenableTeleportをtrue
            if(runeName == "teleportation")
            {
                enableTeleport = true;
            }
        }
        
        // ステータスを計算する
        int baseExperience = 10; // レベルアップに必要な基本経験値
        currentLevel = (int)Math.Sqrt(totalExp / (baseExperience * currentLevelScaleFactor)) + addLevel + 1;
        // タグがEnemyの場合は(currentWorldBackGround-1)*3をレベルに加算
        if (gameObject.tag == "Enemy")
        {
            currentLevel += (worldManager.currentWorld - 1) * 3;

            // gameManagerのroomOptionsのmonsterLevelUpがtrueの場合はモンスターのレベルを1上げる
            if (gameManager.roomOptions.monsterLevelUp)
            {
                currentLevel++;
            }
        }

        // 次のレベルに必要な経験値を計算
        int nextLevel = currentLevel + 1;
        nextLevelExp = Mathf.Pow((nextLevel - addLevel - 1), 2) * baseExperience * currentLevelScaleFactor;

        // 残りの必要経験値
        remainingExp = (int)nextLevelExp - totalExp;

        // ステータスを計算
        Hp = (currentStr + (currentLevel - 1) * levelStr) * 10 + wpnListData.hp + shieldListData.hp + armorListData.hp + accessoriesListData.hp;
        physicalAttackPower = (currentStr + (currentLevel - 1) * levelStr) / 1 + wpnListData.physicalAttackPower + shieldListData.physicalAttackPower + armorListData.physicalAttackPower + accessoriesListData.physicalAttackPower;
        physicalAttackPower *= pysicalAttackMultiplier; // ルーンの物理攻撃力倍率を適用
        magicalAttackPower = (currentMagic + (currentLevel - 1) * levelMagic) / 1 + wpnListData.magicalAttackPower + shieldListData.magicalAttackPower + armorListData.magicalAttackPower + accessoriesListData.magicalAttackPower;
        magicalAttackPower *= magicAttackMultiplier; // ルーンの魔法攻撃力倍率を適用
        physicalDefensePower = (currentStr + (currentLevel - 1) * levelStr) / 10 + wpnListData.physicalDefensePower + shieldListData.physicalDefensePower + armorListData.physicalDefensePower + accessoriesListData.physicalDefensePower;
        physicalDefensePower *= physicalDefenseMultiplier; // ルーンの物理防御力倍率を適用
        magicalDefensePower = (currentMagic + (currentLevel - 1) * levelMagic) / 10 + wpnListData.magicalDefensePower + shieldListData.magicalDefensePower + armorListData.magicalDefensePower + accessoriesListData.magicalDefensePower;
        magicalDefensePower *= magicalDefenseMultiplier; // ルーンの魔法防御力倍率を適用
        resistCondition = (currentResidtCondition + (currentLevel - 1) * levelResidtCondition) / 10 + wpnListData.resistCondition + shieldListData.resistCondition + armorListData.resistCondition + accessoriesListData.resistCondition;
        resistCondition += (conditionGuard * 100); // ルーンの状態異常耐性倍率を適用
        attackDelay =  2.0f + (currentDex + (currentLevel - 1) * levelDex) / 10 + wpnListData.attackDelay + shieldListData.attackDelay + armorListData.attackDelay + accessoriesListData.attackDelay;
        attackDelay *= delayMultiplier; // ルーンの攻撃速度倍率を適用
        Speed =  3 + (currentDex + (currentLevel - 1) * levelDex) / 10 + wpnListData.speed + shieldListData.speed + armorListData.speed + accessoriesListData.speed;
        attackSpeed = Speed * aefSpeedMultiplier; // ルーンの攻撃速度倍率を適用
        moveSpeed = Speed * moveSpeedMultiplier; // ルーンの移動速度倍率を適用
        attackUnitThrough = (int)(currentAttackUnitThrough + (currentLevel - 1) * levelAttackUnitThrough) + wpnListData.attackUnitThrough + shieldListData.attackUnitThrough + armorListData.attackUnitThrough + accessoriesListData.attackUnitThrough;
        attackUnitThrough += attackCharThroughAdd; // ルーンのキャラクター貫通力を適用
        attackObjectThrough = (int)(currentAttackObjectThrough + (currentLevel - 1) * levelAttackObjectThrough) + wpnListData.attackObjectThrough + shieldListData.attackObjectThrough + armorListData.attackObjectThrough + accessoriesListData.attackObjectThrough;
        attackObjectThrough += attackObjectThroughAdd; // ルーンのオブジェクト貫通力を適用
        attackSize = 1 + wpnListData.attackSize + shieldListData.attackSize + armorListData.attackSize + accessoriesListData.attackSize;
        attackSize *= weaponScaleMultiplier; // ルーンの攻撃サイズ倍率を適用
        knockBack = ((currentKnockBack + (currentLevel - 1) * levelKnockBack) + wpnListData.knockBack + shieldListData.knockBack + armorListData.knockBack + accessoriesListData.knockBack) / 100;
        knockBack *= knockBackMultiplier; // ルーンのノックバック倍率を適用
        targetJob = JtargetJob;
        teleportation = teleportations + wpnListData.teleportation + shieldListData.teleportation + armorListData.teleportation + accessoriesListData.teleportation;
        escape = escapes + wpnListData.escape + shieldListData.escape + armorListData.escape + accessoriesListData.escape;
        attackRange = wpnListData.attackRange;
        attackRange *= maxDistanceMultiplier; // ルーンの攻撃距離倍率を適用 
        attackStanceDuration = wpnListData.attackStanceDuration;
        attackStanceDelay = wpnListData.attackStanceDelay;
        socketCount = wpnListData.socketCount + shieldListData.socketCount + armorListData.socketCount + accessoriesListData.socketCount;
        attackLifetime = 1;
        attackLifetime = projectileLifetime; // ルーンの攻撃寿命を適用
        attackDistance = wpnListData.attackRange;
        attackDistance *= maxDistanceMultiplier; // ルーンの攻撃距離倍率を適用
        guardChance = shieldListData.guardChance; // シールドガード確率を適用
        guardChance *= guardChanceMultiplier; // ルーンのガード確率倍率を適用
        criticalChance = (currentDex + (currentLevel - 1) * levelDex) / 100 + wpnListData.criticalChance + shieldListData.criticalChance + armorListData.criticalChance + accessoriesListData.criticalChance;
        criticalChance *= criticalChanceMult; // ルーンのクリティカル確率倍率を適用
        criticalDamage = 3 + wpnListData.criticalDamage + shieldListData.criticalDamage + armorListData.criticalDamage + accessoriesListData.criticalDamage;
        criticalDamage *= criticalDamageMult; // ルーンのクリティカルダメージ倍率を適用
        comboDamage = (1.0f + comboDamageUp) / 10; // ルーンのコンボダメージを加算
        comboCriticalCount =  20 - comboCriticalCountUp; // ルーンのコンボクリティカルカウントを加算

        // 状態異常の能力値を計算
        // 初期化
        poisonAttack = false;
        bleedAttack = false;
        stunAttack = false;
        paralysisAttack = false;
        weakAttack = false;
        defenceDownAttack = false;
        regeneAttack = false;
        bloodSuckAttack = false;
        conditionChance = 0;
        poisonAmount = 0;
        bleedAmount = 0;
        stunDealTime = 0;
        paralysisDealTime = 0;
        weakDealTime = 0;
        defenceDownDealTime = 0;
        bloodSuckAmount = 0;
        regeneAmount = 0;
        poisonDealTime = 0;
        bleedDealTime = 0;
        regeneDealTime = 0;

        // 適用
        conditionChance = 0.1f;

        poisonAmount = poison * 10;
        bleedAmount = bleed * 10;
        stunDealTime = stun/3;
        paralysisDealTime = paralysis/3;
        weakDealTime = weak/3;
        defenceDownDealTime = defenceDown/3;
        bloodSuckAmount = bloodSuck;
        regeneAmount = regene;
        if(job == 0) 
        {
            bloodSuckAmount *= 3;
        }
        if(job == 2) 
        {
            poisonAmount *= 3;
            bleedAmount *= 3;
            stunDealTime *= 3;
            paralysisDealTime *= 3;
            weakDealTime *= 3;
            defenceDownDealTime *= 3;
        }
        if(job == 3) 
        {
            regeneAmount *= 3;
        }

        poisonAttack = poisonAmount > 0;
        if (poisonAttack) poisonDealTime = 10;
        bleedAttack = bleedAmount > 0;
        if (bleedAttack) bleedDealTime = 6;
        stunAttack = stunDealTime > 0;
        paralysisAttack = paralysisDealTime > 0;
        weakAttack = weakDealTime > 0;
        defenceDownAttack = defenceDownDealTime > 0;
        bloodSuckAttack = bloodSuckAmount > 0;
        condition[7] = bloodSuckAttack; // 吸血ルーン所持の場合は常時condition[7]をtrueにする
        regeneAttack = regeneAmount > 0;
        if (regeneAttack) regeneDealTime = 60;

        // 状態異常によるステータスの変化
        if (condition[4]) // 麻痺の場合は移動速度、攻撃速度、攻撃間隔を30%減
        {
            moveSpeed *= 0.7f;
            attackSpeed *= 0.7f;
            attackDelay *= 0.7f;
        }
        if (condition[5]) // 脆弱の場合は物理攻撃力を30%減
        {
            physicalAttackPower *= 0.7f;
            magicalAttackPower *= 0.7f;
        }
        if (condition[6]) // 弱体の場合は物理防御力を30%減
        {
            physicalDefensePower *= 0.7f;
            magicalDefensePower *= 0.7f;
        }

        // タンクの場合はHP、防御力、を2倍にする代わりに移動速度やDelayを半減
        if (job == 4 || job == 34) Hp *= 2;
        {
            Hp *= 2;
            physicalDefensePower *= 2;
            magicalDefensePower *= 2;
            moveSpeed /= 2;
            attackDelay /= 2;
        }

        // プレイヤー状態異常パネルの表示
        if (gameObject.tag == "Ally")
        {
            int unitID = this.ID;
            GameObject conditionPanel = gameManager.ConditionPanelList[unitID - 1];
            // conditionPanelの子オブジェクトをすべて非表示にする
            int index = 0;
            foreach (Transform child in conditionPanel.transform)
            {
                child.gameObject.SetActive(false);
                // 該当の状態異常にかかっている場合は表示する
                if(index < 8 && condition[index+1])
                {
                    child.gameObject.SetActive(true);
                }
                index++;
            }
        }

        // Ememytagの場合、
        if (gameObject.tag == "Enemy")
        {
            // gameManagerのroomOptionsのmonsterHpUpがtrueの場合はモンスターのHPを1.5倍にする
            if(gameManager.roomOptions.monsterHpUp) Hp *= 1.5f;
            // gameManagerのroomOptionsのmonsterAtkUpがtrueの場合はモンスターの物理＆魔法攻撃力を1.5倍にする
            if(gameManager.roomOptions.monsterAtkUp)
            {
                physicalAttackPower *= 1.5f;
                magicalAttackPower *= 1.5f;
            }
            // gameManagerのroomOptionsのmonsterDefUpがtrueの場合はモンスターの物理＆魔法防御力を1.5倍にする
            if(gameManager.roomOptions.monsterDefUp)
            {
                physicalDefensePower *= 1.5f;
                magicalDefensePower *= 1.5f;
            }
            // HpTextにUnitNameとレベルを表示
            hpText.text = unitName + "  Lv." + currentLevel;
        }

        // 持続効果の計算の為、装備結果を保存
        beforeAttackDelay = attackDelay; // 攻撃間隔を保存
        beforeMoveSpeed = moveSpeed; // 攻撃速度を保存

        // 範囲持続攻撃がtrueの場合の処理
        if (areaAttack)
        {
            if (areaAttackPrefab == null)
            {
                areaAttackPrefab = Resources.Load<GameObject>("Prefabs/AttackEffects/Aef_areaAttack");
                areaAttackPrefab = Instantiate(areaAttackPrefab, transform.position, Quaternion.identity);
                // 名前からCloneを削除
                areaAttackPrefab.name = areaAttackPrefab.name.Replace("(Clone)", "");
                // this.gameObjectの子オブジェクトにする
                areaAttackPrefab.transform.parent = transform;
                areaAttackPrefab.transform.localPosition = new Vector3(0, -0.27f, 0);
                // 初期は非表示にする
                areaAttackPrefab.GetComponent<SpriteRenderer>().enabled = false;
            } 
            areaAttackDamage = 0.25f * physicalAttackPower;
            areaDistance = 0.6f + ((areaAttackLv-1) * 0.2f);
            // ルーンレベルによって範囲攻撃の大きさを変更
            areaAttackPrefab.transform.localScale = new Vector3(4.0f + (4.0f * (areaAttackLv-1)/3.0f), 0.8f + (0.8f * (areaAttackLv-1)/3.0f), 4.0f + (4.0f * (areaAttackLv-1)/3.0f));
        }
        // 範囲持続スピードダウンがtrueの場合の処理
        if (areaSlow)
        {
            if (areaSlowPrefab == null)
            {
                areaSlowPrefab = Resources.Load<GameObject>("Prefabs/AttackEffects/Aef_areaSlow");
                areaSlowPrefab = Instantiate(areaSlowPrefab, transform.position, Quaternion.identity);
                // 名前からCloneを削除
                areaSlowPrefab.name = areaSlowPrefab.name.Replace("(Clone)", "");
                // this.gameObjectの子オブジェクトにする
                areaSlowPrefab.transform.parent = transform;
                areaSlowPrefab.transform.localPosition = new Vector3(0, -0.27f, 0);
                // 初期は非表示にする
                areaSlowPrefab.GetComponent<SpriteRenderer>().enabled = false;
            } 
            areaSlowAmount = 0.17f * areaSlowLv; // LVが上がるにつれてスローの効果が大きくなる(Lv x 現状速度の1/6づつ減少)
            areaSlowDistance = 0.6f + ((areaSlowLv-1) * 0.2f);
            // ルーンレベルによって範囲攻撃の大きさを変更
            areaSlowPrefab.transform.localScale = new Vector3(4.0f + (4.0f * (areaSlowLv-1)/3.0f), 0.8f + (0.8f * (areaSlowLv-1)/3.0f), 4.0f + (4.0f * (areaSlowLv-1)/3.0f));
        }

        // 狙われやすさの計算
        easilyTargeted = 50 - (easyTargetLv * 15) + (targetOthersLv * 15);

        // HPの初期化
        maxHp = Hp;

        // 武器プレファブを変更する
        GameObject wpn = Resources.Load<GameObject>("Prefabs/Weapons/" + currentWeapons.ToString("D6"));
        ChangeWeapon(wpn);

        // UnitControllerとAttackControllerにステータスを設定する
        SetStatusUnitController(GetComponent<UnitController>(), GetComponent<AttackController>());

        // ユニットのスキルパネルを更新
        if (gameObject.tag == "Ally")
        {
            iventryUI.UpdateUnitSkillUI(this.gameObject);
        }
    }

    // 武器プレファブを変更する
    public void ChangeWeapon(GameObject wpn)
    {
        //// 武器prefabの変更
        if(weaponPosition!=null)
        {
            //unitのweaponPositionの子オブジェクトをすべて削除
            foreach (Transform child in weaponPosition.transform)
            {
                Destroy(child.gameObject);
            }
            // 武器prefabを生成
            GameObject weapon = Instantiate(wpn, weaponPosition.transform.position, Quaternion.identity);
            weapon.transform.SetParent(weaponPosition.transform);
            // weaponPrefabにセットしてあるコンポーネントをすべて非アクティブにする
            DisableAll(weapon);
            //weaponPrefabの子オブジェクトをアクティブにする
            foreach (Transform child in weapon.transform)
            {
                child.gameObject.SetActive(true);
                // オブジェクト名が"scale"の場合はステータスのattackSizeを適用
                if (child.gameObject.name == "scale")
                {
                    child.localScale *= attackSize;
                }
            }
            weapon.SetActive(false);
            // unitのweaponPrefabにweaponをセット
            // weaponの名前から(Clone)を削除
            weapon.name = weapon.name.Replace("(Clone)", "");
            AttackController attackController = GetComponent<AttackController>();
            attackController.weaponPrefab = weapon;
            attackController.projectilePrefab = weapon.GetComponent<ItemDandDHandler>().wpnAef;
        }
    }

    // 指定したゲームオブジェクトのすべてのコンポーネントを無効化するメソッド
    public void DisableAll(GameObject targetObject)
    {
        // ゲームオブジェクトにアタッチされているすべてのコンポーネントを取得
        Component[] components = targetObject.GetComponents<Component>();

        // 各コンポーネントを順番に無効化
        foreach (Component component in components)
        {
            // コンポーネントの型に応じて、enabled プロパティがあるものを無効化
            if (component is Behaviour)
            {
                ((Behaviour)component).enabled = false;
            }
            else if (component is Renderer)
            {
                ((Renderer)component).enabled = false;
            }
            else if (component is Collider)
            {
                ((Collider)component).enabled = false;
            }
            else if (component is Rigidbody) 
            {
                ((Rigidbody)component).isKinematic = true; // Rigidbodyは enabled を持たないため isKinematic を使う
            }
            // その他のタイプのコンポーネントに対応する場合は、ここに追加する
        }
    }

    // statusをUnitControllerとAttackControllerに設定する
    public void SetStatusUnitController(UnitController unitController, AttackController attackController)
    {
        // ProjectileBehaviorコンポーネントを取得
        ProjectileBehavior projectileBehavior = attackController.projectilePrefab.GetComponent<ProjectileBehavior>();

        // テレポート設定
        if(teleportation  >= 1 || enableTeleport) 
        {
            enableTeleport = true;
            unitController.enableTeleport = true;
            unitController.teleportDistance = Speed / 2;
            unitController.teleportInterval = 3.0f;
        }
        else 
        {
            unitController.enableTeleport = false;
        }
        // 逃走機能設定
        if(escape >= 1)
        {
            unitController.enableEscape = true;
        }
        // ターゲット設定(回復の場合は自分と同じtagをターゲットにする)
        // attackController.projectilePrefab.attributesの要素が1つでもHealingの場合はtargetTagを自分と同じtagにする
        if (projectileBehavior != null)
        {
            // attributesのすべての要素を文字列として取得
            List<ProjectileBehavior.Attribute> attributes = projectileBehavior.attributes;

            unitController.targetSameTag = false;
            unitController.targetSameTagWpn = false;
            for (int i = 0; i < attributes.Count; i++)
            {
                string attributeName = attributes[i].ToString();  // Enumから文字列を取得
                if (attributeName == "Healing")
                {
                    unitController.targetSameTag = true;
                    unitController.targetSameTagWpn = true;
                }
            }
        }
        // 移動速度
        unitController.movementSpeed = Speed / 10.0f;
        // 攻撃範囲
        unitController.approachRange = attackRange;
        // 立ち止まり攻撃の設定
        if (attackStanceDuration > 0)
        {
            unitController.enableAttackStance = true;
            unitController.attackStanceDuration = attackStanceDuration;
            unitController.attackDelay = attackStanceDelay;
        }

    }

    // HPの初期化(全回復)
    public void InitHp()
    {
        currentHp = maxHp;
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
        if (hpText != null)
        {
            // tagがAllyの場合はHP数値を表示
            if (gameObject.tag == "Ally")
            {
                int dispHp = (int)currentHp;
                // HPの表示が0未満にならないようにする
                if (dispHp < 0)
                {
                    dispHp = 0;
                }
                hpText.text = dispHp.ToString("000") + " / " + maxHp.ToString("000");
            }
        }
    }

    // HPを減少させるメソッド
    public void TakeDamage(float damage, Unit attacker, bool defenseless = false)
    {
        // gurdChanceの確率でガード
        if (guardChance > 0 && !defenseless)
        {
            float guardRandom = UnityEngine.Random.Range(0.0f, 100.0f);
            if (guardRandom < guardChance*100)
            {
                // ガード音を再生
                AkSoundEngine.PostEvent("SE_Gurd", gameObject);
                // 盾のプレハブを取得
                GameObject shield = Resources.Load<GameObject>("Prefabs/Equipment/" + currentShields.ToString("D6"));
                // 盾のプレハブを生成
                GameObject shieldPrefab = Instantiate(shield, transform.position, Quaternion.identity);
                shieldPrefab.transform.SetParent(transform);
                // shieldPrefabにセットしてあるItemDandDHandlerとBoxCollider2Dコンポーネントを非アクティブにする
                shieldPrefab.GetComponent<ItemDandDHandler>().enabled = false;
                shieldPrefab.GetComponent<BoxCollider2D>().enabled = false;
                // shieldPrefabの名前から(Clone)を削除
                shieldPrefab.name = shieldPrefab.name.Replace("(Clone)", "");

                // weaponPrefabの子オブジェクトを検索してscaleという名前のオブジェクトがあったら、そこに装備中の盾を表示
                if (GetComponent<AttackController>().weaponPrefab != null)
                {
                    // weaponPrefabが無効な場合でも子オブジェクトを検索する
                    Transform[] children = GetComponent<AttackController>().weaponPrefab.GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in children)
                    {
                        if (child.name == "scale")
                        {
                            // 生成した盾をscaleのワールド座標に移動
                            shieldPrefab.transform.position = child.position;
                            break;
                        }
                    }
                }

                // 1秒後にshieldPrefabを削除
                Destroy(shieldPrefab, 0.5f);
                return; // ガードした場合はダメージ処理を終了
            }
        }

        // attacjerのknockBack確率でノックバック
        float knockBackRandom = UnityEngine.Random.Range(0.0f, 100.0f);
        if (knockBackRandom < attacker.knockBack * 100 && !defenseless)
        {
            // ノックバック音を再生
            //AkSoundEngine.PostEvent("SE_KnockBack", gameObject);
            // ノックバック処理
            Vector2 direction = gameObject.transform.position - attacker.transform.position;
            direction.Normalize();
            // attackerの逆方向にattacker.knockBack分移動する(knockBack距離分移動)
            gameObject.transform.position += new Vector3(direction.x, direction.y, 0) * attacker.knockBack;
            
        }
        // --- 逃走が有効の場合攻撃を受けたら逃走を開始 --- 
        else if (enableEscape)             
        {
            gameObject.GetComponent<UnitController>().enableEscape = true;
        }


        // 攻撃元にコンボ回数の更新通知を送る.
        if (attacker.gameObject.tag == "Ally")
        {
            attacker.comboCountUpdate();
        }

        // クリティカルダメージの計算
        // attakerのクリティカル確率を取得
        // 又はattakerのcomboCountがcomboCriticalCountの倍数の場合はクリティカルダメージを適用
        float criticalRandom = UnityEngine.Random.Range(0.0f, 100.0f);
        bool isCritical = false;
        if ((criticalRandom < attacker.criticalChance * 100 || (attacker.comboCount > 0 && attacker.comboCount % attacker.comboCriticalCount == 0)) && !defenseless )
        {
            isCritical = true;
            damage *= attacker.criticalDamage;
            // クリティカル音を再生
            AkSoundEngine.PostEvent("SE_Critical", gameObject);
        }
        else
        {
            // 通常のダメージ音を再生
            AkSoundEngine.PostEvent("SE_Hit", gameObject);
        }

        // コンボ回数に応じてダメージを増加
        if ( !defenseless) damage += attacker.comboDamage * attacker.comboCount;

        //////// ここまででダメージの計算は終了 ////////
        
        // 状態異常効果を確率で与える(くらわせる)
        if(attacker != this && !defenseless) ConditionAttack(attacker);

        // 攻撃元が吸血状態の場合は攻撃元がダメージのbloodAmount x damage分回復
        if (attacker.condition[7] && !defenseless)
        {
            attacker.Heal(attacker.bloodSuckAmount * damage , attacker);
        }

        // tagがEnemyの場合はダメージを表示する
        if (gameObject.tag == "Enemy")
        {
            // damageTextを複製する
            GameObject damageObjClone = Instantiate(damageText.gameObject, damageText.transform.position, damageText.transform.rotation);
            damageObjClone.transform.SetParent(damageText.transform.parent); // 親を設定

            TextMeshProUGUI damageTextClone = damageObjClone.GetComponent<TextMeshProUGUI>();

            // ダメージ数値のRectTransformを取得し、PosXとPosYを-10.0~10.0のランダム値に変更
            RectTransform damageTextRect = damageObjClone.GetComponent<RectTransform>();
            //位置とスケールをコピー元と同じにする
            damageTextRect.localScale = damageText.GetComponent<RectTransform>().localScale;
            //ランダムな位置にする
            damageTextRect.localPosition = new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), UnityEngine.Random.Range(-10.0f, 10.0f), 0);

            // ダメージ数値を四捨五入して表示
            damageTextClone.text = Mathf.Round(damage).ToString();
            // ダメージの桁数に応じてフォントサイズを拡大(10の位を基準にする)
            damageTextClone.fontSize = 18 + (int)(Mathf.Log10(damage) * 10);
            if (isCritical)
            {
                damageTextClone.color = Color.yellow;
            }
            else
            {
                damageTextClone.color = Color.white;
            }

            // textアニメーションを開始
            StartCoroutine(AnimateDamageText(damageTextRect, damageTextClone));

            //// ダメージを記録
            // BattleManagerがあればインスタンスを取得
            if (BattleManager.Instance != null) battleManager = BattleManager.Instance;
            if (battleManager != null)
            {
                // unitDamageにダメージを記録
                battleManager.RecordUnitDamage(attacker.ID - 1, (int)damage);
            }
        }

        // Hp減少しないフラグがfalseなら　HPを減少させる
        if (!isNoHpReduction) currentHp -= damage;
        UpdateHpBar();

        // unitSpriteを光らせる
        if (gameObject.activeInHierarchy) // ゲームオブジェクトがアクティブな場合のみ実行
        {
            StartCoroutine(FlashSprite(new Color(0.9f, 0.9f, 0.9f, 1.0f)));
        }

        // すでにユニットが死亡していたら何もしない(消滅するまでに複数回呼び出されることがあるため)
        if(live == false)
        {
            return;
        }
        // HPが1より低くなったら死亡
        if (currentHp < 1.0f)
        {
            live = false;
            currentHp = 0;
            Die(attacker.ID);
        };

    }

    // ダメージ数値のアニメーション
    IEnumerator AnimateDamageText(RectTransform damageTextRect, TextMeshProUGUI damageTextClone , bool isCombo = false)
    {
        // フェーズ1: 0.1秒でスケールを0.5 ~ 1.4、alphaを0 ~ 0.64に変更
        float duration1 = 0.1f;
        float elapsedTime = 0f;

        Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
        Vector3 endScale = new Vector3(1.4f, 1.4f, 1f);
        Color startColor = new Color(damageTextClone.color.r, damageTextClone.color.g, damageTextClone.color.b, 0f);
        Color endColor = new Color(damageTextClone.color.r, damageTextClone.color.g, damageTextClone.color.b, 0.64f);

        while (elapsedTime < duration1)
        {
            float t = elapsedTime / duration1;
            damageTextRect.localScale = Vector3.Lerp(startScale, endScale, t);
            damageTextClone.color = Color.Lerp(startColor, endColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 最終状態を設定
        damageTextRect.localScale = endScale;
        damageTextClone.color = endColor;

        // フェーズ2: 0.4秒間維持
        yield return new WaitForSeconds(0.4f);

        // フェーズ3: 1秒でスケールを1.4 ~ 1.0、alphaを0.64 ~ 0に変更
        float duration3 = 1f;
        elapsedTime = 0f;

        Vector3 startScale2 = new Vector3(1.4f, 1.4f, 1f);
        Vector3 endScale2 = new Vector3(1.0f, 1.0f, 1f);
        Color startColor2 = new Color(damageTextClone.color.r, damageTextClone.color.g, damageTextClone.color.b, 0.64f);
        Color endColor2 = new Color(damageTextClone.color.r, damageTextClone.color.g, damageTextClone.color.b, 0f);

        while (elapsedTime < duration3)
        {
            float t = elapsedTime / duration3;
            damageTextRect.localScale = Vector3.Lerp(startScale2, endScale2, t);
            damageTextClone.color = Color.Lerp(startColor2, endColor2, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 最終状態を設定
        damageTextRect.localScale = endScale2;
        damageTextClone.color = endColor2;

        // コンボテキストでなければアニメーションが終わったらオブジェクトを削除
        if (!isCombo)
        {
            Destroy(damageTextRect.gameObject);
        }
    }


    // unitSpriteを光らせる
    private IEnumerator FlashSprite(Color flashColor)
    {
        // ハードライトの色を設定
        unitSprite.material.SetColor("_HardlightColor", flashColor);
        yield return new WaitForSeconds(0.1f);
        // ハードライトの色を元に戻す
        unitSprite.material.SetColor("_HardlightColor", new Color(0.5f, 0.5f, 0.5f, 1.0f));
    }

    // hitされた相手からコンボの更新通知を受け取りコンボ回数を更新するメソッド
    public void comboCountUpdate()
    {
        {
            // 現在の時間を取得
            float currentTime = Time.time;
            // 前回のヒット時間から1秒以上経過していたらコンボ回数をリセット
            if (currentTime - lastHitTime > 1.0f)
            {
                comboCount = 0;
            }
            else
            {
                // 1秒以内にヒットした場合はコンボ回数を加算
                comboCount++;
                // コンボテキストに回数を表示
                comboText.text = "Combo " + comboCount.ToString();
                RectTransform comboTextRect = comboText.GetComponent<RectTransform>();
                // コンボテキストをアニメーション
                StartCoroutine(AnimateDamageText(comboTextRect, comboText, true));
            }

            lastHitTime = currentTime;
        }
    }

    // HPを回復させるメソッド
    public void Heal(float amount, Unit attacker, bool defenseless = false)
    {
        // 回復値を表示する
        // damageTextを複製する
        GameObject damageObjClone = Instantiate(damageText.gameObject, damageText.transform.position, damageText.transform.rotation);
        damageObjClone.transform.SetParent(damageText.transform.parent); // 親を設定

        TextMeshProUGUI damageTextClone = damageObjClone.GetComponent<TextMeshProUGUI>();

        // ダメージ数値のRectTransformを取得し、PosXとPosYを-10.0~10.0のランダム値に変更
        RectTransform damageTextRect = damageObjClone.GetComponent<RectTransform>();
        //位置とスケールをコピー元と同じにする
        damageTextRect.localScale = damageText.GetComponent<RectTransform>().localScale;
        //ランダムな位置にする
        damageTextRect.localPosition = new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), UnityEngine.Random.Range(-10.0f, 10.0f), 0);

        // ダメージ数値を四捨五入して表示
        damageTextClone.text = Mathf.Round(amount).ToString();
        // ダメージの桁数に応じてフォントサイズを拡大(10の位を基準にする)
        damageTextClone.fontSize = 18 + (int)(Mathf.Log10(amount) * 10);
        damageTextClone.color = Color.green;

        // 状態異常効果を確率で与える(くらわせる)
        if(attacker != this && !defenseless) ConditionAttack(attacker);

        // textアニメーションを開始
        StartCoroutine(AnimateDamageText(damageTextRect, damageTextClone));

        // HPを回復
        currentHp += amount;
        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }
        // HPバーを更新
        UpdateHpBar();

        // attackerの状態異常回復フラグがtrueで、自身のcondition[1~8]のいずれかがtrueの場合は状態異常を回復
        if (attacker.conditionRecavery && (condition[1] || condition[2] || condition[3] || condition[4] || condition[5] || condition[6] || condition[8]))
        {
            // attacker.conditionRecaveryChanceの確率で状態異常を回復
            if (UnityEngine.Random.Range(0, 100) < attacker.conditionRecaveryChance)
            {
                for (int i = 1; i < 9; i++)
                {
                    condition[i] = false;
                }
                // 状態異常回復音を再生
                AkSoundEngine.PostEvent("SE_Cure", gameObject);

                Debug.Log("状態異常を回復しました");
            }
        }

        // 攻撃元にコンボ回数の更新通知を送る.
        if (attacker.gameObject.tag == "Ally")
        {
            attacker.comboCountUpdate();
        }

        //// tagがAllyであれば回復を記録
        if (gameObject.tag == "Ally")
        {
            // BattleManagerがあればインスタンスを取得
            if (BattleManager.Instance != null) battleManager = BattleManager.Instance;
            if (battleManager != null)
            {
                // unitDamageにダメージを記録
                battleManager.RecordUnitDamage(attacker.ID - 1, (int)amount);
            }
        }

        // unitSpriteを光らせる
        if (gameObject.activeInHierarchy) // ゲームオブジェクトがアクティブな場合のみ実行
        {
            StartCoroutine(FlashSprite(new Color(0.75f, 1.0f, 0.75f, 1.0f)));
        }
    }

    /// <summary>
    /// 状態異常効果を確率で与える(くらわせる)
    /// </summary>
    /// <param name="attaker"></param>
    private void ConditionAttack(Unit attacker)
    {
        if (attacker.poisonAttack) 
        { 

            if (UnityEngine.Random.Range(0, 100) < attacker.conditionChance*100)
            {
                condition[1] = true;
                poisonDamage =  attacker.poisonAmount; // 毒ダメージを適用
                poisonTime = attacker.poisonDealTime; // 毒時間を適用
                poisonAttacker = attacker; // 毒を与えたユニットを記録
                if(poisonCount < 3) poisonCount++;
            }
        }
        if (attacker.bleedAttack) 
        {
            if (UnityEngine.Random.Range(0, 100) < attacker.conditionChance*100)
            {
                condition[2] = true;
                bleedDamage =  attacker.bleedAmount; // 出血ダメージを適用
                bleedTime += attacker.bleedDealTime; // 出血時間を適用
                bleedAttacker = attacker; // 出血を与えたユニットを記録
                if(bleedCount < 3) bleedCount++;
                //  かかった直後ステータスを更新する
                updateStatus();
            }
        }
        if (attacker.stunAttack) 
        {
            if (!condition[3])
            {
                if (UnityEngine.Random.Range(0, 100) < attacker.conditionChance*100)
                {
                    condition[3] = true;
                    stunTime = attacker.stunDealTime; // スタン時間を適用
                    //  かかった直後ステータスを更新する
                    updateStatus();
                }
            }
        }
        if (attacker.paralysisAttack) 
        {
            if (!condition[4])
            {
                if (UnityEngine.Random.Range(0, 100) < attacker.conditionChance*100)
                {
                    condition[4] = true;
                    paralysisTime = attacker.paralysisDealTime; // 麻痺時間を適用
                    //  かかった直後ステータスを更新する
                    updateStatus();
                }
            }
        }
        if (attacker.weakAttack) 
        {
            if (!condition[5])
            {
                if (UnityEngine.Random.Range(0, 100) < attacker.conditionChance*100)
                {
                    condition[5] = true;
                    weakeTime = attacker.weakDealTime; // 脆弱時間を適用
                    //  かかった直後ステータスを更新する
                    updateStatus();
                }
            }
        }
        if (attacker.defenceDownAttack) 
        {
            if (!condition[6])
            {
                if (UnityEngine.Random.Range(0, 100) < attacker.conditionChance*100)
                {
                    condition[6] = true;
                    defenceDownTime = attacker.defenceDownDealTime; // 弱体時間を適用
                    //  かかった直後ステータスを更新する
                    updateStatus();
                }
            }
        }
        if (attacker.regeneAttack) 
        {
            if (!condition[8])
            {
                if (UnityEngine.Random.Range(0, 100) < attacker.conditionChance*100)
                {
                    condition[8] = true;
                    regeneTime = attacker.regeneDealTime; // 回復時間を適用
                    regeneDamage = attacker.regeneAmount; // 回復ダメージを適用
                    regeneAttacker = attacker; // 回復を与えたユニットを記録
                    //  かかった直後ステータスを更新する
                    updateStatus();
                }
            }
        }
    }

    // ユニットが死亡したら呼び出されるメソッド
    public void Die(int attackUnitID)
    {
        // コントローラーを無効にする前に攻撃を停止
        StopAttack();

        // AttackControllerとUnitControllerを無効にする
        GetComponent<AttackController>().enabled = false;
        GetComponent<UnitController>().enabled = false;

        //tagがEnemyの場合UnitStatusのdrop1～3のアイテムを各ドロップ確率によってドロップする
        if (gameObject.tag == "Enemy")
        {
            // オブジェクト名末尾2桁の数字を取得
            int unitId = int.Parse(gameObject.name.Substring(gameObject.name.Length - 2));
            ItemData.EnemyListData enemyListData = gameManager.itemData.enemyList.Find(x => x.ID == unitId);

            // Drop経験値&Goldを加算
            int dropExp = enemyListData.dropExp;
            if(gameManager.roomOptions.doubleExp) dropExp *= 2;
            gameManager.AddExperience(dropExp);

            int dropGold = enemyListData.dropGold;
            if (gameManager.roomOptions.doubleGold) dropGold *= 2;
            gameManager.AddGold(dropGold);

            //// 獲得経験値とGoldを記録
            // BattleManagerがあればインスタンスを取得
            if (BattleManager.Instance != null) battleManager = BattleManager.Instance;
            if (battleManager != null)
            {
                battleManager.expGained += enemyListData.dropExp;
                battleManager.goldGained += enemyListData.dropGold;
            }

            // 現在のワールドidを取得
            int currentWorldId = worldManager.currentWorld;

            // unitSpriteのオブジェクトを取得してrotetionZを-90にして寝かせる
            unitSprite.gameObject.transform.rotation = Quaternion.Euler(0, 0, -90);

            // worldManagerがあればWorldRuneDropSettingsを取得
            if (worldManager != null)
            {
                if(gameManager.itemData != null)
                {
                    // Runeをドロップする
                    // ルーンドロップの確率を計算
                    int runeDropRate = UnityEngine.Random.Range(0, 100);
                    // gameManagerのroomOptionsのruneDropUpがtrueの場合はルーンドロップ確率を2倍にする
                    if (gameManager.roomOptions.runeDropUp) RuneDropChance *= 2;
                    if (runeDropRate < RuneDropChance)
                    {
                        // ルーンドロップの確率に合致した場合はruneList.worldが0でなく、さらにcurrentWorld以下のランダムなルーンをドロップ
                        List<ItemData.RuneListData> worldRuneList = gameManager.itemData.runeList.FindAll(x => x.world != 0 && x.world <= currentWorldId && (x.ID / 100) % 10 != 9);
                        // ルーンドロップの確率に合致した場合はランダムなルーンをドロップ
                        int randomRuneId = UnityEngine.Random.Range(0, worldRuneList.Count);
                        int dropItem = worldRuneList[randomRuneId].ID;
                        print("****** Rune: " + dropItem + "をドロップ");
                        iventryUI.AddItem(dropItem);
                    }
                }
            }


            // Dropアイテムをドロップする
            // Drop確率を2倍にするかどうか (gameManagerのroomOptionsのitemDropUpがtrueの場合はint itemDropUp=2、そうでない場合はint itemDropUp=1)
            int itemDropUp = gameManager.roomOptions.itemDropUp ? 2 : 1;
            float dropRate = UnityEngine.Random.Range(1.0f, 10.0f);
            if (dropRate < enemyListData.drop1Rate * itemDropUp)
            {
                print("****** " + enemyListData.drop1 + "をドロップ");
                iventryUI.AddItem(enemyListData.drop1);
            }
            dropRate = UnityEngine.Random.Range(1.0f, 10.0f);
            if (dropRate < enemyListData.drop2Rate * itemDropUp)
            {
                print("****** " + enemyListData.drop2 + "をドロップ");
                iventryUI.AddItem(enemyListData.drop2);
            }
            dropRate = UnityEngine.Random.Range(1.0f, 10.0f);
            if (dropRate < enemyListData.drop3Rate * itemDropUp)
            {
                print("****** " + enemyListData.drop3 + "をドロップ");
                iventryUI.AddItem(enemyListData.drop3);
            }
            // Enemyの数を減らす
            gameManager.enemyCount--;

            //// 敵を倒した数を記録
            // BattleManagerがあればインスタンスを取得
            if (BattleManager.Instance != null) battleManager = BattleManager.Instance;
            if (battleManager != null)
            {
                // unitDamageにダメージを記録
                // 誰によって倒されたかを記録
                battleManager.RecordUnitKill(attackUnitID - 1);
            }

            // Enemmyが全滅したら勝利演出を行う
            if (gameManager.enemyCount == 0)
            {
                // バトル終了処理
                if (BattleManager.Instance != null) battleManager = BattleManager.Instance;
                if (battleManager != null)
                {
                    battleManager.OnBattleEnd(gameObject);
                }
            }
            else
            {
                // 敵が全滅していない場合は、消滅する
                // Ef_dieをResourcesから読み込んで生成
                GameObject dieEffect = Resources.Load<GameObject>("Ef_die");
                if (dieEffect != null)
                {
                    Instantiate(dieEffect, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }

        }
        if (gameObject.tag == "Ally")
        {
            // Ef_dieをResourcesから読み込んで生成
            GameObject dieEffect = Resources.Load<GameObject>("Ef_die");
            if (dieEffect != null)
            {
                Instantiate(dieEffect, transform.position, Quaternion.identity);
            }
            // プレイヤーが死亡した場合はSetActive(false)にして非表示にする
            condition = new bool[9] { false, false, false, false, false, false, false, false, false };
            condition[0] = true;
            // ユニットの色を元に戻す(念のため)
            unitSprite.material.SetColor("_HardlightColor", new Color(0.5f, 0.5f, 0.5f, 1.0f));
            gameObject.SetActive(false);
            // livingUnitsのどの要素が死亡したかを判定し、その要素番号と同じ番号のdeadPanelListをSetActive(true)にして表示する
            for (int i = 0; i < gameManager.livingUnits.Count; i++)
            {
                if (gameManager.livingUnits[i] == gameObject)
                {
                    gameManager.deadPanelList[i].SetActive(true);
                }
            }
            // プレイヤーが全員死亡したらゲームオーバー処理を行い、ゲームオーバーシーンに遷移する
            // gameManager.livingUnitsの全unitのconditionが1の場合、ゲームオーバーシーンに遷移する
            foreach (GameObject player in gameManager.livingUnits)
            {
                Unit unit = player.GetComponent<Unit>();
                if (!unit.condition[0])
                {
                    return; // 1人でも生存していればゲームオーバー処理を行わない
                }
            }
            // すべてのプレイヤーが死亡している場合、ゲームオーバーシーンに遷移する
            //gameManager.LoadScene("GameOverScene");
            // EnemyTagのオブジェクトを全て削除
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }
            // バトル終了処理
            if (BattleManager.Instance != null) battleManager = BattleManager.Instance;
            if (battleManager != null)
            {
                battleManager.OnBattleEnd(null,true);
            }
        }
    }

    // ジョブによる初期装備に変更するメソッド
    public void ChangeEqpByJob()
    {
        int jobId = job;
        // JobListDataを取得
        ItemData.JobListData jobListData = gameManager.itemData.jobList.Find(x => x.ID == job);
        if (jobListData != null)
        {
            currentWeapons = jobListData.initWeapon;
            currentShields = jobListData.initShield;
            currentArmor = jobListData.initArmor;
            currentAccessories = jobListData.initAccessories;
            // mainSocketとsubSocketを初期化
            mainSocket = 0;
            subSocket = new int[11] { 0, 0, 0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 };
            // mainSocketとsubSocketのItemDandDHandlerのruneLevelを初期化
            // UnitのIDに対応したIventrySkillListの要素を取得
            if (iventryUI != null)
            {
                List<GameObject> IventrySkillList = null;
                switch (ID)
                {
                    case 1:
                        IventrySkillList = iventryUI.IventrySkillList1;
                        break;
                    case 2:
                        IventrySkillList = iventryUI.IventrySkillList2;
                        break;
                    case 3:
                        IventrySkillList = iventryUI.IventrySkillList3;
                        break;
                    case 4:
                        IventrySkillList = iventryUI.IventrySkillList4;
                        break;
                    case 5:
                        IventrySkillList = iventryUI.IventrySkillList5;
                        break;
                    default:
                        Debug.Log("unitNum is invalid");
                        return;
                }
                // int i 6~17までのIventrySkillListの要素を取得し、すべてのruneLevelを0にする
                for (int i = 6; i < 18; i++)
                {
                    IventrySkillList[i].GetComponent<ItemDandDHandler>().runeLevel = 0;
                }
            }
        }
    }

    // AttackControllerの攻撃を停止するメソッドを実行するメソッド
    public void StopAttack()
    {
        GetComponent<AttackController>().StopAttack();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Unit : MonoBehaviour
{
    public UnitStatus unitStatus;
    public SpriteRenderer unitSprite; // ユニットのスプライト
    private JobStatus jobStatus;
    private EqpStats StatusWeapons;
    private EqpStats StatusShields;
    private EqpStats StatusArmor;
    private EqpStats StatusAccessories;
    private Animator animator; // Animatorコンポーネントの参照
    private bool live = true; // ユニットが生存しているかどうか

    [SerializeField] public string unitName;
    [SerializeField] public int condition; // 0:通常, 1:死亡 2:火傷(時間xダメージ) 3:麻痺(動きが遅くなる), 4:毒(重ねがけ), 5:凍結(動けない、防御力上がる)、6:弱体化(攻撃力が下がる)、7:脆弱化 (防御力が下がる)
    [SerializeField] public int job;
    [SerializeField] public int totalExp;
    [SerializeField] public int currentLevel;
    [SerializeField] public int nextLevelExp;
    [SerializeField] public int addLevel;
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
    [SerializeField] public float attackRange;
    [SerializeField] public float attackSize;
    [SerializeField] public float knockBack;
    [SerializeField] public int targetJob;
    [SerializeField] public float teleportation;
    [SerializeField] public float escape;
    [SerializeField] public float attackStanceDuration;
    [SerializeField] public float attackStanceDelay;

    [SerializeField] public int currentWeapons;
    [SerializeField] public int currentAttackEffects;
    [SerializeField] public int currentShields;
    [SerializeField] public int currentArmor;
    [SerializeField] public int currentAccessories;

    // 武器prefab設定
    public GameObject weaponPosition; // 武器の生成位置

    public GameObject HPBarRoot; // HPバーのルートオブジェクト
    private Animator HPBarAnimator; // HPバーのAnimatorコンポーネント
    public Slider hpBar;
    public Image hpColor;
    public TextMeshProUGUI damageText; // ダメージ数値表示テキスト
    private float maxHp;
    private float currentHp;

    private GameManager gameManager; // GameManagerの参照
    private IventryUI iventryUI; // IventryUIの参照

    void Start()
    {
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

        // unitStatusからステータスを読み込む
        job = unitStatus.job;
        addLevel = unitStatus.addLevel;
        currentWeapons = unitStatus.weapons;
        currentShields = unitStatus.shields;
        currentArmor = unitStatus.armor;
        currentAccessories = unitStatus.accessories;

        updateStatus();
        InitHp();

        // アニメーションを再生
        string stateName = job.ToString("D2") + "_idle";
        if(PlayAnimatorStateIfExists(stateName))
        {
            animator.Play(stateName); // 動いていない場合はIdleステートをセット
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
        // JobStatusを読み込む
        jobStatus = Resources.Load<JobStatus>("JobStatus/JobStatus_" + unitStatus.job.ToString("D2"));
        if (jobStatus == null) {
            Debug.LogError("jobStatus is not assigned.");
            return;
        }
        // JobStatusからステータスを読み込む
        float currentLevelScaleFactor = jobStatus.levelScaleFactor;
        float currentMagic = jobStatus.Magic;
        float currentStr = jobStatus.Str;
        float currentDex = jobStatus.Dex;
        float currentResidtCondition = jobStatus.ResidtCondition;
        float currentAttackUnitThrough = jobStatus.AttackUnitThrough;
        float currentAttackObjectThrough = jobStatus.AttackObjectThrough;
        float currentKnockBack = jobStatus.InitKnockBack;
        float levelMagic = jobStatus.levelMagic;
        float levelStr = jobStatus.levelStr;
        float levelDex = jobStatus.levelDex;
        float levelResidtCondition = jobStatus.levelResidtCondition;
        float levelAttackUnitThrough = jobStatus.levelAttackUnitThrough;
        float levelAttackObjectThrough = jobStatus.levelAttackObjectThrough;
        float levelKnockBack = jobStatus.levelKnockBack;
        int JtargetJob = jobStatus.targetJob;
        float teleportations = jobStatus.teleportation;
        float escapes = jobStatus.escape;

        // EqpStatsからステータスを読み込む
        StatusWeapons = Resources.Load<EqpStats>("WpnStatus/WpnStatus_" + currentWeapons.ToString("D6"));
        if (StatusWeapons == null) {
            Debug.LogError("StatusWeapons is not assigned.: WpnStatus/WpnStatus_" + currentWeapons.ToString("D6"));
            return;
        }
        StatusShields = Resources.Load<EqpStats>("EqpStatus/EqpStatus_" + currentShields.ToString("D6"));
        if (StatusShields == null) {
            Debug.LogError("StatusShields is not assigned.");
            return;
        }
        StatusArmor = Resources.Load<EqpStats>("EqpStatus/EqpStatus_" + currentArmor.ToString("D6"));
        if (StatusArmor == null) {
            Debug.LogError("StatusArmor is not assigned.");
            return;
        }
        StatusAccessories = Resources.Load<EqpStats>("EqpStatus/EqpStatus_" + currentAccessories.ToString("D6"));
        if (StatusAccessories == null) {
            Debug.LogError("StatusAccessories is not assigned.");
            return;
        }

        // ステータスを計算する
        int baseExperience = 10; // レベルアップに必要な基本経験値
        currentLevel = (int)Math.Sqrt(totalExp / (baseExperience * currentLevelScaleFactor)) + addLevel + 1;
        nextLevelExp = (int)Math.Pow(currentLevel * currentLevelScaleFactor, 2) * baseExperience;
        Hp = (currentStr + (currentLevel - 1) * levelStr) * 10 + StatusWeapons.HP + StatusShields.HP + StatusArmor.HP + StatusAccessories.HP;
        physicalAttackPower = (currentStr + (currentLevel - 1) * levelStr) / 1 + StatusWeapons.physicalAttackPower + StatusShields.physicalAttackPower + StatusArmor.physicalAttackPower + StatusAccessories.physicalAttackPower;
        magicalAttackPower = (currentMagic + (currentLevel - 1) * levelMagic) / 1 + StatusWeapons.magicalAttackPower + StatusShields.magicalAttackPower + StatusArmor.magicalAttackPower + StatusAccessories.magicalAttackPower;
        physicalDefensePower = (currentStr + (currentLevel - 1) * levelStr) / 10 + StatusWeapons.physicalDefensePower + StatusShields.physicalDefensePower + StatusArmor.physicalDefensePower + StatusAccessories.physicalDefensePower;
        magicalDefensePower = (currentMagic + (currentLevel - 1) * levelMagic) / 10 + StatusWeapons.magicalDefensePower + StatusShields.magicalDefensePower + StatusArmor.magicalDefensePower + StatusAccessories.magicalDefensePower;
        resistCondition = (currentResidtCondition + (currentLevel - 1) * levelResidtCondition) / 10 + StatusWeapons.resistCondition + StatusShields.resistCondition + StatusArmor.resistCondition + StatusAccessories.resistCondition;
        attackDelay =  2.0f + (currentDex + (currentLevel - 1) * levelDex) / 10 + StatusWeapons.attackDelay + StatusShields.attackDelay + StatusArmor.attackDelay + StatusAccessories.attackDelay;
        Speed =  3 + (currentDex + (currentLevel - 1) * levelDex) / 10 + StatusWeapons.Speed + StatusShields.Speed + StatusArmor.Speed + StatusAccessories.Speed;
        attackUnitThrough = (int)(currentAttackUnitThrough + (currentLevel - 1) * levelAttackUnitThrough) + StatusWeapons.attackUnitThrough + StatusShields.attackUnitThrough + StatusArmor.attackUnitThrough + StatusAccessories.attackUnitThrough;
        attackObjectThrough = (int)(currentAttackObjectThrough + (currentLevel - 1) * levelAttackObjectThrough) + StatusWeapons.attackObjectThrough + StatusShields.attackObjectThrough + StatusArmor.attackObjectThrough + StatusAccessories.attackObjectThrough;
        attackSize = 1;
        knockBack = (currentKnockBack + (currentLevel - 1) * levelKnockBack) / 10 + StatusWeapons.knockBack + StatusShields.knockBack + StatusArmor.knockBack + StatusAccessories.knockBack;
        targetJob = JtargetJob;
        teleportation = teleportations + StatusWeapons.teleportation + StatusShields.teleportation + StatusArmor.teleportation + StatusAccessories.teleportation;
        escape = escapes + StatusWeapons.escape + StatusShields.escape + StatusArmor.escape + StatusAccessories.escape;
        attackRange = StatusWeapons.attackRange;
        attackStanceDuration = StatusWeapons.attackStanceDuration;
        attackStanceDelay = StatusWeapons.attackStanceDelay;

        // HPの初期化
        maxHp = (currentStr + (currentLevel - 1) * levelStr) * 10;

        // 武器プレファブを変更する
        GameObject wpn = Resources.Load<GameObject>("Prefabs/Weapons/" + currentWeapons.ToString("D6"));
        ChangeWeapon(wpn);

        // UnitControllerとAttackControllerにステータスを設定する
        SetStatusUnitController(GetComponent<UnitController>(), GetComponent<AttackController>());
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
        if(teleportation  >= 1) 
        {
            unitController.enableTeleport = true;
            unitController.teleportDistance = Speed;
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

            for (int i = 0; i < attributes.Count; i++)
            {
                string attributeName = attributes[i].ToString();  // Enumから文字列を取得
                if (attributeName == "Healing")
                {
                    unitController.targetSameTag = true;
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

        /*
        unitController.unitName = unitName;
        unitController.condition = condition;
        unitController.job = job;
        unitController.totalExp = totalExp;
        unitController.currentLevel = currentLevel;
        unitController.nextLevelExp = nextLevelExp;
        unitController.addLevel = addLevel;
        unitController.Hp = Hp;
        unitController.physicalAttackPower = physicalAttackPower;
        unitController.magicalAttackPower = magicalAttackPower;
        unitController.physicalDefensePower = physicalDefensePower;
        unitController.magicalDefensePower = magicalDefensePower;
        unitController.resistCondition = resistCondition;
        unitController.attackDelay = attackDelay;
        unitController.Speed = Speed;
        unitController.attackUnitThrough = attackUnitThrough;
        unitController.attackObjectThrough = attackObjectThrough;
        unitController.attackSize = attackSize;
        unitController.knockBack = knockBack;
        unitController.targetJob = targetJob;
        unitController.teleportation = teleportation;
        unitController.escape = escape;
        */
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
    }

    // HPを減少させるメソッド
    public void TakeDamage(float damage)
    {
        // tagがEnemyの場合はダメージを表示する
        if (gameObject.tag == "Enemy")
        {
            // ダメージ数値のRectTransformを取得し、PosXとPosYを-10.0~10.0のランダム値に変更
            RectTransform damageTextRect = damageText.GetComponent<RectTransform>();
            damageTextRect.localPosition = new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), UnityEngine.Random.Range(-10.0f, 10.0f), 0);
            // ダメージ数値を四捨五入して表示
            damageText.text = Mathf.Round(damage).ToString();
            // ダメージの桁数にが上がるにつれてフォントサイズを拡大(10の位を基準にする)
            damageText.fontSize = 18 + (int)(Mathf.Log10(damage) * 10);
            damageText.color = Color.white;
            // ダメージ数値のアニメーションstateを直接再生
            HPBarAnimator.Play("HPBar_damage", 0, 0);
        }

        currentHp -= damage;
        UpdateHpBar();
        // unitSpriteを光らせる
        if (gameObject.activeInHierarchy) // ゲームオブジェクトがアクティブな場合のみ実行
        {
            StartCoroutine(FlashSprite(new Color(0.75f, 0.75f, 0.75f, 1.0f)));
        }

        // すでにユニットが死亡していたら何もしない(消滅するまでに複数回呼び出されることがあるため)
        if(live == false)
        {
            return;
        }
        // HPが0以下になったら死亡
        if (currentHp < 0)
        {
            live = false;
            currentHp = 0;
            Die();
        };

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

    // HPを回復させるメソッド
    public void Heal(float amount)
    {
        // 回復値を表示する
        // 値のRectTransformを取得し、PosXとPosYを-10.0~10.0のランダム値に変更
        RectTransform damageTextRect = damageText.GetComponent<RectTransform>();
        damageTextRect.localPosition = new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), UnityEngine.Random.Range(-10.0f, 10.0f), 0);
        // 値を四捨五入して表示
        damageText.text = Mathf.Round(amount).ToString();
        // 桁数にが上がるにつれてフォントサイズを拡大(10の位を基準にする)
        damageText.fontSize = 12 + (int)(Mathf.Log10(amount) * 10);
        damageText.color = Color.green;
        // 値のアニメーションstateを直接再生
        HPBarAnimator.Play("HPBar_damage", 0, 0);

        currentHp += amount;
        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }
        UpdateHpBar();
        // unitSpriteを光らせる
        if (gameObject.activeInHierarchy) // ゲームオブジェクトがアクティブな場合のみ実行
        {
            StartCoroutine(FlashSprite(new Color(0.75f, 1.0f, 0.75f, 1.0f)));
        }
    }

    // ユニットが死亡したら呼び出されるメソッド
    public void Die()
    {
        // AttackControllerとUnitControllerを無効にする
        GetComponent<AttackController>().enabled = false;
        GetComponent<UnitController>().enabled = false;

        // Drop経験値&Goldを加算
        gameManager.AddExperience(unitStatus.dropExp);
        gameManager.AddGold(unitStatus.dropGold);
        //tagがEnemyの場合UnitStatusのdrop1～3のアイテムを各ドロップ確率によってドロップする
        if (gameObject.tag == "Enemy")
        {
            // unitSpriteのオブジェクトを取得してrotetionZを-90にして寝かせる
            unitSprite.gameObject.transform.rotation = Quaternion.Euler(0, 0, -90);
            float dropRate = UnityEngine.Random.Range(0.0f, 1.0f);
            if (dropRate < unitStatus.drop1Rate)
            {
                print("****** " + unitStatus.drop1 + "をドロップ");
                iventryUI.AddItem(unitStatus.drop1);
            }
            dropRate = UnityEngine.Random.Range(0.0f, 1.0f);
            if (dropRate < unitStatus.drop2Rate)
            {
                print("****** " + unitStatus.drop2 + "をドロップ");
                iventryUI.AddItem(unitStatus.drop2);
            }
            dropRate = UnityEngine.Random.Range(0.0f, 1.0f);
            if (dropRate < unitStatus.drop3Rate)
            {
                print("****** " + unitStatus.drop3 + "をドロップ");
                iventryUI.AddItem(unitStatus.drop3);
            }
            // Enemyの数を減らす
            gameManager.enemyCount--;

            // Enemmyが全滅したら勝利演出を行う
            if (gameManager.enemyCount == 0)
            {
                gameManager.victory(gameObject);
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
            condition = 1;
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
                if (unit.condition != 1)
                {
                    return; // 1人でも生存していればゲームオーバー処理を行わない
                }
                // すべてのプレイヤーが死亡している場合、ゲームオーバーシーンに遷移する
                //gameManager.LoadScene("GameOverScene");
            }
        }
    }
}

using System;
using System.Reflection;
using UnityEngine;

namespace CustomIngameMenu
{
    public class Loader
    {
        private static GameObject _obj;

        public static void Init()
        {
            Load();
        }

        public static void Load()
        {
            if (_obj != null)
            {
                CheatMenu menu = _obj.GetComponent<CheatMenu>();
                if (menu != null) menu.enabled = true;
                return;
            }

            _obj = new GameObject("CheatMenu_RuntimeHost");
            _obj.AddComponent<CheatMenu>();
            UnityEngine.Object.DontDestroyOnLoad(_obj);
        }

        public static void Unload()
        {
            if (_obj != null)
            {
                UnityEngine.Object.Destroy(_obj);
                _obj = null;
            }
        }
    }

    public class CheatMenu : MonoBehaviour
    {
        public bool isOpen = true;
        public KeyCode toggleKey = KeyCode.Insert;
        public int langIndex = 0;

        private Rect winRect = new Rect(100f, 100f, 540f, 580f);
        private int currentTab = 0;

        public bool noRagdoll;
        public bool instantGetUp;
        public bool instantInteract;
        public bool longReach;
        public bool infMoney;

        public bool fastRun;
        public float runSpeed = 2.5f;
        public bool highJump;
        public float jumpHeight = 8.0f;
        public bool infJump;
        public bool moonGrav;
        public float gravForce = -3.5f;
        public bool godFall;

        public bool instaKill;

        public bool autoQuota;
        public bool stopTimer;
        public bool trustMax;
        public bool antiVirus;
        public bool timeMod;
        public float timeScale = 1.0f;

        public bool freeShopping;
        public bool unlockApps;
        public bool unlockFloors;

        public float reachDist = 15f;
        private Vector3 defaultGrav = new Vector3(0f, -9.81f, 0f);
        private float tickTimer = 0f;

        private static Type sgmType;
        private static PropertyInfo sgmInst;
        private static Type npcHealthType;
        private static Type kolkataPlayerType;
        private static PropertyInfo kolkataPlayerLocal;
        private static Type playerInteractorType;
        private static PropertyInfo playerInteractorLocal;
        private static Type playerNameType;
        private static PropertyInfo playerNameLocal;
        private static Type holdInteractType;
        private static Type floorDoorType;
        private static Type callType;
        private static Type kolkataSingletonType;
        private static PropertyInfo kolkataSingletonInst;
        private static Type cursorType;
        private static PropertyInfo cursorInst;
        private static PropertyInfo cursorUsersList;
        private static Type controllerType;
        private static PropertyInfo controllerInst;
        private static FieldInfo controllerBlocker;

        private static readonly string[][] Tabs = new string[][]
        {
            new string[] { "Jogador", "Física", "Inimigos", "Jogo", "Inventário", "Configurações" },
            new string[] { "Player", "Physics", "Enemies", "Game", "Inventory", "Settings" }
        };

        private string T(string pt, string en)
        {
            return langIndex == 0 ? pt : en;
        }

        private void Awake()
        {
            float w = Mathf.Min(540f, Screen.width * 0.9f);
            float h = Mathf.Min(580f, Screen.height * 0.9f);
            winRect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

            try
            {
                PropertyInfo p = typeof(Physics).GetProperty("gravity", BindingFlags.Public | BindingFlags.Static);
                if (p != null) defaultGrav = (Vector3)p.GetValue(null, null);
            }
            catch { }

            InitReflection();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
        }

        private static void InitReflection()
        {
            if (sgmType != null) return;

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (sgmType == null)
                {
                    sgmType = asm.GetType("ScammerGameManager");
                    if (sgmType != null) sgmInst = sgmType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                }
                if (npcHealthType == null) npcHealthType = asm.GetType("NpcHealth");
                if (kolkataPlayerType == null)
                {
                    kolkataPlayerType = asm.GetType("KolkataOsPlayer");
                    if (kolkataPlayerType != null) kolkataPlayerLocal = kolkataPlayerType.GetProperty("LocalPlayerInstance", BindingFlags.Public | BindingFlags.Static);
                }
                if (playerInteractorType == null)
                {
                    playerInteractorType = asm.GetType("PlayerInteractor");
                    if (playerInteractorType != null) playerInteractorLocal = playerInteractorType.GetProperty("LocalPlayerInstance", BindingFlags.Public | BindingFlags.Static);
                }
                if (playerNameType == null)
                {
                    playerNameType = asm.GetType("PlayerName");
                    if (playerNameType != null) playerNameLocal = playerNameType.GetProperty("LocalPlayerInstance", BindingFlags.Public | BindingFlags.Static);
                }
                if (holdInteractType == null) holdInteractType = asm.GetType("HoldInteractable");
                if (floorDoorType == null) floorDoorType = asm.GetType("FloorEntranceDoor");
                if (callType == null) callType = asm.GetType("VictimCall");
                if (kolkataSingletonType == null)
                {
                    kolkataSingletonType = asm.GetType("KolkataOsSingleton");
                    if (kolkataSingletonType != null) kolkataSingletonInst = kolkataSingletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                }
                if (cursorType == null)
                {
                    cursorType = asm.GetType("Metater.MetaCursor");
                    if (cursorType != null)
                    {
                        cursorInst = cursorType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                        cursorUsersList = cursorType.GetProperty("CursorUsers", BindingFlags.Public | BindingFlags.Instance);
                    }
                }
                if (controllerType == null)
                {
                    controllerType = asm.GetType("Metater.MetaPlayerController");
                    if (controllerType != null)
                    {
                        controllerInst = controllerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                        controllerBlocker = controllerType.GetField("blocker", BindingFlags.Public | BindingFlags.Instance);
                    }
                }
            }
        }

        private object GetLocalPlayer()
        {
            try
            {
                InitReflection();
                if (kolkataPlayerLocal != null) return kolkataPlayerLocal.GetValue(null, null);
            }
            catch { }
            return null;
        }

        private Component GetInteractor()
        {
            try
            {
                InitReflection();
                if (playerInteractorLocal != null) return playerInteractorLocal.GetValue(null, null) as Component;
            }
            catch { }
            return null;
        }

        private object GetGameMgr()
        {
            try
            {
                InitReflection();
                if (sgmInst != null) return sgmInst.GetValue(null, null);
            }
            catch { }
            return null;
        }

        private object GetDesktop()
        {
            try
            {
                InitReflection();
                if (kolkataSingletonInst != null) return kolkataSingletonInst.GetValue(null, null);
            }
            catch { }
            return null;
        }

        private void SetGravity(Vector3 g)
        {
            try
            {
                PropertyInfo prop = typeof(Physics).GetProperty("gravity", BindingFlags.Public | BindingFlags.Static);
                if (prop != null && prop.CanWrite) prop.SetValue(null, g, null);
            }
            catch { }
        }

        private Vector3 GetGravity()
        {
            try
            {
                PropertyInfo prop = typeof(Physics).GetProperty("gravity", BindingFlags.Public | BindingFlags.Static);
                if (prop != null) return (Vector3)prop.GetValue(null, null);
            }
            catch { }
            return defaultGrav;
        }

        private void ToggleMenu(bool state)
        {
            if (isOpen == state) return;
            isOpen = state;

            try
            {
                InitReflection();

                Cursor.visible = state;
                Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;

                if (cursorInst != null && cursorUsersList != null)
                {
                    object cursor = cursorInst.GetValue(null, null);
                    if (cursor != null)
                    {
                        object list = cursorUsersList.GetValue(cursor, null);
                        if (list != null)
                        {
                            MethodInfo add = list.GetType().GetMethod("Add");
                            MethodInfo rm = list.GetType().GetMethod("Remove");
                            if (state && add != null) add.Invoke(list, new object[] { this });
                            else if (!state && rm != null) rm.Invoke(list, new object[] { this });
                        }
                    }
                }

                if (controllerInst != null && controllerBlocker != null)
                {
                    object ctrl = controllerInst.GetValue(null, null);
                    if (ctrl != null)
                    {
                        object blk = controllerBlocker.GetValue(ctrl);
                        if (blk != null)
                        {
                            PropertyInfo prop = blk.GetType().GetProperty("InputBlockers");
                            object set = prop != null ? prop.GetValue(blk, null) : null;
                            if (set != null)
                            {
                                MethodInfo add = set.GetType().GetMethod("Add");
                                MethodInfo rm = set.GetType().GetMethod("Remove");
                                if (state && add != null) add.Invoke(set, new object[] { this });
                                else if (!state && rm != null) rm.Invoke(set, new object[] { this });
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void OnDisable()
        {
            ToggleMenu(false);
        }

        private void OnDestroy()
        {
            ToggleMenu(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Delete))
            {
                ToggleMenu(!isOpen);
            }

            if (isOpen)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            HandleMovement();

            tickTimer += Time.deltaTime;
            if (tickTimer >= 0.5f)
            {
                tickTimer = 0f;
                HandleSlowLoop();
            }
        }

        private void HandleMovement()
        {
            try
            {
                Component interactor = GetInteractor();
                Transform tr = interactor != null ? interactor.transform : null;
                CharacterController cc = tr != null ? tr.GetComponent<CharacterController>() : null;
                Component rb = tr != null ? tr.GetComponent("Rigidbody") : null;

                if (fastRun && tr != null)
                {
                    float h = Input.GetAxisRaw("Horizontal");
                    float v = Input.GetAxisRaw("Vertical");
                    if (Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f)
                    {
                        Vector3 dir = (tr.forward * v + tr.right * h).normalized;
                        Vector3 delta = dir * (runSpeed * 5f) * Time.deltaTime;

                        if (cc != null && cc.enabled) cc.Move(delta);
                        else if (rb != null)
                        {
                            PropertyInfo velProp = rb.GetType().GetProperty("velocity");
                            if (velProp != null)
                            {
                                Vector3 cur = (Vector3)velProp.GetValue(rb, null);
                                velProp.SetValue(rb, new Vector3(dir.x * (runSpeed * 5f), cur.y, dir.z * (runSpeed * 5f)), null);
                            }
                        }
                        else tr.position += delta;
                    }
                }

                if ((highJump || infJump) && Input.GetKeyDown(KeyCode.Space))
                {
                    float force = highJump ? jumpHeight : 6.0f;
                    bool jumped = false;

                    if (rb != null)
                    {
                        PropertyInfo velProp = rb.GetType().GetProperty("velocity");
                        if (velProp != null)
                        {
                            Vector3 cur = (Vector3)velProp.GetValue(rb, null);
                            velProp.SetValue(rb, new Vector3(cur.x, force, cur.z), null);
                            jumped = true;
                        }
                    }

                    if (!jumped && tr != null)
                    {
                        tr.position += Vector3.up * (force * 0.25f);
                    }
                }

                if ((noRagdoll || instantGetUp) && interactor != null)
                {
                    Component ragdoll = interactor.GetComponent("PlayerRagdollManager");
                    if (ragdoll != null)
                    {
                        Type t = ragdoll.GetType();
                        if (noRagdoll)
                        {
                            PropertyInfo blocked = t.GetProperty("IsRagdollBlocked", BindingFlags.Public | BindingFlags.Instance);
                            if (blocked != null) blocked.SetValue(ragdoll, true, null);
                        }

                        if (instantGetUp)
                        {
                            PropertyInfo act = t.GetProperty("IsRagdollActive", BindingFlags.Public | BindingFlags.Instance);
                            if (act != null && (bool)act.GetValue(ragdoll, null))
                            {
                                MethodInfo recover = t.GetMethod("ServerForceRecover", BindingFlags.Public | BindingFlags.Instance)
                                                   ?? t.GetMethod("ForceRecover", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                if (recover != null) recover.Invoke(ragdoll, null);
                            }
                        }
                    }
                }

                if (longReach && interactor != null)
                {
                    Type t = interactor.GetType();
                    FieldInfo fp = t.GetField("firstPersonDistance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo tp = t.GetField("thirdPersonDistance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fp != null) fp.SetValue(interactor, reachDist);
                    if (tp != null) tp.SetValue(interactor, reachDist);
                }

                if (timeMod) Time.timeScale = timeScale;
                else if (Math.Abs(Time.timeScale - 1f) > 0.001f) Time.timeScale = 1f;

                if (moonGrav) SetGravity(new Vector3(0f, gravForce, 0f));
                else if (GetGravity().y != defaultGrav.y) SetGravity(defaultGrav);

                if (godFall && interactor != null)
                {
                    Component hp = interactor.GetComponent("PlayerHealth");
                    if (hp != null)
                    {
                        Type t = hp.GetType();
                        FieldInfo fld = t.GetField("fallDamageMultiplier", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                       ?? t.GetField("takeFallDamage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fld != null)
                        {
                            if (fld.FieldType == typeof(float)) fld.SetValue(hp, 0f);
                            else if (fld.FieldType == typeof(bool)) fld.SetValue(hp, false);
                        }
                    }
                }
            }
            catch { }
        }

        private void HandleSlowLoop()
        {
            if (infMoney)
            {
                object player = GetLocalPlayer();
                if (player != null && kolkataPlayerType != null)
                {
                    PropertyInfo spend = kolkataPlayerType.GetProperty("SpendableMoney", BindingFlags.Public | BindingFlags.Instance);
                    long cur = spend != null ? (long)spend.GetValue(player, null) : 0;
                    if (cur < 5000000)
                    {
                        MethodInfo addMoney = kolkataPlayerType.GetMethod("ServerCreditScamMoney", BindingFlags.Public | BindingFlags.Instance);
                        if (addMoney != null) addMoney.Invoke(player, new object[] { 100000L });
                    }
                }
            }

            try
            {
                if (instantInteract && holdInteractType != null)
                {
                    UnityEngine.Object[] arr = UnityEngine.Object.FindObjectsOfType(holdInteractType);
                    FieldInfo fld = holdInteractType.GetField("holdTime", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (arr != null && fld != null)
                    {
                        foreach (UnityEngine.Object it in arr) fld.SetValue(it, 0.01f);
                    }
                }

                if (instaKill && npcHealthType != null)
                {
                    UnityEngine.Object[] arr = UnityEngine.Object.FindObjectsOfType(npcHealthType);
                    FieldInfo hp = npcHealthType.GetField("currentHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                  ?? npcHealthType.GetField("health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (arr != null && hp != null)
                    {
                        foreach (UnityEngine.Object it in arr) hp.SetValue(it, 1f);
                    }
                }

                object mgr = GetGameMgr();
                if (mgr != null && sgmType != null)
                {
                    if (autoQuota)
                    {
                        PropertyInfo quota = sgmType.GetProperty("DailyQuota", BindingFlags.Public | BindingFlags.Instance);
                        PropertyInfo earned = sgmType.GetProperty("CurrentQuotaEarned", BindingFlags.Public | BindingFlags.Instance);
                        if (quota != null && earned != null && earned.CanWrite)
                        {
                            earned.SetValue(mgr, quota.GetValue(mgr, null), null);
                        }
                    }

                    if (stopTimer)
                    {
                        FieldInfo timer = sgmType.GetField("workDayTimer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (timer != null) timer.SetValue(mgr, 600f);
                    }
                }

                if (trustMax && callType != null)
                {
                    UnityEngine.Object[] calls = UnityEngine.Object.FindObjectsOfType(callType);
                    PropertyInfo tr = callType.GetProperty("TrustLevel", BindingFlags.Public | BindingFlags.Instance)
                                      ?? callType.GetProperty("Trust", BindingFlags.Public | BindingFlags.Instance);
                    if (calls != null && tr != null && tr.CanWrite)
                    {
                        foreach (UnityEngine.Object it in calls) tr.SetValue(it, 1.0f, null);
                    }
                }

                if (antiVirus)
                {
                    object desk = GetDesktop();
                    if (desk != null && kolkataSingletonType != null)
                    {
                        PropertyInfo v = kolkataSingletonType.GetProperty("ActiveVirusCount", BindingFlags.Public | BindingFlags.Instance);
                        if (v != null) v.SetValue(desk, 0, null);
                    }
                }

                if (freeShopping)
                {
                    object p = GetLocalPlayer();
                    if (p != null && kolkataPlayerType != null)
                    {
                        MethodInfo free = kolkataPlayerType.GetMethod("RequestFreeScamPurchases", BindingFlags.Public | BindingFlags.Instance);
                        if (free != null) free.Invoke(p, null);
                    }
                }

                if (unlockFloors && floorDoorType != null)
                {
                    UnityEngine.Object[] doors = UnityEngine.Object.FindObjectsOfType(floorDoorType);
                    FieldInfo day = floorDoorType.GetField("unlockDay", BindingFlags.Public | BindingFlags.Instance);
                    if (doors != null && day != null)
                    {
                        foreach (UnityEngine.Object d in doors) day.SetValue(d, 1);
                    }
                }
            }
            catch { }
        }

        private void OnGUI()
        {
            Event e = Event.current;
            if (e != null && e.type == EventType.KeyDown)
            {
                if (e.keyCode == toggleKey || e.keyCode == KeyCode.F1 || e.keyCode == KeyCode.Delete)
                {
                    ToggleMenu(!isOpen);
                    e.Use();
                }
            }

            if (!isOpen) return;

            GUI.depth = 0;
            winRect = GUI.Window(99991, winRect, DrawGui, T("Scam With Your Friends - Menu", "Scam With Your Friends - Cheat Menu"), GUI.skin.window);
        }

        private void DrawGui(int id)
        {
            string[] tabs = Tabs[langIndex];

            GUILayout.BeginHorizontal();
            for (int i = 0; i < tabs.Length; i++)
            {
                if (GUILayout.Button(tabs[i], GUILayout.Height(25))) currentTab = i;
            }
            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(25))) ToggleMenu(false);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            switch (currentTab)
            {
                case 0:
                    GUILayout.Label(T("=== Jogador ===", "=== Player ==="));
                    noRagdoll = GUILayout.Toggle(noRagdoll, T(" Sem Ragdoll", " No Ragdoll"));
                    instantGetUp = GUILayout.Toggle(instantGetUp, T(" Levantar Rápido", " Instant Recovery"));
                    instantInteract = GUILayout.Toggle(instantInteract, T(" Interação Instantânea", " Instant Interaction"));

                    longReach = GUILayout.Toggle(longReach, T(" Alcance Maior", " Long Reach"));
                    if (longReach)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(T("Distância: ", "Reach: ") + reachDist.ToString("F0") + "m");
                        if (GUILayout.Button("-", GUILayout.Width(30))) reachDist = Mathf.Max(3f, reachDist - 2f);
                        if (GUILayout.Button("+", GUILayout.Width(30))) reachDist = Mathf.Min(50f, reachDist + 2f);
                        GUILayout.EndHorizontal();
                    }

                    infMoney = GUILayout.Toggle(infMoney, T(" Dinheiro Infinito", " Unlimited Money"));
                    if (GUILayout.Button(T("+$50.000 Grátis", "Add +$50k")))
                    {
                        object p = GetLocalPlayer();
                        if (p != null && kolkataPlayerType != null)
                        {
                            MethodInfo add = kolkataPlayerType.GetMethod("ServerCreditScamMoney", BindingFlags.Public | BindingFlags.Instance);
                            if (add != null) add.Invoke(p, new object[] { 50000L });
                        }
                    }
                    break;

                case 1:
                    GUILayout.Label(T("=== Física ===", "=== Physics ==="));
                    fastRun = GUILayout.Toggle(fastRun, T(" Velocidade Rápida", " Speedhack"));
                    if (fastRun)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(T("Mult: ", "Speed: ") + runSpeed.ToString("F1") + "x");
                        if (GUILayout.Button("-", GUILayout.Width(30))) runSpeed = Mathf.Max(1.0f, runSpeed - 0.5f);
                        if (GUILayout.Button("+", GUILayout.Width(30))) runSpeed = Mathf.Min(10.0f, runSpeed + 0.5f);
                        GUILayout.EndHorizontal();
                    }

                    highJump = GUILayout.Toggle(highJump, T(" Pulo Alto", " Super Jump"));
                    if (highJump)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(T("Força: ", "Force: ") + jumpHeight.ToString("F1"));
                        if (GUILayout.Button("-", GUILayout.Width(30))) jumpHeight = Mathf.Max(5.0f, jumpHeight - 1.0f);
                        if (GUILayout.Button("+", GUILayout.Width(30))) jumpHeight = Mathf.Min(25.0f, jumpHeight + 1.0f);
                        GUILayout.EndHorizontal();
                    }

                    infJump = GUILayout.Toggle(infJump, T(" Pulo Infinito", " Infinite Jumps"));

                    moonGrav = GUILayout.Toggle(moonGrav, T(" Gravidade Baixa", " Low Gravity"));
                    if (moonGrav)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(T("Gravidade: ", "Gravity: ") + gravForce.ToString("F1"));
                        if (GUILayout.Button("-", GUILayout.Width(30))) gravForce = Mathf.Max(-15.0f, gravForce - 1.0f);
                        if (GUILayout.Button("+", GUILayout.Width(30))) gravForce = Mathf.Min(-0.5f, gravForce + 1.0f);
                        GUILayout.EndHorizontal();
                    }

                    godFall = GUILayout.Toggle(godFall, T(" Sem Dano de Queda", " No Fall Damage"));
                    break;

                case 2:
                    GUILayout.Label(T("=== Inimigos ===", "=== Enemies ==="));
                    instaKill = GUILayout.Toggle(instaKill, T(" Nocaute em 1 Golpe", " 1-Hit Knockout"));
                    break;

                case 3:
                    GUILayout.Label(T("=== Jogo ===", "=== Game ==="));
                    autoQuota = GUILayout.Toggle(autoQuota, T(" Bater Cota Automática", " Always Meet Quota"));
                    stopTimer = GUILayout.Toggle(stopTimer, T(" Travar Temporizador", " Freeze Clock"));
                    if (GUILayout.Button(T("Pular Turno (Bater Ponto)", "Skip Shift")))
                    {
                        object mgr = GetGameMgr();
                        if (mgr != null && sgmType != null)
                        {
                            MethodInfo req = sgmType.GetMethod("ServerRequestDoorClockOut", BindingFlags.Public | BindingFlags.Instance);
                            object name = playerNameLocal != null ? playerNameLocal.GetValue(null, null) : null;
                            if (req != null && name != null) req.Invoke(mgr, new object[] { name });
                        }
                    }
                    trustMax = GUILayout.Toggle(trustMax, T(" Confiança 100% no Chamador", " 100% Caller Trust"));
                    antiVirus = GUILayout.Toggle(antiVirus, T(" Bloquear Vírus no PC", " Remove All Viruses"));

                    timeMod = GUILayout.Toggle(timeMod, T(" Acelerar Jogo (TimeScale)", " Game Speed"));
                    if (timeMod)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(T("Escala: ", "Scale: ") + timeScale.ToString("F1") + "x");
                        if (GUILayout.Button("-", GUILayout.Width(30))) timeScale = Mathf.Max(0.2f, timeScale - 0.2f);
                        if (GUILayout.Button("+", GUILayout.Width(30))) timeScale = Mathf.Min(5.0f, timeScale + 0.2f);
                        GUILayout.EndHorizontal();
                    }
                    break;

                case 4:
                    GUILayout.Label(T("=== Inventário ===", "=== Inventory ==="));
                    freeShopping = GUILayout.Toggle(freeShopping, T(" Compras Grátis", " Free Purchases"));
                    unlockApps = GUILayout.Toggle(unlockApps, T(" Liberar Todos os Aplicativos", " Unlock All Apps"));
                    if (GUILayout.Button(T("Destravar Apps Agora", "Unlock All Apps Now")))
                    {
                        object desk = GetDesktop();
                        if (desk != null && kolkataSingletonType != null)
                        {
                            PropertyInfo prop = kolkataSingletonType.GetProperty("ApplicationAccess", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            object acc = prop != null ? prop.GetValue(desk, null) : null;
                            if (acc != null)
                            {
                                MethodInfo m = acc.GetType().GetMethod("ApplyRunEntitlements", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                if (m != null) m.Invoke(acc, new object[] { new string[] { "scam-gift-card", "dev-scam-tech-support", "scam-lottery-prize", "scam-meme-coin", "credits-wallet" } });
                            }
                        }
                    }
                    unlockFloors = GUILayout.Toggle(unlockFloors, T(" Destrancar Todos os Andares", " Unlock All Floors"));
                    break;

                case 5:
                    GUILayout.Label(T("=== Configurações ===", "=== Settings ==="));
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(T("Idioma: ", "Language: "));
                    if (GUILayout.Button(langIndex == 0 ? "[ Português ]" : "Português")) langIndex = 0;
                    if (GUILayout.Button(langIndex == 1 ? "[ English ]" : "English")) langIndex = 1;
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                    GUILayout.Label(T("Tecla de Menu: [Insert]", "Toggle Key: [Insert]"));

                    if (GUILayout.Button(T("Fechar Janela", "Close Window"))) ToggleMenu(false);
                    if (GUILayout.Button(T("Ejetar Menu", "Unload"))) Loader.Unload();
                    break;
            }

            GUI.DragWindow(new Rect(0, 0, winRect.width, 25));
        }
    }
}

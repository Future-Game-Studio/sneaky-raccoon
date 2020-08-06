using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    PlayerController _player;
    SpawnManager _trashSpawner;
    CameraManager _mainCamera;
    UIController _uiController;
    OwnedItems _ownedItems; 
    AudioController _audioController;

    // Start is called before the first frame update
    void Start()
    {
        _player = TryToFindPlayer();
        _mainCamera = TryToFindMainCamera();
        _trashSpawner = TryToFindTrashSpawner();
        _uiController = TryToFindUIController();
        _ownedItems = GetOwnedItems();
        _audioController = TryToGetAudioController();

        StartCoroutine(SetScoreUI());
        ChangeAudioForSet();
    }

    private AudioController TryToGetAudioController() {
        try {
            return GetComponent<AudioController>();
        }
        catch {
            Debug.LogError("Camera Manager not found! Please, add Camera Manager to scene for correct gameplay!");
            return null;
        }
    }

    private CameraManager TryToFindMainCamera() {
        try {
            return GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();
        }
        catch {
            Debug.LogError("Camera Manager not found! Please, add Camera Manager to scene for correct gameplay!");
            return null;
        }
    }

    private SpawnManager TryToFindTrashSpawner() {
        try {
            return GameObject.FindWithTag("Respawn").GetComponent<SpawnManager>();
        }
        catch {
            Debug.LogError("EnemySpawner not found! Please, add Trash Spawner to scene for correct gameplay!");
            return null;
        }
    }

    private PlayerController TryToFindPlayer() {
        try {
            return GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }
        catch {
            Debug.LogError("Player not found! Please, add Player to scene for correct gameplay!");
            return null;
        }
    }

    private UIController TryToFindUIController() {
        try {
            return GameObject.FindGameObjectWithTag("UIController").GetComponent<UIController>();
        }
        catch {
            Debug.LogError("UIController not found! Please, add UIController to scene for correct gameplay!");
            return null;
        }
    }

    private OwnedItems GetOwnedItems() {
        if (_uiController ==  null) return null;

        if (!PlayerPrefs.HasKey("OwnedItems"))
            return _uiController.ReadOwnedItemsData();
        else
            return JsonUtility.FromJson<OwnedItems>(PlayerPrefs.GetString("OwnedItems"));
    }

    private void FixedUpdate() {
        if (Input.GetKeyDown(KeyCode.Q))
            StopGame();

        if (Input.GetKeyDown(KeyCode.I))
            GoToShop();

        if (Input.GetKeyDown(KeyCode.S))
            StartGame();
        // -------------------------------------------
        if (Input.GetKeyDown(KeyCode.V))
            ChangePlayerSkin("car", "skin_01", 1);

        if (Input.GetKeyDown(KeyCode.B))
            ChangePlayerSkin("car", "skin_02", 2);

        if (Input.GetKeyDown(KeyCode.N))
            ChangePlayerSkin("car", "skin_03", 3);

        if (Input.GetKeyDown(KeyCode.M))
            ChangePlayerSkin("car", "skin_04", 4);
        
        if (Input.GetKeyDown(KeyCode.C))
            ChangePlayerSkin("car", "skin_05", 5);

        // -------------------------------------------
        if (Input.GetKeyDown(KeyCode.F))
            ChangePlayerSkin("skin", "skin_01", 6);

        if (Input.GetKeyDown(KeyCode.G))
            ChangePlayerSkin("skin", "skin_02", 7);

        if (Input.GetKeyDown(KeyCode.H))
            ChangePlayerSkin("skin", "skin_03", 8);

        if (Input.GetKeyDown(KeyCode.J))
            ChangePlayerSkin("skin", "skin_04", 9);
        
        if (Input.GetKeyDown(KeyCode.K))
            ChangePlayerSkin("skin", "skin_05", 10);

        // -------------------------------------------
        if (Input.GetKeyDown(KeyCode.Escape)) 
            Application.Quit(); 
    }

    private IEnumerator SetScoreUI(){
        while (true) {
            if (_player != null && _uiController != null)
                _uiController.SetScore(_player.TotalScore);
            yield return new WaitForEndOfFrame();
        }
    }

    public void ChangePlayerSkin(string animator, string trigger, int skinId) {
        Debug.Log("Changing player skin то " + animator + ":" + trigger);
        if (_player == null)
            _player = TryToFindPlayer();
            
        if (_uiController == null || _ownedItems == null) {
            _uiController = TryToFindUIController();
            _ownedItems = GetOwnedItems();
        }

        if (_player != null){
            ResetAllActiveSkinsForAnimator(animator);
            SetNewOwnedSkins(skinId);
            _player.ChangePlayerSkin(animator, trigger);
            ChangeAudioForSet();
        }
        else
            Debug.LogError("Player not found! Cannot change player skins");
    }

    private void ChangeAudioForSet() {
        if (_audioController == null)
            TryToGetAudioController();

        var equipedItems = _ownedItems.ownedItems.FindAll(item => item.active == 1);

        if (equipedItems == null) {
            Debug.LogError("No equipedItems");
            return;
        }
        
        if (equipedItems.Count > 2) Debug.LogWarning("More than two items equiped!");

        if (equipedItems[0].set == equipedItems[1].set && _audioController != null)
            _audioController.SwitchAudioToSet(equipedItems[0].set);
    }

    private void SetNewOwnedSkins(int skinId) {
        var itemWithThisId = _ownedItems.ownedItems.Find(item => item.id == skinId);
        if (itemWithThisId != null)
            itemWithThisId.active = 1;
        PlayerPrefs.SetString("OwnedItems", JsonUtility.ToJson(_ownedItems));
    }

    public void StartGame() {
        Debug.Log("Moving camera to start point. Start point index: 0");
        if (_mainCamera != null)
            _mainCamera.MoveCameraToPoint(0);

        // do not change the order of next lines 
        // because player will be not able to move in game
        if (_uiController != null)
            _uiController.StartGameUI();

        MovePlayerPositionAndDisableMoving(_player.InGamePosition);
        ChangeScaleForPlayer(_player.InGameScale);
        LoadPlayerScoreFromSaved();
        AllowPlayerMoves();
        StartSpawningTrash();
    }

    public void StopGame() {
        Debug.Log("Moving camera to start point. Start point index: 0");
        if (_mainCamera != null)
            _mainCamera.MoveCameraToPoint(0);

        // do not change the order of next lines 
        // because player will be not able to move in game
        DisableSpawnAndClearAllTrash();
        MovePlayerPositionAndDisableMoving(_player.InGamePosition);
        ChangeScaleForPlayer(_player.InShopScale);
        SavePlayerScore();
    }
    
    public void ContinueGame()
    {
        if (_uiController !=  null)
            _uiController.StartGameUI();

        MovePlayerPositionAndDisableMoving(_player.InGamePosition);
        ChangeScaleForPlayer(_player.InGameScale);
        LoadPlayerScoreFromSaved();
        AllowPlayerMoves();

        Debug.Log("Moving camera to start point. Start point index: 0");
        if (_mainCamera != null)
            _mainCamera.MoveCameraToPoint(0);

        StartSpawningTrash();
    }

    public void GoToShop() {
        SavePlayerScore();

        if (_uiController !=  null)
            _uiController.GoToShopUI();
        
        MovePlayerPositionAndDisableMoving(_player.ShopPosition);
        ChangeScaleForPlayer(_player.InShopScale);

        Debug.Log("Moving camera to menu point. Menu point index: 1");
        if (_mainCamera != null)
            _mainCamera.MoveCameraToPoint(1);

        DisableSpawnAndClearAllTrash();
    }

    public void ChangeScaleForPlayer(Vector3 scale) {
        Debug.Log("Changing player scale to correct");
        if (_player != null) 
            _player.ChangePlayerScale(scale);
    }

    public void AllowPlayerMoves() {
        Debug.Log("Allowing player to move...");
            _player.AllowPlayerMove();
    }

    void MovePlayerPositionAndDisableMoving(Vector3 position){
        Debug.Log("Reseting player's position...");
        if (_player != null) {
            _player.DisablePlayerMove();
            _player.MovePlayerToPosition(position);
        }
    }

    public void StartSpawningTrash() {
        Debug.Log("Starting to spawn enemies!");
        _trashSpawner.StartSpawning(LocationsEnum.Earth);
    }

    public void DisableSpawnAndClearAllTrash() {
        Debug.Log("Stoping trash spawning...");
        if (_trashSpawner != null)
            _trashSpawner.PauseSpawning();
        Debug.Log("Starting process of cleaning...");

        var allFreeTrahs = GameObject.FindGameObjectsWithTag("Trash");
        int trahsCounter = 0;
        foreach (var trash in allFreeTrahs) {
            Destroy(trash);
            trahsCounter++;
        }

        /*var allLandedTrash = GameObject.FindGameObjectsWithTag("TrashOnEarth");
        foreach (var trash in allLandedTrash) {
            Destroy(trash);
            trahsCounter++;
        }*/

        Debug.Log("All trash cleared! Total trash was: " + trahsCounter);
    }

    public void SavePlayerScore() {
        Debug.Log("Saving player score...");
        if (_player != null)
            PlayerPrefs.SetInt("PlayerScore", _player.TotalScore);
    }

    public void LoadPlayerScoreFromSaved() {
        Debug.Log("Loading player score...");
        if (_player != null && PlayerPrefs.HasKey("PlayerScore"))
            _player.TotalScore = PlayerPrefs.GetInt("PlayerScore");
    }

    public bool BuySkin(int skinIndex, int price, string animator, string trigger, string set) {
        Debug.Log("Buying a sking " + skinIndex + " with price " + price);
        if (_player != null) {
            if (_player.TotalScore < price) {
                Debug.Log("You do not have enaught money to buy this item!");
                return false;
            }

            _player.TotalScore -= price;
            _ownedItems.ownedItems.Add(new OwnedItem(skinIndex, 0, animator, set));
            ChangePlayerSkin(animator, trigger, skinIndex);
            SavePlayerScore();
            return true;
        }
        Debug.Log("Cannot buy a sking " + skinIndex + " because player not found!");
        return false;        
    }

    private void ResetAllActiveSkinsForAnimator(string animator) {
        var allSkinsWithThisAnimator = _ownedItems.ownedItems.FindAll(item => item.animator == animator);
        foreach (var item in allSkinsWithThisAnimator)
            item.active = 0;
    }
}

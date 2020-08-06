using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class ShopItem {
    public int id;
    public string sprite;
    public string animator;
    public string set;
    public string trigger;
    public int price;
}

[System.Serializable]
public class ShopItems {
    public List<ShopItem> shopItems;    
}

[System.Serializable]
public class OwnedItem {
    public int id;
    public int active;
    public string animator;
    public string set;

    public OwnedItem(int id, int active, string animator, string set) {
        this.id = id;
        this.active = active;
        this.animator = animator;
        this.set = set;
    }
}

[System.Serializable]
public class OwnedItems {
    public List<OwnedItem> ownedItems;
}

public class UIController : MonoBehaviour
{
    public Animator GuiAnimator;
    public Animator GuiAnimator2;
    public Transform CarsGrid;
    public Transform SkinsGrid;
    public GameObject CarItem;
    public GameObject SkinItem;
    public GameObject CarsShop;
    public GameObject SkinShop;
    public GameObject Panel_health;
    public Text Score;
    public string ShopItemsFilePath = "GameData/shop_items";
    public string OwnedItemsFilePath = "GameData/owned_items";
    private ShopItems ShopItemsData;
    private OwnedItems OwnedItemsData;
    private GameManager _gameManager;
    //---------------------------------------------------------------------------------------------------------------??????????????????????????
    public static UIController CurrentUIController;

    // Start is called before the first frame update
    void Start()
    {
        CurrentUIController = this;
        try {
            _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        }
        catch {
            Debug.LogError("UIController not found! Please, add UIController to scene for correct gameplay!");
        }

        if (!PlayerPrefs.HasKey("OwnedItems"))
            OwnedItemsData = ReadOwnedItemsData();
        else
            OwnedItemsData = JsonUtility.FromJson<OwnedItems>(PlayerPrefs.GetString("OwnedItems"));
        ShopItemsData = ReadShopItemsData();
        CreateItemsInShop();
        //Get main camera
        CameraMain = Camera.main;

    }

    /*
      MaxHealth: the max value of player
      setHealth: current health value

      This function visualize the player health on the panel_health
    */
    public static void setHealth(int MaxHealth, int setHealth)
    {
        int percentages = (int)((float)setHealth / MaxHealth * 100);
        if (percentages < 99)
        {
            CurrentUIController.Panel_health.transform.GetChild(2).gameObject.SetActive(false);
        }
        if (percentages < 66)
        {
            CurrentUIController.Panel_health.transform.GetChild(1).gameObject.SetActive(false);
        }
        if (percentages < 33)
        {
            CurrentUIController.Panel_health.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private ShopItems ReadShopItemsData() {
        try {
            var dataAsJson = Resources.Load<TextAsset>(ShopItemsFilePath);
            return JsonUtility.FromJson<ShopItems>(dataAsJson.text);
        }
        catch {
            Debug.LogError("Cannot find data file \"" + ShopItemsFilePath + "\"! Please, check is this file exist!");
            return null;
        }
    }

    public OwnedItems ReadOwnedItemsData() {
        try {
            Debug.Log(OwnedItemsFilePath);
            var dataAsJson = Resources.Load<TextAsset>(OwnedItemsFilePath);
            Debug.Log(dataAsJson.text);
            return JsonUtility.FromJson<OwnedItems>(dataAsJson.text);
        }
        catch {
            Debug.LogError("Cannot find data file \"" + OwnedItemsFilePath + "\"! Please, check is this file exist!");
            return null;
        }
    }

    private void CreateItemsInShop() {
        Debug.Log("Checking is shop items list is not empty...");
        if (ShopItemsData.shopItems != null) {
            Debug.Log("Found shop items data!");

            var availableCarItems = ShopItemsData.shopItems.FindAll(item => item.animator == "car");
            Debug.Log("Got list of cars in shop! Starting instantiating process...");
            InstantiateItemsInContainter(availableCarItems, CarItem, CarsGrid);
            Debug.Log("All cars instantiated!");

            var availableSkinItems = ShopItemsData.shopItems.FindAll(item => item.animator == "skin");
            Debug.Log("Got list of skins in shop! Starting instantiating process...");
            InstantiateItemsInContainter(availableSkinItems, SkinItem, SkinsGrid);
            Debug.Log("All skins instantiated!");
        }
        else
            Debug.Log("No data in shop items list!");
    }

    private void InstantiateItemsInContainter(List<ShopItem> availableCarItems, GameObject shopItemType, Transform shopItemGrid) {
        foreach (var item in availableCarItems) {
            Debug.Log("Instantiating " + item.animator + " shop item " + item.id);
            var newCar = Instantiate(shopItemType, shopItemGrid);
            newCar.SetActive(true);

            Debug.Log("Loading sprite for " + item.animator + " " + item.id + " : " + item.sprite);
            var newCarSprite = Resources.Load<Sprite>(item.sprite);
            newCar.transform.Find("icon").GetComponent<Image>().sprite = newCarSprite;

            Debug.Log("Setting up price text for " + item.animator + " " + item.id);
            newCar.transform.Find("btn_buy").Find("txt_price").GetComponent<Text>().text = item.price.ToString();

            Debug.Log("Setting up buttons functions for " + item.animator + " " + item.id);
            var buyButton = newCar.transform.Find("btn_buy").GetComponent<Button>();
            var equipButton = newCar.transform.Find("btn_equip").GetComponent<Button>();
            equipButton.onClick.AddListener(delegate { EquipButtonClicked(item); });
            buyButton.onClick.AddListener(delegate { BuyButtonWasClicked(buyButton.gameObject, equipButton.gameObject, item); });

            Debug.Log("Checking did player has this item las time and equip it if it was active...");
            bool isPlayerHasThisItem = OwnedItemsData.ownedItems.Count > 0 && OwnedItemsData.ownedItems.Exists(ownedItem => ownedItem.id == item.id);
            if (isPlayerHasThisItem)
            {
                SwitchBuyButtonToNotActive(buyButton.gameObject, equipButton.gameObject);
                Debug.Log(OwnedItemsData.ownedItems.Find(ownedItem => ownedItem.id == item.id).active == 1);
                if (_gameManager != null && OwnedItemsData.ownedItems.Find(ownedItem => ownedItem.id == item.id).active == 1)
                    _gameManager.ChangePlayerSkin(item.animator, item.trigger, item.id);
            }

        }
    }

    private void EquipButtonClicked(ShopItem item) {
        if (_gameManager != null)
            _gameManager.ChangePlayerSkin(item.animator, item.trigger, item.id);
    }
    private void BuyButtonWasClicked(GameObject thisButton, GameObject equipButton, ShopItem item) {
        if (_gameManager != null) {
            Debug.Log("Buy button for item " + item.id + " was clicked...");
            if (_gameManager.BuySkin(item.id, item.price, item.animator, item.trigger, item.set))
                SwitchBuyButtonToNotActive(thisButton, equipButton);
        }
    }

    private void SwitchBuyButtonToNotActive(GameObject thisButton, GameObject equipButton) {
        thisButton.SetActive(false);
        equipButton.SetActive(true);
    }

    public void ShowCars() {
        CarsShop.SetActive(true);
        SkinShop.SetActive(false);
    }

    public void ShowSkins() {
        CarsShop.SetActive(false);
        SkinShop.SetActive(true);
    }

    public void StartGameUI() {
        GuiAnimator.SetBool("shop_ON", false);
        GuiAnimator.SetBool("game_ON", true);
        GuiAnimator2.SetBool("is_ON", false);
    }

    public void GoToShopUI() {
        GuiAnimator.SetBool("shop_ON", true);
        GuiAnimator.SetBool("game_ON", false);
    }

    public void SetScore(int score) {
        Score.text = score.ToString();
    }

/*************************************************************************************************************************************************/

    private Camera CameraMain;
    private const float topScreenByY = 58.33f;
    private const float bottomScreenByY = -7.67f;

    /* Events */
    public class GameObjectUnityEvent : UnityEvent<GameObject> {}
    public UnityEvent<GameObject> UnityEventPlanetSelected = new GameObjectUnityEvent();
    [System.NonSerialized]
    public UnityEvent EventResizeCameraChanged = new UnityEvent();

    public GameObject buttonSelectPlanet;
    public GameObject buttonPlay;
    public GameObject AllPlanets;
    public GameObject CurrentPlanet;
    public GameObject Player;
    public GameObject spacePlane1;
    public GameObject spacePlane2;
    public GameObject[] rockets;
    private GameObject selectedPlanet;
    /* Move side: If the isRotatedToTop is true, rockets look to the top otherwise rockets look to the botton */
    private bool isRotatedToTop = true;

    float posOfMouseButtonDownInViewportPoint;
    float posOfMouseButtonDownInWorldPoint;
    System.DateTime timeOfMouseButtonDown;

    Vector3 velocityCamera;
    /* Current Velocity for the 'SwipingPlanets' method */
    float currentVelocityForSP;
    float cameraTargetY = 0;
    Coroutine CoroutineSwipingPlanets;
    bool isPressed = false;

    private IEnumerator BackgroundScroll(float y1, float y2)
    {
        float elapsed = 0;
        float
            s1_oldy = spacePlane1.transform.position.y,
            s2_oldy = spacePlane2.transform.position.y;

        while (elapsed <= 1)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 1);
            spacePlane1.transform.position =
                new Vector3(
                    0,
                    Mathf.SmoothStep(s1_oldy, y1, t),
                    0
                );
            spacePlane2.transform.position =
               new Vector3(
                   0,
                   Mathf.SmoothStep(s2_oldy, y2, t),
                   -1
               );
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator SetCameraToPosition(float x, float y, float time)
    {
        float elapsed = 0;

        float
            oldX = CameraMain.transform.position.x,
            oldY = CameraMain.transform.position.y;

        while (elapsed <= time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            CameraMain.transform.position =
                new Vector3(
                    Mathf.SmoothStep(oldX, x, t),
                    Mathf.SmoothStep(oldY, y, t),
                    -10
                );
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator SetPlayerToPosition(float x, float y, float time)
    {
        float elapsed = 0;

        float
            oldX = Player.transform.position.x,
            oldY = Player.transform.position.y;

        while (elapsed <= time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            Player.transform.position =
                new Vector3(
                    Mathf.SmoothStep(oldX, x, t),
                    Mathf.SmoothStep(oldY, y, t),
                    0
                );
            yield return new WaitForFixedUpdate();
        }
    }

    public void SetSelectedPlanet()
    {
        CameraMain.transform.position = new Vector3(0, 0, -10);
        
        Vector3 cpPos = CurrentPlanet.transform.position;
        Quaternion cpRot = selectedPlanet.transform.rotation;

        Destroy(CurrentPlanet);
        CurrentPlanet = Instantiate(selectedPlanet, cpPos, cpRot);
        CurrentPlanet.name = "CurrentPlanet";
        CurrentPlanet.SetActive(true);
        AllPlanets.SetActive(false);

        Player.transform.position = new Vector3(0, -1.36f, 0);

        spacePlane1.transform.position = new Vector3(0, 23.9f, 0);
        spacePlane2.transform.position = new Vector3(0, 27.04f, -1);

        EventResizeCameraChanged.RemoveAllListeners();
    }

    /* Hamster quickly flies to the planet and rockets are detached */
    public IEnumerator hamsterApproachPlanet()// логає коли потрібно досягнути планети
    {
        const float timeToApproachPlanet = .8f;

        float hamsterTargetY = selectedPlanet.transform.position.y + 6.27f;
        /* Rotate rockets */
        if (hamsterTargetY > Player.transform.position.y && !isRotatedToTop)
        {
            Debug.Log("top");
            /* Rotate rockets to the top */
            StartCoroutine( RotateRockets(true) );
            isRotatedToTop = true;
        }
        else if(hamsterTargetY < Player.transform.position.y && isRotatedToTop)
        {
            Debug.Log("down");
            /* Rotate rockets to the top */
            StartCoroutine( RotateRockets(false) );
            isRotatedToTop = false;
        }

        /* Hamster quickly flies to the planet */
        StartCoroutine(SetPlayerToPosition(0, hamsterTargetY, timeToApproachPlanet));

        yield return new WaitForSeconds(timeToApproachPlanet);

        /* Hide rockets for a planet */
        StartCoroutine(ChangeLayers2(rockets, 0, 0));
        /* Rockets unconnect from the player */
        StartCoroutine(ConnectRockets(false));
        /* Rockets rotations */
        if (isRotatedToTop)
        {
            /* Rockets rotations */
            StartCoroutine(RotateRockets(false));
        }

        /* Deactivate */
        StartCoroutine( ChangePlayerMode(false, 1) );
    }

    private IEnumerator ChangePlayerMode(bool isPlayMode, float time)
    {
        yield return new WaitForSeconds(time);
        //Player.GetComponent<PlayerController>().enabled = !isPlayMode;
        rockets[0].SetActive(isPlayMode);
        rockets[1].SetActive(isPlayMode);
        GuiAnimator2.SetBool("is_ON", !isPlayMode);
    }

    public void SetSelectedPlanetCoroutines(GameObject selectedPlanet)
    {
        this.selectedPlanet = selectedPlanet;
        /* Stop planets scrolling */
        StopCoroutine(CoroutineSwipingPlanets);
        UnityEventPlanetSelected.RemoveAllListeners();
        StartCoroutine(ResizeCamera(13f, 5f, 1));
        /* Hamster quickly flies to the planet and rockets are detached */
        StartCoroutine(hamsterApproachPlanet());
               
        StartCoroutine(SetCameraToPosition(0, selectedPlanet.transform.position.y + 7.67f, 1));

        /*
         selectedPlanet.transform.position.y + 7.67f + 23.9f
         selectedPlanet.transform.position.y + 7.67f + 27.04f
         */
        StartCoroutine(
            BackgroundScroll(
                    selectedPlanet.transform.position.y + 31.57f, 
                    selectedPlanet.transform.position.y + 34.71f
                )
            );

        EventResizeCameraChanged.AddListener(SetSelectedPlanet);
    }
    
    /* Rockets rotations */
    private IEnumerator RotateRockets(bool isTop = true)
    {
        /* Rotation time */
        const float rotationTime = .125f;

        float elapsed = 0;
        while (elapsed <= rotationTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationTime);
            for (int i = 0; i < rockets.Length; i++)
            {
                rockets[i].transform.rotation =
                    Quaternion.Euler(
                            (isTop ? 
                                Mathf.SmoothStep(
                                    180,
                                    0,
                                    t
                                )
                                :
                                Mathf.SmoothStep(
                                    0,
                                    180,
                                    t
                                )
                            ),
                            0,
                            0
                        );
            }
            yield return new WaitForFixedUpdate();
        }
    }

    /* 
     Rockets connect to the player 
     isConnected: If the isConnected is true, rockets will connect to the player (fly from the earth to the player)
     otherwise rockets will unconnet from player
    */
    private IEnumerator ConnectRockets(bool isConnected = true)
    {
        /* Time for the connection process */
        const float connectionTime = 1f;
        /* Initial distance to rocket */
        const float distanceToRocket = 3.64f;
        /* Initial/Final rocket scale */
        const float minRocketScale = .08f;
        /* Initial/Final rocket scale */
        const float maxRocketScale = .25f;

        float elapsed = 0;
        while (elapsed <= connectionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / connectionTime);
            for (int i = 0; i < rockets.Length; i++)
            {
                /* Move rockets to the player */
                rockets[i].transform.position = new Vector3(
                    rockets[i].transform.position.x,
                    (isConnected ? 
                            Mathf.SmoothStep(
                                Player.transform.position.y - distanceToRocket, 
                                Player.transform.position.y, 
                                t
                            )
                            :
                            Mathf.SmoothStep(
                                Player.transform.position.y,
                                Player.transform.position.y - distanceToRocket,
                                t
                            )
                        ),
                    0
                );
                /* Scale rockets */
                rockets[i].transform.localScale = new Vector3(
                    Mathf.SmoothStep(isConnected ? minRocketScale : maxRocketScale, isConnected ? maxRocketScale : minRocketScale, t),
                    Mathf.SmoothStep(isConnected ? minRocketScale : maxRocketScale, isConnected ? maxRocketScale : minRocketScale, t),
                    1
                );
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator ChangeLayers2(GameObject[] gameObjects, int layer, float time)
    {
        yield return new WaitForSeconds(time);
        for(int i = 0; i < gameObjects.Length; i++)
        {
            gameObjects[i].GetComponent<SpriteRenderer>().sortingOrder = layer;
        }
    }

    /* Used for: UI Button */
    public void SelectPlanet()
    {
        /* Activate */
        StartCoroutine(ChangePlayerMode(true, 0));

        const float distanceBetweenPlanets = 19;
        const float hamsterPosition = -1.36f;

        /* Rotate rockets to the top */
        
        rockets[0].transform.rotation = Quaternion.Euler(0, 0, 0);
        rockets[1].transform.rotation = Quaternion.Euler(0, 0, 0);
        isRotatedToTop = true;

        UnityEventPlanetSelected.AddListener(SetSelectedPlanetCoroutines);
        StartCoroutine(ResizeCamera(5f, 13f, 1));
        
        Quaternion currentPlanetRotation = CurrentPlanet.transform.rotation;

        for (int i = 0; i < AllPlanets.transform.childCount; i++)
        {
            AllPlanets.transform.GetChild(i).rotation = currentPlanetRotation;
        }

        /* Hide rockets for a planet */
        StartCoroutine(ChangeLayers2(rockets, 0, 0));
        ///* Rockets rotations */
        //StartCoroutine(RotateRockets());
        /* Rockets connect to the player */
        StartCoroutine(ConnectRockets());
        /* Hide rockets for a planet */
        StartCoroutine(ChangeLayers2(rockets, 51, 1));
        /* Find out the planet order in the hierarchy */
        float currentPlanetOrder = CurrentPlanet.GetComponent<PlanetController>().order;
        /* For fix the camera over the planet */
        cameraTargetY = currentPlanetOrder * distanceBetweenPlanets;
        /* For fix the camera over the planet */
        Player.transform.position = new Vector3(0, currentPlanetOrder * distanceBetweenPlanets + hamsterPosition, 0);
        /* Start the scrolling */
        CoroutineSwipingPlanets = StartCoroutine(SwipingPlanets());

        CurrentPlanet.SetActive(false);
        AllPlanets.SetActive(true);
    }

    /* Used for: Resize camera between the transitions to select "the planets mode" or "the game mode" */
    private IEnumerator ResizeCamera(float oldSize, float newSize, float time)
    {
        float elapsed = 0;
        while (elapsed <= time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            CameraMain.orthographicSize = Mathf.SmoothStep(oldSize, newSize, t);
            yield return new WaitForFixedUpdate();
        }
        EventResizeCameraChanged.Invoke();
    }
    /* 
     Main method for scrolling 
     Processes keystrokes
     */
    private IEnumerator SwipingPlanets()
    {
        /* For determine the move side */
        float playerYCopy = Player.transform.position.y;
        currentVelocityForSP = 0;
        velocityCamera = new Vector3(0, 0, 0);
        while (true)
        {
            /* Rockets rotations */
            if (
                /* Avoid random values */
                (Mathf.Abs(playerYCopy - Player.transform.position.y) > .005f) &&
                /* if the player moves in the opposite direction */
                (isRotatedToTop != (playerYCopy - Player.transform.position.y) < 0)
                )
            {
                /* Determine the move side */
                isRotatedToTop = (playerYCopy - Player.transform.position.y) < 0;
                /* Rotate rockets */
                StartCoroutine(RotateRockets(isRotatedToTop));
            }
            /* For determine the move side */
            playerYCopy = Player.transform.position.y;


            /* Smooth scrolling hamster */
            Player.transform.position =
                new Vector3(
                    Player.transform.position.x,
                    Mathf.SmoothDamp(Player.transform.position.y, cameraTargetY - 1.36f, ref currentVelocityForSP, 1f),
                    0);

            /* if the player clicks on the screen */
            if (Input.GetMouseButtonDown(0))
            {
                isPressed = true;
                posOfMouseButtonDownInViewportPoint = CameraMain.ScreenToViewportPoint(Input.mousePosition).y;
                posOfMouseButtonDownInWorldPoint = CameraMain.ScreenToWorldPoint(Input.mousePosition).y;
                timeOfMouseButtonDown = System.DateTime.Now;
            }
            /* if the player released the click on the screen */
            if (Input.GetMouseButtonUp(0))
            {
                isPressed = false;
                System.DateTime timeOfMouseButtonUp = System.DateTime.Now;
                float differenceBetweenMouseTimes = (float)(timeOfMouseButtonUp - timeOfMouseButtonDown).TotalSeconds;
                /* if the player scrolls quickly, scroll through inertia */
                if (differenceBetweenMouseTimes < 0.5f)
                {
                    float posOfMouseButtonUp = CameraMain.ScreenToViewportPoint(Input.mousePosition).y;
                    float differenceBetweenMousePositions = posOfMouseButtonDownInViewportPoint - posOfMouseButtonUp;
                    float cameraOffset =
                        76f *
                        differenceBetweenMousePositions /
                        (differenceBetweenMouseTimes + 1f);

                    cameraTargetY = Mathf.Clamp(CameraMain.transform.position.y + cameraOffset, bottomScreenByY, topScreenByY);
                }
            }

            if (isPressed)
            {
                float posOfMouseButtonUp = CameraMain.ScreenToWorldPoint(Input.mousePosition).y;
                float differenceBetweenMousePositions = posOfMouseButtonDownInWorldPoint - posOfMouseButtonUp;

                CameraMain.transform.position = new Vector3(
                CameraMain.transform.position.x,
                Mathf.Clamp(CameraMain.transform.position.y + differenceBetweenMousePositions, bottomScreenByY - 5, topScreenByY + 5),
                CameraMain.transform.position.z
                );

                cameraTargetY = CameraMain.transform.position.y;
            }
            else
            {
                CameraMain.transform.position =
                    Vector3.SmoothDamp(
                        CameraMain.transform.position,
                        new Vector3(0, cameraTargetY, -10),
                        ref velocityCamera,
                        .2f
                        );
            }
            yield return null;
        }
    }
}

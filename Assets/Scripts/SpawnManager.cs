using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum LocationsEnum
{
    Earth = 0,
    Mars = 1,
    DeathStar = 2,
    AlphaCentauri = 3
}

[Serializable]
public class TrashItem
{
    public int id;
    public string type;
    public string sprite;

    public int score;
    public int speed;
    public int damageToPlanet;
    public int damageToPlayer;
}

[Serializable]
public class Planet
{
    public int id;
    public string name;
    public List<TrashItem> trashItems;
    public List<float> trashProbability;
}

[Serializable]
public class Location
{
    /** Path to json file with list of plsnets **/
    private const string planetsListFilePath = "PlanetsData/planets_list";

    /**
     * The locations has all planets
    **/
    public List<Planet> locations;

    /**
     * The singleLocation has all locations
    **/
    public static Location singleLocation { private set; get; }

    /**
     * Return the Planet by the location id
     **/
    public static Planet GetCurrentPlanetById(LocationsEnum location)
    {
        return singleLocation.locations.Find(planet => planet.id == (int)location);
    }

    /** 
     * Static Constructors. 
     * The static constructor is used to initialize the singleLocation variable  
    **/
    static Location()
    {
        try
        {
            /** 
             * Read the JSON data from the file and put them to the singleLocation object 
             **/
            TextAsset dataAsJson = Resources.Load<TextAsset>(planetsListFilePath);
            singleLocation = JsonUtility.FromJson<Location>(dataAsJson.text);
        }
        catch
        {
            /**
             * If occur some error, it will be printed 
            **/
            throw new Exception("Problem with reading List of Locations /" + planetsListFilePath);
        }

        /** Check the equality **/
        for (short i = 0; i < singleLocation.locations.Count; i++)
        {
            if (singleLocation.locations[i].trashItems.Count != singleLocation.locations[i].trashProbability.Count)
            {
                throw new Exception("Error! Number of the trashItems is not equal to the number of trashProbability");
            }
            int trashItemsCount = singleLocation.locations[i].trashItems.Count;
            if (trashItemsCount > 0)
            {
                float percentages = 0;
                for (short j = 0; j < singleLocation.locations[i].trashItems.Count; j++)
                {
                    percentages += singleLocation.locations[i].trashProbability[j];
                }
                if (percentages > 101 || percentages < 99)
                {
                    throw new Exception("Error! Percentages is not equal to the 100 (must be 100% +-1%) ");
                }
            }
        }
    }
}

public class SpawnManager : MonoBehaviour
{
    /** Singleton **/

    private static SpawnManager instance;

    public static SpawnManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SpawnManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this as SpawnManager;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
    
    private Planet currentPlanet;
    public ushort stepsToIncreaseTrashCount = 5;/** Steps to increase the TrashCount variable **/

    public GameObject trashPrefab; /** trash to falling down **/
    public GameObject trashCloudsPrefab; /** trash to creating the clouds **/

    public float timeToLvlUp = 2.5f; /** Time to the staring the next level **/
    public int trashCount = 3; /** Check if the level can increase **/
    private bool pause = false;/** Pause **/

    private Coroutine spawningProcess;
    private Coroutine spawningCloudOfTrash;
    
    public void StartSpawning(LocationsEnum location)
    {
        if (pause)
        {
            pause = false;
            spawningProcess = StartCoroutine(GenarateFellDownTrash());
            spawningCloudOfTrash = StartCoroutine(GenarateCloud());
        }
        else
        {
            trashCount = 3;
            currentPlanet = Location.GetCurrentPlanetById(location);
            GenarateCloudOneTimeToHorizontal();
            spawningProcess = StartCoroutine(GenarateFellDownTrash());
            spawningCloudOfTrash = StartCoroutine(GenarateCloud());
        }
    }

    public void StopSpawning()
    {
        if (spawningProcess != null)
        {
            StopCoroutine(spawningProcess);
            StopCoroutine(spawningCloudOfTrash);
        }
        spawningProcess = null;
        spawningCloudOfTrash = null;
        pause = false;
    }

    public void PauseSpawning()
    {
        if (pause)
        {
            pause = false;
            spawningProcess = StartCoroutine(GenarateFellDownTrash());
            spawningCloudOfTrash = StartCoroutine(GenarateCloud());
        }
        else
        {
            pause = true;
            StopCoroutine(spawningProcess);
            StopCoroutine(spawningCloudOfTrash);
        }
    }

    private void GenarateCloudOneTimeToHorizontal()
    {
        float StartSpawningPoint = ScriptStaticVars.horizontalSize + .2f;
        for (float  i = StartSpawningPoint; i >= -StartSpawningPoint; i-=.5f)
        {
            for (short j = 0; j < 4; j++)
            {
                int randomTrashItemId = RandomWithProbability(currentPlanet.trashProbability);
                BuildTrashCloud(i, currentPlanet.trashItems[randomTrashItemId].sprite);
            }
        }
    }

    /** 
     * Random number with some probability
     * More info: https://docs.unity3d.com/Manual/RandomNumbers.html
    **/
    private int RandomWithProbability(List<float> probability)
    {
        float total = 0;

        foreach (float elem in probability)
        {
            total += elem;
        }

        float randomPoint = Random.value * total;

        for (int i = 0; i < probability.Count; i++)
        {
            if (randomPoint < probability[i])
            {
                return i;
            }
            else
            {
                randomPoint -= probability[i];
            }
        }
        return probability.Count - 1;
    }

    private IEnumerator GenarateCloud()
    {
        while (true)
        {
            /** Spawn 4 random TrashCloud items **/
            for (short i = 0; i < 4; i++)
            {
                int randomTrashItemId = RandomWithProbability(currentPlanet.trashProbability);
                BuildTrashCloud(ScriptStaticVars.horizontalSizePlusOne, currentPlanet.trashItems[randomTrashItemId].sprite);
            }
            /** Wait for the next generation **/
            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator GenarateFellDownTrash()
    {
        int orderInLayer = 0;
        while (true)
        {
            /** Spawning trash **/
            for (int i = 0; i < trashCount; i++)
            {
                int randomTrashItemId = RandomWithProbability(currentPlanet.trashProbability);
                BuildTrashByType(currentPlanet.trashItems[randomTrashItemId], orderInLayer);
                orderInLayer++;
            }
            /** Check whether it need to increase the amount of trash **/
            if (stepsToIncreaseTrashCount == 0)
            {
                trashCount++;
                stepsToIncreaseTrashCount = 5;
            }
            /** Countdown steps to increase the TrashCount **/
            stepsToIncreaseTrashCount--;
            yield return new WaitForSeconds(timeToLvlUp);
        }
    }

    private void BuildTrashCloud(float startPoint, string pathToSprite)
    {
        /** Start coordinate for spawning **/
        Vector3 startCoord = new Vector3(
            Random.Range(startPoint, startPoint + .5f),
            Random.Range(5f, 4f),
            0
            );
        /** End (target) coordinate for cling to the planet **/
        Vector3 aimCoord = new Vector3(
            -100,
            Random.Range(5.3f, 4f),
            0);
        GameObject trash = Instantiate(
            trashCloudsPrefab,
            startCoord,
            Quaternion.identity
        );

        /** SpriteRenderer **/
        SpriteRenderer trashSpriteRenderer = trash.GetComponent<SpriteRenderer>();
        trashSpriteRenderer.sprite = Resources.Load<Sprite>(pathToSprite);

        /* aimCoord */
        ScriptTrashCloudsController trashCloudsEntity = trash.GetComponent<ScriptTrashCloudsController>();
        trashCloudsEntity.aimPoint = aimCoord;

        /** Torque **/
        trashCloudsEntity.torque = Random.Range(0, 150);

        /** Speed **/
        trashCloudsEntity.speed = Random.Range(1f, 4f);
    }

    private void BuildTrashByType(TrashItem trashObject, int orderInLayer)
    {
        /** Start coordinate for spawning **/
        Vector3 startCoord = new Vector3(
            Random.Range(-ScriptStaticVars.horizontalSize, ScriptStaticVars.horizontalSize), 
            Random.Range(5.5f, 8.5f), 
            0
            );
        /** End (target) coordinate for cling to the planet **/
        Vector3 aimCoord = new Vector3(
            Random.Range(-ScriptStaticVars.horizontalSize, ScriptStaticVars.horizontalSize), 
            -5, 
            0);

        GameObject trash = Instantiate(
            trashPrefab, 
            startCoord, 
            Quaternion.identity
            );

        /** SpriteRenderer **/
        SpriteRenderer trashSpriteRenderer = trash.GetComponent<SpriteRenderer>();
        trashSpriteRenderer.sortingOrder = orderInLayer;
        trashSpriteRenderer.sprite = Resources.Load<Sprite>(trashObject.sprite);

        /** TrashItem **/
        TrashController trashEntity = trash.GetComponent<TrashController>();
        trashEntity.trashItem = trashObject;

        /** Torque **/
        trashEntity.torque = Random.Range(-20, 20);

        /** Target **/
        trashEntity.aimPoint = aimCoord;
        
        /** Random layer **/
        if (Random.Range(0, 2) == 0)
        {
            trash.layer = 8; // TrashOne
        }
        else
        {
            trash.layer = 9; // TrashSecond
        }
    }
}

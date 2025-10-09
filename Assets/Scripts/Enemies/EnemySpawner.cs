using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which spawns enemies in an area around it.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("GameObject References")]
    [Tooltip("The enemy prefab to use when spawning enemies")]
    public GameObject enemyPrefab = null;
    [Tooltip("The target of the spawned enemies")]
    public Transform target = null;

    [Header("Spawn Position")]
    [Tooltip("The distance within which enemies can spawn in the X direction")]
    [Min(0)]
    public float spawnRangeX = 10.0f;
    [Tooltip("The distance within which enemies can spawn in the Y direction")]
    [Min(0)]
    public float spawnRangeY = 10.0f;

    [Header("Spawn Variables")]
    [Tooltip("The maximum number of enemies that can be spawned from this spawner")]
    public int maxSpawn = 20;
    [Tooltip("Ignores the max spawn limit if true")]
    public bool spawnInfinite = true;

    // The number of enemies that have been spawned
    private int currentlySpawned = 0;

    [Tooltip("The time delay between spawning enemies. This will be divided by the Score divided by the Difficulty variable")]
    public float spawnDelay = 2.5f;

    //How quickly should spawns increase frequency?
    [Tooltip("The denominator in the Score/Difficulty calculation. A higher value will make the game progress slower.")]
    public float difficultyIncreaseThreshold = 500;

    // The most recent spawn time
    private float lastSpawnTime = Mathf.NegativeInfinity;

    [Tooltip("The object to make projectiles child objects of.")]
    public Transform projectileHolder = null;



    /// <summary>
    /// Description:
    /// Standard Unity function called every frame
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void Update()
    {
        CheckSpawnTimer();
    }

    /// <summary>
    /// Description:
    /// Checks if it is time to spawn an enemy
    /// Spawns an enemy if it is time
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void CheckSpawnTimer()
    {
        ///this probably results in a gradual change over time. 
        /// It's also linear, which is frustrating for pacing reasons. I don't have the mental willpower to crunch the 
        /// numbers that could make this exponential, though, so it stays 
        /// as it stands, this is fine, because I want an abrupt increase to be signaled to the player and I have no way to 
        /// do that without breaking the game loop
        float adjustedSpawnDelay = spawnDelay / (GameManager.score / difficultyIncreaseThreshold);

        if (adjustedSpawnDelay > spawnDelay)
        {
            adjustedSpawnDelay = spawnDelay; //prevent potentially nasty number interactions resulting in an exorbitant wait
            // Debug.Log("Difficulty adjuster returned illegally large value. Correcting..."); // this spams the console every frame. My current understanding of C# won't let me fix this
        }

        if (float.IsInfinity(adjustedSpawnDelay))
        {
            adjustedSpawnDelay = spawnDelay; //prevents the game from softlocking immediately
            Debug.Log("Difficulty adjuster returned invalid value, overriding to prevent softlock"); //see line 83
        }
        
        // If it is time for an enemy to be spawned
        if (Time.timeSinceLevelLoad > lastSpawnTime + adjustedSpawnDelay && (currentlySpawned < maxSpawn || spawnInfinite))
        {
            // Determine spawn location
            Vector3 spawnLocation = GetSpawnLocation();

            // Spawn an enemy
            SpawnEnemy(spawnLocation);

            //move to new location
            LocationRandomizer();
        }
    }

    /// A more just world would have me make this customizable and not hardcoded
    /// But I'm a bit pressed for time
    /// Maybe the opportunity will present itself as I program
    /// <summary>
    /// Generates random number between 0 and 4
    /// Teleports the object to one of four positions based on that number
    /// </summary>
    private void LocationRandomizer()
    {
        int newLocation = UnityEngine.Random.Range(0, 4);
        switch (newLocation)
        { //oh floof this is going to be a pita if I want to make this customizable later
            case 0: //goes north
                gameObject.transform.position = new Vector3(0, 15);
                Debug.Log("Going Up!");
                break;
            case 1: //goes west(?)
                gameObject.transform.position = new Vector3(15, 0);
                Debug.Log("Going Left!");
                break;
            case 2: //goes east
                gameObject.transform.position = new Vector3(-15, 0);
                Debug.Log("Going Right!");
                break;
            case 3: //goes south
                gameObject.transform.position = new Vector3(0, -15);
                Debug.Log("Going Down!");
                break;
            default: //floof help us if we see this
                Debug.LogWarning("Switch returned a value that shouldn't be possible! The position has been left unchanged and you probably need to check it out");
                break;

        }
    }

    /// <summary>
    /// Description:
    /// Spawn and set up an instance of the enemy prefab
    /// Inputs: 
    /// Vector3 spawnLocation
    /// Returns: 
    /// void (no return)
    /// </summary>
    /// <param name="spawnLocation">The location to spawn an enmy at</param>
    private void SpawnEnemy(Vector3 spawnLocation)
    {
        // Make sure the prefab is valid
        if (enemyPrefab != null)
        {
            // Create the enemy gameobject
            GameObject enemyGameObject = Instantiate(enemyPrefab, spawnLocation, enemyPrefab.transform.rotation, null);
            Enemy enemy = enemyGameObject.GetComponent<Enemy>();
            ShootingController[] shootingControllers = enemyGameObject.GetComponentsInChildren<ShootingController>();

            // Setup the enemy if necessary
            if (enemy != null)
            {
                enemy.followTarget = target;
            }
            foreach (ShootingController gun in shootingControllers)
            {
                gun.projectileHolder = projectileHolder;
            }

            // Incremment the spawn count
            currentlySpawned++;
            lastSpawnTime = Time.timeSinceLevelLoad;
        }
    }

    /// <summary>
    /// Description:
    /// Returns a generated spawn location for an enemy
    /// Inputs: 
    /// none
    /// Returns: 
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The spawn location as determined by the function</returns>
    protected virtual Vector3 GetSpawnLocation()
    {
        // Get random coordinates
        float x = UnityEngine.Random.Range(0 - spawnRangeX, spawnRangeX);
        float y = UnityEngine.Random.Range(0 - spawnRangeY, spawnRangeY);
        // Return the coordinates as a vector
        return new Vector3(transform.position.x + x, transform.position.y + y, 0);
    }
}

using System.Collections;
using UnityEngine;

[UnitySingleton(UnitySingletonAttribute.Type.FromPrefab, false)]
public class GameMaster : UnitySingleton<GameMaster> {
    protected GameMaster() { } // Singleton - can't use the constructor!
    public Transform playerPrefab;
    public Transform spawnPoint;
    public float spawnDelay = 2;
    public Transform spawnPrefab;

    public IEnumerator RespawnPlayer() {
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(spawnDelay);

        // Spawn player
        Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        // Spawn effect particles
        Transform clone = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);
        Destroy(clone.gameObject, 3f);
    }
}


internal static class Initializer {
    [RuntimeInitializeOnLoadMethod] // We don't want lazy initialization for the GameMaster
    private static void OnRuntimeMethodLoad() { GameMaster.TouchInstance(); }
}
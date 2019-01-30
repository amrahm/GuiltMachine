using System.Collections;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    public static GameMaster gm;

    private void Awake()
    {
        if (gm == null)
        {
            gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameMaster>();
        }
    }

    public Transform playerPrefab;
    public Transform spawnPoint;
    public float spawnDelay = 2;
    public Transform spawnPrefab;

    public IEnumerator RespawnPlayer()
    {
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(spawnDelay);

        // Spawn player
        Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        // Spawn effect particles
        Transform clone = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);
        Destroy(clone.gameObject, 3f);
    }
}

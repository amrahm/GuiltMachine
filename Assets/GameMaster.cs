using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

	public static void KillPlayer(Player player)
    {
        Destroy(player.gameObject);
    }
}

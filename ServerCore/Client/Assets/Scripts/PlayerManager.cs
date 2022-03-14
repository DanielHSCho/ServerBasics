using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    MyPlayer _myPlayer;
    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public static PlayerManager Instance { get; } = new PlayerManager();

    public void Add(S_PlayerList packet)
    {
        Object obj = Resources.Load("Player");

        foreach(S_PlayerList.Player playerPkt in packet.players) {
            GameObject go = Object.Instantiate(obj) as GameObject;

            if (playerPkt.isSelf) {
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.transform.position = new Vector3(playerPkt.posX, playerPkt.posY, playerPkt.posZ);
                _myPlayer = myPlayer;
            } else {
                Player player = go.AddComponent<Player>();
                player.transform.position = new Vector3(playerPkt.posX, playerPkt.posY, playerPkt.posZ);
                _players.Add(playerPkt.playerId, player);
            }
        }
    }
}

using UnityEngine;

public class GameMenu : MonoBehaviour
{
    void Start()
    {
        
    }

    public void OnClickRespawn()
    {
        var mapObj = GameMaster.Instance.GetCurrentMap();
        if (mapObj == null)
            return;

        mapObj.SpawnPlayer();
    }
}

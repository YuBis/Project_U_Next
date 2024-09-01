using UnityEngine;

public class LobbyMenu : MonoBehaviour
{
    void Start()
    {
        
    }

    public void OnClickStartGame()
    {
        StateManager.Instance.ChangeState(StateType.TEST);
    }
}

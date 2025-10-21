using UnityEngine;

public class RoomListButton : MonoBehaviour
{
    private string roomName;

    private void OnEnable()
    {
        if(transform.TryGetComponent(out UnityEngine.UI.Button btn))
        {
            btn.onClick.AddListener(JoinRoom);
        }
    }

    private void OnDisable()
    {
        if(transform.TryGetComponent(out UnityEngine.UI.Button btn))
        {
            btn.onClick.RemoveListener(JoinRoom);
        }
    }

    private void JoinRoom()
    {
        RoomList.Singleton.JoinRoomByName(roomName);
    }

    public void SetRoomName(string name)
    {
        roomName = name;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    [SerializeField]
    SocketManager socketManager;
    public void OnButtonClicked(TMP_Text _text)
    {
        if (socketManager.sentChoice == false)
        {
            socketManager.SendPlayerChoiceToServer(_text.text);
            socketManager.playerChoice.text = "You chose: " + _text.text;
            socketManager.sentChoice = true;
        }
    }
}

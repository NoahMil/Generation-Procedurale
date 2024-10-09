using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid : MonoBehaviour
{
    [ContextMenu("CALL")]
    public void Call()
    {
        float cutProb = Random.Range(0.2f, 0.8f);
        int roomHeight = 10;
        int cutValue = Mathf.FloorToInt(roomHeight * cutProb);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Call();
        }
    }


    private List<Room> Bsp(Room room)
    {
        bool divideTopDown = Random.value > 0.5f;
        float cutProb = Random.Range(0.2f, 0.8f);
        int cutValue = Mathf.RoundToInt(cutProb * 10);

        if (divideTopDown)
        {
            if (divideTopDown)
            {
                Room firstSplitRoom = new Room(room.Width, room.Height * cutValue, new Vector2Int(room.Position.x, room.Position.y - cutValue));
            }
        }

        else
        {
            
        }
        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *  This class controls your player. It deals with user inputs such as mouse and keyboard.
 *  It also controls the game state.
 *  
 */
public class Game : MonoBehaviour
{
    public GameObject player;
    public Room[] rooms;
    public AudioClip[] numbers;
    public Texture2D[] digits;
    public Camera playerCamera;
    public Texture2D circleCursor;

    public enum MODE { WALK, CONTROL};
    public MODE mode = MODE.WALK;

    Room currentRoom;
    CharacterController controller;
    float speed = 4; 
    float gravity = 0; 

    public static Game instance;

    
    void Start()
    {
        instance = this;
        controller = player.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i].ResetCode();
            var panel = rooms[i].GetComponent<Panel>();
            if ( panel != null) panel.callback = GotNumber;
        }
        currentRoom = rooms[0];
    }


    
    void GotNumber(Room room, int n, float probability)
    {
        GetComponent<AudioSource>().PlayOneShot(numbers[n]);
        Debug.Log("Predicted number " + n + "\nProbability " + (probability * 100) + "%");

        
        (bool correct, bool completed) = room.CheckCode(n);
        if (!correct)
        {
            
            currentRoom = room;
            Invoke("SoundAlarm", 0.5f);
        }
        if (completed)
        {
            if (room.doorState == Room.DOOR_STATE.CLOSED)
            {
                currentRoom = room;
                Invoke("PlayMessage", 1f); 
            }
            room.OpenDoor();
        }
    }

    
    void SoundAlarm()
    {
        currentRoom.GetComponent<Panel>().SoundAlarm();
        currentRoom.ResetCode();
    }

    void PlayMessage()
    {
        if (currentRoom.message != null)
        {
            GetComponent<AudioSource>().PlayOneShot(currentRoom.message);
        }
    }

    void Update()
    {
        
        float mouseSensitivity = 1f;
        float vert = Input.GetAxis("Vertical");
        float horiz = Input.GetAxis("Horizontal");
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;

        if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl)) 
        {
            for (int i = 0; i < rooms.Length; i++)
                rooms[i].OpenDoor();
        }

        float factor = 1;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) factor = 2;

        gravity -= 9.81f * Time.deltaTime;
        controller.Move((player.transform.forward * vert + player.transform.right * horiz) * Time.deltaTime * speed * factor + player.transform.up * (gravity) * Time.deltaTime);
        if (controller.isGrounded) gravity = 0;
   
        if (mode == MODE.WALK)
        {
            controller.transform.Rotate(Vector3.up, mouseX);
        }

        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (mode)
            {
                case MODE.WALK:
                    mode = MODE.CONTROL;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.SetCursor(circleCursor, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case MODE.CONTROL:
                    mode = MODE.WALK;
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
            }
        }

        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Application.Quit();
        }
    }
}

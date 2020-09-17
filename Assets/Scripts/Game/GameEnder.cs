using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnder : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Quit", 5.0f);   
    }

    private void Quit()
    {
        Application.Quit();
    }
}

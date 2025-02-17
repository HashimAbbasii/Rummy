using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManagerPersistancy : MonoBehaviour
{
    public static DataManagerPersistancy instance;
    // Start is called before the first frame update

    [SerializeField] public TNVirtualKeyboard tnVirtualKeyboard;
    void Start()
    {
        if(instance == null)
        {

            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

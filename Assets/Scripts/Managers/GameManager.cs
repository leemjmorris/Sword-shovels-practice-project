using UnityEngine;

public class GameManager : MonoBehaviour
{
   private void Start()
   {
       DontDestroyOnLoad(gameObject);
   }
}

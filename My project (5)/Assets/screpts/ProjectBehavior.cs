
using UnityEngine;

public class ProjectBehavior : MonoBehaviour
{
  
    public Transform firePosition;
    public GameObject Projectile;
   
    private void Update()
    {
     if (Input.GetKeyDown(KeyCode.E))
     {
        Instantiate(Projectile, firePosition.position, firePosition.rotation); 
     }
    }
}


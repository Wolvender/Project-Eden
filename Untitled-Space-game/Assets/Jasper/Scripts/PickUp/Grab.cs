using UnityEngine;

public class Grab : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        IPickUp pickUp = collision.gameObject.GetComponent<IPickUp>();
        if (pickUp != null)
        {
            pickUp.PickUp();
        }
    }
}

using UnityEngine;
using UnityEngine.Events;

public class NewMonoBehaviourScript : MonoBehaviour, IPickUp
{
    public UnityEvent OnPickUp;

    public void PickUp()
    {
        OnPickUp.Invoke();
    }
}

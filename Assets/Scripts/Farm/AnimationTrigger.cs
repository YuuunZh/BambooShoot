using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    public Animator Animator;
    public KeyCode key;
    void Start()
    {
        Animator=GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            Animator.SetTrigger("Go");
            print("yes");
        }
    }
}

using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    public SnakeController controller;

    void Update()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        HandleKeyboardInput();
#elif UNITY_ANDROID
        HandleSwipeInput();
#endif
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) controller.SetDirection(Vector3Int.up);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) controller.SetDirection(Vector3Int.down);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) controller.SetDirection(Vector3Int.left);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) controller.SetDirection(Vector3Int.right);
    }

    void HandleSwipeInput()
    {
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
        {
            startTouch = t.position;
        }

        if (t.phase == TouchPhase.Ended)
        {
            Vector2 swipe = t.position - startTouch;

            if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            {
                if (swipe.x > 0) controller.SetDirection(Vector3Int.right);
                else controller.SetDirection(Vector3Int.left);
            }
            else
            {
                if (swipe.y > 0) controller.SetDirection(Vector3Int.up);
                else controller.SetDirection(Vector3Int.down);
            }
        }
    }

    private Vector2 startTouch;
}
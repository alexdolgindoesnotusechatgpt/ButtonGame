using UnityEngine;
using TMPro;

public class ChatBubble : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text messageText;

    [Header("Movement")]
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private float floatDistance = 0.2f;

    // A list of annoying things the employee says
    private string[] messages = new string[]
    {
        "HI!",
        "What? Wow...",
        "Hey...",
        "Did you hear?",
        "About the boss...",
        "He's so bossy!",
        "Working hard?",
        "Look at this!",
        "Crazy weather!",
        "Do you have a sec?",
        "Just one question...",
        "Wait, really?"
    };

    private Vector3 startPos;
    private float timeOffset;

    void Start()
    {
        startPos = transform.position;
        timeOffset = Random.Range(0f, 10f); // Randomize start so they don't all move in sync

        // Pick a random message
        if (messageText != null)
        {
            string randomMsg = messages[Random.Range(0, messages.Length)];
            messageText.text = randomMsg;
        }
    }

    void Update()
    {
        // Simple bobbing motion
        float newY = startPos.y + Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatDistance;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnMouseDown()
    {
        // Pop visual effect or sound could go here
        Destroy(gameObject);
    }
}

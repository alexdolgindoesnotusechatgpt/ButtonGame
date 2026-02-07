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
        "HR says crying is allowed, but only between 12:00 and 12:15.",
        "You type really loud. It’s impressive. Like a woodpecker.",
        "Working hard or hardly working? Haha!",
        "Hey, quick question. It’s not about work. I just forgot my password. Again.",
        "I’m just gonna stand here and watch you for a sec. I need to absorb your productivity.",
        "Did you see the email I sent you? I sent it three seconds ago.",
        "Whoa, is that Excel? I love Excel. Can I show you a shortcut? Move over.",
        "You missed a comma. No, go back. Back. Right there.",
        "Let's circle back and touch base offline about the synergy.",
        "I'm just trying to be a changemaker in this cubicle space.",
        "My therapist says I need to practice 'boundary invasion'. How am I doing?",
        "Ideally, the battery bar should be green. Red is usually bad."
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
